# Victory Command - City System

This module contains all ScriptableObject definitions and starting data for core city buildings in Victory Command.

## Structure
Victory Command — short project summary

This repository contains the Unity project for Victory Command, a prototype strategy/city system with editor tooling and ScriptableObject-driven game data.

Primary pointers
- Design docs and system blueprint: `docs/ARCHITECTURE.md` (canonical entrypoint for automated tools and contributors).
- Sanitized design (safe for automated ingestion): `docs/ARCHITECTURE_SANITIZED.md`.
- Module breakdown: `docs/modules/` (per-subsystem documents and data models).

Where to edit
- Gameplay code: `Assets/Scripts/`
- Editor tooling: `Assets/Editor/` (CityTileEditor and helpers)
- ScriptableObject data: `Assets/Scripts/Data/` and `Assets/Resources/`

If you plan to run automated analysis, point the tool at `docs/ARCHITECTURE.md` — it contains a TOC and links to machine-readable specs.
