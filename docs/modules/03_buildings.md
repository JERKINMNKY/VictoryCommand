# Building Catalogue & Upgrade System

This module defines the authoritative building roster for *Victory Command*.  
It replaces earlier placeholder notes and introduces a full, data-first catalogue that can be wired straight into runtime systems.

---

## 0. Design Goals

* **Data-driven** – all costs, times, and effects stem from JSON/ScriptableObject data, never hardcoded.
* **System-aware** – each building exists to unlock or reinforce a specific mechanic in the loop (economy, logistics, research, warfare, late game).
* **Extensible** – schema versioning and documented formulae allow rebalance without code churn.
* **Fictionally sound** – some structures are creative inventions to cover missing mechanics (e.g., Dark Energy). Anything speculative is marked with ⚠️.

---

## 1. Global Formulas & Conventions

| Symbol | Description | Formula / Notes |
| --- | --- | --- |
| `schemaVersion` | Data schema version | `1` (increment when the JSON/ScriptableObject contract changes) |
| `baseCost` | Cost at level 1 | Stored per resource (Food, Steel, Oil, RareMetal, DarkEnergy) |
| `baseTime` | Build time at level 1 | Seconds |
| `costCurve` | Cost scaling | `cost(level) = baseCost × level^1.25` |
| `timeCurve` | Time scaling | `time(level) = baseTime × 1.3^(level-1)` |
| `tileUse` | Tiles consumed | Default `1`, otherwise listed explicitly |
| `uniquePerCity` | Whether only one instance is allowed | Boolean |
| `prerequisites` | Building-level requirements | List of `{ buildingId, minLevel }` |
| `townHallRequired` | Minimum Town Hall level to construct | Integer |
| `effects` | Gameplay modifiers applied per level | Attached to `StatModifierRegistry` or slot unlock tables |

All values can be overridden on a per-level basis when the generic formula is insufficient. Such overrides are tagged with `supportsOverrides = true` in the JSON definition.

---

## 2. Master Catalogue (Overview)

