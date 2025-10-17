using UnityEngine;

namespace IFC.Data
{
    /// <summary>
    /// Primary city control. Required to unlock higher building caps and train officers.
    /// </summary>
    [CreateAssetMenu(fileName = "StaffHQData", menuName = "IFC/Buildings/StaffHQ", order = 1)]
    public class StaffHQData : ScriptableObject
    {
        [Header("Basic Info")]
        [SerializeField] private string _buildingName = "StaffHQ";
        [SerializeField] private int _level;
        [SerializeField] private int _maxLevel = 20;
        [SerializeField] private int _upgradeTimeSeconds;

        [Header("Cost Per Resource Type")]
        [SerializeField] private ResourceCost[] _costByResource;

        public string BuildingName => _buildingName;
        public int Level => _level;
        public int MaxLevel => _maxLevel;
        public int UpgradeTimeSeconds => _upgradeTimeSeconds;
        public ResourceCost[] CostByResource => _costByResource;
    }
}