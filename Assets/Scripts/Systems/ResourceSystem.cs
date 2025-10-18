using System.Text;
using UnityEngine;

// Layer: Core Simulation
// Life Domain: Autonomy & Logistics
namespace IFC.Systems
{
    /// <summary>
    /// Handles per-city production and morale adjustments.
    /// </summary>
    public class ResourceSystem : MonoBehaviour
    {
        public float moralePenaltyThreshold = 0.9f;
        public float minimumMoraleFactor = 0.35f;
        public float maximumMoraleFactor = 1.25f;

        private GameState _state;

        public void Initialize(GameState state)
        {
            _state = state;
        }

        public void ProcessTick(int secondsPerTick)
        {
            if (_state == null)
            {
                Debug.LogWarning("ResourceSystem not initialised with state");
                return;
            }

            var message = new StringBuilder();
            message.AppendLine("[ResourceSystem] Production tick");

            for (int c = 0; c < _state.cities.Count; c++)
            {
                var city = _state.cities[c];
                float moraleFactor = CalculateMoraleFactor(city.morale);
                int totalProduced = 0;

                for (int p = 0; p < city.production.Count; p++)
                {
                    var productionField = city.production[p];
                    if (productionField.fields <= 0 || productionField.outputPerField <= 0)
                    {
                        continue;
                    }

                    int produced = Mathf.RoundToInt(productionField.fields * productionField.outputPerField * moraleFactor);
                    var stockpile = GameStateBuilder.FindStockpile(city, productionField.resourceType);
                    if (stockpile == null)
                    {
                        continue;
                    }

                    int availableCapacity = Mathf.Max(0, stockpile.capacity - stockpile.amount);
                    int applied = Mathf.Min(availableCapacity, produced);
                    stockpile.amount += applied;
                    totalProduced += applied;
                }

                message.AppendLine($"  {city.displayName}: morale={city.morale:0.00} factor={moraleFactor:0.00} produced={totalProduced}");
            }

            Debug.Log(message.ToString());
        }

        private float CalculateMoraleFactor(float morale)
        {
            if (morale >= 1f)
            {
                return Mathf.Min(maximumMoraleFactor, morale + 0.1f);
            }

            if (morale >= moralePenaltyThreshold)
            {
                return morale;
            }

            if (morale <= 0f)
            {
                return minimumMoraleFactor;
            }

            float t = morale / moralePenaltyThreshold;
            return Mathf.Lerp(minimumMoraleFactor, moralePenaltyThreshold, t);
        }
    }
}
