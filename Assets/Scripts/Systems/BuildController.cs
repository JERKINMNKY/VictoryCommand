using System;
using IFC.Data;
using UnityEngine;

namespace IFC.Systems
{
    public class BuildController
    {
        private readonly GameState _state;
        private readonly BuildingCatalog _catalog;
        private readonly InventoryState _inventory;
        private readonly TileCapTable _tileCaps;

        public BuildController(GameState state, BuildingCatalog catalog, InventoryState inventory, TileCapTable tileCaps)
        {
            _state = state;
            _catalog = catalog;
            _inventory = inventory ?? new InventoryState();
            _tileCaps = tileCaps;
        }

        public bool TryPlaceBuilding(string cityId, string buildingKey, out BuildFail fail, bool log = true)
        {
            fail = BuildFail.Unknown;
            if (!TryGetCity(cityId, out var city))
            {
                return false;
            }

            if (!BuildRules.TryEvaluatePlacement(city, buildingKey, _catalog, _tileCaps, out fail))
            {
                return false;
            }

            if (!_catalog.TryGet(buildingKey, 1, out var levelData))
            {
                fail = BuildFail.Unknown;
                return false;
            }

            if (!ConsumeResources(city, levelData, out fail))
            {
                return false;
            }

            city.SetBuildingLevel(buildingKey, 1);
            GameEventHub.Publish(new BuildingConstructedEvent
            {
                cityId = city.cityId,
                buildingKey = buildingKey,
                level = 1
            });
            if (log)
            {
                Debug.Log($"[Build] Place {city.displayName}:{buildingKey} L1");
            }

            fail = BuildFail.None;
            return true;
        }

        public bool TryEnqueueUpgrade(string cityId, string buildingKey, int targetLevel, out BuildFail fail, out TimeSpan eta)
        {
            fail = BuildFail.Unknown;
            eta = TimeSpan.Zero;

            if (!TryGetCity(cityId, out var city))
            {
                return false;
            }

            int currentLevel = city.GetBuildingLevel(buildingKey);
            if (targetLevel <= currentLevel)
            {
                fail = BuildFail.Unknown;
                return false;
            }

            if (!BuildRules.TryEvaluateUpgrade(city, buildingKey, targetLevel, _catalog, _tileCaps, _inventory, out fail))
            {
                return false;
            }

            if (!_catalog.TryGet(buildingKey, targetLevel, out var levelData))
            {
                fail = BuildFail.Unknown;
                return false;
            }

            if (!HasQueueSlot(city))
            {
                fail = BuildFail.QueueFull;
                return false;
            }

            if (!ConsumeResources(city, levelData, out fail))
            {
                return false;
            }

            bool consumedToken = false;
            if (targetLevel >= BuildQueueSystem.UpgradeTokenGateLevel)
            {
                _inventory.Consume(BuildQueueSystem.UpgradeTokenId, 1);
                consumedToken = true;
            }

            Debug.Log(consumedToken
                ? $"[Build] Start {city.displayName}:{buildingKey} L{currentLevel}->{targetLevel} ConsumedToken"
                : $"[Build] Start {city.displayName}:{buildingKey} L{currentLevel}->{targetLevel}");

            int seconds = levelData.UpgradeTimeSeconds > 0 ? levelData.UpgradeTimeSeconds : 60;
            var order = new BuildOrderState
            {
                buildingType = buildingKey,
                targetLevel = targetLevel,
                secondsRemaining = seconds,
                blueprintGateSatisfied = true
            };

            city.buildQueue.activeOrders.Add(order);
            eta = TimeSpan.FromSeconds(seconds);
            fail = BuildFail.None;
            return true;
        }

        private bool TryGetCity(string cityId, out CityState city)
        {
            city = _state?.GetCityById(cityId);
            return city != null;
        }

        private bool HasQueueSlot(CityState city)
        {
            return city.buildQueue.activeOrders.Count < city.buildQueue.baseSlots;
        }

        private bool ConsumeResources(CityState city, BuildingData data, out BuildFail fail)
        {
            fail = BuildFail.None;
            if (data == null || data.CostByResource == null)
            {
                return true;
            }

            for (int i = 0; i < data.CostByResource.Count; i++)
            {
                var cost = data.CostByResource[i];
                var stockpile = GameStateBuilder.FindStockpile(city, cost.resourceType);
                if (stockpile == null || stockpile.amount < cost.amount)
                {
                    fail = BuildFail.InsufficientResources;
                    return false;
                }
            }

            for (int i = 0; i < data.CostByResource.Count; i++)
            {
                var cost = data.CostByResource[i];
                var stockpile = GameStateBuilder.FindStockpile(city, cost.resourceType);
                stockpile.amount -= cost.amount;
            }

            return true;
        }
    }
}
