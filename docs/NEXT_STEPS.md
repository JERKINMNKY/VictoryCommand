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

### Upgrade Tokens (Levels 10–20)
- Introduce a shared `InventoryState` that tracks resources and token stacks (`UpgradeToken`, `bp.steel_plant`, etc.).
- Update seed data so each city (or the global player) owns initial token balances for migration testing.
- Retrofit `BuildQueueSystem`:
  - Levels 1–9: resource-only.
  - Levels 10–20: require =1 `UpgradeToken` before queueing; gate log format `[Build] Gate {city}:{building} L{to} RequiresToken`.
  - Consume token immediately on start, emit logs: `[Build] Start {city}:{building} L{from}->{to} ConsumedToken` and `[Build] Done {city}:{building} L{to}`.
- Provide `DevActions` helpers (CLI/editor) for granting tokens, enqueueing upgrades, and advancing ticks for integration tests.
- Add EditMode coverage: gated success/failure (L9→L10), gated consumption (L12→L13), non-gated flow (L5→L6), token decrement assertions.

### Building Functionality Framework
- Define `BuildingFunctionType` enum plus strategy handlers (`IBuildingFunction`) for production, training, population growth, trading, capacity buffs, etc.
- Extend city/building data to reference function type + parameters per level (e.g., per-tick yields, caps, unlock tables).
- Tick orchestration:
  - Evaluate building functions after resource production but before transport/auto-logistics.
  - Emit diagnostics such as `[Pop] House@Alpha +4 population (L3)`.
- Tooling:
  - Debug inspector window listing active building functions and current outputs.
  - Simulation controls to trigger `Build → Tick → Effect` pipelines manually.
- Reference War2Victory decompiled caches to align naming, cadence, and late-game behavior (e.g., Arms Plant queue semantics).
- Provide EditMode tests per function archetype (SteelPlant production, Arms Plant training boost, House population growth, Market resource swap).

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

### Starter Profile & TH>10 Scaffolding
- Define `StartProfile` data (JSON or SO) with new-player city composition, starting inventory, tile caps per Town Hall, rookie missions, and city tags (e.g., `Capital`).
- Allow `GameLoop` to load a profile via inspector/seed override and build runtime state through `GameStateBuilder`.
- Enforce tile caps (editor + runtime) using profile-driven lookup; log `[Build] Locked TileCap TH={th} cap={cap} used={used}` when exceeded.
- Extend `BuildingData` with prerequisite entries (`type`, `minLevel`) validated in editor/runtime; log `[Build] Locked {buildingKey} Needs {reqType}={level}` on failure.
- Hook rookie missions into `MissionSystem` via published events (`BuildingConstructed`, `BuildingUpgraded`, etc.) and grant rewards (including `UpgradeToken`).
- Add city-tag-based slot unlock scaffolding for Town Hall >10 (e.g., `UltraEnergy` slots) and log `[Unlock] UltraEnergySlot +{delta} (TH={level})` when thresholds are hit.
- Provide inspector `DevActionsBehaviour` shortcuts (GrantResource, GrantUpgradeToken, PlaceBuilding, EnqueueUpgrade, AdvanceTicks) so designers can trigger logs without UI.
- Auto-spawn UGUI MVP via `CityUIBootstrap` (top bar, four building cards, tile footer, toast overlay) and refresh after each tick.
- Document quick verification: add `DevActionsBehaviour`, right-click context to hit caps/prereqs, run scene to observe UI upgrades and mission rewards.
- War2Victory reference: consulted `War2V_Decompiled/assets/cache/extracted/gamecaches/main/17297588590134.json` (tile unlock cadence) and related cache entries for blueprint/token gating behavior.
Recent Changes (October 2025)
- Start Profile workflow
  - Added importer to create `StartProfileAsset` from JSON (Tools → Profiles → Import Start Profile). See files under `Assets/Editor/StartProfileImporter.cs` and `Assets/Scripts/Systems/Profiles/StartProfile*`.
  - Added editor drawer to select building IDs from the data catalog instead of free text when editing a start profile (`StartProfileBuildingLevelDrawer`).
  - Default new‑player scaffold applied when data is sparse: Capital city with TownHall 1, CommandHQ 1, StaffBureau 1, commander as mayor, baseline stockpile, and tile caps 1–10.
- UI boot reliability
  - Defer UI bootstrap until after game state + catalogs load, preventing early “missing building” warnings and ensuring canvases spawn predictably.
  - Unity 2023 font fallback: use `LegacyRuntime.ttf`/Arial OS when built‑in Arial is unavailable.
- Data resilience
  - JSON parsing now trims BOM/zero‑width characters to avoid profile parse failures.
- Content validation
  - Tools → Validate Content IDs checks profile/seed building IDs against `content/data/buildings.json` and flags legacy identifiers.

Short Verification Checklist
- Create/import a `StartProfileAsset` and assign it on `GameLoop`.
- Press Play and confirm the UI canvas appears with top bar, 4 building cards, tile footer, and tick logs.
- Use DevActionsBehaviour context menu to grant resources/tokens, place buildings, enqueue upgrades, and advance ticks.
