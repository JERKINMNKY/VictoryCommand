using UnityEngine;

namespace IFC.Data
{
    [CreateAssetMenu(fileName = "OfficerData", menuName = "IFC/Officer Data", order = 2)]
    public class OfficerData : ScriptableObject
    {
        public enum StatType { Politics, Military, Knowledge }

        [SerializeField] private string _officerName;
        [SerializeField] private int _starLevel;
        [SerializeField] private float _loyalty;
        [SerializeField] private int _gearSlots = 6;

        [SerializeField] private int _politics;
        [SerializeField] private int _military;
        [SerializeField] private int _knowledge;

        public string OfficerName => _officerName;
        public int StarLevel => _starLevel;
        public float Loyalty => _loyalty;
        public int GearSlots => _gearSlots;

        public int Politics => _politics;
        public int Military => _military;
        public int Knowledge => _knowledge;
    }
}
