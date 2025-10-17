using UnityEngine;

namespace IFC.Data
{
    [CreateAssetMenu(fileName = "StaffHQData", menuName = "IFC/Buildings/StaffHQ", order = 1)]
    public class StaffHQData : ScriptableObject
    {
        [Header("Building Info")]
        [Tooltip("Primary city control. Required to unlock higher building caps and train officers.")]
        public string buildingName = "StaffHQ";
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
