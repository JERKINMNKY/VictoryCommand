Assets/Editor — Editor tooling

This folder contains custom Unity Editor windows and utilities used for authoring city data.

Key files:
- CityTileEditor.cs — Visual editor for CityData. Supports:
  - Visual grid display of tiles
  - Auto-fill starter layout
  - Auto-assign grid positions
  - Grouped Undo for major operations
  - Drag-and-drop assign with validation via CityManager
  - Diagnostics showing runtime vs serialized assignment

Notes:
- Editor scripts are in `Assets/Editor` and are compiled into the Unity editor assembly. They will not be included in builds.
- You can open the City Tile Editor via `IFC → City Tile Editor` in the Unity menu.

Known issues:
- Early versions created temporary sample assets for testing; these should be removed before committing main branches.
- If you see console errors about assets under `Assets/Assets/` that indicates a nested/corrupt assets folder — move it out and reimport.

Testing tips:
- Use the sample building creator (bottom of the editor) to create test `BuildingData` assets if none exist.