using System;
using IFC.Data;
using IFC.Systems.UI;
using UnityEngine;

namespace IFC.Systems
{
    /// <summary>
    /// Simple developer utilities for manipulating simulation state during tests/debugging.
    /// </summary>
    public static class DevActions
    {
        public static void GrantUpgradeToken(GameLoop loop, int amount)
        {
            var inventory = loop?.CurrentState?.player?.tokenInventory;
            if (inventory == null || amount <= 0)
            {
                Debug.LogWarning("[Dev] Invalid token grant request.");
                return;
            }

            inventory.Add(BuildQueueSystem.UpgradeTokenId, amount);
            Debug.Log($"[Dev] Granted {BuildQueueSystem.UpgradeTokenId} x{amount}");
            UIRefreshService.RefreshAll();
        }

        public static void GrantResource(GameLoop loop, string cityId, string resourceKey, int amount)
        {
            if (loop?.CurrentState == null || string.IsNullOrEmpty(cityId) || string.IsNullOrEmpty(resourceKey) || amount == 0)
            {
                Debug.LogWarning("[Dev] Invalid resource grant request.");
                return;
            }

            var city = loop.CurrentState.GetCityById(cityId);
            if (city == null)
            {
                Debug.LogWarning($"[Dev] City {cityId} not found.");
                return;
            }

            if (Enum.TryParse(resourceKey, out IFC.Data.ResourceType resourceType))
            {
                var stockpile = GameStateBuilder.FindStockpile(city, resourceType);
                if (stockpile == null)
                {
                    long hardCap = CityConstants.RESOURCE_HARD_CAP;
                    long initial = Math.Max(10000L, Math.Abs((long)amount));
                    initial = Math.Min(hardCap, initial);
                    city.stockpiles.Add(new ResourceStockpile
                    {
                        resourceType = resourceType,
                        amount = Math.Max(0, Math.Min(hardCap, (long)amount)),
                        baseCapacity = initial,
                        capacity = initial
                    });
                }
                else
                {
                    long delta = amount;
                    long hardCap = CityConstants.RESOURCE_HARD_CAP;
                    stockpile.baseCapacity = Math.Min(stockpile.baseCapacity, hardCap);
                    stockpile.capacity = Math.Min(stockpile.capacity, hardCap);
                    stockpile.amount = Math.Max(0, Math.Min(hardCap, stockpile.amount + delta));
                }

                Debug.Log($"[Dev] Granted {resourceKey} x{amount} to {city.displayName}");
            }
            else
            {
                loop.CurrentState?.player?.tokenInventory.Add(resourceKey, amount);
                Debug.Log($"[Dev] Added inventory item {resourceKey} x{amount}");
            }

            UIRefreshService.RefreshAll();
        }

        public static void PlaceBuilding(GameLoop loop, string cityId, string buildingKey)
        {
            if (loop?.BuildController == null)
            {
                Debug.LogWarning("[Dev] BuildController not initialised.");
                return;
            }

            if (loop.BuildController.TryPlaceBuilding(cityId, buildingKey, out var fail))
            {
                UIRefreshService.RefreshAll();
                return;
            }

            Debug.LogWarning($"[Dev] PlaceBuilding failed: {fail}");
        }

        public static void EnqueueUpgrade(GameLoop loop, string cityId, string buildingKey, int targetLevel)
        {
            if (loop?.BuildController == null)
            {
                Debug.LogWarning("[Dev] BuildController not initialised.");
                return;
            }

            if (loop.BuildController.TryEnqueueUpgrade(cityId, buildingKey, targetLevel, out var fail, out var eta))
            {
                Debug.Log($"[Dev] Enqueued upgrade ETA {eta:mm\\:ss}");
                UIRefreshService.RefreshAll();
                return;
            }

            Debug.LogWarning($"[Dev] EnqueueUpgrade failed: {fail}");
        }

        public static void AdvanceTicks(GameLoop loop, int ticks)
        {
            if (loop == null || ticks <= 0)
            {
                Debug.LogWarning("[Dev] Invalid tick advance request.");
                return;
            }

            for (int i = 0; i < ticks; i++)
            {
                loop.ExecuteTick();
            }

            Debug.Log($"[Dev] Advanced simulation by {ticks} tick(s).");
            UIRefreshService.RefreshAll();
        }

        public static void DumpBuildingIds(GameLoop loop)
        {
            if (loop == null)
            {
                Debug.LogWarning("[Dev] Loop is null.");
                return;
            }
            var keys = loop.BuildingCatalog?.GetAllKeys();
            if (keys == null)
            {
                Debug.LogWarning("[Dev] BuildingCatalog not initialised.");
                return;
            }
            Debug.Log("[Dev] Building IDs:\n - " + string.Join("\n - ", keys));
        }

        public static void DumpCitySummary(GameLoop loop)
        {
            if (loop?.CurrentState == null)
            {
                Debug.LogWarning("[Dev] No state to dump.");
                return;
            }
            foreach (var city in loop.CurrentState.cities)
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                sb.AppendLine($"[Dev] City {city.cityId} ({city.displayName})");
                for (int i = 0; i < city.buildings.Count; i++)
                {
                    var b = city.buildings[i];
                    sb.AppendLine($"  - {b.buildingKey}: L{b.level}");
                }
                Debug.Log(sb.ToString());
            }
        }
    }
}
