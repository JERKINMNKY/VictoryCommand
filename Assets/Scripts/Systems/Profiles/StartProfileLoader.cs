using System;
using System.Collections.Generic;
using System.IO;
using IFC.Systems.Util;
using IFC.Systems;
using UnityEngine;

namespace IFC.Systems.Profiles
{
    public class StartProfileDefinition
    {
        public string profileId = "NewPlayer";
        public StartProfileCityDefinition startingCity = new StartProfileCityDefinition();
        public TileCapTable tileCaps = new TileCapTable();
        public List<string> rookieMissions = new List<string>();
        public List<string> cityTags = new List<string>();
        public UnlockRuleSet unlockRules = new UnlockRuleSet();
    }

    public class StartProfileCityDefinition
    {
        public string id = "Capital";
        public string mayorOfficerId = string.Empty;
        public Dictionary<string, int> buildings = new Dictionary<string, int>();
        public Dictionary<string, int> stockpile = new Dictionary<string, int>();
        public List<string> routes = new List<string>();
        public List<string> garrison = new List<string>();
        public List<string> tags = new List<string>();
    }

    public static class StartProfileLoader
    {
        public static StartProfileDefinition Load(StartProfileAsset asset, string jsonPath)
        {
            if (asset != null)
            {
                return FromAsset(asset);
            }

            if (!string.IsNullOrEmpty(jsonPath))
            {
                string absolutePath = jsonPath;
                if (!Path.IsPathRooted(jsonPath))
                {
                    absolutePath = Path.Combine(Application.dataPath, "..", jsonPath);
                }

                if (File.Exists(absolutePath))
                {
                    string json = File.ReadAllText(absolutePath);
                    return FromJson(json);
                }

                Debug.LogWarning($"[StartProfileLoader] JSON profile not found at {absolutePath}");
            }

            Debug.LogWarning("[StartProfileLoader] No profile asset or JSON provided. Using default.");
            return new StartProfileDefinition();
        }

        public static StartProfileDefinition FromAsset(StartProfileAsset asset)
        {
            var definition = new StartProfileDefinition
            {
                profileId = asset.profileId,
                tileCaps = new TileCapTable { entries = new List<TileCapEntry>(asset.tileCaps) },
                rookieMissions = new List<string>(asset.rookieMissions),
                cityTags = new List<string>(asset.cityTags),
                unlockRules = new UnlockRuleSet { rules = new List<UnlockRule>(asset.unlockRules) }
            };

            var city = definition.startingCity;
            city.id = asset.startingCity.id;
            city.mayorOfficerId = asset.startingCity.mayorOfficerId;
            city.routes.AddRange(asset.startingCity.routes);
            city.garrison.AddRange(asset.startingCity.garrison);
            city.tags.AddRange(asset.startingCity.tags);

            foreach (var building in asset.startingCity.buildings)
            {
                if (string.IsNullOrEmpty(building.buildingType))
                {
                    continue;
                }
                city.buildings[building.buildingType] = Mathf.Max(0, building.level);
            }

            foreach (var stock in asset.startingCity.stockpile)
            {
                if (string.IsNullOrEmpty(stock.itemId))
                {
                    continue;
                }
                city.stockpile[stock.itemId] = stock.amount;
            }

            return definition;
        }

        public static StartProfileDefinition FromJson(string json)
        {
            var definition = new StartProfileDefinition();
            var root = MiniJSON.Deserialize(json) as Dictionary<string, object>;
            if (root == null)
            {
                Debug.LogError("[StartProfileLoader] Failed to parse profile JSON.");
                return definition;
            }

            definition.profileId = GetString(root, "profileId", "NewPlayer");
            ParseTileCaps(root, definition.tileCaps);
            definition.rookieMissions = ParseStringList(root, "rookieMissions");
            definition.cityTags = ParseStringList(root, "cityTags");
            ParseUnlockRules(root, definition.unlockRules);

            if (root.TryGetValue("startingCity", out var cityObj) && cityObj is Dictionary<string, object> cityDict)
            {
                ParseCity(cityDict, definition.startingCity);
            }

            return definition;
        }

