using System;
using System.Text;
using IFC.Data;
using UnityEngine;

// Layer: Core Simulation
// Life Domain: Progression
namespace IFC.Systems
{
    /// <summary>
    /// Resolves mission progress and completion rewards.
    /// </summary>
    public class MissionSystem : MonoBehaviour
    {
        private GameState _state;
        private bool _subscribed;

        public void Initialize(GameState state)
        {
            _state = state;
            SubscribeEvents();
        }

        public void ProcessTick(int secondsPerTick)
        {
            // tick-driven logic handled by event callbacks
        }

        private void OnDisable()
        {
            UnsubscribeEvents();
        }

        private void SubscribeEvents()
        {
            if (_subscribed)
            {
                return;
            }

            GameEventHub.Subscribe<BuildingConstructedEvent>(OnBuildingConstructed);
            GameEventHub.Subscribe<BuildingUpgradedEvent>(OnBuildingUpgraded);
            _subscribed = true;
        }

        private void UnsubscribeEvents()
        {
            if (!_subscribed)
            {
                return;
            }

            GameEventHub.Unsubscribe<BuildingConstructedEvent>(OnBuildingConstructed);
            GameEventHub.Unsubscribe<BuildingUpgradedEvent>(OnBuildingUpgraded);
            _subscribed = false;
        }

        private void OnBuildingConstructed(BuildingConstructedEvent evt)
        {
            ProcessMissionTrigger(MissionTrigger.BuildingConstructed, evt.buildingKey, evt.level, evt.cityId);
        }

        private void OnBuildingUpgraded(BuildingUpgradedEvent evt)
        {
            ProcessMissionTrigger(MissionTrigger.BuildingUpgraded, evt.buildingKey, evt.toLevel, evt.cityId);
        }

        private void ProcessMissionTrigger(MissionTrigger trigger, string buildingKey, int level, string cityId)
        {
            if (_state?.missions == null)
            {
                return;
            }

            for (int i = 0; i < _state.missions.activeMissions.Count; i++)
            {
                var mission = _state.missions.activeMissions[i];
                if (mission.rewardClaimed)
                {
                    continue;
                }

                var definition = MissionDefinitionRegistry.Get(mission.missionId);
                if (definition == null || definition.trigger != trigger)
                {
                    continue;
                }

                if (!string.Equals(definition.buildingKey, buildingKey, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (level < definition.targetLevel)
                {
                    continue;
                }

                mission.progress = Mathf.Min(mission.target, mission.progress + 1);
                _state.missions.activeMissions[i] = mission;
                Debug.Log($"[Mission] Progress {mission.missionId} {mission.progress}/{mission.target}");

                if (mission.progress >= mission.target)
                {
                    ApplyReward(definition, cityId);
                    mission.rewardClaimed = true;
                    _state.missions.activeMissions[i] = mission;
                    if (!_state.missions.completedMissionIds.Contains(mission.missionId))
                    {
                        _state.missions.completedMissionIds.Add(mission.missionId);
                    }
                    Debug.Log($"[Mission] Complete {mission.missionId}");
                }
            }
        }

        private void ApplyReward(MissionDefinition definition, string cityId)
        {
            if (definition.reward == null || _state == null)
            {
                return;
            }

            if (definition.reward.resourceType.HasValue)
            {
                var city = _state.GetCityById(cityId);
                if (city != null)
                {
                    var stockpile = GameStateBuilder.FindStockpile(city, definition.reward.resourceType.Value);
                    if (stockpile != null)
                    {
                        long hardCap = CityConstants.RESOURCE_HARD_CAP;
                        stockpile.amount = Math.Max(0, Math.Min(hardCap, stockpile.amount + definition.reward.amount));
                    }
                }
            }
            else if (!string.IsNullOrEmpty(definition.reward.inventoryItemId))
            {
                _state.player?.tokenInventory.Add(definition.reward.inventoryItemId, definition.reward.amount);
            }
        }
    }
}
