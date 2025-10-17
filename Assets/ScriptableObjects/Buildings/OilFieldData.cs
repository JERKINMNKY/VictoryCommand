using UnityEngine;

namespace IFC.Data
{
    /// <summary>
    /// Produces Oil used for air/naval units and upkeep.
    /// </summary>
    [CreateAssetMenu(fileName = "OilFieldData", menuName = "IFC/Buildings/OilField", order = 1)]
    public class OilFieldData : ScriptableObject
    {
        [Header("Basic Info")]
        [SerializeField] private string _buildingName = "OilField";
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