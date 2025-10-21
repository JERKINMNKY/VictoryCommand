# Next Steps

Layer focus: Core Simulation | Life Domains: Autonomy, Security, Logistics

1. **Officer Effects** — model mayor and wall officer bonuses impacting morale, build slots, and production.
2. **Queue Persistence** — persist build/training progress across sessions with JSON save/export.
3. **Combat & Raids** — prototype defensive battle resolution that consumes wall HP and troops.
4. **Economy Balancing** — replace placeholder production rates with authoritative War 2 Victory tables.
5. **UI Hooks** — expose tick summaries in Unity UI to visualize the loop without relying on console logs.

---

## Implementation Blueprint (Codex 2025-02-23)

### Officer Integration
- Extend `OfficerData` with role tags (e.g., Mayor, Defense, Logistics) and bonus curves (flat percentage + per-star scaling).
- Add officer assignments to `CityState` (mayor, defense, logistics slots) and plumb buffs into existing systems:
  - `BuildQueueSystem` — adjust `activeSlots` and per-order `secondsRemaining` by construction speed buffs.
  - `ResourceSystem` — apply additive production modifiers and morale clamps before capacity checks.
  - `DefenseSystem` — increase `repairPerTick`, reduce `moraleDamageAmount`, and gate fortification burn.
- Author officer assignment change events so future UI/server layers can react without polling ticks.

### Persistence Layer
- Introduce `SaveGameManager` MonoBehaviour with explicit `Save()` / `Load()` APIs writing `GameState` to `PersistentDataPath`.
- Serialize incremental progress (`secondsRemaining`, `resourcesCommitted`, stockpile amounts, morale) using `JsonUtility`.
- Register save hooks on `Application.quitting` and manual triggers to guarantee queue progress survives restarts.
- Version saves with a `schemaVersion` field and migration callbacks to keep compatibility as the data model evolves.

### World Map & Campaign Scaffolding
- Create `WorldState` aggregate holding `TerritoryState`, `FrontState`, and `CampaignProgress` lists.
- Define a `MapSystem` tick that resolves territory resource ticks, NPC spawns, and dispatch travel times.
- Synchronize `CityState.outgoingDispatches` with world travel timers so logistics/raids share a single transport model.
- Prepare ScriptableObject templates for territories, enemy presets, and campaign reward tracks.

### Quest & Mission Framework
- Add `MissionState` collection to `GameState` plus `MissionDefinition` assets describing triggers and rewards.
- Implement an event bus (e.g., `GameEventHub`) where systems publish events (`BuildingUpgraded`, `UnitTrained`, `BattleResolved`).
- Build `MissionSystem` that subscribes to events, tracks progress, and grants rewards upon completion.

### Battle Simulation Skeleton
- Create `BattleRequest` DTOs (attacker units/officers, defender city/terrain) and a `BattleSystem` service.
- Resolve combat in deterministic phases: formation modifiers, officer buffs, unit matchup, wall absorption, loot distribution.
- Emit `BattleReport` summaries consumable by missions, UI, and persistence.
- Integrate with `DefenseSystem` to feed wall HP changes and with `TrainingSystem`/`Garrison` for casualties.

### UI & Tooling Alignment
- Extend `CityTileEditor` to preview officer bonuses and tile effects pulled from the new data.
- Replace console-only tick feedback with lightweight UI panels (city summary, mission tracker, campaign status).
- Instrument `GameLoop` with profiler markers to help balance tick cost as systems expand.
