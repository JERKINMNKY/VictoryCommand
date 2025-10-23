using System.Collections.Generic;
using UnityEngine;

namespace IFC.Data
{
    public enum BuildingType
    {
        Core,
        Military,
        Resource,
        Logistics,
        Support,
        Research,
        Defense,
        Endgame,
        LateGame
    }

    [System.Serializable]
    public class TileData
    {
        public int tileIndex;
        public bool isUnlocked = false;
        public int unlockLevel = 1;
        public UnityEngine.Vector2Int gridPosition = UnityEngine.Vector2Int.zero;
        public BuildingData assignedBuilding;
    }

    [CreateAssetMenu(fileName = "BuildingData", menuName = "IFC/Building Data", order = 1)]
    public class BuildingData : ScriptableObject
    {
        [SerializeField] private string _buildingName;
        [SerializeField] private int _level;
        [SerializeField] private int _upgradeTimeSeconds;
        [SerializeField] private List<ResourceCost> _costByResource;

        [Header("Game Rules")]
        public BuildingType buildingType = BuildingType.Core;
        public bool isUniquePerCity = false;
        [Tooltip("Minimum city level required to unlock this building type")]
        public int unlockLevel = 1;
    [Tooltip("Maximum instances of this building per city. 0 = no limit.")]
    public int maxPerCity = 0;
        [Tooltip("Building requirements (type and minimum level) before this can be upgraded/placed.")]
        public List<BuildingRequirement> requires = new List<BuildingRequirement>();

        public string BuildingName => _buildingName;
        public int Level => _level;
        public int UpgradeTimeSeconds => _upgradeTimeSeconds;
        public IReadOnlyList<ResourceCost> CostByResource => _costByResource;
    }

    [System.Serializable]
    public class BuildingRequirement
    {
        public string buildingType = string.Empty;
        public int minLevel = 1;
    }
}
