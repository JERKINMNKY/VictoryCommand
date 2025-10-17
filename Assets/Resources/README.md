Assets/Resources — Serialized game data

This folder stores runtime-available assets (ScriptableObject instances) that can be loaded via `Resources.Load`.

Known assets:
- `Assets/Resources/Cities/CityData.asset` — Example CityData asset used for editor testing.
- `Assets/Resources/Buildings/BuildingData.asset` — (may be created or added by designers)

Notes:
- Keep `Resources` content small; prefer explicit references where possible.
- If you use `Resources.Load`, document the resource path and expected type.