using System;
using System.Collections.Generic;
using IFC.Data;
using UnityEngine;

namespace IFC.Systems
{
    /// <summary>
    /// Generates building function states and other runtime modifiers based on catalog definitions.
    /// Keeps CityState.buildingFunctions in sync with actual building levels.
    /// </summary>
    public class BuildingEffectRuntime : MonoBehaviour
    {
        private GameState _state;
        private BuildingCatalog _catalog;
        private bool _subscribed;

        public void Initialize(GameState state, BuildingCatalog catalog)
        {
            _state = state;
            _catalog = catalog;
            RebuildAllCities();
            SubscribeEvents();
        }

        private void OnDisable()
        {
            UnsubscribeEvents();
        }

        public void RebuildAllCities()
        {
            if (_state == null)
            {
                return;
            }

            for (int i = 0; i < _state.cities.Count; i++)
            {
                RebuildCity(_state.cities[i]);
            }
        }

        public void RebuildBuilding(string cityId, string buildingId)
        {
            if (_state == null || string.IsNullOrEmpty(cityId) || string.IsNullOrEmpty(buildingId))
            {
                return;
            }

            var city = _state.GetCityById(cityId);
            if (city == null)
            {
                return;
            }

            ApplyBuildingEffects(city, buildingId);
        }

        private void RebuildCity(CityState city)
        {
            if (city == null)
            {
                return;
            }

            if (city.buildingFunctions == null)
            {
                city.buildingFunctions = new List<BuildingFunctionState>();
            }
            else
            {
                city.buildingFunctions.Clear();
            }

            city.officerCapacity = 0;
            city.marchSlots = 0;
            city.transportSlots = 0;

            for (int i = 0; i < city.buildings.Count; i++)
            {
                var building = city.buildings[i];
                if (building.level > 0)
                {
                    ApplyBuildingEffects(city, building.buildingKey);
                }
            }
        }

        private void ApplyBuildingEffects(CityState city, string buildingId)
        {
            if (_catalog == null || !_catalog.TryGetDefinition(buildingId, out var definition))
            {
                return;
            }

            if (city.buildingFunctions == null)
            {
                city.buildingFunctions = new List<BuildingFunctionState>();
            }

            city.buildingFunctions.RemoveAll(f => string.Equals(f.buildingId, buildingId, StringComparison.OrdinalIgnoreCase));

            var levelState = city.GetBuilding(buildingId);
            int level = levelState != null ? Mathf.Max(0, levelState.level) : 0;
            if (level <= 0)
            {
                return;
            }

            if (definition.effects == null)
            {
                return;
            }

            for (int i = 0; i < definition.effects.Count; i++)
            {
                var effect = definition.effects[i];
                if (effect == null)
                {
                    continue;
                }

                if (effect.minLevel > level)
                {
                    continue;
                }

                if (effect.maxLevel > 0 && level > effect.maxLevel)
                {
                    continue;
                }

                switch (effect.type)
                {
                    case "ResourceProduction":
                        ApplyResourceProduction(city, definition, level, effect);
                        break;
                    case "PopulationGrowth":
                        ApplyPopulationGrowth(city, definition, level, effect);
                        break;
                    case "TrainingSpeed":
                        ApplyTrainingSpeed(city, definition, level, effect);
                        break;
                    case "ResourceExchange":
                        ApplyResourceExchange(city, definition, level, effect);
                        break;
                    case "TokenGeneration":
                        ApplyTokenGeneration(city, definition, level, effect);
                        break;
                    case "StorageBonus":
                        ApplyStorageBonus(city, definition, level, effect, percent: false);
                        break;
                    case "StorageBonusPercent":
                        ApplyStorageBonus(city, definition, level, effect, percent: true);
                        break;
                    case "OfficerCap":
                        ApplyOfficerCap(city, definition, level, effect);
                        break;
                    case "MarchSlots":
                        ApplyMarchSlots(city, level, effect);
                        break;
                    case "TransportQueues":
                        ApplyTransportSlots(city, level, effect);
                        break;
                    case "TileCap":
                    case "TokenGate":
                    case "TransportSpeed":
                    case "TransportCapacity":
                    case "OfficerBonus":
                    case "MarchPayload":
                    case "EnergyRating":
                    case "QueueControl":
                    case "UnlockBuilding":
                    case "SlotUnlock":
                    case "UnlockResearchBranch":
                    case "UnlockRecruitment":
                    case "UnlockMission":
                    case "ConversionUnlock":
                    case "DamageMitigation":
                    case "DefenseDamage":
                    case "WallHp":
                    case "FortificationSlots":
                    case "IntelRange":
                    case "MarchWarning":
                    case "AirDispatchCooldown":
                    case "OfficerXp":
                    case "MaintenanceCost":
                    case "TrainingUpkeep":
                    case "CasualtyRecovery":
                    case "MoraleDecayReduction":
                    case "ResourceProductionPercent":
                        // Effects acknowledged but handled elsewhere (stat modifiers, unlock systems, etc.)
                        break;
                    default:
                        Debug.LogWarning($"[BuildingEffectRuntime] Unsupported effect '{effect.type}' for building {definition.id}");
                        break;
                }
            }
        }

