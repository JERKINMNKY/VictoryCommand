using UnityEngine;

namespace IFC.Data
{
    [CreateAssetMenu(fileName = "HouseData", menuName = "IFC/Buildings/House", order = 1)]
    public class HouseData : ScriptableObject
    {
        [Header("Building Info")]
        [Tooltip("Provides population for labor and tax revenue.")]
        public string buildingName = "House";
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