| Building Id | Display Name | Category | Max Level | Unique | Tiles | Core Unlock | Primary Effect |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `TownHall` | Town Hall | Core Command | 20 | yes | 4 | Always | Governs city level, tile cap, unlock routing |
| `CommandHQ` | Command HQ | Core Command | 20 | yes | 2 | TownHall ≥2 | March slots, transport queues |
| `StaffBureau` | Staff Bureau | Core Command | 15 | yes | 1 | TownHall ≥3 | Officer cap, mayor bonuses |
| `LogisticsOffice` | Logistics Office | Core Command | 15 | yes | 1 | TownHall ≥4, CommandHQ ≥3 | Dispatch speed, transport capacity |
| `OfficerQuarters` | Officer Quarters | Core Command | 10 | no | 1 | StaffBureau ≥3 | Officer housing, morale floor |
| `Warehouse` | Storage Vault | Economy | 20 | no | 1 | TownHall ≥2 | Resource safe cap per resource |
| `Farm` | Hydroponic Farm | Economy | 20 | no | 1 | TownHall ≥2 | Food production fields |
| `SteelFoundry` | Steel Foundry | Economy | 20 | no | 1 | TownHall ≥3 | Steel production |
| `OilRefinery` | Oil Refinery | Economy | 20 | no | 1 | TownHall ≥4 | Oil production |
| `RareMine` | Rare Metal Mine | Economy | 20 | no | 1 | TownHall ≥5, ResearchLab ≥2 | Rare metal production |
| `PowerPlant` | Power Plant | Economy | 20 | no | 1 | TownHall ≥4 | Energy multiplier for advanced buildings |
| `TradePort` | Trade Port | Economy | 15 | no | 1 | TownHall ≥4 | Resource exchange (market) |
| `SupplyDepot` | Supply Depot | Logistics | 20 | no | 1 | TownHall ≥3 | Increases storage + dispatch payload |
| `TransportHub` | Transport Hub | Logistics | 15 | yes | 1 | CommandHQ ≥5 | Auto-transport slots + speed |
| `AerialRelay` | Aerial Relay | Logistics | 10 | yes | 1 | Airfield ≥2 | Reduces air dispatch cooldowns |
| `Barracks` | Barracks | Military Support | 20 | no | 1 | TownHall ≥3 | Infantry queue + training speed buff |
| `ArmsPlant` | Arms Plant | Military Production | 20 | no | 1 | Barracks ≥3 | Infantry production & conversions |
| `VehicleFactory` | Vehicle Factory | Military Production | 20 | no | 1 | ArmsPlant ≥5, PowerPlant ≥4 | Ground vehicle production |
| `Airfield` | Airfield | Military Production | 20 | no | 2 | TownHall ≥6, CommandHQ ≥6 | Air unit queues |
| `NavalYard` | Naval Yard | Military Production | 20 | no | 3 (coastal) | TownHall ≥7, TradePort ≥4 | Naval unit queues |
| `MissileSilo` | Missile Silo | Military Production | 15 | yes | 2 | ResearchLab ≥5, PowerPlant ≥7 | Missile/rocket unit production |
| `DroneLab` | Drone Lab | Military Production | 15 | yes | 1 | ResearchLab ≥6, Airfield ≥8 | Advanced drone units ⚠️ |
| `Workshop` | Workshop | Military Support | 20 | no | 1 | VehicleFactory ≥4 | Training speed + upkeep reductions |
| `Academy` | Officer Academy | Military Support | 15 | yes | 1 | StaffBureau ≥5 | Officer XP training |
| `Hospital` | Field Hospital | Military Support | 15 | yes | 1 | Barracks ≥6 | Casualty recovery, morale buffer |
| `CommandTower` | Command Tower | Military Support | 10 | yes | 1 | CommandHQ ≥8 | Global queue management |
| `ResearchLab` | Research Lab | Research & Tech | 20 | yes | 1 | TownHall ≥4 | Unlocks research tree |
| `AdvancedLab` | Quantum Lab | Research & Tech | 15 | yes | 1 | ResearchLab ≥8, PowerPlant ≥10 | Late-game research branches ⚠️ |
| `ShieldGenerator` | Shield Generator | Defense | 15 | yes | 2 | PowerPlant ≥6, Wall ≥8 | Temporary damage mitigation ⚠️ |
| `Wall` | Fortress Wall | Defense | 20 | yes | Perimeter | TownHall ≥2 | HP buffer + fortification points |
| `Turret` | Artillery Turret | Defense | 20 | no | 1 | Wall ≥5 | Passive defense damage |
| `SensorArray` | Sensor Array | Defense/Intel | 15 | yes | 1 | ResearchLab ≥4 | Fog of war reveal, early warning |
| `UltraConverter` | Ultra Converter | Late Game | 10 | yes | 1 | TownHall ≥11, PowerPlant ≥12 | Converts units using DarkEnergy |
| `DarkReactor` | Dark Energy Reactor | Late Game | 10 | yes | 2 | UltraConverter ≥3, AdvancedLab ≥4 | Generates DarkEnergy currency ⚠️ |
| `TokenForge` | Upgrade Token Forge | Late Game | 10 | yes | 1 | TownHall ≥10, ResearchLab ≥7 | Produces UpgradeTokens over time ⚠️ |

⚠️ indicates a fictionalised structure introduced to fill mechanical gaps; these need design validation later.

---

## 3. Detailed Building Sheets

Each subsection captures:
* Base cost/time (level 1)
* Scaling behaviour (cost/time curves plus notable overrides)
* Per-level effects summary
* Prerequisites and unlock conditions
* Tile caps or special placement rules

### 3.1 Core Command

#### Town Hall (`TownHall`)
* **Base Cost**: Food 2 000, Steel 1 200, Oil 600, RareMetal 200
* **Base Time**: 600 s
* **Scaling**: cost/time formulas from §1; levels 10, 15, 20 add +10% time multipliers (handled via overrides).
* **Effects per Level**:
  * +2 tile cap (levels 1–10), +1 tile cap (11–20)
  * Unlock flags for building tiers (see `townHallRequired`)
  * Levels 10+ require Upgrade Tokens; levels 11, 13, 15 unlock slot rewards (`UltraEnergy`, `Airfield`, `NavalDock`)
* **Prereqs**: none; unique; consumes 4 tiles (city centre footprint).

#### Command HQ (`CommandHQ`)
* **Base Cost**: Food 1 200, Steel 1 600, Oil 600
* **Base Time**: 480 s
* **Effects**: +1 march slot at levels 1, 3, 6, 9, 12, 15, 18; +1 transport queue every 5 levels.
* **Prereqs**: TownHall ≥2; unique; 2 tiles.
*Runtime*: Slots feed `CityState.marchSlots` / `transportSlots`; shown in the city summary overlay.

