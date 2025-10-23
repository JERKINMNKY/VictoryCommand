using System;
using System.Collections.Generic;

namespace IFC.Data
{
    public static class PlayerRankTable
    {
        public const string Private = "Private";
        public const string Corporal = "Corporal";
        public const string Sergeant = "Sergeant";
        public const string Lieutenant = "Lieutenant";
        public const string Captain = "Captain";
        public const string Major = "Major";
        public const string Colonel = "Colonel";
        public const string Brigadier = "Brigadier";
        public const string MajorGeneral = "MajorGeneral";
        public const string LieutenantGeneral = "LTG";

        private static readonly Dictionary<string, int> CityLimits = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            { Private, 1 },
            { Corporal, 2 },
            { Sergeant, 3 },
            { Lieutenant, 5 },
            { Captain, 7 },
            { Major, 9 },
            { Colonel, 12 },
            { Brigadier, 14 },
            { MajorGeneral, 15 },
            { LieutenantGeneral, 17 }
        };

        public static int GetCityLimit(string rank)
        {
            if (string.IsNullOrEmpty(rank))
            {
                return CityLimits[Private];
            }

            return CityLimits.TryGetValue(rank, out var limit) ? limit : CityLimits[Private];
        }

        public static bool TryGetCityLimit(string rank, out int limit)
        {
            if (string.IsNullOrEmpty(rank))
            {
                limit = CityLimits[Private];
                return true;
            }

            return CityLimits.TryGetValue(rank, out limit);
        }
    }
}
