namespace IFC.Systems
{
    public static class CityStateExtensions
    {
        public static BuildingLevelState GetBuilding(this CityState city, string buildingKey)
        {
            if (city == null || string.IsNullOrEmpty(buildingKey))
            {
                return null;
            }

            for (int i = 0; i < city.buildings.Count; i++)
            {
                if (city.buildings[i].buildingKey == buildingKey)
                {
                    return city.buildings[i];
                }
            }

            return null;
        }

        public static int GetBuildingLevel(this CityState city, string buildingKey)
        {
            var state = GetBuilding(city, buildingKey);
            return state != null ? state.level : 0;
        }

        public static void SetBuildingLevel(this CityState city, string buildingKey, int level)
        {
            if (city == null || string.IsNullOrEmpty(buildingKey))
            {
                return;
            }

            for (int i = 0; i < city.buildings.Count; i++)
            {
                if (city.buildings[i].buildingKey == buildingKey)
                {
                    city.buildings[i].level = level;
                    return;
                }
            }

            city.buildings.Add(new BuildingLevelState
            {
                buildingKey = buildingKey,
                level = level
            });
        }

        public static int CountOccupiedTiles(this CityState city)
        {
            if (city?.buildings == null)
            {
                return 0;
            }

            int count = 0;
            for (int i = 0; i < city.buildings.Count; i++)
            {
                if (city.buildings[i].level > 0)
                {
                    count++;
                }
            }

            return count;
        }

        public static int GetTownHallLevel(this CityState city)
        {
            return city.GetBuildingLevel("TownHall");
        }
    }
}