        private void ApplyResourceProduction(CityState city, BuildingDefinition definition, int level, BuildingEffectDefinition effect)
        {
            if (!Enum.TryParse(effect.resource, true, out ResourceType resourceType))
            {
                Debug.LogWarning($"[BuildingEffectRuntime] Unknown resource '{effect.resource}' for building {definition.id}");
                return;
            }

            float amount = effect.baseAmount + effect.amountPerLevel * Mathf.Max(0, level - 1);
            var state = new BuildingFunctionState
            {
                buildingId = definition.id,
                buildingType = definition.id,
                functionType = BuildingFunctionType.ResourceProduction,
                level = level,
                amountPerTick = amount,
                resourceType = resourceType
            };

            city.buildingFunctions.Add(state);
        }

        private void ApplyPopulationGrowth(CityState city, BuildingDefinition definition, int level, BuildingEffectDefinition effect)
        {
            float amount = effect.baseAmount + effect.amountPerLevel * Mathf.Max(0, level - 1);
            var state = new BuildingFunctionState
            {
                buildingId = definition.id,
                buildingType = definition.id,
                functionType = BuildingFunctionType.PopulationGrowth,
                level = level,
                amountPerTick = amount
            };
            city.buildingFunctions.Add(state);
        }

        private void ApplyTrainingSpeed(CityState city, BuildingDefinition definition, int level, BuildingEffectDefinition effect)
        {
            float multiplier = effect.baseMultiplier + effect.perLevelDelta * Mathf.Max(0, level - 1);
            if (effect.minMultiplier > 0f)
            {
                multiplier = Mathf.Max(effect.minMultiplier, multiplier);
            }

            if (effect.maxMultiplier > 0f)
            {
                multiplier = Mathf.Min(effect.maxMultiplier, multiplier);
            }

            var state = new BuildingFunctionState
            {
                buildingId = definition.id,
                buildingType = definition.id,
                functionType = BuildingFunctionType.TrainingSpeedBuff,
                level = level,
                trainingMultiplier = multiplier,
                facilityId = string.IsNullOrEmpty(effect.targetFacility) ? definition.id : effect.targetFacility
            };
            city.buildingFunctions.Add(state);
        }

        private void ApplyResourceExchange(CityState city, BuildingDefinition definition, int level, BuildingEffectDefinition effect)
        {
            if (!Enum.TryParse(effect.resource, true, out ResourceType source))
            {
                Debug.LogWarning($"[BuildingEffectRuntime] Unknown exchange resource '{effect.resource}' for building {definition.id}");
                return;
            }

            if (!Enum.TryParse(effect.targetResource, true, out ResourceType target))
            {
                Debug.LogWarning($"[BuildingEffectRuntime] Unknown exchange target '{effect.targetResource}' for building {definition.id}");
                return;
            }

            float amount = effect.baseAmount + effect.amountPerLevel * Mathf.Max(0, level - 1);
            float rate = effect.baseRate + effect.ratePerLevel * Mathf.Max(0, level - 1);
            if (effect.maxRate > 0f)
            {
                rate = Mathf.Min(effect.maxRate, rate);
            }

            var state = new BuildingFunctionState
            {
                buildingId = definition.id,
                buildingType = definition.id,
                functionType = BuildingFunctionType.ResourceExchange,
                level = level,
                amountPerTick = amount,
                resourceType = source,
                targetResource = target,
                exchangeRate = rate
            };
            city.buildingFunctions.Add(state);
        }

        private void ApplyTokenGeneration(CityState city, BuildingDefinition definition, int level, BuildingEffectDefinition effect)
        {
            // Token generation is handled via dedicated inventory pipelines, not generic building functions.
        }

