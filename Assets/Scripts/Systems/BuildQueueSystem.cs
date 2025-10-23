using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using IFC.Data;

// Layer: Core Simulation
// Life Domain: Autonomy & Logistics
namespace IFC.Systems
{
    /// <summary>
    /// Handles building upgrade queues and blueprint gate rules.
    /// </summary>
    public class BuildQueueSystem : MonoBehaviour
    {
        public const string UpgradeTokenId = "UpgradeToken";
        public const int UpgradeTokenGateLevel = 10;
        [SerializeField] private bool verboseLogging = false;
        private GameState _state;
        private BuildingCatalog _buildingCatalog;
        private BuildingEffectRuntime _effectRuntime;

        public void Initialize(GameState state)
        {
            _state = state;
        }

        public void SetBuildingCatalog(BuildingCatalog catalog)
        {
            _buildingCatalog = catalog;
        }

        public void SetEffectRuntime(BuildingEffectRuntime runtime)
        {
            _effectRuntime = runtime;
        }

        public void ProcessTick(int secondsPerTick)
        {
            if (_state == null)
            {
                Debug.LogWarning("BuildQueueSystem not initialised with state");
                return;
            }

            var sb = new StringBuilder();
            sb.AppendLine("[BuildQueueSystem] Processing build queues");

            for (int c = 0; c < _state.cities.Count; c++)
            {
                var city = _state.cities[c];
                var officerBonuses = city.officerBonuses ?? new OfficerBonusState();
                int slotBonus = Mathf.Max(0, Mathf.RoundToInt(Mathf.Max(0f, officerBonuses.GetConstructionSpeedMultiplier() - 1f)));
                int effectiveSlots = Mathf.Max(1, city.buildQueue.baseSlots + slotBonus);
                int activeSlots = Mathf.Min(city.buildQueue.activeOrders.Count, effectiveSlots);
                int completedThisTick = 0;

                for (int i = 0; i < activeSlots; i++)
                {
                    var order = city.buildQueue.activeOrders[i];
                    if (!MeetsPrerequisites(city, order))
                    {
                        continue;
                    }
                    if (!HasTileCapacity(city, order))
                    {
                        continue;
                    }
                    if (!IsUpgradeGateSatisfied(city, order))
                    {
                        sb.AppendLine($"  {city.displayName}: waiting for UpgradeToken ({order.buildingType} Lv{order.targetLevel})");
                        continue;
                    }
                    int tickReduction = Mathf.CeilToInt(secondsPerTick * officerBonuses.GetConstructionSpeedMultiplier());
                    order.secondsRemaining = Mathf.Max(0, order.secondsRemaining - tickReduction);
                    if (order.secondsRemaining == 0)
                    {
                        completedThisTick++;
                        city.SetBuildingLevel(order.buildingType, order.targetLevel);
                        _effectRuntime?.RebuildBuilding(city.cityId, order.buildingType);
                        GameEventHub.Publish(new BuildingUpgradedEvent
                        {
                            cityId = city.cityId,
                            buildingKey = order.buildingType,
                            fromLevel = Mathf.Max(0, order.targetLevel - 1),
                            toLevel = order.targetLevel
                        });
                        if (order.buildingType.Equals("TownHall", StringComparison.OrdinalIgnoreCase))
                        {
                            ApplyUnlockRules(city);
                        }
                        Debug.Log($"[Build] Done {city.displayName}:{order.buildingType} L{order.targetLevel}");
                        city.buildQueue.activeOrders.RemoveAt(i);
                        i--;
                        activeSlots = Mathf.Min(city.buildQueue.activeOrders.Count, effectiveSlots);
                    }
                }

                if (completedThisTick == 0 && city.buildQueue.activeOrders.Count == 0)
                {
                    sb.AppendLine($"  {city.displayName}: idle (no orders)");
                }
            }

            if (verboseLogging)
            {
                Debug.Log(sb.ToString());
            }
        }

        private bool IsUpgradeGateSatisfied(CityState city, BuildOrderState order)
        {
            if (order.blueprintGateSatisfied)
            {
                return true;
            }

            if (order.targetLevel < UpgradeTokenGateLevel)
            {
                return true;
            }

            var inventory = _state?.player?.tokenInventory;
            if (inventory == null)
            {
                Debug.Log($"[Build] Gate {city.displayName}:{order.buildingType} L{order.targetLevel} RequiresToken");
                return false;
            }

            if (!inventory.Consume(UpgradeTokenId, 1))
            {
                Debug.Log($"[Build] Gate {city.displayName}:{order.buildingType} L{order.targetLevel} RequiresToken");
                return false;
            }

            order.blueprintGateSatisfied = true;
            if (verboseLogging)
            {
                int fromLevel = Mathf.Max(0, order.targetLevel - 1);
                Debug.Log($"[Build] Start {city.displayName}:{order.buildingType} L{fromLevel}->{order.targetLevel} ConsumedToken");
            }
            return true;
        }

        private bool HasTileCapacity(CityState city, BuildOrderState order)
        {
            if (_state?.tileCaps == null)
            {
                return true;
            }

            int currentLevel = city.GetBuildingLevel(order.buildingType);
            if (currentLevel > 0)
            {
                return true;
            }

            if (order.targetLevel <= 0)
            {
                return true;
            }

            int townHallLevel = city.GetTownHallLevel();
            int cap = _state.tileCaps.GetCap(townHallLevel);
            if (cap <= 0)
            {
                return true;
            }

            int used = city.CountOccupiedTiles();
            if (used >= cap)
            {
                Debug.Log($"[Build] Locked TileCap TH={townHallLevel} cap={cap} used={used}");
                return false;
            }

            return true;
        }

        private bool MeetsPrerequisites(CityState city, BuildOrderState order)
        {
            if (_buildingCatalog == null || !_buildingCatalog.TryGet(order.buildingType, order.targetLevel, out var data))
            {
                return true;
            }

            for (int i = 0; i < data.requires.Count; i++)
            {
                var requirement = data.requires[i];
                if (string.IsNullOrEmpty(requirement.buildingType))
                {
                    continue;
                }

                int level = city.GetBuildingLevel(requirement.buildingType);
                if (level < requirement.minLevel)
                {
                    Debug.Log($"[Build] Locked {order.buildingType} Needs {requirement.buildingType}≥{requirement.minLevel}");
                    return false;
                }
            }

            return true;
        }

        private void ApplyUnlockRules(CityState city)
        {
            if (_state?.unlockRules == null)
            {
                return;
            }

            int thLevel = city.GetTownHallLevel();
            for (int i = 0; i < _state.unlockRules.rules.Count; i++)
            {
                var rule = _state.unlockRules.rules[i];
                if (thLevel < rule.townHallMin)
                {
                    continue;
                }

                if (!city.tags.Contains(rule.tag))
                {
                    continue;
                }

                if (string.IsNullOrEmpty(rule.unlock.slotType) || rule.unlock.count <= 0)
                {
                    continue;
                }

                city.slotUnlocks.TryGetValue(rule.unlock.slotType, out var current);
                if (current >= rule.unlock.count)
                {
                    continue;
                }

                int delta = rule.unlock.count - current;
                city.slotUnlocks[rule.unlock.slotType] = rule.unlock.count;
                Debug.Log($"[Unlock] {rule.unlock.slotType}Slot +{delta} (TH={thLevel})");
                GameEventHub.Publish(new TileCapChangedEvent
                {
                    cityId = city.cityId,
                    townHallLevel = thLevel,
                    cap = _state.tileCaps != null ? _state.tileCaps.GetCap(thLevel) : 0
                });
            }
        }
    }
}
