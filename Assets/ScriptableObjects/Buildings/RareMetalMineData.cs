using UnityEngine;

namespace IFC.Data
{
    /// <summary>
    /// Produces Rare Metal for Ultra units and tech.
    /// </summary>
    [CreateAssetMenu(fileName = "RareMetalMineData", menuName = "IFC/Buildings/RareMetalMine", order = 1)]
    public class RareMetalMineData : ScriptableObject
    {
        [Header("Basic Info")]
        [SerializeField] private string _buildingName = "RareMetalMine";
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