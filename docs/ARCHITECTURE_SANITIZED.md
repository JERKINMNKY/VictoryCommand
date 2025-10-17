# Architecture (Sanitized) — Victory Command

This document is a sanitized, AI-friendly version of the architecture notes originally provided by the project maintainer. All explicit references to third-party games, trademarks, or site-specific URLs have been removed or replaced with neutral phrasing such as "existing SLG examples" or "public sources".

Purpose
- Provide a safe, canonical design blueprint suitable for automated ingestion (Codex, static analyzers) and public distribution.

High-level systems covered (summary)
- City / Base System — city metadata, tiles, specialization, relocation rules.
- Resource Production & Logistics — resource types, field production, warehouse safe capacity, dispatch.
- Building Construction & Upgrades — building types, level/pre-reqs, blueprint gating, cost/time scaling.
- Research / Tech System — tech branches, queues, shared/alliance tech options.
- Troop Training & Conversion — unit factories, conversion to advanced units, upkeep.
- Combat — battle engine choices, unit stats, counters, loot, battle phases.
- Officers / Gear / Skills — hero/officer systems, gear rarities, skills and set synergies.
- Upkeep, Morale, Desertion — upkeep mechanics, morale effects, desertion rules.
- City Defense & Walls — wall mechanics, garrison, siege phases.
- Alliance / Social — alliance research, buffs, wars, shared resources.
- Missions, Events, Economy, Dispatch, Relocation — progression, monetization, transport rules, and relocation mechanics.

How to use this file
- Use `docs/ARCHITECTURE_SANITIZED.md` as the primary document for automated tools.
- The verbatim original is preserved in `docs/ARCHITECTURE_SOURCE.txt` for maintainers (not recommended for public ingestion).

Notes for maintainers
- If you need to expose source attributions or revert to the verbatim text for legal review, consult `ARCHITECTURE_SOURCE.txt` in `docs/`.
- Keep this sanitized file up-to-date when you evolve the design to ensure AI ingestion remains safe.
