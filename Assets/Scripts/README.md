Assets/Scripts — Gameplay code

This folder contains gameplay code for Victory Command. Files may be split across subfolders:
- `Assets/Scripts/Data/` — Data model ScriptableObjects (BuildingData, CityData, ResourceCost)
- `Assets/Scripts/CitySystem/` — Core placement and validation logic (CityManager)

Notes:
- Many scripts are prototypes. Check for duplicate or legacy versions in `Assets/Scripts/Data` vs `Assets/ScriptableObjects/Buildings`.
- When modifying ScriptableObject fields, use `SerializedObject`/`SerializedProperty` in editor tools to ensure proper serialization and Undo support.