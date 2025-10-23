using System.Collections.Generic;
using System.IO;
using UnityEngine;
using IFC.Data;
using IFC.Systems.Officers;
using IFC.Systems.Profiles;
using IFC.Systems.UI;

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
        [SerializeField] private bool useStartProfile = false;
        [SerializeField] private bool logBuildingIdsOnStart = false;
        [SerializeField] private StartProfileAsset startProfileAsset;
        [SerializeField] private string startProfileJsonPath = "content/profiles/StartProfile_NewPlayer.json";

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
        private BuildingFunctionSystem _buildingFunctionSystem;
        private BuildingEffectRuntime _buildingEffectRuntime;
        private StatModifierRegistry _statModifiers;
        private readonly Dictionary<string, OfficerAssignmentView> _officerAssignmentViews = new Dictionary<string, OfficerAssignmentView>();
        private IOfficerStatsProvider _officerStatsProvider;
        private readonly BuildingCatalog _buildingCatalog = new BuildingCatalog();
        private BuildController _buildController;
        private bool _buildingDefinitionsLoaded;

        public GameState CurrentState => _state;
        public BuildController BuildController => _buildController;
        public BuildingCatalog BuildingCatalog => _buildingCatalog;

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
            _buildingFunctionSystem = GetOrCreate<BuildingFunctionSystem>();
            _buildingEffectRuntime = GetOrCreate<BuildingEffectRuntime>();
            // Defer UI bootstrap and dev helpers until after state + catalogs are loaded
            
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
            ResetStatModifiers();
            _buildingFunctionSystem.ProcessTick(secondsPerTick);
            ApplyOfficerBuffs();
            _trainingSystem.ProcessTick(secondsPerTick);
            _defenseSystem.ProcessTick(secondsPerTick);
            _autoTransportSystem.ProcessTick(secondsPerTick);
            _mapSystem.ProcessTick(secondsPerTick);
            _missionSystem.ProcessTick(secondsPerTick);
            _battleSystem.ProcessTick(secondsPerTick);
            UIRefreshService.RefreshAll();
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
            EnsureBuildingCatalog();
            _buildQueueSystem.SetBuildingCatalog(_buildingCatalog);
            if (logBuildingIdsOnStart) { DevActions.DumpBuildingIds(this); DevActions.DumpCitySummary(this); }
            _buildQueueSystem.SetEffectRuntime(_buildingEffectRuntime);
            _buildController = new BuildController(_state, _buildingCatalog, _state.inventory, _state.tileCaps);
            _statModifiers = new StatModifierRegistry();
            BuildOfficerAssignmentViews();
            _trainingSystem.Initialize(_state, _statModifiers);
            _buildingFunctionSystem.Initialize(_state, _statModifiers);
            _buildingEffectRuntime.Initialize(_state, _buildingCatalog);
            _autoTransportSystem.Initialize(_state);
            _defenseSystem.Initialize(_state);
            _mapSystem.Initialize(_state);
            _missionSystem.Initialize(_state);
            _battleSystem.Initialize(_state);
            _timer = 0f;

            // Ensure UI and dev helpers exist after systems and catalogs are initialized
            GetOrCreate<CityUIBootstrap>();
            GetOrCreate<DevActionsBehaviour>();

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
            if (useStartProfile)
            {
                var profile = StartProfileLoader.Load(startProfileAsset, startProfileJsonPath);
                var profileState = GameStateBuilder.FromStartProfile(profile);
                SetGameState(profileState, $"profile:{profile.profileId}");
                return;
            }

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

            for (int i = 0; i < _state.cities.Count; i++)
            {
                var city = _state.cities[i];
                _officerAssignmentViews.TryGetValue(city.cityId, out var view);
                OfficerBuffPass.Apply(city, view, _statModifiers);
            }
        }

        private void ResetStatModifiers()
        {
            _statModifiers?.Clear();
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

        private void EnsureBuildingCatalog()
        {
            if (!_buildingDefinitionsLoaded)
            {
                var definitionCollection = BuildingDefinitionLoader.LoadFromJson();
                if (definitionCollection != null)
                {
                    _buildingCatalog.LoadDefinitions(definitionCollection);
                    _buildingDefinitionsLoaded = true;
                }
                else
                {
                    Debug.LogWarning("[GameLoop] Building definition collection failed to load; falling back to legacy ScriptableObjects.");
                }
            }

            BuildingData[] definitions = Resources.LoadAll<BuildingData>("ScriptableObjects/Buildings");
            if (definitions == null || definitions.Length == 0)
            {
                definitions = Resources.LoadAll<BuildingData>(string.Empty);
            }

            for (int i = 0; i < definitions.Length; i++)
            {
                var data = definitions[i];
                if (data == null)
                {
                    continue;
                }

                _buildingCatalog.Add(data);
            }
        }
    }
}
