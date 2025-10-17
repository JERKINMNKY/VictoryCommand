using UnityEngine;

namespace IFC.Data
{
    [CreateAssetMenu(fileName = "WarehouseData", menuName = "IFC/Buildings/Warehouse", order = 1)]
    public class WarehouseData : ScriptableObject
    {
        [Header("Building Info")]
        [Tooltip("Protects resources from being plundered and increases storage capacity.")]
        public string buildingName = "Warehouse";
        public int maxLevel = 20;
        public int baseUpgradeTimeSeconds;
        public int baseCostFood;
        public int baseCostSteel;
        public int baseCostOil;
        public int baseCostRareMetal;
        public int baseCostGold;
        public int baseCostDarkEnergy;
    }
}
