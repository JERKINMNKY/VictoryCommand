#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using IFC.Data;
using IFC.Systems.Util;
using UnityEditor;
using UnityEngine;

namespace IFC.EditorTools
{
    public static class ContentIdValidator
    {
        private const string BuildingsPath = "content/data/buildings.json";
        private static readonly string ReportPath = Path.Combine("Assets", "Editor", "ValidationReport.txt");
        private static readonly string[] LegacyBuildingIds =
        {
            "GeneralHQ",
            "MilitaryInstitute",
            "GHQ"
        };

        [MenuItem("Tools/Validate Content IDs")]
        public static void RunValidation()
        {
            var report = new StringBuilder();
            var buildingIds = LoadBuildingIds(report);
            report.AppendLine("=== Content Validation Report ===");
            report.AppendLine($"Timestamp: {DateTime.UtcNow:o}");
            report.AppendLine();

            ValidateProfile(buildingIds, report, "content/profiles/StartProfile_NewPlayer.json");
            ValidateSeed(buildingIds, report, "content/seed/game_state.json");
            ScanLegacyTokens(report);

            Directory.CreateDirectory(Path.GetDirectoryName(ReportPath) ?? "Assets/Editor");
            File.WriteAllText(ReportPath, report.ToString());
            AssetDatabase.Refresh();
            Debug.Log($"[ContentIdValidator] Validation complete. See {ReportPath} for details.");
        }

        private static HashSet<string> LoadBuildingIds(StringBuilder report)
        {
            var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var collection = BuildingDefinitionLoader.LoadFromJson(BuildingsPath);
            if (collection == null)
            {
                report.AppendLine($"[ERROR] Failed to load building definitions from {BuildingsPath}");
                return result;
            }

            foreach (var building in collection.buildings)
            {
                if (building != null && !string.IsNullOrEmpty(building.id))
                {
                    result.Add(building.id);
                }
            }

            return result;
        }

        private static void ValidateProfile(HashSet<string> buildingIds, StringBuilder report, string relativePath)
        {
            string path = Path.Combine(Application.dataPath, "..", relativePath);
            if (!File.Exists(path))
            {
                report.AppendLine($"[WARN] Profile file not found: {relativePath}");
                return;
            }

            try
            {
                var json = File.ReadAllText(path);
                if (!(MiniJSON.Deserialize(json) is Dictionary<string, object> root))
                {
                    report.AppendLine($"[ERROR] Could not parse JSON: {relativePath}");
                    return;
                }

                if (root.TryGetValue("startingCity", out var cityObj) && cityObj is Dictionary<string, object> city)
                {
                    if (city.TryGetValue("buildings", out var buildingsObj) && buildingsObj is Dictionary<string, object> buildingsDict)
                    {
                        foreach (var kvp in buildingsDict)
                        {
                            if (!buildingIds.Contains(kvp.Key))
                            {
                                report.AppendLine($"[WARN] Profile {relativePath} references unknown building '{kvp.Key}'");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                report.AppendLine($"[ERROR] Failed to validate profile {relativePath}: {ex.Message}");
            }
        }

        private static void ValidateSeed(HashSet<string> buildingIds, StringBuilder report, string relativePath)
        {
            string path = Path.Combine(Application.dataPath, "..", relativePath);
            if (!File.Exists(path))
            {
                report.AppendLine($"[WARN] Seed file not found: {relativePath}");
                return;
            }

            try
            {
                var json = File.ReadAllText(path);
                if (!(MiniJSON.Deserialize(json) is Dictionary<string, object> root))
                {
                    report.AppendLine($"[ERROR] Could not parse JSON: {relativePath}");
                    return;
                }

                if (root.TryGetValue("cities", out var citiesObj) && citiesObj is List<object> cities)
                {
                    for (int c = 0; c < cities.Count; c++)
                    {
                        if (!(cities[c] is Dictionary<string, object> city))
                        {
                            continue;
                        }

                        if (city.TryGetValue("buildings", out var buildingsObj) && buildingsObj is List<object> buildings)
                        {
                            foreach (var entry in buildings.OfType<Dictionary<string, object>>())
                            {
                                if (entry.TryGetValue("buildingType", out var typeObj))
                                {
                                    var key = typeObj?.ToString();
                                    if (!string.IsNullOrEmpty(key) && !buildingIds.Contains(key))
                                    {
                                        var cityId = city.TryGetValue("id", out var idObj) ? idObj?.ToString() ?? "unknown" : "unknown";
                                        report.AppendLine($"[WARN] Seed {relativePath} city '{cityId}' references unknown building '{key}'");
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                report.AppendLine($"[ERROR] Failed to validate seed {relativePath}: {ex.Message}");
            }
        }

        private static void ScanLegacyTokens(StringBuilder report)
        {
            var assetRoot = Application.dataPath;
            var legacyHits = new List<string>();

            foreach (var file in Directory.GetFiles(assetRoot, "*.cs", SearchOption.AllDirectories))
            {
                string text;
                try
                {
                    text = File.ReadAllText(file);
                }
                catch (Exception ex)
                {
                    report.AppendLine($"[WARN] Could not read {file}: {ex.Message}");
                    continue;
                }

                foreach (var legacy in LegacyBuildingIds)
                {
                    if (text.Contains(legacy, StringComparison.Ordinal))
                    {
                        legacyHits.Add($"{file.Replace(assetRoot, "Assets")}: contains legacy id '{legacy}'");
                    }
                }
            }

            foreach (var hit in legacyHits)
            {
                report.AppendLine($"[WARN] {hit}");
            }
        }
    }
}
#endif
