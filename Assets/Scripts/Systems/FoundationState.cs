using System;
using System.Collections.Generic;
using UnityEngine;
using IFC.Data;

// Layer: Core Simulation
// Life Domain: Autonomy & Logistics
namespace IFC.Systems
{
    public enum OfficerRole
    {
        Mayor,
        Defense,
        Logistics
    }

    [Serializable]
    public class SeedOfficerAssignment
    {
        public OfficerRole role = OfficerRole.Mayor;
        public string officerId = string.Empty;
        public float moraleBonus = 0f;
        public float productionBonus = 0f;
        public float constructionSpeedBonus = 0f;
        public float defenseRepairBonus = 0f;
        public float moraleDamageReduction = 0f;
    }

    [Serializable]
    public class OfficerAssignment
    {
        public OfficerRole role = OfficerRole.Mayor;
        public string officerId = string.Empty;
    }

    [Serializable]
    public class OfficerBonusState
    {
        public float moraleBonus;
        public float productionMultiplier;
        public float constructionSpeedMultiplier;
        public float defenseRepairMultiplier;
        public float moraleDamageMultiplier;

        public OfficerBonusState()
        {
            moraleBonus = 0f;
            productionMultiplier = 1f;
            constructionSpeedMultiplier = 1f;
            defenseRepairMultiplier = 1f;
            moraleDamageMultiplier = 1f;
        }

        public float GetProductionMultiplier()
        {
            return Mathf.Max(0f, productionMultiplier);
        }

        public float GetConstructionSpeedMultiplier()
        {
            return Mathf.Max(0.1f, constructionSpeedMultiplier);
        }

        public float GetDefenseRepairMultiplier()
        {
            return Mathf.Max(0f, defenseRepairMultiplier);
        }

        public float GetMoraleDamageMultiplier()
        {
            return Mathf.Clamp(moraleDamageMultiplier, 0.1f, 2f);
        }
    }

    [Serializable]
    public class SeedGameConfig
    {
        public List<SeedCityConfig> cities = new List<SeedCityConfig>();
        public SeedWorldConfig world = new SeedWorldConfig();
        public SeedMissionConfig missions = new SeedMissionConfig();
        public List<SeedBattleRequest> pendingBattles = new List<SeedBattleRequest>();
    }

    [Serializable]
    public class SeedCityConfig
    {
        public string id = "city";
        public string displayName = "New City";
        public float morale = 1f;
        public SeedWallConfig wall = new SeedWallConfig();
        public SeedBuildQueueConfig buildQueue = new SeedBuildQueueConfig();
        public List<SeedResourceStockpile> resources = new List<SeedResourceStockpile>();
        public List<SeedProductionField> production = new List<SeedProductionField>();
        public List<SeedTrainingQueueConfig> trainingQueues = new List<SeedTrainingQueueConfig>();
        public List<SeedTransportRouteConfig> transportRoutes = new List<SeedTransportRouteConfig>();
        public List<SeedOfficerAssignment> officers = new List<SeedOfficerAssignment>();
    }

    [Serializable]
    public class SeedWallConfig
    {
        public int maxHp = 1000;
        public int currentHp = 1000;
        public int fortificationPoints = 0;
    }

    [Serializable]
    public class SeedResourceStockpile
    {
        public ResourceType resourceType = ResourceType.Food;
        public int stored = 0;
        public int capacity = 10000;
    }

    [Serializable]
    public class SeedProductionField
    {
        public ResourceType resourceType = ResourceType.Food;
        public int fields = 0;
        public int outputPerField = 1;
    }

    [Serializable]
    public class SeedBuildQueueConfig
    {
        public int baseSlots = 2;
        public int blueprintTokens = 0;
        public List<SeedBuildOrder> orders = new List<SeedBuildOrder>();
    }

    [Serializable]
    public class SeedBuildOrder
    {
        public string buildingType = "Farm";
        public int targetLevel = 2;
        public int secondsRemaining = 0;
    }

    [Serializable]
    public class SeedTrainingQueueConfig
    {
        public string unitType = "Infantry";
        public int quantity = 0;
        public int durationSeconds = 0;
        public List<TrainingCost> costs = new List<TrainingCost>();
        public int upkeepFoodPerTick = 0;
    }

    [Serializable]
    public class SeedTransportRouteConfig
    {
        public string targetCityId = string.Empty;
        public ResourceType resourceType = ResourceType.Food;
        public int amountPerShipment = 0;
        public int intervalSeconds = 60;
        public int secondsUntilNext = 60;
    }

    [Serializable]
    public class SeedWorldConfig
    {
        public List<SeedTerritoryConfig> territories = new List<SeedTerritoryConfig>();
        public List<SeedFrontConfig> fronts = new List<SeedFrontConfig>();
    }

