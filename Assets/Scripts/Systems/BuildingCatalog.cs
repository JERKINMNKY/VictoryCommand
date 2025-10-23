using System;
using System.Collections.Generic;
using System.Reflection;
using IFC.Data;
using UnityEngine;

namespace IFC.Systems
{
    public class BuildingCatalog
    {
        private readonly Dictionary<string, SortedDictionary<int, BuildingData>> _catalog = new Dictionary<string, SortedDictionary<int, BuildingData>>();
        private readonly Dictionary<string, BuildingDefinition> _definitions = new Dictionary<string, BuildingDefinition>();
        private readonly Dictionary<string, BuildingLevelCache> _levelCache = new Dictionary<string, BuildingLevelCache>();

        private static readonly FieldInfo NameField = typeof(BuildingData).GetField("_buildingName", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo LevelField = typeof(BuildingData).GetField("_level", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo TimeField = typeof(BuildingData).GetField("_upgradeTimeSeconds", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo CostField = typeof(BuildingData).GetField("_costByResource", BindingFlags.NonPublic | BindingFlags.Instance);

        public void Add(BuildingData data)
        {
            if (data == null)
            {
                return;
            }

            var key = string.IsNullOrEmpty(data.BuildingName) ? data.name : data.BuildingName;
            if (string.IsNullOrEmpty(key))
            {
                return;
            }

            if (!_catalog.TryGetValue(key, out var levels))
            {
                levels = new SortedDictionary<int, BuildingData>();
                _catalog.Add(key, levels);
            }

            levels[data.Level] = data;
        }

        public bool TryGet(string buildingKey, int level, out BuildingData data)
        {
            data = null;
            if (string.IsNullOrEmpty(buildingKey) || level <= 0)
            {
                return false;
            }

            if (_catalog.TryGetValue(buildingKey, out var levels) && levels.TryGetValue(level, out data))
            {
                return true;
            }

            return false;
        }

        public IEnumerable<int> GetLevels(string buildingKey)
        {
            if (string.IsNullOrEmpty(buildingKey) || !_catalog.TryGetValue(buildingKey, out var levels))
            {
                yield break;
            }

            foreach (var kvp in levels)
            {
                yield return kvp.Key;
            }
        }

        public bool TryGetDefinition(string buildingKey, out BuildingDefinition definition)
        {
            return _definitions.TryGetValue(buildingKey, out definition);
        }

        public void LoadDefinitions(BuildingDefinitionCollection collection)
        {
            if (collection == null || collection.buildings == null)
            {
                Debug.LogWarning("[BuildingCatalog] No building definitions supplied.");
                return;
            }

            for (int i = 0; i < collection.buildings.Count; i++)
            {
                var definition = collection.buildings[i];
                if (definition == null || string.IsNullOrEmpty(definition.id))
                {
                    continue;
                }

                if (_definitions.ContainsKey(definition.id))
                {
                    Debug.LogWarning($"[BuildingCatalog] Duplicate definition for {definition.id} ignored.");
                    continue;
                }

                _definitions.Add(definition.id, definition);
                _levelCache[definition.id] = new BuildingLevelCache(definition, collection);

                for (int level = 1; level <= Mathf.Max(1, definition.maxLevel); level++)
                {
                    var runtimeData = CreateRuntimeBuildingData(definition, collection, level);
                    Add(runtimeData);
                }
            }
        }

        public int GetMaxLevel(string buildingKey)
        {
            if (_definitions.TryGetValue(buildingKey, out var def))
            {
                return def.maxLevel;
            }

            return 0;
        }

        private BuildingData CreateRuntimeBuildingData(BuildingDefinition definition, BuildingDefinitionCollection collection, int level)
        {
            var instance = ScriptableObject.CreateInstance<BuildingData>();

            NameField?.SetValue(instance, definition.id);
            LevelField?.SetValue(instance, level);
            TimeField?.SetValue(instance, CalculateUpgradeTime(definition, collection, level));
            CostField?.SetValue(instance, BuildCostList(definition, collection, level));

            instance.buildingType = MapCategory(definition.category);
            instance.isUniquePerCity = definition.uniquePerCity;
            instance.unlockLevel = Mathf.Max(1, definition.townHallRequired);
            instance.maxPerCity = definition.maxPerCity;
            instance.requires = BuildPrerequisites(definition);
            return instance;
        }

        private int CalculateUpgradeTime(BuildingDefinition definition, BuildingDefinitionCollection collection, int level)
        {
            float multiplier = definition.GetTimeMultiplier(collection.timeCurveMultiplier);
            float value = definition.baseTimeSeconds * Mathf.Pow(multiplier, level - 1);
            var overrideData = definition.FindOverride(level);
            if (overrideData != null)
            {
                value *= overrideData.timeMultiplier;
            }

            return Mathf.Max(1, Mathf.RoundToInt(value));
        }

        private List<ResourceCost> BuildCostList(BuildingDefinition definition, BuildingDefinitionCollection collection, int level)
        {
            var costs = new List<ResourceCost>();
            if (definition.baseCosts == null)
            {
                return costs;
            }

            float exponent = definition.GetCostExponent(collection.costCurveExponent);
            var overrideData = definition.FindOverride(level);
            float overrideMultiplier = overrideData != null ? Mathf.Max(0f, overrideData.costMultiplier) : 1f;

            for (int i = 0; i < definition.baseCosts.Count; i++)
            {
                var baseCost = definition.baseCosts[i];
                if (string.IsNullOrEmpty(baseCost.resource))
                {
                    continue;
                }

                if (!Enum.TryParse<ResourceType>(baseCost.resource, true, out var resourceType))
                {
                    Debug.LogWarning($"[BuildingCatalog] Unknown resource '{baseCost.resource}' for building {definition.id}");
                    continue;
                }

                float scaled = baseCost.amount * Mathf.Pow(level, exponent) * overrideMultiplier;
                costs.Add(new ResourceCost
                {
                    resourceType = resourceType,
                    amount = Mathf.Max(0, Mathf.RoundToInt(scaled))
                });
            }

            return costs;
        }

        private List<BuildingRequirement> BuildPrerequisites(BuildingDefinition definition)
        {
            var list = new List<BuildingRequirement>();
            if (definition.prerequisites == null)
            {
                return list;
            }

            for (int i = 0; i < definition.prerequisites.Count; i++)
            {
                var prereq = definition.prerequisites[i];
                if (prereq == null || string.IsNullOrEmpty(prereq.buildingId))
                {
                    continue;
                }

                list.Add(new BuildingRequirement
                {
                    buildingType = prereq.buildingId,
                    minLevel = Mathf.Max(1, prereq.minLevel)
                });
            }

            return list;
        }

        private BuildingType MapCategory(string category)
        {
            if (string.IsNullOrEmpty(category))
            {
                return BuildingType.Core;
            }

            if (Enum.TryParse<BuildingType>(category, true, out var mapped))
            {
                return mapped;
            }

            switch (category.ToLowerInvariant())
            {
                case "core":
                    return BuildingType.Core;
                case "economy":
                case "resource":
                    return BuildingType.Resource;
                case "logistics":
                    return BuildingType.Logistics;
                case "military":
                    return BuildingType.Military;
                case "militarysupport":
                    return BuildingType.Support;
                case "research":
                    return BuildingType.Research;
                case "defense":
                    return BuildingType.Defense;
                case "lategame":
                case "endgame":
                    return BuildingType.LateGame;
                default:
                    return BuildingType.Core;
            }
        }

        private class BuildingLevelCache
        {
            public readonly BuildingDefinition Definition;
            public readonly BuildingDefinitionCollection Collection;

            public BuildingLevelCache(BuildingDefinition definition, BuildingDefinitionCollection collection)
            {
                Definition = definition;
                Collection = collection;
            }
        }

        public IEnumerable<string> GetAllKeys()
        {
            var set = new System.Collections.Generic.HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
            foreach (var key in _definitions.Keys)
            {
                set.Add(key);
            }

            foreach (var kvp in _catalog)
            {
                set.Add(kvp.Key);
            }

            return set;
        }
    }
}
