using UnityEngine;

namespace IFC.Systems
{
    [DisallowMultipleComponent]
    public class DevActionsBehaviour : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameLoop gameLoop;

        [Header("Defaults")]
        [SerializeField] private string cityId = "Capital";
        [SerializeField] private string buildingKey = "TownHall";
        [SerializeField] private string resourceKey = "Food";
        [SerializeField] private int amount = 1;
        [SerializeField] private int upgradeTargetLevel = 2;
        [SerializeField] private int advanceTicksCount = 1;

        private GameLoop Loop => gameLoop != null ? gameLoop : GetComponent<GameLoop>();

        [ContextMenu("Dev/Grant Upgrade Token")]
        private void ContextGrantUpgradeToken()
        {
            DevActions.GrantUpgradeToken(Loop, amount);
        }

        [ContextMenu("Dev/Grant Resource")]
        private void ContextGrantResource()
        {
            DevActions.GrantResource(Loop, cityId, resourceKey, amount);
        }

        [ContextMenu("Dev/Place Building")]
        private void ContextPlaceBuilding()
        {
            DevActions.PlaceBuilding(Loop, cityId, buildingKey);
        }

        [ContextMenu("Dev/Enqueue Upgrade")]
        private void ContextEnqueueUpgrade()
        {
            DevActions.EnqueueUpgrade(Loop, cityId, buildingKey, upgradeTargetLevel);
        }

        [ContextMenu("Dev/Advance Ticks")]
        private void ContextAdvanceTicks()
        {
            DevActions.AdvanceTicks(Loop, advanceTicksCount);
        }
    }
}
