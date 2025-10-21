using System;
using IFC.Data;
using UnityEngine;

namespace IFC.Systems
{
    public static class BuildRules
    {
        public static bool TryEvaluatePlacement(CityState city, string buildingKey, BuildingCatalog catalog, TileCapTable tileCaps, out BuildFail fail, bool log = true)
        {
            fail = BuildFail.Unknown;
            if (city == null || catalog == null)
            {
                return false;
            }

            if (!catalog.TryGet(buildingKey, 1, out var data))
            {
                fail = BuildFail.Unknown;
                return false;
            }

            if (!CheckTileCap(city, buildingKey, tileCaps, out fail, log))
            {
                return false;
            }

            if (!CheckPrerequisites(city, data, buildingKey, out fail, log))
            {
                return false;
            }

            fail = BuildFail.None;
            return true;
        }

        public static bool TryEvaluateUpgrade(CityState city, string buildingKey, int targetLevel, BuildingCatalog catalog, TileCapTable tileCaps, InventoryState inventory, out BuildFail fail, bool log = true)
        {
            fail = BuildFail.Unknown;
            if (city == null || catalog == null)
            {
                return false;
            }

            if (!catalog.TryGet(buildingKey, targetLevel, out var data))
            {
                fail = BuildFail.Unknown;
                return false;
            }

            if (!CheckPrerequisites(city, data, buildingKey, out fail, log))
            {
                return false;
            }

            if (targetLevel >= BuildQueueSystem.UpgradeTokenGateLevel)
            {
                if (inventory == null || inventory.GetQuantity(BuildQueueSystem.UpgradeTokenId) < 1)
                {
                    if (log)
                    {
                        Debug.Log($"[Build] Gate {city.displayName}:{buildingKey} L{targetLevel} RequiresToken");
                    }
                    fail = BuildFail.RequiresToken;
                    return false;
                }
            }

            fail = BuildFail.None;
            return true;
        }

        private static bool CheckTileCap(CityState city, string buildingKey, TileCapTable tileCaps, out BuildFail fail, bool log)
        {
            fail = BuildFail.None;
            if (tileCaps == null)
            {
                return true;
            }

            int currentLevel = city.GetBuildingLevel(buildingKey);
            if (currentLevel > 0)
            {
                return true;
            }

            int cap = tileCaps.GetCap(city.GetTownHallLevel());
            if (cap <= 0)
            {
                return true;
            }

            int used = city.CountOccupiedTiles();
            if (used >= cap)
            {
                if (log)
                {
                    Debug.Log($"[Build] Locked TileCap TH={city.GetTownHallLevel()} cap={cap} used={used}");
                }
                fail = BuildFail.TileCapReached;
                return false;
            }

            return true;
        }

        private static bool CheckPrerequisites(CityState city, BuildingData data, string buildingKey, out BuildFail fail, bool log)
        {
            fail = BuildFail.None;
            if (data == null || data.requires == null)
            {
                return true;
            }

            for (int i = 0; i < data.requires.Count; i++)
            {
                var req = data.requires[i];
                if (string.IsNullOrEmpty(req.buildingType))
                {
                    continue;
                }

                int level = req.buildingType.Equals("TownHall", StringComparison.OrdinalIgnoreCase)
                    ? city.GetTownHallLevel()
                    : city.GetBuildingLevel(req.buildingType);

                if (level < req.minLevel)
                {
                    if (log)
                    {
                        Debug.Log($"[Build] Locked {buildingKey} Needs {req.buildingType}≥{req.minLevel}");
                    }
                    fail = BuildFail.PrereqNotMet;
                    return false;
                }
            }

            return true;
        }
    }
}
