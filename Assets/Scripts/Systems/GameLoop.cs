using System.IO;
using UnityEngine;

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
            _buildQueueSystem.Initialize(_state);
            _trainingSystem.Initialize(_state);
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
    }
}
