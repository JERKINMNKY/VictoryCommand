using UnityEngine;
using IFC.Data;

namespace IFC.CitySystem
{
    public static class CityManager
    {
        public static bool CanPlaceBuilding(CityData city, TileData tile, BuildingData building, out string reason)
        {
            reason = string.Empty;

            if (tile == null)
            {
                reason = "Tile is null.";
                return false;
            }

            if (!tile.isUnlocked)
            {
                reason = $"Tile is locked. Requires City Level {tile.unlockLevel}.";
                return false;
            }

            if (city.cityLevel < building.unlockLevel)
            {
                reason = $"City Level too low to build {building.BuildingName} (Requires Lv {building.unlockLevel}).";
                return false;
            }

            if (building.isUniquePerCity)
            {
                foreach (var t in city.cityTiles)
                {
                    if (t.assignedBuilding != null && t.assignedBuilding.BuildingName == building.BuildingName)
                    {
                        reason = $"{building.BuildingName} is unique and already built.";
                        return false;
                    }
                }
            }

            // Enforce per-building cap if set (0 = unlimited)
            if (building.maxPerCity > 0)
            {
                if (GetBuildingCount(city, building.BuildingName) >= building.maxPerCity)
                {
                    reason = $"You’ve reached the max of {building.maxPerCity} {building.BuildingName}(s).";
                    return false;
                }
            }

            return true;
        }

        public static bool PlaceBuilding(CityData city, TileData tile, BuildingData building)
        {
            if (CanPlaceBuilding(city, tile, building, out string reason))
            {
                tile.assignedBuilding = building;
                return true;
            }

            Debug.LogWarning($"Cannot place building: {reason}");
            return false;
        }

        private static int GetBuildingCount(CityData city, string buildingName)
        {
            int count = 0;
            foreach (var t in city.cityTiles)
            {
                if (t.assignedBuilding != null && t.assignedBuilding.BuildingName == buildingName)
                {
                    count++;
                }
            }
            return count;
        }
    }
}
