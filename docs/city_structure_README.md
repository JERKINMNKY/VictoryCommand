
# City Building Structure — Victory Command

This document defines the core city structure used in the WW2-inspired base-building game, *Victory Command*. Each city operates on a layered architecture, where buildings are grouped by function and role.

## 🏗️ Categories

1. **Core Command & Management**  
   Buildings responsible for city governance, officer management, research, storage, and troop production control.

2. **Production Buildings**  
   Facilities that generate the essential resources used throughout the economy and military systems.

3. **Support & Utility Buildings**  
   Buildings that enhance training, conversion, logistics, trade, or end-game mechanics.

## 🧱 Building Definitions

| Category                   | Building Name       | Max Level | Description |
|---------------------------|---------------------|-----------|-------------|
| Core Command & Management | Staff HQ            | 20        | Controls overall city operation, required for unlocks. |
|                           | General HQ          | 20        | Governs march slots and military dispatch limits. |
|                           | Military Institute  | 10        | Officer recruitment and generation. |
|                           | Research Center     | 20        | Unlocks technologies across economy and combat. |
|                           | Warehouse           | 20        | Increases resource storage and plunder protection. |
|                           | House               | 20        | Provides population and passive gold income via taxes. |
|                           | Wall                | 20        | Defends city from invasions, supports fortifications. |
|                           | Airport             | 20        | Produces aircraft and related advanced units. |
|                           | Naval Yard          | 20        | Produces ships if city is coastal. |
|                           | Arms Plant          | 20        | Produces basic land troops and performs conversions. |
|                           | Ultra Converter     | 10        | Converts legacy units to advanced using Dark Energy. |
| Production Buildings      | Farm                | 20        | Produces food, used for training and upkeep. |
|                           | Steel Plant         | 20        | Produces steel, core material for armor and buildings. |
|                           | Oil Field           | 20        | Produces oil, used in vehicles and upkeep. |
|                           | Rare Metal Mine     | 20        | Produces rare metals, used for advanced gear and tech. |
| Support & Utility         | Barracks            | 20        | Trains infantry and boosts troop capacity. |
|                           | Vehicle Factory     | 20        | Produces tanks and trucks. |
|                           | Dockyard            | 20        | Builds naval units (coastal only). |
|                           | Academy             | 10        | Boosts officer XP and skill training. |
|                           | Market              | 10        | Allows trade and resource exchange. |
|                           | Hospital            | 10        | Heals wounded troops after combat. |
|                           | Command Tower       | 10        | Central UI for managing build queues across cities. |

---

## ⚠️ Developer Notes

- Future expansions may introduce additional buildings like Radar, Intel, Superweapons Facility, or Trade Port.
- Ultra Converter will consume **Dark Energy** as an exclusive resource from capital cities or anomalies.
- Each category is implemented as a separate folder in Unity (e.g., `Assets/Scripts/Buildings/Management`).

## 📁 Data Source

This list is maintained in `/docs/city_structure.csv` and `/docs/city_structure_README.md`.

