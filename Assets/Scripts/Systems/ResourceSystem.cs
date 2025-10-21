using System.Text;
using UnityEngine;
using IFC.Systems.Officers;

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
        [SerializeField]
        private AnimationCurve politicsToProduction = new AnimationCurve(
            new Keyframe(0f, 0.85f),
            new Keyframe(5f, 1f),
            new Keyframe(10f, 1.15f),
            new Keyframe(20f, 2f));

        private GameState _state;
        private IOfficerStatsProvider _officerStatsProvider;

        public void Initialize(GameState state)
        {
            _state = state;
        }

        public void SetOfficerStatsProvider(IOfficerStatsProvider provider)
        {
            _officerStatsProvider = provider;
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
                var officerBonuses = city.officerBonuses ?? new OfficerBonusState();
                float effectiveMorale = Mathf.Clamp(city.morale + officerBonuses.moraleBonus, 0f, 1.5f);
                float moraleFactor = CalculateMoraleFactor(effectiveMorale);
                float productionMultiplier = officerBonuses.GetProductionMultiplier();
                float politicsMultiplier = EvaluatePoliticsMultiplier(city.mayorOfficerId, out int politicsScore);
                float effectiveFactor = Mathf.Clamp(moraleFactor * productionMultiplier * politicsMultiplier,
                    minimumMoraleFactor * Mathf.Max(0.1f, productionMultiplier) * Mathf.Max(0.1f, politicsMultiplier),
                    maximumMoraleFactor * Mathf.Max(1f, productionMultiplier) * Mathf.Max(1f, politicsMultiplier));
                int totalProduced = 0;

                for (int p = 0; p < city.production.Count; p++)
                {
                    var productionField = city.production[p];
                    if (productionField.fields <= 0 || productionField.outputPerField <= 0)
                    {
                        continue;
                    }

                    int produced = Mathf.RoundToInt(productionField.fields * productionField.outputPerField * effectiveFactor);
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

                message.AppendLine($"  {city.displayName}: morale={city.morale:0.00} politics={politicsScore} mul={politicsMultiplier:0.00} factor={effectiveFactor:0.00} produced={totalProduced}");
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

        private float EvaluatePoliticsMultiplier(string mayorOfficerId, out int politics)
        {
            politics = 0;
            if (_officerStatsProvider == null || string.IsNullOrEmpty(mayorOfficerId))
            {
                return 1f;
            }

            if (_officerStatsProvider.TryGetPolitics(mayorOfficerId, out politics))
            {
                if (politicsToProduction == null || politicsToProduction.length == 0)
                {
                    return 1f;
                }

                var evaluated = politicsToProduction.Evaluate(politics);
                return Mathf.Clamp(evaluated, 0.1f, 3f);
            }

            return 1f;
        }
    }
}
