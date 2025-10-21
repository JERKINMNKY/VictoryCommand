using System.Text;
using UnityEngine;

// Layer: Core Simulation
// Life Domain: Progression
namespace IFC.Systems
{
    /// <summary>
    /// Resolves mission progress and completion rewards.
    /// </summary>
    public class MissionSystem : MonoBehaviour
    {
        private GameState _state;

        public void Initialize(GameState state)
        {
            _state = state;
        }

        public void ProcessTick(int secondsPerTick)
        {
            if (_state?.missions == null)
            {
                return;
            }

            bool anyCompleted = false;
            var sb = new StringBuilder();
            sb.AppendLine("[MissionSystem] Progress check");

            for (int i = _state.missions.activeMissions.Count - 1; i >= 0; i--)
            {
                var mission = _state.missions.activeMissions[i];
                if (mission.rewardClaimed)
                {
                    continue;
                }

                if (mission.target <= 0)
                {
                    mission.target = 1;
                }

                if (mission.progress >= mission.target)
                {
                    mission.rewardClaimed = true;
                    if (!_state.missions.completedMissionIds.Contains(mission.missionId))
                    {
                        _state.missions.completedMissionIds.Add(mission.missionId);
                    }

                    sb.AppendLine($"  Mission {mission.missionId} completed (progress {mission.progress}/{mission.target}).");
                    anyCompleted = true;
                    _state.missions.activeMissions.RemoveAt(i);
                }
            }

            if (anyCompleted)
            {
                Debug.Log(sb.ToString());
            }
        }
    }
}