#### Staff Bureau (`StaffBureau`)
* **Base Cost**: Food 800, Steel 1 000, Oil 300
* **Base Time**: 360 s
* **Effects**: officer cap +1 every 2 levels, officer bonus multipliers (morale/production/construction) +3% per level.
* **Prereqs**: TownHall ≥3, CommandHQ ≥2.
*Runtime*: Officer cap applied via `BuildingEffectRuntime` → `CityState.officerCapacity`; commander asset provides baseline mayor stats.

#### Logistics Office (`LogisticsOffice`)
* **Base Cost**: Food 900, Steel 900, Oil 500
* **Base Time**: 420 s
* **Effects**: transport cooldown −3%/level, auto-transport payload +5%/level, unlocks Transport Hub at level 5.
* **Prereqs**: TownHall ≥4, CommandHQ ≥3.

#### Officer Quarters (`OfficerQuarters`)
* **Base Cost**: Food 600, Steel 400, Oil 200
* **Base Time**: 240 s
* **Effects**: officer morale floor +2%/level, housing capacity tied to officer rarity tiers.
* **Prereqs**: StaffBureau ≥3; multiple instances allowed.

### 3.2 Economy & Infrastructure

#### Storage Vault (`Warehouse`)
* **Base Cost**: Food 500, Steel 600
* **Base Time**: 180 s
* **Effects**: increases safe capacity per resource by 2 000 + (level × 1 200). Stackable.
* **Prereqs**: TownHall ≥2.
*Runtime*: Storage bonuses are applied to `ResourceSystem` (flat + percent) with a 4 000 000 000 hard cap per resource.

#### Hydroponic Farm (`Farm`)
* **Base Cost**: Food 0, Steel 300
* **Base Time**: 150 s
* **Effects**: adds food field output `base 20 × level × moraleFactor`.
* **Prereqs**: TownHall ≥2; cap 12 per city until TownHall ≥10.

#### Steel Foundry (`SteelFoundry`)
* **Base Cost**: Food 300, Steel 0, Oil 200
* **Base Time**: 180 s
* **Effects**: steel production `base 18 × level`.
* **Prereqs**: TownHall ≥3.

#### Oil Refinery (`OilRefinery`)
* **Base Cost**: Food 250, Steel 200, Oil 0
* **Base Time**: 210 s
* **Effects**: oil production `base 15 × level`; integrates with Power Plant multiplier.
* **Prereqs**: TownHall ≥4.

#### Rare Metal Mine (`RareMine`)
* **Base Cost**: Food 200, Steel 300, RareMetal 0
* **Base Time**: 240 s
* **Effects**: rare metal production `base 8 × level`.
* **Prereqs**: TownHall ≥5, ResearchLab ≥2.

#### Power Plant (`PowerPlant`)
* **Base Cost**: Food 400, Steel 500, Oil 300
* **Base Time**: 260 s
* **Effects**: city-wide energy rating; multiplies Airfield, VehicleFactory, ShieldGenerator operational efficiency ( +2% per level ).
* **Prereqs**: TownHall ≥4.

#### Trade Port (`TradePort`)
* **Base Cost**: Food 350, Steel 350, Oil 200
* **Base Time**: 220 s
* **Effects**: unlocks market exchange; conversion rate begins at 0.25 and improves +0.02 per level (clamped to 0.75).
* **Prereqs**: TownHall ≥4; coastal flag adds NavalYard prerequisite.

### 3.3 Logistics & Storage

#### Supply Depot (`SupplyDepot`)
* **Base Cost**: Food 550, Steel 650
* **Base Time**: 200 s
* **Effects**: +5% storage capacity per level, +2% march payload per level.
* **Prereqs**: TownHall ≥3.
*Runtime*: Percent capacity bonus feeds `ResourceSystem` during storage recalculation.

#### Transport Hub (`TransportHub`)
* **Base Cost**: Food 700, Steel 900, Oil 500
* **Base Time**: 400 s
* **Effects**: +1 auto-transport route at levels 1, 4, 7, 10, 13, 15.
* **Prereqs**: CommandHQ ≥5, LogisticsOffice ≥6; unique.
*Runtime*: Routes contribute to `CityState.transportSlots`.

#### Aerial Relay (`AerialRelay`)
* **Base Cost**: Food 500, Steel 600, Oil 800
* **Base Time**: 360 s
* **Effects**: reduces air dispatch cooldowns by 5%/level; integrates with Airfield.
* **Prereqs**: Airfield ≥2; unique.

