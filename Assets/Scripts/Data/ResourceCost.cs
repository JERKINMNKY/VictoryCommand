using UnityEngine;

namespace IFC.Data
{
    /// <summary>
    /// Resource type and amount pair used for building costs.
    /// </summary>
    [System.Serializable]
    public struct ResourceCost
    {
        public ResourceType resourceType;
        public int amount;
    }

    /// <summary>
    /// Enum representing all supported resource types.
    /// </summary>
    public enum ResourceType
    {
        Food,
        Steel,
        Oil,
        RareMetal,
        Gold,
        DarkEnergy
    }
}
