#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using IFC.Systems;
using IFC.Systems.Profiles;
using UnityEditor;
using UnityEngine;

namespace IFC.EditorTools
{
    public static class StartProfileImporter
    {
        private const string DefaultJson = "content/profiles/StartProfile_NewPlayer.json";
        private const string DefaultAssetPath = "Assets/Profiles/StartProfile_NewPlayer.asset";

        [MenuItem("Tools/Profiles/Create Empty Start Profile")]
        public static void CreateEmpty()
        {
            EnsureFolder("Assets/Profiles");
            var asset = ScriptableObject.CreateInstance<StartProfileAsset>();
            AssetDatabase.CreateAsset(asset, AssetDatabase.GenerateUniqueAssetPath(DefaultAssetPath));
            AssetDatabase.SaveAssets();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
        }

        [MenuItem("Tools/Profiles/Import Start Profile (Default JSON)")]
        public static void ImportDefault()
        {
            string path = Path.Combine(Application.dataPath, "..", DefaultJson);
            ImportFromPath(path, DefaultAssetPath);
        }

        [MenuItem("Tools/Profiles/Import Start Profile (Choose JSON)…")]
        public static void ImportFromFile()
        {
            string startDir = Path.Combine(Application.dataPath, "..");
            string path = EditorUtility.OpenFilePanel("Select Start Profile JSON", startDir, "json");
            if (string.IsNullOrEmpty(path))
            {
                return;
            }
            string assetPath = EditorUtility.SaveFilePanelInProject("Save Start Profile Asset", "StartProfile", "asset", "Choose a save location for the Start Profile asset.", "Assets/Profiles");
            if (string.IsNullOrEmpty(assetPath))
            {
                return;
            }
            ImportFromPath(path, assetPath);
        }

        private static void ImportFromPath(string absoluteJsonPath, string assetPath)
        {
            if (!File.Exists(absoluteJsonPath))
            {
                Debug.LogError($"[StartProfileImporter] JSON not found at {absoluteJsonPath}");
                return;
            }

            string json = File.ReadAllText(absoluteJsonPath);
            var definition = StartProfileLoader.FromJson(json);
            if (definition == null)
            {
                Debug.LogError("[StartProfileImporter] Failed to parse JSON profile.");
                return;
            }

            EnsureFolder(Path.GetDirectoryName(assetPath));

            var asset = ScriptableObject.CreateInstance<StartProfileAsset>();
            FillAssetFromDefinition(asset, definition);

            AssetDatabase.CreateAsset(asset, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
            Debug.Log($"[StartProfileImporter] Imported profile '{asset.profileId}' to {assetPath}");
        }

        private static void FillAssetFromDefinition(StartProfileAsset asset, StartProfileDefinition def)
        {
            asset.profileId = def.profileId;

            // Tile caps
            asset.tileCaps.Clear();
            if (def.tileCaps?.entries != null)
            {
                asset.tileCaps.AddRange(def.tileCaps.entries);
            }

            // Rookie missions, city tags, unlock rules
            asset.rookieMissions = new List<string>(def.rookieMissions ?? new List<string>());
            asset.cityTags = new List<string>(def.cityTags ?? new List<string>());
            asset.unlockRules = def.unlockRules?.rules != null ? new List<UnlockRule>(def.unlockRules.rules) : new List<UnlockRule>();

            // City
            var city = asset.startingCity;
            city.id = def.startingCity.id;
            city.mayorOfficerId = def.startingCity.mayorOfficerId;
            city.routes = new List<string>(def.startingCity.routes ?? new List<string>());
            city.garrison = new List<string>(def.startingCity.garrison ?? new List<string>());
            city.tags = new List<string>(def.startingCity.tags ?? new List<string>());

            city.buildings.Clear();
            if (def.startingCity.buildings != null)
            {
                foreach (var kvp in def.startingCity.buildings)
                {
                    city.buildings.Add(new StartProfileBuildingLevel { buildingType = kvp.Key, level = kvp.Value });
                }
            }

            city.stockpile.Clear();
            if (def.startingCity.stockpile != null)
            {
                foreach (var kvp in def.startingCity.stockpile)
                {
                    city.stockpile.Add(new StartProfileStockItem { itemId = kvp.Key, amount = kvp.Value });
                }
            }
        }

        private static void EnsureFolder(string folder)
        {
            if (string.IsNullOrEmpty(folder)) return;
            if (AssetDatabase.IsValidFolder(folder)) return;
            string parent = Path.GetDirectoryName(folder).Replace("\\", "/");
            string name = Path.GetFileName(folder);
            if (!AssetDatabase.IsValidFolder(parent))
            {
                EnsureFolder(parent);
            }
            AssetDatabase.CreateFolder(parent, name);
        }
    }
}
#endif

