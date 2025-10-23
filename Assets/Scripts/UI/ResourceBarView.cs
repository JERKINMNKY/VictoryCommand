using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace IFC.Systems.UI
{
    public class ResourceBarView : MonoBehaviour
    {
        [SerializeField] private Text label;

        public void Initialize(Text text)
        {
            label = text;
        }

        public void Refresh(CityViewModel viewModel)
        {
            if (label == null || !viewModel.IsValid)
            {
                return;
            }

            var sb = new StringBuilder();
            AppendResource(sb, viewModel, Data.ResourceType.Food);
            AppendResource(sb, viewModel, Data.ResourceType.Steel);
            AppendResource(sb, viewModel, Data.ResourceType.Oil);
            AppendResource(sb, viewModel, Data.ResourceType.RareMetal);
            sb.Append(" | Tokens ");
            sb.Append(viewModel.GetInventoryQuantity(BuildQueueSystem.UpgradeTokenId));
            label.text = sb.ToString();
        }

        private static void AppendResource(StringBuilder sb, CityViewModel vm, Data.ResourceType type)
        {
            var stockpile = GameStateBuilder.FindStockpile(vm.City, type);
            if (stockpile != null)
            {
                sb.Append(type);
                sb.Append(':');
                sb.Append(stockpile.amount.ToString("N0"));
                sb.Append(' ');
            }
        }
    }
}
