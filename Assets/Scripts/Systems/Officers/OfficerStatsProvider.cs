using System.Collections.Generic;
using IFC.Data;

namespace IFC.Systems.Officers
{
    public interface IOfficerStatsProvider
    {
        bool TryGetPolitics(string officerId, out int politics);
    }

    /// <summary>
    /// Simple provider that maps officer identifiers to their politics stat.
    /// </summary>
    public class OfficerStatsProvider : IOfficerStatsProvider
    {
        private readonly Dictionary<string, int> _politicsById = new Dictionary<string, int>();

        public OfficerStatsProvider(IEnumerable<OfficerData> officers, bool useAssetNameAsFallback = true)
        {
            if (officers == null)
            {
                return;
            }

            foreach (var officer in officers)
            {
                if (officer == null)
                {
                    continue;
                }

                var id = ResolveOfficerId(officer, useAssetNameAsFallback);
                if (string.IsNullOrEmpty(id))
                {
                    continue;
                }

                if (_politicsById.ContainsKey(id))
                {
                    _politicsById[id] = officer.Politics;
                }
                else
                {
                    _politicsById.Add(id, officer.Politics);
            }
        }

            if (!_politicsById.ContainsKey("Commander"))
            {
                _politicsById.Add("Commander", 50);
            }
        }

        public bool TryGetPolitics(string officerId, out int politics)
        {
            if (string.IsNullOrEmpty(officerId))
            {
                politics = 0;
                return false;
            }

            return _politicsById.TryGetValue(officerId, out politics);
        }

        private static string ResolveOfficerId(OfficerData officer, bool useAssetNameAsFallback)
        {
            if (officer == null)
            {
                return string.Empty;
            }

            if (!string.IsNullOrEmpty(officer.OfficerName))
            {
                return officer.OfficerName;
            }

            if (useAssetNameAsFallback)
            {
                return officer.name;
            }

            return string.Empty;
        }
    }
}
