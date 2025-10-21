using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace IFC.Systems.UI
{
    public class CityUIController : MonoBehaviour, IUIRefreshable
    {
        [SerializeField] private GameLoop gameLoop;
        [SerializeField] private string cityId = "Capital";
        [SerializeField] private ResourceBarView resourceBar;
        [SerializeField] private TileCounterView tileCounter;
        [SerializeField] private ToastView toastView;
        [SerializeField] private List<BuildingCardView> buildingCards = new List<BuildingCardView>();
        [SerializeField] private string[] buildingOrder = { "TownHall", "GeneralHQ", "MilitaryInstitute", "Wall" };

        private CityViewModel _viewModel;

        private void OnEnable()
        {
            if (gameLoop == null)
            {
#if UNITY_2023_1_OR_NEWER
                gameLoop = Object.FindFirstObjectByType<GameLoop>();
#else
                gameLoop = Object.FindObjectOfType<GameLoop>();
#endif
            }

            if (buildingCards.Count == 0)
            {
                buildingCards.AddRange(GetComponentsInChildren<BuildingCardView>(true));
            }

            SetupCards();
            UIRefreshService.Register(this);
            Refresh();
        }

        private void OnDisable()
        {
            UIRefreshService.Unregister(this);
        }

        public void Refresh()
        {
            _viewModel = new CityViewModel(gameLoop, cityId);
            if (!_viewModel.IsValid)
            {
                return;
            }

            resourceBar?.Refresh(_viewModel);
            tileCounter?.Refresh(_viewModel);
            for (int i = 0; i < buildingCards.Count; i++)
            {
                buildingCards[i]?.Refresh(_viewModel);
            }
        }

        public void TryUpgrade(string buildingKey)
        {
            if (_viewModel == null || !_viewModel.IsValid)
            {
                toastView?.Show("No city loaded");
                return;
            }

            var controller = gameLoop?.BuildController;
            if (controller == null)
            {
                toastView?.Show("Build controller unavailable");
                return;
            }

            int targetLevel = _viewModel.GetBuildingLevel(buildingKey) + 1;
            if (controller.TryEnqueueUpgrade(cityId, buildingKey, targetLevel, out var fail, out var eta))
            {
                toastView?.Show($"Upgrade started ({eta:mm\\:ss})");
                UIRefreshService.RefreshAll();
            }
            else
            {
                toastView?.Show(GetFailMessage(fail));
            }
        }

        private static string GetFailMessage(BuildFail fail)
        {
            switch (fail)
            {
                case BuildFail.RequiresToken:
                    return "Requires UpgradeToken";
                case BuildFail.InsufficientResources:
                    return "Insufficient resources";
                case BuildFail.PrereqNotMet:
                    return "Prerequisite not met";
                case BuildFail.TileCapReached:
                    return "Tile cap reached";
                case BuildFail.QueueFull:
                    return "Queue full";
                default:
                    return "Cannot upgrade";
            }
        }

        public void Initialize(GameLoop loop, string city, ResourceBarView resource, TileCounterView counter, ToastView toast, List<BuildingCardView> cards)
        {
            gameLoop = loop;
            cityId = city;
            resourceBar = resource;
            tileCounter = counter;
            toastView = toast;
            buildingCards = cards;
            SetupCards();
            Refresh();
        }

        private void SetupCards()
        {
            for (int i = 0; i < buildingCards.Count && i < buildingOrder.Length; i++)
            {
                buildingCards[i].Setup(buildingOrder[i], this);
            }
        }
    }
}
