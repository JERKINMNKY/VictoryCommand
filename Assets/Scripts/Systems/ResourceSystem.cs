using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using IFC.Data;
using IFC.Systems.Officers;

// Layer: Core Simulation
// Life Domain: Autonomy & Logistics
namespace IFC.Systems
{
    /// <summary>
    /// Handles per-city production and morale adjustments.
    /// </summary>
    public class ResourceSystem : MonoBehaviour
    {
        private const long HardResourceCap = CityConstants.RESOURCE_HARD_CAP;
        public float moralePenaltyThreshold = 0.9f;
        public float minimumMoraleFactor = 0.35f;
        public float maximumMoraleFactor = 1.25f;
        [SerializeField]
        private AnimationCurve politicsToProduction = new AnimationCurve(
            new Keyframe(0f, 0.85f),
            new Keyframe(5f, 1f),
            new Keyframe(10f, 1.15f),
            new Keyframe(20f, 2f));

        private GameState _state;
        private IOfficerStatsProvider _officerStatsProvider;

        public void Initialize(GameState state)
        {
            _state = state;
        }

        public void SetOfficerStatsProvider(IOfficerStatsProvider provider)
        {
            _officerStatsProvider = provider;
        }

        public void ProcessTick(int secondsPerTick)
        {
            if (_state == null)
            {
                Debug.LogWarning("ResourceSystem not initialised with state");
                return;
            }

            var message = new StringBuilder();
            message.AppendLine("[ResourceSystem] Production tick");

            for (int c = 0; c < _state.cities.Count; c++)
            {
                var city = _state.cities[c];
                var officerBonuses = city.officerBonuses ?? new OfficerBonusState();
                float effectiveMorale = Mathf.Clamp(city.morale + officerBonuses.moraleBonus, 0f, 1.5f);
                float moraleFactor = CalculateMoraleFactor(effectiveMorale);
                float productionMultiplier = officerBonuses.GetProductionMultiplier();
                float politicsMultiplier = EvaluatePoliticsMultiplier(city.mayorOfficerId, out int politicsScore);
                float effectiveFactor = Mathf.Clamp(moraleFactor * productionMultiplier * politicsMultiplier,
                    minimumMoraleFactor * Mathf.Max(0.1f, productionMultiplier) * Mathf.Max(0.1f, politicsMultiplier),
                    maximumMoraleFactor * Mathf.Max(1f, productionMultiplier) * Mathf.Max(1f, politicsMultiplier));

                var resourceDelta = new Dictionary<ResourceType, long>();
                var flatBonusPerResource = new Dictionary<ResourceType, long>();
                var percentBonusPerResource = new Dictionary<ResourceType, float>();
                long flatBonusAll = 0;
                float percentBonusAll = 0f;
                float populationGain = 0f;

                // Legacy production fields
                for (int p = 0; p < city.production.Count; p++)
                {
                    var productionField = city.production[p];
                    if (productionField.fields <= 0 || productionField.outputPerField <= 0)
                    {
                        continue;
                    }

                    double raw = productionField.fields * productionField.outputPerField * effectiveFactor;
                    long produced = Math.Max(0L, (long)Math.Round(raw));
                    AddResourceDelta(resourceDelta, productionField.resourceType, produced);
                }

                // Building function outputs
                for (int f = 0; f < city.buildingFunctions.Count; f++)
                {
                    var function = city.buildingFunctions[f];
                    switch (function.functionType)
                    {
                        case BuildingFunctionType.ResourceProduction:
                            {
                                double raw = function.amountPerTick * effectiveFactor;
                                long produced = Math.Max(0L, (long)Math.Round(raw));
                                AddResourceDelta(resourceDelta, function.resourceType, produced);
                                break;
                            }
                        case BuildingFunctionType.PopulationGrowth:
                            populationGain += function.amountPerTick;
                            break;
                        case BuildingFunctionType.CapacityBoost:
                            if (function.capacityFlatBonus != 0)
                            {
                                if (function.appliesToAllResources)
                                {
                                    flatBonusAll += function.capacityFlatBonus;
                                }
                                else
                                {
                                    AddFlatBonus(flatBonusPerResource, function.resourceType, function.capacityFlatBonus);
                                }
                            }

                            if (Math.Abs(function.capacityPercentBonus) > float.Epsilon)
                            {
                                if (function.appliesToAllResources)
                                {
                                    percentBonusAll += function.capacityPercentBonus;
                                }
                                else
                                {
                                    AddPercentBonus(percentBonusPerResource, function.resourceType, function.capacityPercentBonus);
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }

                // Apply storage capacity bonuses before adding new resources
                for (int s = 0; s < city.stockpiles.Count; s++)
                {
                    var stockpile = city.stockpiles[s];
                    if (stockpile.baseCapacity <= 0)
                    {
                        stockpile.baseCapacity = Math.Min(stockpile.capacity, HardResourceCap);
                    }
                    stockpile.baseCapacity = Math.Min(stockpile.baseCapacity, HardResourceCap);

                    long flatBonus = flatBonusAll;
                    if (flatBonusPerResource.TryGetValue(stockpile.resourceType, out var specificFlat))
                    {
                        flatBonus += specificFlat;
                    }

                    float percentBonus = percentBonusAll;
                    if (percentBonusPerResource.TryGetValue(stockpile.resourceType, out var specificPercent))
                    {
                        percentBonus += specificPercent;
                    }

                    long capacity = stockpile.baseCapacity;
                    if (flatBonus != 0)
                    {
                        capacity = Math.Max(0L, capacity + flatBonus);
                    }

                    if (Math.Abs(percentBonus) > float.Epsilon)
                    {
                        capacity = (long)Math.Round(capacity * (1f + percentBonus));
                    }

                    capacity = Math.Max(0, Math.Min(HardResourceCap, capacity));
                    stockpile.capacity = capacity;
                    if (stockpile.amount > capacity)
                    {
                        stockpile.amount = capacity;
                    }
                }

                long totalProduced = 0;
                foreach (var kvp in resourceDelta)
                {
                    var stockpile = GameStateBuilder.FindStockpile(city, kvp.Key);
                    if (stockpile == null)
                    {
                        stockpile = new ResourceStockpile
                        {
                            resourceType = kvp.Key,
                            baseCapacity = 0,
                            capacity = 0,
                            amount = 0
                        };
                        city.stockpiles.Add(stockpile);
                    }

                    long capacity = Math.Min(HardResourceCap, Math.Max(0L, stockpile.capacity));
                    long available = Math.Max(0L, capacity - stockpile.amount);
                    long applied = Math.Min(available, Math.Max(0L, kvp.Value));
                    if (applied > 0)
                    {
                        stockpile.amount = Math.Min(HardResourceCap, stockpile.amount + applied);
                        totalProduced += applied;
                    }
                }

                if (populationGain > 0f)
                {
                    city.population += Mathf.RoundToInt(populationGain);
                }

                message.AppendLine($"  {city.displayName}: morale={city.morale:0.00} politics={politicsScore} mul={politicsMultiplier:0.00} factor={effectiveFactor:0.00} produced={totalProduced}");
            }

            Debug.Log(message.ToString());
        }

        private static void AddResourceDelta(Dictionary<ResourceType, long> map, ResourceType resource, long amount)
        {
            if (!map.TryGetValue(resource, out var current))
            {
                map[resource] = amount;
            }
            else
            {
                map[resource] = current + amount;
            }
        }

        private static void AddFlatBonus(Dictionary<ResourceType, long> map, ResourceType resource, long amount)
        {
            if (!map.TryGetValue(resource, out var current))
            {
                map[resource] = amount;
            }
            else
            {
                map[resource] = current + amount;
            }
        }

        private static void AddPercentBonus(Dictionary<ResourceType, float> map, ResourceType resource, float amount)
        {
            if (!map.TryGetValue(resource, out var current))
            {
                map[resource] = amount;
            }
            else
            {
                map[resource] = current + amount;
            }
        }

        private float CalculateMoraleFactor(float morale)
        {
            if (morale >= 1f)
            {
                return Mathf.Min(maximumMoraleFactor, morale + 0.1f);
            }

            if (morale >= moralePenaltyThreshold)
            {
                return morale;
            }

            if (morale <= 0f)
            {
                return minimumMoraleFactor;
            }

            float t = morale / moralePenaltyThreshold;
            return Mathf.Lerp(minimumMoraleFactor, moralePenaltyThreshold, t);
        }

        private float EvaluatePoliticsMultiplier(string mayorOfficerId, out int politics)
        {
            politics = 0;
            if (_officerStatsProvider == null || string.IsNullOrEmpty(mayorOfficerId))
            {
                return 1f;
            }

            if (_officerStatsProvider.TryGetPolitics(mayorOfficerId, out politics))
            {
                if (politicsToProduction == null || politicsToProduction.length == 0)
                {
                    return 1f;
                }

                var evaluated = politicsToProduction.Evaluate(politics);
                return Mathf.Clamp(evaluated, 0.1f, 3f);
            }

            return 1f;
        }
    }
}