    [Serializable]
    public class SeedTerritoryConfig
    {
        public string territoryId = string.Empty;
        public string ownerCityId = string.Empty;
        public float controlProgress = 0f;
    }

    [Serializable]
    public class SeedFrontConfig
    {
        public string frontId = string.Empty;
        public List<string> territoryIds = new List<string>();
        public bool unlocked = false;
    }

    [Serializable]
    public class SeedMissionConfig
    {
        public List<SeedMissionProgress> active = new List<SeedMissionProgress>();
        public List<string> completed = new List<string>();
    }

    [Serializable]
    public class SeedMissionProgress
    {
        public string missionId = string.Empty;
        public int progress = 0;
        public int target = 0;
        public bool rewardClaimed = false;
    }

    [Serializable]
    public class SeedBattleRequest
    {
        public string battleId = string.Empty;
        public string attackerCityId = string.Empty;
        public string defenderCityId = string.Empty;
        public int secondsUntilResolution = 0;
    }

    [Serializable]
    public class TrainingCost
    {
        public ResourceType resourceType = ResourceType.Food;
        public int amount = 0;
    }

    [Serializable]
    public class GameState
    {
        public List<CityState> cities = new List<CityState>();
        public WorldState world = new WorldState();
        public MissionTrackerState missions = new MissionTrackerState();
        public BattleQueueState battleQueue = new BattleQueueState();

        public CityState GetCityById(string cityId)
        {
            for (int i = 0; i < cities.Count; i++)
            {
                var city = cities[i];
                if (city.cityId == cityId)
                {
                    return city;
                }
            }

            return null;
        }
    }

    [Serializable]
    public class CityState
    {
        public string cityId = string.Empty;
        public string displayName = string.Empty;
        public float morale = 1f;
        public WallState wall = new WallState();
        public BuildQueueState buildQueue = new BuildQueueState();
        public List<ResourceStockpile> stockpiles = new List<ResourceStockpile>();
        public List<ProductionFieldState> production = new List<ProductionFieldState>();
        public List<TrainingQueueState> trainingQueues = new List<TrainingQueueState>();
        public List<TransportRouteState> transportRoutes = new List<TransportRouteState>();
        public List<GarrisonUnit> garrison = new List<GarrisonUnit>();
        public List<OfficerAssignment> officers = new List<OfficerAssignment>();
        public OfficerBonusState officerBonuses = new OfficerBonusState();
    }

    [Serializable]
    public class ResourceStockpile
    {
        public ResourceType resourceType = ResourceType.Food;
        public int amount = 0;
        public int capacity = 10000;
    }

    [Serializable]
    public class ProductionFieldState
    {
        public ResourceType resourceType = ResourceType.Food;
        public int fields = 0;
        public int outputPerField = 0;
    }

    [Serializable]
    public class BuildQueueState
    {
        public int baseSlots = 2;
        public int blueprintTokens = 0;
        public List<BuildOrderState> activeOrders = new List<BuildOrderState>();
    }

    [Serializable]
    public class BuildOrderState
    {
        public string buildingType = "";
        public int targetLevel = 1;
        public int secondsRemaining = 0;
        public bool blueprintGateSatisfied = false;
    }

    [Serializable]
    public class TrainingQueueState
    {
        public string unitType = string.Empty;
        public int quantity = 0;
        public int durationSeconds = 0;
        public int secondsRemaining = 0;
        public int upkeepFoodPerTick = 0;
        public List<TrainingCost> costs = new List<TrainingCost>();
        public bool resourcesCommitted = false;
    }

    [Serializable]
    public class TransportRouteState
    {
        public string targetCityId = string.Empty;
        public ResourceType resourceType = ResourceType.Food;
        public int amountPerShipment = 0;
        public int intervalSeconds = 60;
        public int secondsUntilNext = 60;
    }

    [Serializable]
    public class WallState
    {
        public int maxHp = 1000;
        public int currentHp = 1000;
        public int fortificationPoints = 0;
    }

    [Serializable]
    public class GarrisonUnit
    {
        public string unitType = string.Empty;
        public int quantity = 0;
    }

    [Serializable]
    public class WorldState
    {
        public List<TerritoryState> territories = new List<TerritoryState>();
        public List<FrontState> fronts = new List<FrontState>();
    }

    [Serializable]
    public class TerritoryState
    {
        public string territoryId = string.Empty;
        public string ownerCityId = string.Empty;
        public float controlProgress = 0f;
    }

    [Serializable]
    public class FrontState
    {
        public string frontId = string.Empty;
        public List<string> territoryIds = new List<string>();
        public bool unlocked = false;
    }

    [Serializable]
    public class MissionTrackerState
    {
        public List<MissionProgressState> activeMissions = new List<MissionProgressState>();
        public List<string> completedMissionIds = new List<string>();
    }

