using System;
using System.IO;
using UnityEngine;

// Layer: Core Simulation
// Life Domain: Autonomy & Logistics
namespace IFC.Systems
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(GameLoop))]
    public class SaveGameManager : MonoBehaviour
    {
        private const int CurrentSchemaVersion = 1;

        [SerializeField] private string saveFileName = "autosave.json";
        [SerializeField] private bool loadOnStart = true;

        private GameLoop _gameLoop;

        private string SavePath => Path.Combine(Application.persistentDataPath, saveFileName);

        private void Awake()
        {
            _gameLoop = GetComponent<GameLoop>();
        }

        private void Start()
        {
            if (!loadOnStart)
            {
                return;
            }

            if (TryLoad(out var state))
            {
                _gameLoop.SetGameState(state, "save");
                Debug.Log($"[SaveGameManager] Loaded save from {SavePath}");
            }
        }

        private void OnApplicationQuit()
        {
            Save();
        }

        public void Save()
        {
            if (_gameLoop == null || _gameLoop.CurrentState == null)
            {
                Debug.LogWarning("[SaveGameManager] No game state available to save.");
                return;
            }

            var payload = new PersistedGameState
            {
                schemaVersion = CurrentSchemaVersion,
                state = _gameLoop.CurrentState
            };

            var json = JsonUtility.ToJson(payload, true);
            var directory = Path.GetDirectoryName(SavePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(SavePath, json);
            Debug.Log($"[SaveGameManager] Saved game to {SavePath}");
        }

        public bool TryLoad(out GameState state)
        {
            state = null;

            if (!File.Exists(SavePath))
            {
                return false;
            }

            try
            {
                var json = File.ReadAllText(SavePath);
                var payload = JsonUtility.FromJson<PersistedGameState>(json);
                if (payload == null)
                {
                    Debug.LogWarning("[SaveGameManager] Failed to parse save file.");
                    return false;
                }

                if (payload.schemaVersion != CurrentSchemaVersion)
                {
                    Debug.LogWarning($"[SaveGameManager] Schema mismatch (save {payload.schemaVersion} vs current {CurrentSchemaVersion}).");
                    return false;
                }

                state = payload.state;
                return state != null;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveGameManager] Error loading save: {ex.Message}");
                return false;
            }
        }

        [Serializable]
        private class PersistedGameState
        {
            public int schemaVersion;
            public GameState state;
        }
    }
}
