using System.Text;
using UnityEngine;

// Layer: Core Simulation
// Life Domain: Security & Defense
namespace IFC.Systems
{
    /// <summary>
    /// Placeholder battle resolver that converts queued requests into reports.
    /// </summary>
    public class BattleSystem : MonoBehaviour
    {
        private GameState _state;

        public void Initialize(GameState state)
        {
            _state = state;
        }

        public void ProcessTick(int secondsPerTick)
        {
            if (_state?.battleQueue == null || _state.battleQueue.pendingBattles.Count == 0)
            {
                return;
            }

            var sb = new StringBuilder();
            sb.AppendLine("[BattleSystem] Resolving queued battles");

            for (int i = _state.battleQueue.pendingBattles.Count - 1; i >= 0; i--)
            {
                var battle = _state.battleQueue.pendingBattles[i];
                battle.secondsUntilResolution = Mathf.Max(0, battle.secondsUntilResolution - secondsPerTick);
                if (battle.secondsUntilResolution > 0)
                {
                    continue;
                }

                var report = new BattleReport
                {
                    battleId = battle.battleId,
                    attackerCityId = battle.attackerCityId,
                    defenderCityId = battle.defenderCityId,
                    outcome = "ResolvedPlaceholder"
                };

                _state.battleQueue.battleHistory.Add(report);
                sb.AppendLine($"  Battle {battle.battleId}: {battle.attackerCityId} vs {battle.defenderCityId} -> {report.outcome}");
                _state.battleQueue.pendingBattles.RemoveAt(i);
            }

            Debug.Log(sb.ToString());
        }
    }
}
