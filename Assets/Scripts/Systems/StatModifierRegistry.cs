using System.Collections.Generic;

namespace IFC.Systems
{
    /// <summary>
    /// Tracks runtime stat multipliers keyed by logical entity id + stat key.
    /// </summary>
    public class StatModifierRegistry
    {
        private readonly Dictionary<(string id, string statKey), float> _multipliers = new Dictionary<(string, string), float>();

        public void SetMultiplier(string id, string statKey, float value)
        {
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(statKey))
            {
                return;
            }

            _multipliers[(id, statKey)] = value;
        }

        public float GetMultiplierOrDefault(string id, string statKey, float defaultValue = 1f)
        {
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(statKey))
            {
                return defaultValue;
            }

            return _multipliers.TryGetValue((id, statKey), out var value) ? value : defaultValue;
        }

        public void Clear()
        {
            _multipliers.Clear();
        }
    }
}
