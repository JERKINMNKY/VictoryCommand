using UnityEngine;

namespace IFC.Data
{
    /// <summary>
    /// Used to recruit officers. Higher levels increase chance of top-tier officers.
    /// </summary>
    [CreateAssetMenu(fileName = "MilitaryInstituteData", menuName = "IFC/Buildings/MilitaryInstitute", order = 1)]
    public class MilitaryInstituteData : ScriptableObject
    {
        [Header("Basic Info")]
        [SerializeField] private string _buildingName = "MilitaryInstitute";
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