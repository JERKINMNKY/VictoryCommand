using UnityEngine;

namespace IFC.Data
{
    /// <summary>
    /// Provides population and generates gold based on tax rate.
    /// </summary>
    [CreateAssetMenu(fileName = "HouseData", menuName = "IFC/Buildings/House", order = 1)]
    public class HouseData : ScriptableObject
    {
        [Header("Basic Info")]
        [SerializeField] private string _buildingName = "House";
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