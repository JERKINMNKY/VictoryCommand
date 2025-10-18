using System.Text;
using UnityEngine;

// Layer: Core Simulation
// Life Domain: Security & Defense
namespace IFC.Systems
{
    /// <summary>
    /// Applies morale driven defense decay and passive repair.
    /// </summary>
    public class DefenseSystem : MonoBehaviour
    {
        public int repairPerTick = 5;
        public int moraleDamageThreshold = 70;
        public int moraleDamageAmount = 15;

        private GameState _state;

        public void Initialize(GameState state)
        {
            _state = state;
        }

        public void ProcessTick(int secondsPerTick)
        {
            if (_state == null)
            {
                Debug.LogWarning("DefenseSystem not initialised with state");
                return;
            }

            var sb = new StringBuilder();
            sb.AppendLine("[DefenseSystem] Wall upkeep");

            for (int c = 0; c < _state.cities.Count; c++)
            {
                var city = _state.cities[c];
                int moralePercent = Mathf.RoundToInt(city.morale * 100f);

                if (moralePercent < moraleDamageThreshold)
                {
                    city.wall.currentHp = Mathf.Max(0, city.wall.currentHp - moraleDamageAmount);
                    if (city.wall.fortificationPoints > 0 && city.wall.currentHp == 0)
                    {
                        city.wall.fortificationPoints = Mathf.Max(0, city.wall.fortificationPoints - 1);
                        city.wall.currentHp = Mathf.Min(city.wall.maxHp, moraleDamageAmount);
                    }
                    sb.AppendLine($"  {city.displayName}: morale {moralePercent}% -> wall damage, HP {city.wall.currentHp}/{city.wall.maxHp}");
                }
                else
                {
                    int repaired = Mathf.Min(repairPerTick, city.wall.maxHp - city.wall.currentHp);
                    if (repaired > 0)
                    {
                        city.wall.currentHp += repaired;
                        sb.AppendLine($"  {city.displayName}: repaired {repaired}, HP {city.wall.currentHp}/{city.wall.maxHp}");
                    }
                }
            }

            Debug.Log(sb.ToString());
        }
    }
}
