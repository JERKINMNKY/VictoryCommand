# Victory Command Repository Map

Layer focus: Core Simulation | Life Domains: Autonomy, Security, Logistics

## Top-Level
- `Assets/` — Unity content and scripts for gameplay, ScriptableObjects, and editor tools.
- `content/seed/` — JSON seeds for the reconstructed city loop.
- `docs/` — Architecture notes, system status, schemas, and roadmap artifacts.
- `tools/` — Offline utilities (e.g., tick simulator).

## Key Gameplay Scripts
- `Assets/Scripts/Data/` — Core data ScriptableObjects (CityData, BuildingData, Resource enums).
- `Assets/Scripts/Systems/`
  - Legacy: `ResourceTickService` (single city resource pump).
  - New core loop: `FoundationState`, `GameLoop`, `ResourceSystem`, `BuildQueueSystem`, `TrainingSystem`, `DefenseSystem`, `AutoTransportSystem`.
- `Assets/Scripts/Prototypes/` — Early data models for generated code (PlayerModel, CityModel).

## Resources & ScriptableObjects
- `Assets/Resources/Buildings/` — Building definition ScriptableObjects for each structure.
- `Assets/Resources/Cities/CityData.asset` — Example city ScriptableObject with tile layout.

## Editor & Tools
- `Assets/Editor/CityTileEditor.cs` — Custom inspector tooling for tile assignments.
- `tools/tick_simulator.py` — Python harness to replay production ticks.

## Documentation Highlights
- `docs/ARCHITECTURE.md` — Detailed architecture trace (source of truth).
- `docs/specs/game_spec.full.schema.json` — JSON schema for reconstructed systems.
- `docs/SYSTEM_STATUS.md` — Live matrix of system coverage.
- `docs/NEXT_STEPS.md` — Prioritised TODOs for future iterations.
