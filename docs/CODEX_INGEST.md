# Codex ingestion guide

This file documents the recommended order and hints for automated ingestion (Codex or similar) so generated code aligns with the project's design intent.

Recommended ingestion order (strict):

1. `docs/ARCHITECTURE_SANITIZED.md` — Read this first. It is the canonical, safe, human-level summary.
2. `docs/ARCHITECTURE.md` — Index and links to modules and specs.
3. `docs/modules/*` — Read each subsystem document. Priority order: `01_city_system.md`, `02_resources.md`, `03_buildings.md`, `05_units_and_conversion.md`, `06_combat.md`, `07_officers_and_gear.md`, then the rest.
4. `docs/specs/game_spec.yaml` and `docs/specs/game_spec.json` — Load these as the canonical data contract. Validate against JSON Schema if present.
5. `docs/specs/*` (future) — JSON Schema and generated models (TypeScript, Python dataclasses).
6. `docs/samples/*` — Load sample seeds to get initial values and balancing hints.
7. `docs/diagrams/*` — PlantUML diagrams for system structure and relationships.
8. `docs/ARCHITECTURE_SOURCE.txt` — Only if you need verbatim provenance; treat as non-authoritative for code generation.

Parsing hints & priorities:
- Prefer `game_spec.yaml` types and field names as the ground truth for code generation.
- If the YAML and JSON differ, prefer the YAML (more likely to be maintained manually).
- Use `ARCHITECTURE_SANITIZED.md` for intent and module interactions.
- Use `entities.md` to map entity names to filenames and expected locations (CityData → `Assets/Scripts/Data/CityData.cs`).

Output guidelines for Codex-generated code
- Generate clear, typed DTOs matching `game_spec` (TypeScript / C# / Python as requested).
- Add small unit tests (happy path) that validate schema adherence and basic behavior (e.g., CanPlaceBuilding returns true/false per sample seed).
- Provide a small runner script (e.g., `tools/tick_simulator.py`) to demonstrate the tick loop; use sample seeds.

Notes
- Do not ingest `ARCHITECTURE_SOURCE.txt` unless explicitly required; it contains verbatim external references and is for maintainers only.
- If you generate code for Unity / C#, prefer ScriptableObject shape for `BuildingData`/`CityData` DTOs and provide editor-friendly attributes where appropriate.
