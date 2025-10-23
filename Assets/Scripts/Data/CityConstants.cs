using System;

namespace IFC.Data
{
    public static class CityConstants
    {
        public const int MAX_CITY_LEVEL = 20;
        public const int MAX_CITY_TILES = 25;     // Editable in CityData
        public const int MAX_RESOURCE_TILES = 39; // Fixed for production fields
        public const long RESOURCE_HARD_CAP = 4_000_000_000L;
        // Editor defaults (used by CityTileEditor)
        public const int DefaultCityCols = 5;
        public const int DefaultResCols = 6;
    }
}
