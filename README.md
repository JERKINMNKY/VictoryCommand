# Victory Command - City System

This module now ships with a playable core simulation loop that mirrors the War 2 Victory production + logistics cadence. The focus is on Core Simulation for the Autonomy, Security, and Logistics life domains within Project Atlas.

## Structure
Victory Command — short project summary

This repository contains the Unity project for Victory Command, a prototype strategy/city system with editor tooling and ScriptableObject-driven game data.

Primary pointers
- Design docs and system blueprint: `docs/ARCHITECTURE.md` (canonical entrypoint for automated tools and contributors).
- Sanitized design (safe for automated ingestion): `docs/ARCHITECTURE_SANITIZED.md`.
- Repository overview: `docs/REPO_MAP.md` and `docs/SYSTEM_STATUS.md` for current system coverage.
- JSON schema + placeholder data: `docs/specs/game_spec.full.schema.json` and `content/seed/game_state.json`.

Where to edit
- Gameplay code: `Assets/Scripts/`
- Editor tooling: `Assets/Editor/` (CityTileEditor and helpers)
- ScriptableObject data: `Assets/Scripts/Data/` and `Assets/Resources/`

If you plan to run automated analysis, point the tool at `docs/ARCHITECTURE.md` — it contains a TOC and links to machine-readable specs.

## Quickstart — Run a Prototype Tick

1. Open `Assets/Scenes/SampleScene.unity` in Unity 2021+.
2. Add an empty GameObject to the scene and attach `GameLoop` (it auto-spawns the required systems).
3. Enter Play Mode. The loop loads `content/seed/game_state.json` and executes ticks every `secondsPerTick` (default 60 seconds of sim time).
4. Watch the Console for logs from `BuildQueueSystem`, `ResourceSystem`, `TrainingSystem`, `DefenseSystem`, and `AutoTransportSystem`.

To iterate offline, run `python tools/tick_simulator.py` to replay a condensed tick summary using the same seed data.
