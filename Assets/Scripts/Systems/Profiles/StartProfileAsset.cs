using System.Collections.Generic;
using UnityEngine;

namespace IFC.Systems.Profiles
{
    [CreateAssetMenu(fileName = "StartProfile_NewPlayer", menuName = "IFC/Profiles/Start Profile")]
    public class StartProfileAsset : ScriptableObject
    {
        public string profileId = "NewPlayer";
        public StartProfileCityAsset startingCity = new StartProfileCityAsset();
        public List<TileCapEntry> tileCaps = new List<TileCapEntry>();
        public List<string> rookieMissions = new List<string>();
        public List<string> cityTags = new List<string>();
        public List<UnlockRule> unlockRules = new List<UnlockRule>();
    }

    [System.Serializable]
    public class StartProfileCityAsset
    {
        public string id = "Capital";
        public string mayorOfficerId = string.Empty;
        public List<StartProfileBuildingLevel> buildings = new List<StartProfileBuildingLevel>();
        public List<StartProfileStockItem> stockpile = new List<StartProfileStockItem>();
        public List<string> routes = new List<string>();
        public List<string> garrison = new List<string>();
        public List<string> tags = new List<string>();
    }

    [System.Serializable]
    public class StartProfileBuildingLevel
    {
        public string buildingType = string.Empty;
        public int level = 0;
    }

    [System.Serializable]
    public class StartProfileStockItem
    {
        public string itemId = string.Empty;
        public int amount = 0;
    }
}
