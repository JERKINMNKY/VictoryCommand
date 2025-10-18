using System;
using System.Collections.Generic;
using UnityEngine;
using IFC.Data;

// Layer: Core Simulation
// Life Domain: Autonomy & Logistics
namespace IFC.Systems
{
    [Serializable]
    public class SeedGameConfig
    {
        public List<SeedCityConfig> cities = new List<SeedCityConfig>();
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
    public class TrainingCost
    {
        public ResourceType resourceType = ResourceType.Food;
        public int amount = 0;
    }

    [Serializable]
    public class GameState
    {
        public List<CityState> cities = new List<CityState>();

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

                state.cities.Add(cityState);
            }

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
    }
}
