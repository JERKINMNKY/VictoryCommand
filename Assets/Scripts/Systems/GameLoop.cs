using System.Collections.Generic;
using System.IO;
using UnityEngine;
using IFC.Data;
using IFC.Systems.Officers;

// Layer: Core Simulation
// Life Domain: Autonomy & Logistics
namespace IFC.Systems
{
    /// <summary>
    /// Central tick orchestrator for the prototype loop.
    /// </summary>
    public class GameLoop : MonoBehaviour
    {
        public int secondsPerTick = 60;
        public string seedFileName = "game_state.json";

        private float _timer;
        private GameState _state;
        private ResourceSystem _resourceSystem;
        private BuildQueueSystem _buildQueueSystem;
        private TrainingSystem _trainingSystem;
        private AutoTransportSystem _autoTransportSystem;
        private DefenseSystem _defenseSystem;
        private MapSystem _mapSystem;
        private MissionSystem _missionSystem;
        private BattleSystem _battleSystem;
        private StatModifierRegistry _statModifiers;
        private readonly Dictionary<string, OfficerAssignmentView> _officerAssignmentViews = new Dictionary<string, OfficerAssignmentView>();
        private IOfficerStatsProvider _officerStatsProvider;

        public GameState CurrentState => _state;

        private void Awake()
        {
            _resourceSystem = GetOrCreate<ResourceSystem>();
            _buildQueueSystem = GetOrCreate<BuildQueueSystem>();
            _trainingSystem = GetOrCreate<TrainingSystem>();
            _autoTransportSystem = GetOrCreate<AutoTransportSystem>();
            _defenseSystem = GetOrCreate<DefenseSystem>();
            _mapSystem = GetOrCreate<MapSystem>();
            _missionSystem = GetOrCreate<MissionSystem>();
            _battleSystem = GetOrCreate<BattleSystem>();

            LoadSeed();
        }

        private void Update()
        {
            if (_state == null)
            {
                return;
            }

            _timer += Time.deltaTime;
            if (_timer >= secondsPerTick)
            {
                ExecuteTick();
                _timer = 0f;
            }
        }

        public void ExecuteTick()
        {
            if (_state == null)
            {
                Debug.LogWarning("GameLoop tick skipped - no state loaded");
                return;
            }

            Debug.Log("=== Victory Command Prototype Tick ===");
            _buildQueueSystem.ProcessTick(secondsPerTick);
            _resourceSystem.ProcessTick(secondsPerTick);
            ApplyOfficerBuffs();
            _trainingSystem.ProcessTick(secondsPerTick);
            _defenseSystem.ProcessTick(secondsPerTick);
            _autoTransportSystem.ProcessTick(secondsPerTick);
            _mapSystem.ProcessTick(secondsPerTick);
            _missionSystem.ProcessTick(secondsPerTick);
            _battleSystem.ProcessTick(secondsPerTick);
        }

        public void SetGameState(GameState state, string source = "runtime")
        {
            _state = state;
            if (_state == null)
            {
                Debug.LogWarning("[GameLoop] Attempted to set a null game state.");
                return;
            }

            _resourceSystem.Initialize(_state);
            EnsureOfficerStatsProvider();
            _resourceSystem.SetOfficerStatsProvider(_officerStatsProvider);
            _buildQueueSystem.Initialize(_state);
            _statModifiers = new StatModifierRegistry();
            BuildOfficerAssignmentViews();
            _trainingSystem.Initialize(_state, _statModifiers);
            _autoTransportSystem.Initialize(_state);
            _defenseSystem.Initialize(_state);
            _mapSystem.Initialize(_state);
            _missionSystem.Initialize(_state);
            _battleSystem.Initialize(_state);
            _timer = 0f;

            Debug.Log($"[GameLoop] Loaded state ({source}) with {_state.cities.Count} cities.");
        }

        private T GetOrCreate<T>() where T : Component
        {
            var component = GetComponent<T>();
            if (component == null)
            {
                component = gameObject.AddComponent<T>();
            }

            return component;
        }

        private void LoadSeed()
        {
            string seedPath = Path.Combine(Application.dataPath, "..", "content", "seed", seedFileName);
            if (!File.Exists(seedPath))
            {
                Debug.LogError($"Seed file not found at {seedPath}");
                return;
            }

            string json = File.ReadAllText(seedPath);
            var seed = JsonUtility.FromJson<SeedGameConfig>(json);
            var state = GameStateBuilder.FromSeed(seed);
            SetGameState(state, $"seed:{seedPath}");
        }

        private void BuildOfficerAssignmentViews()
        {
            _officerAssignmentViews.Clear();
            for (int i = 0; i < _state.cities.Count; i++)
            {
                var city = _state.cities[i];
                var view = new OfficerAssignmentView();
                if (city.officers != null)
                {
                    for (int o = 0; o < city.officers.Count; o++)
                    {
                        var assignment = city.officers[o];
                        if (assignment.role != OfficerRole.Logistics || assignment.facilityIds == null)
                        {
                            continue;
                        }

                        for (int f = 0; f < assignment.facilityIds.Count; f++)
                        {
                            var facilityId = assignment.facilityIds[f];
                            view.SetAssigned(facilityId, true);
                        }
                    }
                }

                _officerAssignmentViews[city.cityId] = view;
            }
        }

        private void ApplyOfficerBuffs()
        {
            if (_state == null || _statModifiers == null)
            {
                return;
            }

            _statModifiers.Clear();
            for (int i = 0; i < _state.cities.Count; i++)
            {
                var city = _state.cities[i];
                _officerAssignmentViews.TryGetValue(city.cityId, out var view);
                OfficerBuffPass.Apply(city, view, _statModifiers);
            }
        }

        private void EnsureOfficerStatsProvider()
        {
            if (_officerStatsProvider != null)
            {
                return;
            }

            OfficerData[] officers = Resources.LoadAll<OfficerData>("Officers");
            if (officers == null || officers.Length == 0)
            {
                officers = Resources.LoadAll<OfficerData>(string.Empty);
            }

            _officerStatsProvider = new OfficerStatsProvider(officers);
        }
    }
}