### 3.4 Military Production & Support

#### Barracks (`Barracks`)
* **Base Cost**: Food 400, Steel 500
* **Base Time**: 180 s
* **Effects**: unlocks infantry training; training time multiplier `0.95 - 0.01×(level-1)` (floor 0.70).
* **Prereqs**: TownHall ≥3.

#### Arms Plant (`ArmsPlant`)
* **Base Cost**: Food 600, Steel 700, Oil 200
* **Base Time**: 220 s
* **Effects**: infantry production throughput; levels ≥10 require Upgrade Token.
* **Prereqs**: Barracks ≥3.

#### Vehicle Factory (`VehicleFactory`)
* **Base Cost**: Food 700, Steel 900, Oil 600
* **Base Time**: 300 s
* **Effects**: vehicle unit queues; consumes Power Plant energy rating.
* **Prereqs**: ArmsPlant ≥5, PowerPlant ≥4.

#### Airfield (`Airfield`)
* **Base Cost**: Food 900, Steel 1100, Oil 900
* **Base Time**: 360 s
* **Effects**: air unit queues; unlocks Aerial Relay and Drone Lab.
* **Prereqs**: TownHall ≥6, CommandHQ ≥6.

#### Naval Yard (`NavalYard`)
* **Base Cost**: Food 1200, Steel 1500, Oil 1300, RareMetal 200
* **Base Time**: 480 s
* **Effects**: naval unit queues; only available on coastal cities (city-tag enforcement).
* **Prereqs**: TownHall ≥7, TradePort ≥4.

#### Missile Silo (`MissileSilo`)
* **Base Cost**: Food 800, Steel 1400, Oil 1000, RareMetal 600
* **Base Time**: 540 s
* **Effects**: missile/rocket unit production; unlocks missile mission events.
* **Prereqs**: ResearchLab ≥5, PowerPlant ≥7; unique.

#### Drone Lab (`DroneLab`) ⚠️
* **Base Cost**: Food 900, Steel 1000, Oil 1200, RareMetal 800
* **Base Time**: 520 s
* **Effects**: advanced drone units; reduces fog-of-war delay.
* **Prereqs**: ResearchLab ≥6, Airfield ≥8; unique.

#### Workshop (`Workshop`)
* **Base Cost**: Food 500, Steel 700, Oil 300
* **Base Time**: 260 s
* **Effects**: training upkeep reduction 2%/level, queue time reduction 1%/level for Vehicles/Air/Naval.
* **Prereqs**: VehicleFactory ≥4.

#### Officer Academy (`Academy`)
* **Base Cost**: Food 400, Steel 600, Oil 200
* **Base Time**: 240 s
* **Effects**: officer XP per tick +5%/level; unlocks elite recruitment rites at level 10.
* **Prereqs**: StaffBureau ≥5; unique.

#### Field Hospital (`Hospital`)
* **Base Cost**: Food 500, Steel 400, Oil 300
* **Base Time**: 220 s
* **Effects**: casualty recovery `2% × level` per tick; morale decay reduction.
* **Prereqs**: Barracks ≥6; unique.

#### Command Tower (`CommandTower`)
* **Base Cost**: Food 600, Steel 800, Oil 400, RareMetal 200
* **Base Time**: 320 s
* **Effects**: global queue dashboard; unlocks simultaneous multi-city queue management; UI only – deterministic logic handled via events.
* **Prereqs**: CommandHQ ≥8; unique.

### 3.5 Research & Tech

#### Research Lab (`ResearchLab`)
* **Base Cost**: Food 500, Steel 800, Oil 400, RareMetal 200
* **Base Time**: 300 s
* **Effects**: unlocks tech tree nodes; research queue length increases every 5 levels.
* **Prereqs**: TownHall ≥4; unique.

#### Quantum Lab (`AdvancedLab`) ⚠️
* **Base Cost**: Food 700, Steel 1100, Oil 700, RareMetal 600
* **Base Time**: 420 s
* **Effects**: late-game research (Dark Energy, advanced logistics).
* **Prereqs**: ResearchLab ≥8, PowerPlant ≥10; unique.

### 3.6 Defense & Intel

#### Fortress Wall (`Wall`)
* **Base Cost**: Food 300, Steel 500, Oil 200
* **Base Time**: 200 s
* **Effects**: wall HP `base 1 000 + 200 × level`; fortification point unlocks (level 5, 10, 15).
* **Prereqs**: TownHall ≥2; unique.

