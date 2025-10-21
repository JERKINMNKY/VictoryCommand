using System.Text;
using UnityEngine;

// Layer: Core Simulation
// Life Domain: Autonomy & Logistics
namespace IFC.Systems
{
    /// <summary>
    /// Handles building upgrade queues and blueprint gate rules.
    /// </summary>
    public class BuildQueueSystem : MonoBehaviour
    {
        public const string UpgradeTokenId = "UpgradeToken";
        private const int UpgradeTokenGateLevel = 10;
        private GameState _state;

        public void Initialize(GameState state)
        {
            _state = state;
        }

        public void ProcessTick(int secondsPerTick)
        {
            if (_state == null)
            {
                Debug.LogWarning("BuildQueueSystem not initialised with state");
                return;
            }

            var sb = new StringBuilder();
            sb.AppendLine("[BuildQueueSystem] Processing build queues");

            for (int c = 0; c < _state.cities.Count; c++)
            {
                var city = _state.cities[c];
                var officerBonuses = city.officerBonuses ?? new OfficerBonusState();
                int slotBonus = Mathf.Max(0, Mathf.RoundToInt(Mathf.Max(0f, officerBonuses.GetConstructionSpeedMultiplier() - 1f)));
                int effectiveSlots = Mathf.Max(1, city.buildQueue.baseSlots + slotBonus);
                int activeSlots = Mathf.Min(city.buildQueue.activeOrders.Count, effectiveSlots);
                int completedThisTick = 0;

                for (int i = 0; i < activeSlots; i++)
                {
                    var order = city.buildQueue.activeOrders[i];
                    if (!IsUpgradeGateSatisfied(city, order))
                    {
                        sb.AppendLine($"  {city.displayName}: waiting for UpgradeToken ({order.buildingType} Lv{order.targetLevel})");
                        continue;
                    }
                    int tickReduction = Mathf.CeilToInt(secondsPerTick * officerBonuses.GetConstructionSpeedMultiplier());
                    order.secondsRemaining = Mathf.Max(0, order.secondsRemaining - tickReduction);
                    if (order.secondsRemaining == 0)
                    {
                        completedThisTick++;
                        Debug.Log($"[Build] Done {city.displayName}:{order.buildingType} L{order.targetLevel}");
                        city.buildQueue.activeOrders.RemoveAt(i);
                        i--;
                        activeSlots = Mathf.Min(city.buildQueue.activeOrders.Count, effectiveSlots);
                    }
                }

                if (completedThisTick == 0 && city.buildQueue.activeOrders.Count == 0)
                {
                    sb.AppendLine($"  {city.displayName}: idle (no orders)");
                }
            }

            Debug.Log(sb.ToString());
        }

        private bool IsUpgradeGateSatisfied(CityState city, BuildOrderState order)
        {
            if (order.blueprintGateSatisfied)
            {
                return true;
            }

            if (order.targetLevel < UpgradeTokenGateLevel)
            {
                return true;
            }

            if (_state?.inventory == null || !_state.inventory.Consume(UpgradeTokenId, 1))
            {
                Debug.Log($"[Build] Gate {city.displayName}:{order.buildingType} L{order.targetLevel} requires {UpgradeTokenId} x1");
                return false;
            }

            order.blueprintGateSatisfied = true;
            int fromLevel = Mathf.Max(0, order.targetLevel - 1);
            Debug.Log($"[Build] Start {city.displayName}:{order.buildingType} L{fromLevel}->{order.targetLevel} (consumed {UpgradeTokenId} x1)");
            return true;
        }
    }
}
