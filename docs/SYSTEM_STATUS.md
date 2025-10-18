# System Status Matrix

Layer focus: Core Simulation | Life Domains: Autonomy, Security, Logistics

| System | Exists? | Evidence | Inferred Purpose | Action |
| --- | --- | --- | --- | --- |
| ResourceSystem | ✅ | `Assets/Scripts/Systems/ResourceSystem.cs` | Manage per-city production with morale penalties | Implemented and hooked into `GameLoop` |
| BuildQueueSystem | ✅ | `Assets/Scripts/Systems/BuildQueueSystem.cs` | Enforce upgrade slots + blueprint gates | Implemented with two-slot default |
| TrainingSystem | ✅ | `Assets/Scripts/Systems/TrainingSystem.cs` | Consume resources + deliver trained units | Implemented with upkeep + garrison updates |
| AutoTransportSystem | ✅ | `Assets/Scripts/Systems/AutoTransportSystem.cs` | Ship surpluses between cities | Implemented with interval timers |
| DefenseSystem | ✅ | `Assets/Scripts/Systems/DefenseSystem.cs` | Track wall HP and morale-driven decay | Implemented with repair + morale damage |
| GameLoop | ✅ | `Assets/Scripts/Systems/GameLoop.cs` | Orchestrate tick order and load seed | Implemented, loads `/content/seed/game_state.json` |
| Officer System | ❌ | Mentioned in docs but no runtime code | Assign mayors and wall officers | Plan for morale + queue boosts |
| Logistics Planner | ❌ | Alluded to by Command Tower docs | Manage cross-city scheduling | Requires design after auto-transport validation |
| Ultra Converter | ❌ | Building data exists, no runtime | Consume Ultra Energy for conversions | Requires resource expansion |
| Combat Resolver | ❌ | Design docs reference raids | Resolve attacks between cities | Future milestone |

## Notes
- Systems declare Unity layer + life domain for Atlas traceability.
- Resource ticks feed training + transport flows; future work should introduce persistence + officer modifiers.
