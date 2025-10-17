using UnityEngine;

namespace IFC.Data
{
    [CreateAssetMenu(fileName = "ResearchCenterData", menuName = "IFC/Buildings/ResearchCenter", order = 1)]
    public class ResearchCenterData : ScriptableObject
    {
        [Header("Building Info")]
        [Tooltip("Unlocks and upgrades technologies.")]
        public string buildingName = "ResearchCenter";
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
