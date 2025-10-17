# Architecture Overview — Victory Command

This folder contains a structured, machine- and human-readable breakdown of the game's core systems. The content is adapted from an uploaded reference (originally labeled Readme2.0.txt) and reorganized into modular documents to make automated analysis (Codex, static analyzers, and humans) straightforward.

Note on provenance: sections that reference existing games or public resources are kept as "inspired by / reconstructed from public sources". This document is intended as a design blueprint and not an exact copy of any proprietary material.

Files in this directory:

- `ARCHITECTURE_SOURCE.txt` — raw copy of the uploaded document for traceability.
- `modules/` — split module documents. Key files:
  - `01_city_system.md`
  - `02_resources.md`
  - `03_buildings.md`
  - `04_research.md`
  - `05_units_and_conversion.md`
  - `06_combat.md`
  - `07_officers_and_gear.md`
  - `08_economy_and_monetization.md`
  - `09_dispatch_and_logistics.md`
  - `10_missions_events.md`
  - `11_alliance_social.md`
  - `12_roadmap.md`
  - `entities.md` (data model JSON)
- `diagrams/uml_spec.puml` — PlantUML source for the system class diagram.
- `specs/game_spec.yaml` — compact YAML version of the entity spec.
- `specs/game_spec.json` — compact JSON version of the entity spec.

How to use this as the root guidance for automated tools:

1. Read `ARCHITECTURE.md` (this file) to understand organization.
2. Use `ARCHITECTURE_SOURCE.txt` when you need the raw uploaded text.
3. For specific subsystem logic, open the corresponding file in `modules/`.

If you intend Codex or other AI tools to process the repository overnight, point them at `docs/ARCHITECTURE.md` as the canonical design summary.

---

Next steps and governance:

- This repo includes a TODO (`docs/todo.md`) describing intended refinements and IP handling decisions. See `docs/modules/12_roadmap.md` for phased implementation guidance.

Sanitized and raw sources:
- `ARCHITECTURE_SANITIZED.md` — a cleaned, AI-friendly version of the design (recommended for automated ingestion).
- `ARCHITECTURE_SOURCE.txt` — the raw uploaded text (for maintainers and legal review).
