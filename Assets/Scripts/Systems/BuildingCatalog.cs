using System.Collections.Generic;
using IFC.Data;

namespace IFC.Systems
{
    public class BuildingCatalog
    {
        private readonly Dictionary<string, SortedDictionary<int, BuildingData>> _catalog = new Dictionary<string, SortedDictionary<int, BuildingData>>();

        public void Add(BuildingData data)
        {
            if (data == null)
            {
                return;
            }

            var key = string.IsNullOrEmpty(data.BuildingName) ? data.name : data.BuildingName;
            if (string.IsNullOrEmpty(key))
            {
                return;
            }

            if (!_catalog.TryGetValue(key, out var levels))
            {
                levels = new SortedDictionary<int, BuildingData>();
                _catalog.Add(key, levels);
            }

            levels[data.Level] = data;
        }

        public bool TryGet(string buildingKey, int level, out BuildingData data)
        {
            data = null;
            if (string.IsNullOrEmpty(buildingKey) || level <= 0)
            {
                return false;
            }

            if (_catalog.TryGetValue(buildingKey, out var levels) && levels.TryGetValue(level, out data))
            {
                return true;
            }

            return false;
        }

        public IEnumerable<int> GetLevels(string buildingKey)
        {
            if (string.IsNullOrEmpty(buildingKey) || !_catalog.TryGetValue(buildingKey, out var levels))
            {
                yield break;
            }

            foreach (var kvp in levels)
            {
                yield return kvp.Key;
            }
        }
    }
}
