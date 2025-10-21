using System;
using System.Collections.Generic;
using IFC.Data;

namespace IFC.Systems.UI
{
    public class CityViewModel
    {
        private readonly GameLoop _loop;
        private readonly string _cityId;
        private readonly CityState _city;
        private readonly GameState _state;
        private readonly BuildingCatalog _catalog;
        private readonly TileCapTable _tileCaps;
        private readonly InventoryState _inventory;

        public CityViewModel(GameLoop loop, string cityId)
        {
            _loop = loop;
            _cityId = cityId;
            _state = loop?.CurrentState;
            _city = _state?.GetCityById(cityId);
            _catalog = loop?.BuildingCatalog;
            _tileCaps = _state?.tileCaps;
            _inventory = _state?.inventory;
        }

        public bool IsValid => _city != null;

        public IReadOnlyList<ResourceStockpile> Stockpiles => _city?.stockpiles;

        public int GetInventoryQuantity(string itemId)
        {
            return _inventory?.GetQuantity(itemId) ?? 0;
        }

        public int GetBuildingLevel(string buildingKey)
        {
            return _city?.GetBuildingLevel(buildingKey) ?? 0;
        }

        public bool TryGetNextLevelData(string buildingKey, out BuildingData data)
        {
            data = null;
            if (_catalog == null || _city == null)
            {
                return false;
            }

            int targetLevel = GetBuildingLevel(buildingKey) + 1;
            if (targetLevel <= 0)
            {
                targetLevel = 1;
            }

            return _catalog.TryGet(buildingKey, targetLevel, out data);
        }

        public BuildFail EvaluateUpgrade(string buildingKey)
        {
            if (_city == null || _catalog == null)
            {
                return BuildFail.Unknown;
            }

            int targetLevel = GetBuildingLevel(buildingKey) + 1;
            if (!_catalog.TryGet(buildingKey, targetLevel, out var data))
            {
                return BuildFail.Unknown;
            }

            if (!BuildRules.TryEvaluateUpgrade(_city, buildingKey, targetLevel, _catalog, _tileCaps, _inventory, out var fail, log: false))
            {
                return fail;
            }

            if (!HasResources(data))
            {
                return BuildFail.InsufficientResources;
            }

            if (targetLevel >= BuildQueueSystem.UpgradeTokenGateLevel && GetInventoryQuantity(BuildQueueSystem.UpgradeTokenId) < 1)
            {
                return BuildFail.RequiresToken;
            }

            if (!HasQueueSlot())
            {
                return BuildFail.QueueFull;
            }

            return BuildFail.None;
        }

        public bool HasResources(BuildingData data)
        {
            if (data == null || data.CostByResource == null)
            {
                return true;
            }

            for (int i = 0; i < data.CostByResource.Count; i++)
            {
                var cost = data.CostByResource[i];
                var stockpile = GameStateBuilder.FindStockpile(_city, cost.resourceType);
                if (stockpile == null || stockpile.amount < cost.amount)
                {
                    return false;
                }
            }

            return true;
        }

        public int GetNextUpgradeDuration(string buildingKey)
        {
            if (TryGetNextLevelData(buildingKey, out var data))
            {
                return data.UpgradeTimeSeconds > 0 ? data.UpgradeTimeSeconds : 60;
            }

            return 60;
        }

        public int GetTileUsage()
        {
            return _city?.CountOccupiedTiles() ?? 0;
        }

        public int GetTileCap()
        {
            int thLevel = GetTownHallLevel();
            return _tileCaps != null ? _tileCaps.GetCap(thLevel) : 0;
        }

        public int GetTownHallLevel()
        {
            return _city?.GetTownHallLevel() ?? 0;
        }

        public CityState City => _city;

        public GameLoop Loop => _loop;

        private bool HasQueueSlot()
        {
            if (_city == null)
            {
                return false;
            }

            return _city.buildQueue.activeOrders.Count < _city.buildQueue.baseSlots;
        }
    }
}
