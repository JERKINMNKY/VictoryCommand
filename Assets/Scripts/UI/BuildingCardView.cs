using System;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace IFC.Systems.UI
{
    public class BuildingCardView : MonoBehaviour
    {
        [SerializeField] private Text nameText;
        [SerializeField] private Text levelText;
        [SerializeField] private Text costText;
        [SerializeField] private Text etaText;
        [SerializeField] private Text lockText;
        [SerializeField] private Button upgradeButton;

        private string _buildingKey;
        private CityUIController _controller;

        public void Initialize(Text name, Text level, Text cost, Text eta, Text lockLabel, Button button)
        {
            nameText = name;
            levelText = level;
            costText = cost;
            etaText = eta;
            lockText = lockLabel;
            upgradeButton = button;
        }

        public void Setup(string buildingKey, CityUIController controller)
        {
            _buildingKey = buildingKey;
            _controller = controller;
            nameText.text = buildingKey;
            if (upgradeButton != null)
            {
                upgradeButton.onClick.RemoveAllListeners();
                upgradeButton.onClick.AddListener(OnUpgradeClicked);
            }
        }

        public void Refresh(CityViewModel viewModel)
        {
            if (!viewModel.IsValid)
            {
                return;
            }

            int level = viewModel.GetBuildingLevel(_buildingKey);
            levelText.text = $"Level {level}";

            if (viewModel.TryGetNextLevelData(_buildingKey, out var data))
            {
                costText.text = FormatCost(data);
                int seconds = viewModel.GetNextUpgradeDuration(_buildingKey);
                etaText.text = seconds > 0 ? TimeSpan.FromSeconds(seconds).ToString("mm\\:ss") : "--";
        }
            else
            {
                costText.text = "Max";
                etaText.text = "--";
            }

            var fail = viewModel.EvaluateUpgrade(_buildingKey);
            if (upgradeButton != null)
            {
                upgradeButton.interactable = fail == BuildFail.None;
            }

            if (lockText != null)
            {
                if (fail == BuildFail.None)
                {
                    lockText.gameObject.SetActive(false);
                }
                else
                {
                    lockText.gameObject.SetActive(true);
                    lockText.text = GetFailMessage(fail);
                }
            }
        }

        private void OnUpgradeClicked()
        {
            _controller?.TryUpgrade(_buildingKey);
        }

        private static string FormatCost(Data.BuildingData data)
        {
            if (data == null || data.CostByResource == null || data.CostByResource.Count == 0)
            {
                return "--";
            }

            var sb = new StringBuilder();
            for (int i = 0; i < data.CostByResource.Count; i++)
            {
                var cost = data.CostByResource[i];
                sb.Append(cost.amount);
                sb.Append(' ');
                sb.Append(cost.resourceType);
                if (i < data.CostByResource.Count - 1)
                {
                    sb.Append(" | ");
                }
            }

            return sb.ToString();
        }

        private static string GetFailMessage(BuildFail fail)
        {
            switch (fail)
            {
                case BuildFail.RequiresToken:
                    return "Needs UpgradeToken";
                case BuildFail.InsufficientResources:
                    return "Needs Resources";
                case BuildFail.PrereqNotMet:
                    return "Prerequisite Missing";
                case BuildFail.TileCapReached:
                    return "Tile Cap Reached";
                case BuildFail.QueueFull:
                    return "Queue Full";
                default:
                    return "";
            }
        }
    }
}
