
using UnityEngine;

namespace IFC.Data
{
    [CreateAssetMenu(fileName = "CityLevelData", menuName = "IFC/City/Level Data", order = 1)]
    public class CityLevelData : ScriptableObject
    {
        public int level;
        public int unlockedBuildingSlots;
        public int maxResourceCap;
    }
}
