using UnityEngine;

namespace IFC.Data
{
    /// <summary>
    /// Sets dispatch limits and number of march slots for troops.
    /// </summary>
    [CreateAssetMenu(fileName = "GeneralHQData", menuName = "IFC/Buildings/GeneralHQ", order = 1)]
    public class GeneralHQData : ScriptableObject
    {
        [Header("Basic Info")]
        [SerializeField] private string _buildingName = "GeneralHQ";
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