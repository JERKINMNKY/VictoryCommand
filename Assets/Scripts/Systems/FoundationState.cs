using System;
using System.Collections.Generic;
using UnityEngine;
using IFC.Data;
using IFC.Systems.Profiles;

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
        public List<string> facilityIds = new List<string>();
    }

    [Serializable]
    public class OfficerAssignment
    {
        public OfficerRole role = OfficerRole.Mayor;
        public string officerId = string.Empty;
        public List<string> facilityIds = new List<string>();
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

    public enum BuildingFunctionType
    {
        None,
        PopulationGrowth,
        ResourceProduction,
        TrainingSpeedBuff,
        ResourceExchange,
        CapacityBoost
    }

    [Serializable]
    public class TileCapEntry
    {
        public int townHallLevel = 1;
        public int maxTiles = 16;
    }

    [Serializable]
    public class TileCapTable
    {
        public List<TileCapEntry> entries = new List<TileCapEntry>();

        public int GetCap(int townHallLevel)
        {
            int cap = 0;
            for (int i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                if (townHallLevel >= entry.townHallLevel)
                {
                    cap = Mathf.Max(cap, entry.maxTiles);
                }
            }

            return cap;
        }
    }

    [Serializable]
    public class UnlockRule
    {
        public string tag = string.Empty;
        public int townHallMin = 1;
        public UnlockReward unlock = new UnlockReward();
    }

    [Serializable]
    public class UnlockReward
    {
        public string slotType = string.Empty;
        public int count = 0;
    }

    [Serializable]
    public class UnlockRuleSet
    {
        public List<UnlockRule> rules = new List<UnlockRule>();
    }

    [Serializable]
    public class SeedInventoryItem
    {
        public string itemId = "UpgradeToken";
        public int quantity = 0;
    }

    [Serializable]
    public class SeedInventoryConfig
    {
        public List<SeedInventoryItem> items = new List<SeedInventoryItem>();
    }

    [Serializable]
    public class InventoryItemState
    {
        public string itemId = string.Empty;
        public int quantity = 0;
    }

    [Serializable]
    public class InventoryState
    {
        public List<InventoryItemState> items = new List<InventoryItemState>();

        public int GetQuantity(string itemId)
        {
            if (string.IsNullOrEmpty(itemId))
            {
                return 0;
            }

            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].itemId == itemId)
                {
                    return items[i].quantity;
                }
            }

            return 0;
        }

        public bool Consume(string itemId, int amount)
        {
            if (string.IsNullOrEmpty(itemId) || amount <= 0)
            {
                return false;
            }

            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].itemId == itemId)
                {
                    if (items[i].quantity < amount)
                    {
                        return false;
                    }

                    items[i].quantity -= amount;
                    return true;
                }
            }

            return false;
        }

        public void Add(string itemId, int amount)
        {
            if (string.IsNullOrEmpty(itemId) || amount == 0)
            {
                return;
            }

            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].itemId == itemId)
                {
                    items[i].quantity = Mathf.Max(0, items[i].quantity + amount);
                    return;
                }
            }

            items.Add(new InventoryItemState
            {
                itemId = itemId,
                quantity = Mathf.Max(0, amount)
            });
        }
    }

    [Serializable]
    public class PlayerState
    {
        public string rank = PlayerRankTable.Private;
        public int maxCities = PlayerRankTable.GetCityLimit(PlayerRankTable.Private);
        public InventoryState tokenInventory = new InventoryState();

        public void EnsureConsistency()
        {
            if (string.IsNullOrEmpty(rank))
            {
                rank = PlayerRankTable.Private;
            }

            maxCities = PlayerRankTable.GetCityLimit(rank);

            if (tokenInventory == null)
            {
                tokenInventory = new InventoryState();
            }
        }

        public void SetRank(string newRank)
        {
            rank = string.IsNullOrEmpty(newRank) ? PlayerRankTable.Private : newRank;
            maxCities = PlayerRankTable.GetCityLimit(rank);
        }
    }

    [Serializable]
    public class SeedBuildingFunctionConfig
    {
        public string buildingId = string.Empty;
        public string buildingType = string.Empty;
        public BuildingFunctionType functionType = BuildingFunctionType.None;
        public int level = 1;
        public float amountPerTick = 0f;
        public ResourceType resourceType = ResourceType.Food;
        public ResourceType targetResource = ResourceType.Steel;
        public float exchangeRate = 1f;
        public string facilityId = string.Empty;
        public float trainingMultiplier = 1f;
    }

    [Serializable]
    public class BuildingFunctionState
    {
        public string buildingId = string.Empty;
        public string buildingType = string.Empty;
        public BuildingFunctionType functionType = BuildingFunctionType.None;
        public int level = 1;
        public float amountPerTick = 0f;
        public ResourceType resourceType = ResourceType.Food;
        public ResourceType targetResource = ResourceType.Steel;
        public float exchangeRate = 1f;
        public string facilityId = string.Empty;
        public float trainingMultiplier = 1f;
        public long capacityFlatBonus = 0;
        public float capacityPercentBonus = 0f;
        public bool appliesToAllResources = true;
    }

    [Serializable]
    public class SeedBuildingLevel
    {
        public string buildingType = string.Empty;
        public int level = 0;
    }

    [Serializable]
    public class SeedGameConfig
    {
        public List<SeedCityConfig> cities = new List<SeedCityConfig>();
        public SeedWorldConfig world = new SeedWorldConfig();
        public SeedMissionConfig missions = new SeedMissionConfig();
        public List<SeedBattleRequest> pendingBattles = new List<SeedBattleRequest>();
        public SeedInventoryConfig inventory = new SeedInventoryConfig();
        public TileCapTable tileCaps = new TileCapTable();
        public UnlockRuleSet unlockRules = new UnlockRuleSet();
        public List<string> rookieMissions = new List<string>();
    }

    [Serializable]
    public class SeedCityConfig
    {
        public string id = "city";
        public string mayorId = string.Empty;
        public string displayName = "New City";
        public float morale = 1f;
        public int population = 0;
        public SeedWallConfig wall = new SeedWallConfig();
        public SeedBuildQueueConfig buildQueue = new SeedBuildQueueConfig();
        public List<SeedResourceStockpile> resources = new List<SeedResourceStockpile>();
        public List<SeedProductionField> production = new List<SeedProductionField>();
        public List<SeedTrainingQueueConfig> trainingQueues = new List<SeedTrainingQueueConfig>();
        public List<SeedTransportRouteConfig> transportRoutes = new List<SeedTransportRouteConfig>();
        public List<SeedOfficerAssignment> officers = new List<SeedOfficerAssignment>();
        public List<SeedBuildingFunctionConfig> buildingFunctions = new List<SeedBuildingFunctionConfig>();
        public List<string> tags = new List<string>();
        public List<SeedBuildingLevel> buildings = new List<SeedBuildingLevel>();
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
        public long stored = 0;
        public long capacity = 10000;
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
        public string facilityId = string.Empty;
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
        public PlayerState player = new PlayerState();
        public TileCapTable tileCaps = new TileCapTable();
        public UnlockRuleSet unlockRules = new UnlockRuleSet();
        public List<string> rookieMissionIds = new List<string>();

#pragma warning disable CS0618
        [System.Obsolete("Use player.tokenInventory instead.")]
        public InventoryState inventory
        {
            get
            {
                player ??= new PlayerState();
                player.EnsureConsistency();
                return player.tokenInventory;
            }
            set
            {
                player ??= new PlayerState();
                player.tokenInventory = value ?? new InventoryState();
            }
        }
#pragma warning restore CS0618

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
        public string mayorOfficerId = string.Empty;
        public string displayName = string.Empty;
        public float morale = 1f;
        public int population = 0;
        public WallState wall = new WallState();
        public BuildQueueState buildQueue = new BuildQueueState();
        public List<ResourceStockpile> stockpiles = new List<ResourceStockpile>();
        public List<ProductionFieldState> production = new List<ProductionFieldState>();
        public List<TrainingQueueState> trainingQueues = new List<TrainingQueueState>();
        public List<TransportRouteState> transportRoutes = new List<TransportRouteState>();
        public List<GarrisonUnit> garrison = new List<GarrisonUnit>();
        public List<OfficerAssignment> officers = new List<OfficerAssignment>();
        public OfficerBonusState officerBonuses = new OfficerBonusState();
        public List<BuildingFunctionState> buildingFunctions = new List<BuildingFunctionState>();
        public List<string> tags = new List<string>();
        public List<BuildingLevelState> buildings = new List<BuildingLevelState>();
        public Dictionary<string, int> slotUnlocks = new Dictionary<string, int>();
        public int officerCapacity = 0;
        public int marchSlots = 0;
        public int transportSlots = 0;
    }

    [Serializable]
    public class ResourceStockpile
    {
        public ResourceType resourceType = ResourceType.Food;
        public long amount = 0;
        public long capacity = 10000;
        public long baseCapacity = 10000;
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
        public string facilityId = string.Empty;
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
    public class BuildingLevelState
    {
        public string buildingKey = string.Empty;
        public int level = 0;
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
                    mayorOfficerId = citySeed.mayorId,
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
                cityState.population = Mathf.Max(0, citySeed.population);

                for (int r = 0; r < citySeed.resources.Count; r++)
                {
                    var resourceSeed = citySeed.resources[r];
                    long stored = Math.Max(0, Math.Min(resourceSeed.capacity, resourceSeed.stored));
                    long capacity = Math.Max(resourceSeed.capacity, resourceSeed.stored);
                    cityState.stockpiles.Add(new ResourceStockpile
                    {
                        resourceType = resourceSeed.resourceType,
                        amount = stored,
                        capacity = capacity,
                        baseCapacity = capacity
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
                        upkeepFoodPerTick = Mathf.Max(0, queueSeed.upkeepFoodPerTick),
                        facilityId = queueSeed.facilityId ?? string.Empty
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
                    var officerAssignment = new OfficerAssignment
                    {
                        role = assignmentSeed.role,
                        officerId = assignmentSeed.officerId
                    };

                    if (assignmentSeed.facilityIds != null)
                    {
                        for (int f = 0; f < assignmentSeed.facilityIds.Count; f++)
                        {
                            var facilityId = assignmentSeed.facilityIds[f];
                            if (!string.IsNullOrEmpty(facilityId))
                            {
                                officerAssignment.facilityIds.Add(facilityId);
                            }
                        }
                    }

                    cityState.officers.Add(officerAssignment);
                    ApplyOfficerAssignment(cityState.officerBonuses, assignmentSeed);
                }

                for (int bf = 0; bf < citySeed.buildingFunctions.Count; bf++)
                {
                    var functionSeed = citySeed.buildingFunctions[bf];
                    cityState.buildingFunctions.Add(BuildBuildingFunctionState(functionSeed));
                }

                if (citySeed.tags != null)
                {
                    for (int t = 0; t < citySeed.tags.Count; t++)
                    {
                        var tag = citySeed.tags[t];
                        if (!string.IsNullOrEmpty(tag))
                        {
                            cityState.tags.Add(tag);
                        }
                    }
                }

                for (int b = 0; b < citySeed.buildings.Count; b++)
                {
                    var buildingSeed = citySeed.buildings[b];
                    if (string.IsNullOrEmpty(buildingSeed.buildingType))
                    {
                        continue;
                    }

                    cityState.buildings.Add(new BuildingLevelState
                    {
                        buildingKey = buildingSeed.buildingType,
                        level = Mathf.Max(0, buildingSeed.level)
                    });
                }

                state.cities.Add(cityState);
            }

            state.world = BuildWorldState(seed.world);
            state.missions = BuildMissionTracker(seed.missions);
            state.battleQueue = BuildBattleQueue(seed.pendingBattles);
            state.player.tokenInventory = BuildInventoryState(seed.inventory);
            state.player.EnsureConsistency();
            state.player.maxCities = Math.Max(state.player.maxCities, state.cities.Count);
            state.tileCaps = seed.tileCaps ?? new TileCapTable();
            state.unlockRules = seed.unlockRules ?? new UnlockRuleSet();
            if (seed.rookieMissions != null)
            {
                state.rookieMissionIds.AddRange(seed.rookieMissions);
                for (int i = 0; i < seed.rookieMissions.Count; i++)
                {
                    var def = MissionDefinitionRegistry.Get(seed.rookieMissions[i]);
                    if (def != null)
                    {
                        state.missions.activeMissions.Add(new MissionProgressState
                        {
                            missionId = def.missionId,
                            progress = 0,
                            target = 1,
                            rewardClaimed = false
                        });
                    }
                }
            }

            return state;
        }

        public static GameState FromStartProfile(Profiles.StartProfileDefinition profile)
        {
            if (profile == null)
            {
                return new GameState();
            }

            var seed = new SeedGameConfig();
            var cityConfig = new SeedCityConfig
            {
                id = string.IsNullOrEmpty(profile.startingCity.id) ? "city" : profile.startingCity.id,
                mayorId = profile.startingCity.mayorOfficerId,
                tags = new List<string>(profile.cityTags)
            };

            if (profile.startingCity.tags != null)
            {
                for (int t = 0; t < profile.startingCity.tags.Count; t++)
                {
                    var tag = profile.startingCity.tags[t];
                    if (!string.IsNullOrEmpty(tag) && !cityConfig.tags.Contains(tag))
                    {
                        cityConfig.tags.Add(tag);
                    }
                }
            }

            foreach (var kvp in profile.startingCity.buildings)
            {
                cityConfig.buildings.Add(new SeedBuildingLevel
                {
                    buildingType = kvp.Key,
                    level = Mathf.Max(0, kvp.Value)
                });
            }

            foreach (var kvp in profile.startingCity.stockpile)
            {
                if (System.Enum.TryParse(kvp.Key, out ResourceType resourceType))
                {
                    long stored = Math.Max(0, (long)kvp.Value);
                    long capacity = Math.Max(10000L, stored + 5000L);
                    cityConfig.resources.Add(new SeedResourceStockpile
                    {
                        resourceType = resourceType,
                        stored = stored,
                        capacity = capacity
                    });
                }
                else
                {
                    seed.inventory.items.Add(new SeedInventoryItem
                    {
                        itemId = kvp.Key,
                        quantity = Mathf.Max(0, kvp.Value)
                    });
                }
            }

            seed.cities.Add(cityConfig);
            seed.tileCaps = profile.tileCaps != null
                ? new TileCapTable { entries = new List<TileCapEntry>(profile.tileCaps.entries) }
                : new TileCapTable();
            seed.unlockRules = profile.unlockRules != null
                ? new UnlockRuleSet { rules = new List<UnlockRule>(profile.unlockRules.rules) }
                : new UnlockRuleSet();
            seed.rookieMissions = new List<string>(profile.rookieMissions ?? new List<string>());

            return FromSeed(seed);
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

        private static InventoryState BuildInventoryState(SeedInventoryConfig seedInventory)
        {
            var inventory = new InventoryState();
            if (seedInventory == null)
            {
                return inventory;
            }

            for (int i = 0; i < seedInventory.items.Count; i++)
            {
                var item = seedInventory.items[i];
                if (string.IsNullOrEmpty(item.itemId) || item.quantity <= 0)
                {
                    continue;
                }

                inventory.Add(item.itemId, item.quantity);
            }

            return inventory;
        }

        private static BuildingFunctionState BuildBuildingFunctionState(SeedBuildingFunctionConfig config)
        {
            if (config == null)
            {
                return new BuildingFunctionState();
            }

            return new BuildingFunctionState
            {
                buildingId = config.buildingId,
                buildingType = config.buildingType,
                functionType = config.functionType,
                level = Mathf.Max(1, config.level),
                amountPerTick = config.amountPerTick,
                resourceType = config.resourceType,
                targetResource = config.targetResource,
                exchangeRate = config.exchangeRate,
                facilityId = config.facilityId,
                trainingMultiplier = Mathf.Clamp(config.trainingMultiplier <= 0f ? 1f : config.trainingMultiplier, 0.1f, 5f)
            };
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