        private static void ParseCity(Dictionary<string, object> cityDict, StartProfileCityDefinition city)
        {
            city.id = GetString(cityDict, "id", city.id);
            city.mayorOfficerId = GetString(cityDict, "mayorOfficerId", string.Empty);
            city.routes = ParseStringList(cityDict, "routes");
            city.garrison = ParseStringList(cityDict, "garrison");
            city.tags = ParseStringList(cityDict, "tags");

            if (cityDict.TryGetValue("buildings", out var buildingsObj) && buildingsObj is Dictionary<string, object> buildingDict)
            {
                foreach (var kvp in buildingDict)
                {
                    city.buildings[kvp.Key] = ToInt(kvp.Value);
                }
            }

            if (cityDict.TryGetValue("stockpile", out var stockObj) && stockObj is Dictionary<string, object> stockDict)
            {
                foreach (var kvp in stockDict)
                {
                    city.stockpile[kvp.Key] = ToInt(kvp.Value);
                }
            }
        }

        private static void ParseTileCaps(Dictionary<string, object> root, TileCapTable table)
        {
            if (!root.TryGetValue("tileCapByTH", out var capObj))
            {
                return;
            }

            if (capObj is Dictionary<string, object> capDict)
            {
                foreach (var kvp in capDict)
                {
                    if (int.TryParse(kvp.Key, out int th))
                    {
                        table.entries.Add(new TileCapEntry
                        {
                            townHallLevel = th,
                            maxTiles = ToInt(kvp.Value)
                        });
                    }
                }

                table.entries.Sort((a, b) => a.townHallLevel.CompareTo(b.townHallLevel));
            }
        }

        private static void ParseUnlockRules(Dictionary<string, object> root, UnlockRuleSet set)
        {
            if (!root.TryGetValue("unlockRules", out var unlockObj) || !(unlockObj is List<object> unlockList))
            {
                return;
            }

            foreach (var ruleObj in unlockList)
            {
                if (ruleObj is Dictionary<string, object> ruleDict)
                {
                    var rule = new UnlockRule
                    {
                        tag = GetString(ruleDict, "tag", string.Empty),
                        townHallMin = ToInt(ruleDict.TryGetValue("townHallMin", out var thObj) ? thObj : 0),
                        unlock = new UnlockReward()
                    };

                    if (ruleDict.TryGetValue("unlock", out var unlockDetailObj) && unlockDetailObj is Dictionary<string, object> unlockDict)
                    {
                        rule.unlock.slotType = GetString(unlockDict, "slotType", string.Empty);
                        rule.unlock.count = ToInt(unlockDict.TryGetValue("count", out var countObj) ? countObj : 0);
                    }

                    set.rules.Add(rule);
                }
            }
        }

        private static List<string> ParseStringList(Dictionary<string, object> dict, string key)
        {
            var list = new List<string>();
            if (!dict.TryGetValue(key, out var obj) || !(obj is List<object> rawList))
            {
                return list;
            }

            for (int i = 0; i < rawList.Count; i++)
            {
                if (rawList[i] != null)
                {
                    list.Add(rawList[i].ToString());
                }
            }

            return list;
        }

        private static string GetString(Dictionary<string, object> dict, string key, string defaultValue = "")
        {
            if (dict != null && dict.TryGetValue(key, out var obj) && obj != null)
            {
                return obj.ToString();
            }
            return defaultValue;
        }

        private static int ToInt(object obj)
        {
            if (obj == null)
            {
                return 0;
            }

            if (obj is long l)
            {
                return (int)l;
            }

            if (obj is double d)
            {
                return (int)d;
            }

            if (int.TryParse(obj.ToString(), out int value))
            {
                return value;
            }

            return 0;
        }
    }
}
