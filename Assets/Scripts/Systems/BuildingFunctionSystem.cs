using System;
using System.Text;
using UnityEngine;
using IFC.Data;
using IFC.Systems.Officers;

// Layer: Core Simulation
// Life Domain: Economy & Military
namespace IFC.Systems
{
    /// <summary>
    /// Executes data-driven building functionality (population, production, training buffs, trading, etc.).
    /// </summary>
    public class BuildingFunctionSystem : MonoBehaviour
    {
        private GameState _state;
        private StatModifierRegistry _statModifiers;

        public void Initialize(GameState state, StatModifierRegistry statModifiers)
        {
            _state = state;
            _statModifiers = statModifiers;
        }

        public void ProcessTick(int secondsPerTick)
        {
            if (_state == null || _state.cities.Count == 0)
            {
                return;
            }

            var logBuilder = new StringBuilder();
            logBuilder.AppendLine("[BuildingFunctionSystem] Effects");
            int logLines = 0;

            for (int c = 0; c < _state.cities.Count; c++)
            {
                var city = _state.cities[c];
                for (int i = 0; i < city.buildingFunctions.Count; i++)
                {
                    var function = city.buildingFunctions[i];
                    switch (function.functionType)
                    {
                        case BuildingFunctionType.PopulationGrowth:
                        case BuildingFunctionType.ResourceProduction:
                            // Handled by ResourceSystem now (still logged there)
                            break;
                        case BuildingFunctionType.TrainingSpeedBuff:
                            if (ApplyTrainingSpeedBuff(function, city))
                            {
                                logBuilder.AppendLine($"  [Train] {function.buildingId}@{city.displayName} multiplier {function.trainingMultiplier:0.00} (facility {function.facilityId})");
                                logLines++;
                            }
                            break;
                        case BuildingFunctionType.ResourceExchange:
                            if (ApplyResourceExchange(city, function, out int spent, out int gained))
                            {
                                logBuilder.AppendLine($"  [Trade] {function.buildingId}@{city.displayName} -{spent} {function.resourceType} +{gained} {function.targetResource} (L{function.level})");
                                logLines++;
                            }
                            break;
                        case BuildingFunctionType.CapacityBoost:
                        case BuildingFunctionType.None:
                        default:
                            break;
                    }
                }
            }

            if (logLines > 0)
            {
                Debug.Log(logBuilder.ToString());
            }
        }

        private bool ApplyTrainingSpeedBuff(BuildingFunctionState function, CityState city)
        {
            if (_statModifiers == null)
            {
                return false;
            }

            if (string.IsNullOrEmpty(function.facilityId))
            {
                return false;
            }

            float multiplier = function.trainingMultiplier;
            if (multiplier <= 0f)
            {
                multiplier = 1f;
            }

            multiplier = Mathf.Clamp(multiplier, 0.1f, 5f);
            float current = _statModifiers.GetMultiplierOrDefault(function.facilityId, OfficerBuffPass.TrainingTimeMultiplierKey, 1f);
            _statModifiers.SetMultiplier(function.facilityId, OfficerBuffPass.TrainingTimeMultiplierKey, current * multiplier);
            return !Mathf.Approximately(multiplier, 1f);
        }

        private bool ApplyResourceExchange(CityState city, BuildingFunctionState function, out int spent, out int gained)
        {
            spent = Mathf.RoundToInt(function.amountPerTick);
            gained = 0;
            if (spent <= 0)
            {
                return false;
            }

            var source = GameStateBuilder.FindStockpile(city, function.resourceType);
            var target = GameStateBuilder.FindStockpile(city, function.targetResource);
            if (source == null || target == null)
            {
                return false;
            }

            long transferable = Math.Min(source.amount, (long)spent);
            if (transferable <= 0)
            {
                return false;
            }

            long potentialGain = (long)Math.Round(transferable * Mathf.Max(0f, function.exchangeRate));
            if (potentialGain <= 0)
            {
                return false;
            }

            long capacity = Math.Max(0L, target.capacity - target.amount);
            if (capacity <= 0)
            {
                return false;
            }

            long applied = Math.Min(capacity, potentialGain);
            if (applied <= 0)
            {
                return false;
            }

            source.amount = Math.Max(0, source.amount - transferable);
            target.amount = Math.Min(target.capacity, target.amount + applied);
            spent = (int)Math.Min(int.MaxValue, transferable);
            gained = (int)Math.Min(int.MaxValue, applied);
            return applied > 0;
        }
    }
}