    [Serializable]
    public class MissionProgressState
    {
        public string missionId = string.Empty;
        public int progress = 0;
        public int target = 0;
        public bool rewardClaimed = false;
    }

    [Serializable]
    public class BattleQueueState
    {
        public List<BattleRequest> pendingBattles = new List<BattleRequest>();
        public List<BattleReport> battleHistory = new List<BattleReport>();
    }

    [Serializable]
    public class BattleRequest
    {
        public string battleId = string.Empty;
        public string attackerCityId = string.Empty;
        public string defenderCityId = string.Empty;
        public int secondsUntilResolution = 0;
    }

    [Serializable]
    public class BattleReport
    {
        public string battleId = string.Empty;
        public string attackerCityId = string.Empty;
        public string defenderCityId = string.Empty;
        public string outcome = "Pending";
    }

    public static class GameStateBuilder
    {
        public static GameState FromSeed(SeedGameConfig seed)
        {
            var state = new GameState();
            for (int i = 0; i < seed.cities.Count; i++)
            {
                var citySeed = seed.cities[i];
                var cityState = new CityState
                {
                    cityId = citySeed.id,
                    displayName = citySeed.displayName,
                    morale = Mathf.Clamp01(citySeed.morale),
                    wall = new WallState
                    {
                        currentHp = Mathf.Clamp(citySeed.wall.currentHp, 0, citySeed.wall.maxHp),
                        maxHp = Mathf.Max(1, citySeed.wall.maxHp),
                        fortificationPoints = Mathf.Max(0, citySeed.wall.fortificationPoints)
                    },
                    buildQueue = new BuildQueueState
                    {
                        baseSlots = Mathf.Max(1, citySeed.buildQueue.baseSlots),
                        blueprintTokens = Mathf.Max(0, citySeed.buildQueue.blueprintTokens)
                    }
                };

                for (int r = 0; r < citySeed.resources.Count; r++)
                {
                    var resourceSeed = citySeed.resources[r];
                    cityState.stockpiles.Add(new ResourceStockpile
                    {
                        resourceType = resourceSeed.resourceType,
                        amount = Mathf.Clamp(resourceSeed.stored, 0, resourceSeed.capacity),
                        capacity = Mathf.Max(resourceSeed.capacity, resourceSeed.stored)
                    });
                }

                for (int p = 0; p < citySeed.production.Count; p++)
                {
                    var prodSeed = citySeed.production[p];
                    cityState.production.Add(new ProductionFieldState
                    {
                        resourceType = prodSeed.resourceType,
                        fields = Mathf.Max(0, prodSeed.fields),
                        outputPerField = Mathf.Max(0, prodSeed.outputPerField)
                    });
                }

                for (int b = 0; b < citySeed.buildQueue.orders.Count; b++)
                {
                    var orderSeed = citySeed.buildQueue.orders[b];
                    cityState.buildQueue.activeOrders.Add(new BuildOrderState
                    {
                        buildingType = orderSeed.buildingType,
                        targetLevel = Mathf.Max(1, orderSeed.targetLevel),
                        secondsRemaining = Mathf.Max(0, orderSeed.secondsRemaining),
                        blueprintGateSatisfied = false
                    });
                }

                for (int t = 0; t < citySeed.trainingQueues.Count; t++)
                {
                    var queueSeed = citySeed.trainingQueues[t];
                    var queueState = new TrainingQueueState
                    {
                        unitType = queueSeed.unitType,
                        quantity = Mathf.Max(0, queueSeed.quantity),
                        durationSeconds = Mathf.Max(1, queueSeed.durationSeconds),
                        secondsRemaining = Mathf.Max(1, queueSeed.durationSeconds),
                        upkeepFoodPerTick = Mathf.Max(0, queueSeed.upkeepFoodPerTick)
                    };
                    for (int c = 0; c < queueSeed.costs.Count; c++)
                    {
                        var cost = queueSeed.costs[c];
                        queueState.costs.Add(new TrainingCost
                        {
                            resourceType = cost.resourceType,
                            amount = Mathf.Max(0, cost.amount)
                        });
                    }

                    cityState.trainingQueues.Add(queueState);
                }

                for (int tr = 0; tr < citySeed.transportRoutes.Count; tr++)
                {
                    var routeSeed = citySeed.transportRoutes[tr];
                    cityState.transportRoutes.Add(new TransportRouteState
                    {
                        targetCityId = routeSeed.targetCityId,
                        resourceType = routeSeed.resourceType,
                        amountPerShipment = Mathf.Max(0, routeSeed.amountPerShipment),
                        intervalSeconds = Mathf.Max(1, routeSeed.intervalSeconds),
                        secondsUntilNext = Mathf.Clamp(routeSeed.secondsUntilNext, 0, routeSeed.intervalSeconds)
                    });
                }

                for (int o = 0; o < citySeed.officers.Count; o++)
                {
                    var assignmentSeed = citySeed.officers[o];
                    cityState.officers.Add(new OfficerAssignment
                    {
                        role = assignmentSeed.role,
                        officerId = assignmentSeed.officerId
                    });
                    ApplyOfficerAssignment(cityState.officerBonuses, assignmentSeed);
                }

                state.cities.Add(cityState);
            }

            state.world = BuildWorldState(seed.world);
            state.missions = BuildMissionTracker(seed.missions);
            state.battleQueue = BuildBattleQueue(seed.pendingBattles);

            return state;
        }