#### Artillery Turret (`Turret`)
* **Base Cost**: Food 400, Steel 800, Oil 400, RareMetal 150
* **Base Time**: 260 s
* **Effects**: passive city defense damage; scales with Research (`Defense` branch).
* **Prereqs**: Wall ≥5.

#### Shield Generator (`ShieldGenerator`) ⚠️
* **Base Cost**: Food 600, Steel 1000, Oil 800, RareMetal 700
* **Base Time**: 360 s
* **Effects**: temporary damage mitigation buff (up to 20% at max level); consumes Power Plant energy.
* **Prereqs**: PowerPlant ≥6, Wall ≥8; unique.

#### Sensor Array (`SensorArray`)
* **Base Cost**: Food 300, Steel 600, Oil 500
* **Base Time**: 280 s
* **Effects**: reveals incoming march ETA, extends scouting radius.
* **Prereqs**: ResearchLab ≥4; unique.

### 3.7 Late Game & Special

#### Ultra Converter (`UltraConverter`)
* **Base Cost**: Food 800, Steel 1200, Oil 900, RareMetal 900, DarkEnergy 50
* **Base Time**: 480 s
* **Effects**: converts legacy units to Ultra variants; consumes DarkEnergy tokens.
* **Prereqs**: TownHall ≥11, PowerPlant ≥12, CommandTower ≥6; unique.

#### Dark Energy Reactor (`DarkReactor`) ⚠️
* **Base Cost**: Food 900, Steel 1300, Oil 1200, RareMetal 1100
* **Base Time**: 540 s
* **Effects**: generates DarkEnergy (base 5/hour + 1/hour per level); increases Ultra unit upkeep cap.
* **Prereqs**: UltraConverter ≥3, AdvancedLab ≥4; unique; 2 tiles.

#### Upgrade Token Forge (`TokenForge`) ⚠️
* **Base Cost**: Food 700, Steel 900, Oil 700, RareMetal 600
* **Base Time**: 360 s
* **Effects**: produces UpgradeTokens every `3600 × 0.95^(level-1)` seconds.
* **Prereqs**: TownHall ≥10, ResearchLab ≥7; unique.

---

## 4. Data Gaps & TODOs

* ⚠️ Fictional buildings (`DroneLab`, `AdvancedLab`, `ShieldGenerator`, `DarkReactor`, `TokenForge`) need design sign-off.
* Naval Yard coastal enforcement relies on city tag `coastal` – ensure StartProfile and CityTileEditor expose this.
* Wall perimeter consumption is represented as `tiles = "perimeter"` in JSON; runtime must translate to block placement.
* Missile Silo + Drone Lab require corresponding unit definitions when the unit roster is expanded.

---

## 5. Runtime Integration Checklist

| System | Action |
| --- | --- |
| `BuildingCatalog` | Load definitions from JSON (`content/data/buildings.json`), register all levels. |
| `BuildController` | Consume costs directly from data; apply token gating and tile caps from catalogue metadata. |
| `BuildQueueSystem` | Validate prerequisites / tile caps using catalogue; publish `[Build]` logs. |
| `BuildingFunctionSystem` | Map `effects` payload to stat modifiers (population, production, multipliers, unlocks). |
| `CityTileEditor` | Display costs, times, prereqs; respect tile caps; support Undo. |
| `UI` (`BuildingCardView`, `CityViewModel`) | Bind to new `BuildingDefinitionViewModel` (to be implemented). |
| `Tests` | Extend `BuildRulesTests`, `BuildQueueTests`, add data integrity tests to ensure every level has cost/time/effect info. |

---

## 6. Provenance & Versioning

* `content/data/buildings.json` – canonical source.
* `schemaVersion = 1` – bump on structural changes.
* Manual edits in Unity inspector should regenerate JSON to keep runtime + docs in sync (see upcoming tooling).

---

## 7. Future Work

1. Implement extraction tooling to sync War2V cache into our schema where applicable.
2. Wire `BuildingDefinitionViewModel` + UI states (ETA, disabled reasons, energy costs).
3. Add profile presets per faction/city archetype using this catalogue.
4. Align mission rewards and DevActions helpers with the new building IDs.

Keep this document updated whenever the catalogue evolves to maintain a clean handoff loop for the next supersprint.
