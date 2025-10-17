using UnityEngine;

namespace IFC.Data
{
    /// <summary>
    /// Converts troops using Dark Energy into Ultra units.
    /// </summary>
    [CreateAssetMenu(fileName = "UltraConverterData", menuName = "IFC/Buildings/UltraConverter", order = 1)]
    public class UltraConverterData : ScriptableObject
    {
        [Header("Basic Info")]
        [SerializeField] private string _buildingName = "UltraConverter";
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