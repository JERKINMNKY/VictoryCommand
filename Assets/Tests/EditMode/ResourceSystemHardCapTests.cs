using IFC.Data;
using NUnit.Framework;
using UnityEngine;

namespace IFC.Systems.Tests
{
    public class ResourceSystemHardCapTests
    {
        [Test]
        public void ProductionDoesNotExceedHardCap()
        {
            var go = new GameObject("ResourceSystem_Test");
            var system = go.AddComponent<ResourceSystem>();

            var state = new GameState();
            var city = new CityState
            {
                cityId = "city",
                displayName = "Test City",
                morale = 1f
            };

            city.stockpiles.Add(new ResourceStockpile
            {
                resourceType = ResourceType.Food,
                amount = CityConstants.RESOURCE_HARD_CAP - 500,
                baseCapacity = CityConstants.RESOURCE_HARD_CAP,
                capacity = CityConstants.RESOURCE_HARD_CAP
            });

            city.production.Add(new ProductionFieldState
            {
                resourceType = ResourceType.Food,
                fields = 10_000,
                outputPerField = 10_000
            });

            state.cities.Add(city);
            state.player = new PlayerState();
            state.player.EnsureConsistency();

            system.Initialize(state);
            system.ProcessTick(60);

            Assert.AreEqual(CityConstants.RESOURCE_HARD_CAP, city.stockpiles[0].amount);

            Object.DestroyImmediate(go);
        }
    }
}
