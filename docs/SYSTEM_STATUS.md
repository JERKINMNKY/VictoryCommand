# System Status Matrix

Layer focus: Core Simulation | Life Domains: Autonomy, Security, Logistics

| System | Exists? | Evidence | Inferred Purpose | Action |
| --- | --- | --- | --- | --- |
| ResourceSystem | ✅ | `Assets/Scripts/Systems/ResourceSystem.cs` | Data-driven production, morale/politics modifiers, storage boosts, hard-cap enforcement | Implemented with building-function inputs + 4 B clamp |
| BuildQueueSystem | ✅ | `Assets/Scripts/Systems/BuildQueueSystem.cs` | Enforce upgrade slots + blueprint gates | Implemented with two-slot default |
| TrainingSystem | ✅ | `Assets/Scripts/Systems/TrainingSystem.cs` | Consume resources + deliver trained units | Implemented with upkeep + garrison updates |
| AutoTransportSystem | ✅ | `Assets/Scripts/Systems/AutoTransportSystem.cs` | Ship surpluses between cities | Implemented with interval timers |
| DefenseSystem | ✅ | `Assets/Scripts/Systems/DefenseSystem.cs` | Track wall HP and morale-driven decay | Implemented with repair + morale damage |
| GameLoop | ✅ | `Assets/Scripts/Systems/GameLoop.cs` | Orchestrate tick order and load seed | Implemented, loads `/content/seed/game_state.json` |
| Officer System | ⚠️ | `Assets/Resources/Officers/Commander.asset`, `BuildingEffectRuntime.ApplyOfficerCap` | Mayor stats, officer slot caps, Commander baseline | Need assignment UX + bonuses beyond production |
| Player Progression | ✅ | `Assets/Scripts/Systems/FoundationState.cs` (`PlayerState`), `PlayerRankTable` | Track rank, city cap, shared token inventory | Rank table seeded (Private=1 city); enforce limits in future |
| Logistics Planner | ❌ | Alluded to by Command Tower docs | Manage cross-city scheduling | Requires design after auto-transport validation |
| Ultra Converter | ❌ | Building data exists, no runtime | Consume Ultra Energy for conversions | Requires resource expansion |
| Combat Resolver | ❌ | Design docs reference raids | Resolve attacks between cities | Future milestone |

## Notes
- Systems declare Unity layer + life domain for Atlas traceability.
- Resource ticks feed training + transport flows; future work should introduce persistence + officer modifiers.