        public static ResourceStockpile FindStockpile(CityState city, ResourceType resourceType)
        {
            for (int i = 0; i < city.stockpiles.Count; i++)
            {
                if (city.stockpiles[i].resourceType == resourceType)
                {
                    return city.stockpiles[i];
                }
            }

            return null;
        }

        public static GarrisonUnit GetOrCreateGarrison(CityState city, string unitType)
        {
            for (int i = 0; i < city.garrison.Count; i++)
            {
                if (city.garrison[i].unitType == unitType)
                {
                    return city.garrison[i];
                }
            }

            var newUnit = new GarrisonUnit { unitType = unitType, quantity = 0 };
            city.garrison.Add(newUnit);
            return newUnit;
        }

        private static WorldState BuildWorldState(SeedWorldConfig seedWorld)
        {
            var world = new WorldState();
            if (seedWorld == null)
            {
                return world;
            }

            for (int i = 0; i < seedWorld.territories.Count; i++)
            {
                var territorySeed = seedWorld.territories[i];
                world.territories.Add(new TerritoryState
                {
                    territoryId = territorySeed.territoryId,
                    ownerCityId = territorySeed.ownerCityId,
                    controlProgress = Mathf.Clamp01(territorySeed.controlProgress)
                });
            }

            for (int i = 0; i < seedWorld.fronts.Count; i++)
            {
                var frontSeed = seedWorld.fronts[i];
                var frontState = new FrontState
                {
                    frontId = frontSeed.frontId,
                    unlocked = frontSeed.unlocked
                };

                for (int t = 0; t < frontSeed.territoryIds.Count; t++)
                {
                    var territoryId = frontSeed.territoryIds[t];
                    if (!string.IsNullOrEmpty(territoryId))
                    {
                        frontState.territoryIds.Add(territoryId);
                    }
                }

                world.fronts.Add(frontState);
            }

            return world;
        }

        private static MissionTrackerState BuildMissionTracker(SeedMissionConfig seedMissions)
        {
            var tracker = new MissionTrackerState();
            if (seedMissions == null)
            {
                return tracker;
            }

            for (int i = 0; i < seedMissions.active.Count; i++)
            {
                var missionSeed = seedMissions.active[i];
                tracker.activeMissions.Add(new MissionProgressState
                {
                    missionId = missionSeed.missionId,
                    progress = Mathf.Max(0, missionSeed.progress),
                    target = Mathf.Max(0, missionSeed.target),
                    rewardClaimed = missionSeed.rewardClaimed
                });
            }

            for (int i = 0; i < seedMissions.completed.Count; i++)
            {
                var completedId = seedMissions.completed[i];
                if (!string.IsNullOrEmpty(completedId))
                {
                    tracker.completedMissionIds.Add(completedId);
                }
            }

            return tracker;
        }

        private static BattleQueueState BuildBattleQueue(List<SeedBattleRequest> seedBattles)
        {
            var queue = new BattleQueueState();
            if (seedBattles == null)
            {
                return queue;
            }

            for (int i = 0; i < seedBattles.Count; i++)
            {
                var battleSeed = seedBattles[i];
                queue.pendingBattles.Add(new BattleRequest
                {
                    battleId = battleSeed.battleId,
                    attackerCityId = battleSeed.attackerCityId,
                    defenderCityId = battleSeed.defenderCityId,
                    secondsUntilResolution = Mathf.Max(0, battleSeed.secondsUntilResolution)
                });
            }

            return queue;
        }

        private static void ApplyOfficerAssignment(OfficerBonusState target, SeedOfficerAssignment assignment)
        {
            if (target == null || assignment == null)
            {
                return;
            }

            target.moraleBonus += assignment.moraleBonus;
            target.productionMultiplier += assignment.productionBonus;
            target.constructionSpeedMultiplier += assignment.constructionSpeedBonus;
            target.defenseRepairMultiplier += assignment.defenseRepairBonus;

            float reduction = Mathf.Clamp01(assignment.moraleDamageReduction);
            float currentMultiplier = target.moraleDamageMultiplier;
            float newMultiplier = Mathf.Clamp(currentMultiplier * (1f - reduction), 0.1f, 2f);
            target.moraleDamageMultiplier = newMultiplier;
        }
    }
}
