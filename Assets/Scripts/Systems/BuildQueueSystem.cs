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
        private static readonly int[] BlueprintLevels = { 10, 13, 17, 20 };
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
                int activeSlots = Mathf.Min(city.buildQueue.activeOrders.Count, city.buildQueue.baseSlots);
                int completedThisTick = 0;

                for (int i = 0; i < activeSlots; i++)
                {
                    var order = city.buildQueue.activeOrders[i];
                    if (!IsBlueprintGateSatisfied(city, order))
                    {
                        sb.AppendLine($"  {city.displayName}: Waiting for blueprint for {order.buildingType} Lv{order.targetLevel}");
                        order.blueprintGateSatisfied = false;
                        continue;
                    }

                    order.blueprintGateSatisfied = true;
                    order.secondsRemaining = Mathf.Max(0, order.secondsRemaining - secondsPerTick);
                    if (order.secondsRemaining == 0)
                    {
                        completedThisTick++;
                        sb.AppendLine($"  {city.displayName}: Completed {order.buildingType} -> Lv{order.targetLevel}");
                        city.buildQueue.activeOrders.RemoveAt(i);
                        i--;
                        activeSlots = Mathf.Min(city.buildQueue.activeOrders.Count, city.buildQueue.baseSlots);
                    }
                }

                if (completedThisTick == 0 && city.buildQueue.activeOrders.Count == 0)
                {
                    sb.AppendLine($"  {city.displayName}: idle (no orders)");
                }
            }

            Debug.Log(sb.ToString());
        }

        private bool IsBlueprintGateSatisfied(CityState city, BuildOrderState order)
        {
            if (order.blueprintGateSatisfied)
            {
                return true;
            }

            bool requiresBlueprint = false;
            for (int b = 0; b < BlueprintLevels.Length; b++)
            {
                if (order.targetLevel >= BlueprintLevels[b])
                {
                    requiresBlueprint = true;
                }
            }

            if (!requiresBlueprint)
            {
                return true;
            }

            if (city.buildQueue.blueprintTokens > 0)
            {
                city.buildQueue.blueprintTokens -= 1;
                return true;
            }

            return false;
        }
    }
}
