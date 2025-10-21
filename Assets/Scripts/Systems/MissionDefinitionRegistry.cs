using System.Collections.Generic;
using IFC.Data;

namespace IFC.Systems
{
    public enum MissionTrigger
    {
        BuildingConstructed,
        BuildingUpgraded
    }

    public class MissionReward
    {
        public ResourceType? resourceType;
        public string inventoryItemId;
        public int amount;
    }

    public class MissionDefinition
    {
        public string missionId;
        public MissionTrigger trigger;
        public string buildingKey;
        public int targetLevel;
        public MissionReward reward;
        public string description;
    }

    public static class MissionDefinitionRegistry
    {
        private static readonly Dictionary<string, MissionDefinition> Definitions = new Dictionary<string, MissionDefinition>
        {
            {
                "BuildFarmL1",
                new MissionDefinition
                {
                    missionId = "BuildFarmL1",
                    trigger = MissionTrigger.BuildingConstructed,
                    buildingKey = "Farm",
                    targetLevel = 1,
                    reward = new MissionReward { resourceType = ResourceType.Food, amount = 500 },
                    description = "Construct a Farm level 1"
                }
            },
            {
                "UpgradeTownHall2",
                new MissionDefinition
                {
                    missionId = "UpgradeTownHall2",
                    trigger = MissionTrigger.BuildingUpgraded,
                    buildingKey = "TownHall",
                    targetLevel = 2,
                    reward = new MissionReward { inventoryItemId = BuildQueueSystem.UpgradeTokenId, amount = 1 },
                    description = "Upgrade Town Hall to level 2"
                }
            },
            {
                "BuildArmsPlantL1",
                new MissionDefinition
                {
                    missionId = "BuildArmsPlantL1",
                    trigger = MissionTrigger.BuildingConstructed,
                    buildingKey = "ArmsPlant",
                    targetLevel = 1,
                    reward = new MissionReward { resourceType = ResourceType.Steel, amount = 400 },
                    description = "Construct an Arms Plant level 1"
                }
            }
        };

        public static MissionDefinition Get(string missionId)
        {
            if (missionId != null && Definitions.TryGetValue(missionId, out var def))
            {
                return def;
            }

            return null;
        }
    }
}
