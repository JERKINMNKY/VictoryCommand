using UnityEngine;

namespace IFC.Systems
{
    /// <summary>
    /// Simple developer utilities for manipulating simulation state during tests/debugging.
    /// </summary>
    public static class DevActions
    {
        public static void GrantUpgradeToken(GameLoop loop, int amount)
        {
            if (loop?.CurrentState?.inventory == null || amount <= 0)
            {
                return;
            }

            loop.CurrentState.inventory.Add(BuildQueueSystem.UpgradeTokenId, amount);
            Debug.Log($"[Dev] Granted {BuildQueueSystem.UpgradeTokenId} x{amount}");
        }

        public static void EnqueueUpgrade(GameLoop loop, string cityId, string buildingType, int targetLevel, int buildSeconds)
        {
            if (loop?.CurrentState == null || string.IsNullOrEmpty(cityId) || string.IsNullOrEmpty(buildingType) || targetLevel <= 0)
            {
                return;
            }

            var city = loop.CurrentState.GetCityById(cityId);
            if (city == null)
            {
                Debug.LogWarning($"[Dev] City {cityId} not found for upgrade enqueue.");
                return;
            }

            var order = new BuildOrderState
            {
                buildingType = buildingType,
                targetLevel = targetLevel,
                secondsRemaining = Mathf.Max(1, buildSeconds),
                blueprintGateSatisfied = false
            };

            city.buildQueue.activeOrders.Add(order);
            Debug.Log($"[Dev] Enqueued {buildingType} -> Lv{targetLevel} for {city.displayName} ({buildSeconds}s).");
        }

        public static void AdvanceTicks(GameLoop loop, int ticks)
        {
            if (loop == null || ticks <= 0)
            {
                return;
            }

            for (int i = 0; i < ticks; i++)
            {
                loop.ExecuteTick();
            }

            Debug.Log($"[Dev] Advanced simulation by {ticks} tick(s).");
        }
    }
}
