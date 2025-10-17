using System.Collections.Generic;
using UnityEngine;

namespace IFC.Data
{
    [CreateAssetMenu(fileName = "CityData", menuName = "IFC/City/CityData", order = 0)]
    public class CityData : ScriptableObject
    {
        [Header("Core Info")]
        public string cityName = "New City";
        public int cityLevel = 1;
        public int maxLevel = 20;

        [Header("Building Slot Unlocks")]
        [Tooltip("Each level unlocks a set number of building slots.")]
        public List<int> buildingSlotsUnlockedByLevel = new List<int>();

        [Header("Current Buildings")]
        [Tooltip("Track buildings assigned to this city.")]
        public List<BuildingData> assignedBuildings = new List<BuildingData>();

        [Header("Tile Maps")]
        [Tooltip("Total tile slots unlocked based on city level.")]
        public int unlockedCityTiles = 0;

        [Tooltip("Main city building tiles")]
    public List<TileData> cityTiles = new List<TileData>(CityConstants.MAX_CITY_TILES);

    [Header("Editor Settings (persisted)")]
    [Tooltip("How many columns to display in the City Grid editor. Default from CityConstants.")]
    public int editorCityGridColumns = CityConstants.DefaultCityCols;

    [Tooltip("How many columns to display in the Resource Grid editor. Default from CityConstants.")]
    public int editorResourceGridColumns = CityConstants.DefaultResCols;

    [Tooltip("Last selected tile index in the editor (persisted).")]
    public int editorLastSelectedTile = -1;

        [Tooltip("Resource production fields")]
        public List<TileData> resourceTiles = new List<TileData>(CityConstants.MAX_RESOURCE_TILES);
    }
}
