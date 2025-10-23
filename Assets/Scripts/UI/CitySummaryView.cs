using System.Collections.Generic;
using System.Text;
using IFC.Data;
using UnityEngine;
using UnityEngine.UI;

namespace IFC.Systems.UI
{
    public class CitySummaryView : MonoBehaviour
    {
        [SerializeField] private Text label;

        public void Initialize(Text text)
        {
            label = text;
        }

        public void Refresh(CityViewModel viewModel)
        {
            if (label == null || viewModel == null || !viewModel.IsValid)
            {
                return;
            }

            var city = viewModel.City;
            var sb = new StringBuilder();
            sb.Append("Morale: ");
            sb.AppendLine(city.morale.ToString("0.00"));

            int officerCount = city.officers?.Count ?? 0;
            if (city.officerCapacity > 0)
            {
                sb.AppendLine($"Officers: {Mathf.Min(officerCount, city.officerCapacity)} / {city.officerCapacity}");
            }
            else
            {
                sb.AppendLine($"Officers: {officerCount}");
            }

            sb.AppendLine($"March Slots: {city.marchSlots}");
            sb.AppendLine($"Transport Slots: {city.transportSlots}");
            sb.AppendLine($"Upgrade Tokens: {viewModel.GetInventoryQuantity(BuildQueueSystem.UpgradeTokenId)}");

            AppendStorage(sb, city);
            AppendProduction(sb, city);

            label.text = sb.ToString();
        }

        private static void AppendStorage(StringBuilder sb, CityState city)
        {
            if (city.stockpiles == null || city.stockpiles.Count == 0)
            {
                return;
            }

            sb.AppendLine("Storage (amount / capacity):");
            for (int i = 0; i < city.stockpiles.Count; i++)
            {
                var stockpile = city.stockpiles[i];
                sb.Append("  ");
                sb.Append(stockpile.resourceType);
                sb.Append(": ");
                sb.Append(stockpile.amount.ToString("N0"));
                sb.Append(" / ");
                sb.Append(stockpile.capacity.ToString("N0"));
                sb.AppendLine();
            }
        }

        private static void AppendProduction(StringBuilder sb, CityState city)
        {
            var totals = new Dictionary<ResourceType, double>();
            float populationGain = 0f;

            if (city.production != null)
            {
                for (int i = 0; i < city.production.Count; i++)
                {
                    var field = city.production[i];
                    if (field.fields <= 0 || field.outputPerField <= 0)
                    {
                        continue;
                    }

                    double amount = field.fields * field.outputPerField;
                    AddToTotals(totals, field.resourceType, amount);
                }
            }

            if (city.buildingFunctions != null)
            {
                for (int i = 0; i < city.buildingFunctions.Count; i++)
                {
                    var function = city.buildingFunctions[i];
                    switch (function.functionType)
                    {
                        case BuildingFunctionType.ResourceProduction:
                            AddToTotals(totals, function.resourceType, function.amountPerTick);
                            break;
                        case BuildingFunctionType.PopulationGrowth:
                            populationGain += function.amountPerTick;
                            break;
                        default:
                            break;
                    }
                }
            }

            if (totals.Count > 0)
            {
                sb.AppendLine("Base Production / tick:");
                foreach (var kvp in totals)
                {
                    sb.Append("  ");
                    sb.Append(kvp.Key);
                    sb.Append(": ");
                    sb.AppendLine(kvp.Value.ToString("N1"));
                }
            }

            if (populationGain > 0f)
            {
                sb.AppendLine($"Population Growth / tick: {populationGain:0.0}");
            }
        }

        private static void AddToTotals(Dictionary<ResourceType, double> totals, ResourceType resourceType, double amount)
        {
            if (amount <= 0)
            {
                return;
            }

            if (totals.TryGetValue(resourceType, out var current))
            {
                totals[resourceType] = current + amount;
            }
            else
            {
                totals[resourceType] = amount;
            }
        }
    }
}
