using System;
using System.Collections.Generic;
using UnityEngine;

namespace IFC.Data
{
    [Serializable]
    public class BuildingDefinitionCollection
    {
        public int schemaVersion = 1;
        public float costCurveExponent = 1.25f;
        public float timeCurveMultiplier = 1.3f;
        public List<BuildingDefinition> buildings = new List<BuildingDefinition>();
    }

    [Serializable]
    public class BuildingDefinition
    {
        public string id;
        public string displayName;
        public string category;
        public int maxLevel = 1;
        public bool uniquePerCity = false;
        public int tiles = 1;
        public int townHallRequired = 1;
        public int maxPerCity = 0;
        public int baseTimeSeconds = 60;
        public List<BuildingCostDefinition> baseCosts = new List<BuildingCostDefinition>();
        public List<BuildingPrerequisite> prerequisites = new List<BuildingPrerequisite>();
        public List<BuildingLevelOverride> levelOverrides = new List<BuildingLevelOverride>();
        public List<BuildingEffectDefinition> effects = new List<BuildingEffectDefinition>();
        public float? costCurveExponent;
        public float? timeCurveMultiplier;

        public float GetCostExponent(float fallback) => costCurveExponent ?? fallback;

        public float GetTimeMultiplier(float fallback) => timeCurveMultiplier ?? fallback;

        public BuildingLevelOverride FindOverride(int level)
        {
            if (levelOverrides == null)
            {
                return null;
            }

            for (int i = 0; i < levelOverrides.Count; i++)
            {
                if (levelOverrides[i].level == level)
                {
                    return levelOverrides[i];
                }
            }

            return null;
        }
    }

    [Serializable]
    public class BuildingCostDefinition
    {
        public string resource;
        public int amount;
    }

    [Serializable]
    public class BuildingPrerequisite
    {
        public string buildingId;
        public int minLevel = 1;
    }

    [Serializable]
    public class BuildingLevelOverride
    {
        public int level;
        public float timeMultiplier = 1f;
        public float costMultiplier = 1f;
    }

    [Serializable]
    public class BuildingEffectDefinition
    {
        public string type;
        public string resource;
        public string targetResource;
        public string bonusType;
        public string slotType;
        public string buildingId;
        public string branchId;
        public string missionId;
        public string targetFacility;
        public float perLevel;
        public float baseAmount;
        public float amountPerLevel;
        public float baseMultiplier = 1f;
        public float perLevelDelta = 0f;
        public float minMultiplier = 0f;
        public float maxMultiplier = 0f;
        public float baseRate = 0f;
        public float ratePerLevel = 0f;
        public float maxRate = 1f;
        public int baseSeconds = 0;
        public float multiplierPerLevel = 1f;
        public int amount = 0;
        public List<int> levels = new List<int>();
        public int level = 0;
        public int minLevel = 1;
        public int maxLevel = 0;
    }
}
