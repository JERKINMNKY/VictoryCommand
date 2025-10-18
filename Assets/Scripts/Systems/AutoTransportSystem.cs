using System.Text;
using UnityEngine;

// Layer: Core Simulation
// Life Domain: Autonomy & Logistics
namespace IFC.Systems
{
    /// <summary>
    /// Ships resources between cities on a fixed cadence.
    /// </summary>
    public class AutoTransportSystem : MonoBehaviour
    {
        private GameState _state;

        public void Initialize(GameState state)
        {
            _state = state;
        }

        public void ProcessTick(int secondsPerTick)
        {
            if (_state == null)
            {
                Debug.LogWarning("AutoTransportSystem not initialised with state");
                return;
            }

            var sb = new StringBuilder();
            sb.AppendLine("[AutoTransportSystem] Routes");

            for (int c = 0; c < _state.cities.Count; c++)
            {
                var sourceCity = _state.cities[c];
                for (int r = 0; r < sourceCity.transportRoutes.Count; r++)
                {
                    var route = sourceCity.transportRoutes[r];
                    if (string.IsNullOrEmpty(route.targetCityId))
                    {
                        continue;
                    }

                    route.secondsUntilNext = Mathf.Max(0, route.secondsUntilNext - secondsPerTick);
                    if (route.secondsUntilNext > 0)
                    {
                        continue;
                    }

                    var targetCity = _state.GetCityById(route.targetCityId);
                    if (targetCity == null)
                    {
                        sb.AppendLine($"  {sourceCity.displayName}: Missing target {route.targetCityId}");
                        route.secondsUntilNext = route.intervalSeconds;
                        continue;
                    }

                    var sourceStockpile = GameStateBuilder.FindStockpile(sourceCity, route.resourceType);
                    var targetStockpile = GameStateBuilder.FindStockpile(targetCity, route.resourceType);
                    if (sourceStockpile == null || targetStockpile == null)
                    {
                        route.secondsUntilNext = route.intervalSeconds;
                        continue;
                    }

                    int transferable = Mathf.Min(route.amountPerShipment, sourceStockpile.amount);
                    if (transferable <= 0)
                    {
                        sb.AppendLine($"  {sourceCity.displayName}: Not enough {route.resourceType} for shipment");
                        route.secondsUntilNext = route.intervalSeconds;
                        continue;
                    }

                    int freeCapacity = Mathf.Max(0, targetStockpile.capacity - targetStockpile.amount);
                    int shipped = Mathf.Min(transferable, freeCapacity);
                    sourceStockpile.amount -= shipped;
                    targetStockpile.amount += shipped;
                    route.secondsUntilNext = route.intervalSeconds;

                    sb.AppendLine($"  {sourceCity.displayName} -> {targetCity.displayName}: {shipped} {route.resourceType}");
                }
            }

            Debug.Log(sb.ToString());
        }
    }
}
