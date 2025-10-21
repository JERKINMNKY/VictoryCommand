using System.Text;
using UnityEngine;

// Layer: Core Simulation
// Life Domain: Logistics & World
namespace IFC.Systems
{
    /// <summary>
    /// Maintains territory control and front unlock progression.
    /// </summary>
    public class MapSystem : MonoBehaviour
    {
        [Range(0f, 0.05f)]
        public float uncontrolledDecayPerSecond = 0.01f;

        [Range(0f, 0.05f)]
        public float controlledRecoveryPerSecond = 0.005f;

        private GameState _state;

        public void Initialize(GameState state)
        {
            _state = state;
        }

        public void ProcessTick(int secondsPerTick)
        {
            if (_state?.world == null || _state.world.territories.Count == 0)
            {
                return;
            }

            var sb = new StringBuilder();
            sb.AppendLine("[MapSystem] Territory upkeep");

            float decayStep = uncontrolledDecayPerSecond * secondsPerTick;
            float recoveryStep = controlledRecoveryPerSecond * secondsPerTick;

            for (int i = 0; i < _state.world.territories.Count; i++)
            {
                var territory = _state.world.territories[i];
                if (string.IsNullOrEmpty(territory.ownerCityId))
                {
                    territory.controlProgress = Mathf.Max(0f, territory.controlProgress - decayStep);
                    sb.AppendLine($"  {territory.territoryId}: uncontrolled, decay {decayStep:0.000}, progress {territory.controlProgress:0.00}");
                }
                else
                {
                    territory.controlProgress = Mathf.Clamp01(territory.controlProgress + recoveryStep);
                    sb.AppendLine($"  {territory.territoryId}: held by {territory.ownerCityId}, progress {territory.controlProgress:0.00}");
                }
            }

            Debug.Log(sb.ToString());
        }
    }
}
