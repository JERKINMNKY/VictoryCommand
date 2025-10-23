using System;
using System.Text;
using UnityEngine;
using IFC.Systems.Officers;

// Layer: Core Simulation
// Life Domain: Security & Defense
namespace IFC.Systems
{
    /// <summary>
    /// Simulates troop training queues and resource consumption.
    /// </summary>
    public class TrainingSystem : MonoBehaviour
    {
        private GameState _state;
        private StatModifierRegistry _statModifiers;
        private const string TrainingMultiplierKey = OfficerBuffPass.TrainingTimeMultiplierKey;

        public void Initialize(GameState state, StatModifierRegistry statModifiers)
        {
            _state = state;
            _statModifiers = statModifiers;
        }

        public void ProcessTick(int secondsPerTick)
        {
            if (_state == null)
            {
                Debug.LogWarning("TrainingSystem not initialised with state");
                return;
            }

            var sb = new StringBuilder();
            sb.AppendLine("[TrainingSystem] Training tick");

            for (int c = 0; c < _state.cities.Count; c++)
            {
                var city = _state.cities[c];
                for (int q = 0; q < city.trainingQueues.Count; q++)
                {
                    var queue = city.trainingQueues[q];
                    if (queue.quantity == 0)
                    {
                        continue;
                    }

                    if (!queue.resourcesCommitted)
                    {
                        if (TryConsumeResources(city, queue))
                        {
                            queue.resourcesCommitted = true;
                            int baseDuration = Mathf.Max(1, queue.durationSeconds);
                            int adjustedDuration = CalculateAdjustedDuration(queue, baseDuration);
                            queue.secondsRemaining = Mathf.Max(1, adjustedDuration);
                            sb.AppendLine($"  {city.displayName}: Began training {queue.quantity} {queue.unitType}");
                        }
                        else
                        {
                            sb.AppendLine($"  {city.displayName}: Waiting for resources to train {queue.unitType}");
                            continue;
                        }
                    }

                    queue.secondsRemaining = Mathf.Max(0, queue.secondsRemaining - secondsPerTick);
                    ApplyUpkeep(city, queue);

                    if (queue.secondsRemaining == 0 && queue.resourcesCommitted)
                    {
                        var garrison = GameStateBuilder.GetOrCreateGarrison(city, queue.unitType);
                        garrison.quantity += queue.quantity;
                        sb.AppendLine($"  {city.displayName}: Completed {queue.quantity} {queue.unitType}. Garrison now {garrison.quantity}");
                        queue.resourcesCommitted = false;
                    }
                }
            }

            Debug.Log(sb.ToString());
        }

        private bool TryConsumeResources(CityState city, TrainingQueueState queue)
        {
            for (int i = 0; i < queue.costs.Count; i++)
            {
                var cost = queue.costs[i];
                var stockpile = GameStateBuilder.FindStockpile(city, cost.resourceType);
                if (stockpile == null || stockpile.amount < cost.amount)
                {
                    return false;
                }
            }

            for (int i = 0; i < queue.costs.Count; i++)
            {
                var cost = queue.costs[i];
                var stockpile = GameStateBuilder.FindStockpile(city, cost.resourceType);
                stockpile.amount -= cost.amount;
            }

            return true;
        }

        private void ApplyUpkeep(CityState city, TrainingQueueState queue)
        {
            if (queue.upkeepFoodPerTick <= 0)
            {
                return;
            }

            var stockpile = GameStateBuilder.FindStockpile(city, IFC.Data.ResourceType.Food);
            if (stockpile == null)
            {
                return;
            }

            stockpile.amount = Math.Max(0, stockpile.amount - queue.upkeepFoodPerTick);
        }

        private int CalculateAdjustedDuration(TrainingQueueState queue, int baseDuration)
        {
            if (_statModifiers == null)
            {
                return baseDuration;
            }

            string facilityId = queue.facilityId;
            float multiplier = _statModifiers.GetMultiplierOrDefault(facilityId, TrainingMultiplierKey, 1f);
            multiplier = Mathf.Clamp(multiplier, 0.1f, 10f);
            int adjusted = Mathf.Max(1, Mathf.RoundToInt(baseDuration * multiplier));

            if (!Mathf.Approximately(multiplier, 1f))
            {
                Debug.Log($"[TrainingSystem] Facility={facilityId} unit={queue.unitType} base={baseDuration} adjusted={adjusted} (multiplier={multiplier:0.00})");
            }

            return adjusted;
        }
    }
}
