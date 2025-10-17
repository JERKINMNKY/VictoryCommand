Assets/Scripts/Data — ScriptableObject data models

This folder contains canonical data models for the city system:

- `BuildingData.cs` — Defines building metadata: name, level, costs, unlock rules, building type, maxPerCity, etc.
- `CityData.cs` — City asset that records unlocked tiles, lists of `TileData` for city and resource tiles, and editor persistence fields.
- `CityConstants.cs` — Constants used by the editor and runtime (defaults for grid sizes, max tiles)
- `ResourceCost.cs` — ResourceType enum and resource cost struct.

Guidance:
- Prefer one canonical `BuildingData` class and avoid duplicate definitions. If duplicates exist, remove or namespace legacy versions.
- When creating or editing CityData, use the City Tile Editor window for safety and Undo support.