        private void ApplyStorageBonus(CityState city, BuildingDefinition definition, int level, BuildingEffectDefinition effect, bool percent)
        {
            bool appliesToAll = string.IsNullOrEmpty(effect.resource);
            ResourceType resourceType = ResourceType.Food;
            if (!appliesToAll)
            {
                if (!Enum.TryParse(effect.resource, true, out resourceType))
                {
                    Debug.LogWarning($"[BuildingEffectRuntime] Unknown storage resource '{effect.resource}' for building {definition.id}");
                    return;
                }
            }

            var state = new BuildingFunctionState
            {
                buildingId = definition.id,
                buildingType = definition.id,
                functionType = BuildingFunctionType.CapacityBoost,
                level = level,
                resourceType = resourceType,
                appliesToAllResources = appliesToAll
            };

            if (percent)
            {
                float percentBonus = 0f;
                if (effect.perLevel != 0f)
                {
                    percentBonus += effect.perLevel * level;
                }
                if (effect.baseAmount != 0f)
                {
                    percentBonus += effect.baseAmount;
                }
                if (effect.amountPerLevel != 0f)
                {
                    percentBonus += effect.amountPerLevel * Mathf.Max(0, level - 1);
                }
                if (effect.baseRate != 0f)
                {
                    percentBonus += effect.baseRate;
                }
                if (effect.ratePerLevel != 0f)
                {
                    percentBonus += effect.ratePerLevel * Mathf.Max(0, level - 1);
                }

                state.capacityPercentBonus = percentBonus;
            }
            else
            {
                float amount = effect.baseAmount + effect.amountPerLevel * Mathf.Max(0, level - 1);
                state.capacityFlatBonus = (long)Mathf.RoundToInt(amount);
            }

            city.buildingFunctions.Add(state);
        }

        private void ApplyOfficerCap(CityState city, BuildingDefinition definition, int level, BuildingEffectDefinition effect)
        {
            float value = effect.baseAmount + effect.amountPerLevel * Mathf.Max(0, level - 1);
            if (effect.perLevel != 0f)
            {
                value += effect.perLevel * level;
            }

            int delta = Mathf.Max(0, Mathf.RoundToInt(value));
            city.officerCapacity = Mathf.Max(0, city.officerCapacity + delta);
        }

        private void ApplyMarchSlots(CityState city, int level, BuildingEffectDefinition effect)
        {
            int thresholds = GetThresholdCount(effect, level);
            if (thresholds <= 0)
            {
                return;
            }

            int amountPerThreshold = effect.amount != 0 ? effect.amount : 1;
            city.marchSlots = Mathf.Max(0, city.marchSlots + thresholds * amountPerThreshold);
        }

        private void ApplyTransportSlots(CityState city, int level, BuildingEffectDefinition effect)
        {
            int thresholds = GetThresholdCount(effect, level);
            if (thresholds <= 0)
            {
                return;
            }

            int amountPerThreshold = effect.amount != 0 ? effect.amount : 1;
            city.transportSlots = Mathf.Max(0, city.transportSlots + thresholds * amountPerThreshold);
        }

        private static int GetThresholdCount(BuildingEffectDefinition effect, int level)
        {
            if (effect.levels != null && effect.levels.Count > 0)
            {
                int count = 0;
                for (int i = 0; i < effect.levels.Count; i++)
                {
                    if (level >= effect.levels[i])
                    {
                        count++;
                    }
                }
                return count;
            }

            if (effect.perLevel != 0f)
            {
                return Mathf.Max(0, Mathf.RoundToInt(effect.perLevel * level));
            }

            return level > 0 ? 1 : 0;
        }

        private void SubscribeEvents()
        {
            if (_subscribed)
            {
                return;
            }

            GameEventHub.Subscribe<BuildingConstructedEvent>(OnBuildingChanged);
            GameEventHub.Subscribe<BuildingUpgradedEvent>(OnBuildingUpgraded);
            _subscribed = true;
        }

        private void UnsubscribeEvents()
        {
            if (!_subscribed)
            {
                return;
            }

            GameEventHub.Unsubscribe<BuildingConstructedEvent>(OnBuildingChanged);
            GameEventHub.Unsubscribe<BuildingUpgradedEvent>(OnBuildingUpgraded);
            _subscribed = false;
        }

        private void OnBuildingChanged(BuildingConstructedEvent evt)
        {
            RebuildBuilding(evt.cityId, evt.buildingKey);
        }

        private void OnBuildingUpgraded(BuildingUpgradedEvent evt)
        {
            RebuildBuilding(evt.cityId, evt.buildingKey);
        }
    }
}
