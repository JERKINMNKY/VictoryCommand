using UnityEngine;

namespace IFC.Data
{
    /// <summary>
    /// Primary land troop factory. Enables Ultra Conversions.
    /// </summary>
    [CreateAssetMenu(fileName = "ArmsPlantData", menuName = "IFC/Buildings/ArmsPlant", order = 1)]
    public class ArmsPlantData : ScriptableObject
    {
        [Header("Basic Info")]
        [SerializeField] private string _buildingName = "ArmsPlant";
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