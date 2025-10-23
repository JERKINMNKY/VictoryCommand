using System;
using System.IO;
using UnityEngine;

namespace IFC.Data
{
    public static class BuildingDefinitionLoader
    {
        private const string DefaultPath = "content/data/buildings.json";

        public static BuildingDefinitionCollection LoadFromJson(string overridePath = null)
        {
            string path = overridePath;
            if (string.IsNullOrEmpty(path))
            {
                path = Path.Combine(Application.dataPath, "..", DefaultPath);
            }
            else if (!Path.IsPathRooted(path))
            {
                path = Path.Combine(Application.dataPath, "..", path);
            }

            if (!File.Exists(path))
            {
                Debug.LogError($"[BuildingDefinitionLoader] Definition file not found at {path}");
                return null;
            }

            try
            {
                string json = File.ReadAllText(path);
                var collection = JsonUtility.FromJson<BuildingDefinitionCollectionWrapper>(json);
                if (collection == null)
                {
                    Debug.LogError($"[BuildingDefinitionLoader] Failed to parse definitions from {path}");
                    return null;
                }

                return collection.ToCollection();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[BuildingDefinitionLoader] Error loading definitions: {ex}");
                return null;
            }
        }

        [Serializable]
        private class BuildingDefinitionCollectionWrapper
        {
            public int schemaVersion = 1;
            public float costCurveExponent = 1.25f;
            public float timeCurveMultiplier = 1.3f;
            public BuildingDefinition[] buildings;

            public BuildingDefinitionCollection ToCollection()
            {
                var collection = new BuildingDefinitionCollection
                {
                    schemaVersion = schemaVersion,
                    costCurveExponent = costCurveExponent,
                    timeCurveMultiplier = timeCurveMultiplier
                };

                if (buildings != null)
                {
                    collection.buildings.AddRange(buildings);
                }

                return collection;
            }
        }
    }
}
