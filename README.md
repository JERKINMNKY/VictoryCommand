# Victory Command - City System

This module contains all ScriptableObject definitions and starting data for core city buildings in Victory Command.

## Structure
- `Assets/Scripts/Data/` — ScriptableObject definitions for each building
- `Assets/Resources/ScriptableObjects/Buildings/` — Unity asset folder (empty for now)
- `Assets/Scenes/City.unity` — Placeholder for main city scene

Each SO defines:
- Max Level
- Base cost per resource type
- Description and name

## Next Steps
- Open Unity and load this folder
- Create instances of each `BuildingData` via `Assets → Create → IFC → Buildings → [Type]`
- Drag them into the scene or link to building prefabs
