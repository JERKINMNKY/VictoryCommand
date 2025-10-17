using System.Collections.Generic;
using UnityEngine;

namespace IFC.Data
{
    [CreateAssetMenu(fileName = "BuildingData", menuName = "IFC/Building Data", order = 1)]
    public class BuildingData : ScriptableObject
    {
        [SerializeField] private string _buildingName;
        [SerializeField] private int _level;
        [SerializeField] private int _upgradeTimeSeconds;
        [SerializeField] private List<ResourceCost> _costByResource;

        public string BuildingName => _buildingName;
        public int Level => _level;
        public int UpgradeTimeSeconds => _upgradeTimeSeconds;
        public IReadOnlyList<ResourceCost> CostByResource => _costByResource;
    }
}
