# City / Base System

Purpose: represent each player’s cities (nodes) where production, troop training, defense, etc. occur.

Key data per city (summary):
- city id, name, coordinates (optional)
- owner, alliance
- city level, blueprint cap, specialization
- building slots and grid layout
- labor (total/assigned/idle), morale, reputation

Mechanics and design choices:
- City specialization: production / military / conversion
- Resettle / bounce mechanics (reputation-based)
- Wall and defense interaction

See `entities.md` for the canonical data model for City and Building.
