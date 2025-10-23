# Units & Forces Overview

Initial balance capture for combat units based on `w2v_units_partial.csv`.  
Values mirror the current War2V extraction and will evolve as we decode the full data tables (expect renames and new entries).

**Data Source**
- `w2v_units_partial.csv` (checked in at repo root)

**Interpretation Notes**
- `VS Air/Ground/Navy/Fort` are relative damage coefficients (higher = better). War2V uses these as per-target multipliers.
- `Range`, `Speed`, and `Attack Speed` are raw game units; we keep them intact until cadence normalization.
- `Command Points` in the source represent march-slot cost. Only true end-game units will convert this value to the new Dark/Nuclear Energy currency (name TBD); mid-game units will continue to use classic Command Points. Blank entries still need authoritative values.
- `Steel` cost only appears on some early-tier units in the partial dump; most late-game units still need metal cost populated.

## Air Superiority & Strike

| Unit | HP | vs Air | vs Ground | vs Navy | vs Fort | Range | Speed | Training | Food | Oil | Rare Metal | Pop | Cmd Pts / Energy |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Ultra-Fighter | 1100 | 95 | 25 | 30 | 30 | 800 | 2800 | 00:11:40 | 5 | 115 | 2500 | 10 | 22 |
| Fighter Plane Lightning | 1050 | 85 | 30 | 40 | 25 | 900 | 2600 | 00:11:20 | 4 | 110 | 2200 | 10 | 20 |
| Ultra-Bomber | 1200 | 9 | 120 | 125 | 55 | 2000 | 2200 | 00:11:40 | 8 | 130 | 2000 | 26 | 15 |
| Bomber | 950 | 8 | 95 | 100 | 50 | 1800 | 2100 | 00:10:50 | 6 | 110 | 1800 | 22 | 13 |

## Air Defense & Missile

| Unit | HP | vs Air | vs Ground | vs Navy | vs Fort | Range | Speed | Training | Food | Oil | Rare Metal | Pop | Cmd Pts / Energy |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Air Defense Division | 1600 | 110 | 55 | 120 | 45 | 600 | 1000 | 02:20:00 | 5 | 100 | 2000 | 15 | 15 |
| Ultra-SAM | 1000 | 220 | 25 | 25 | 20 | 1000 | 700 | 00:12:00 | 7 | 95 | 3000 | 18 | 18 |
| Ultra-SSM | 900 | 15 | 240 | 150 | 110 | 1500 | 800 | 00:13:20 | 10 | 90 | 5000 | 20 | 20 |
| Rocket | 700 | 3 | 80 | 90 | 40 | 1000 | 600 | 00:09:30 | 5 | 80 | 1500 | 16 | 12 |

## Naval Task Force

| Unit | HP | vs Air | vs Ground | vs Navy | vs Fort | Range | Speed | Training | Food | Oil | Rare Metal | Pop | Cmd Pts / Energy |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Battleship: King George V | 1500 | 73 | 85 | 120 | 110 | 1800 | 900 | 00:25:00 | 40 | 1200 | 14500 | 40 | – |
| Carrier: Ark Royal | 2200 | 110 | 95 | 100 | 90 | 3100 | 850 | 00:36:56 | 55 | 3000 | 25000 | 55 | – |

## Armored & Mechanized Corps

| Unit | HP | vs Air | vs Ground | vs Navy | vs Fort | Range | Speed | Training | Food | Oil | Rare Metal | Pop | Cmd Pts / Energy | Steel |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Heavy Tank | 1400 | 5 | 150 | 70 | 90 | 600 | 500 | 00:15:00 | 9 | 140 | 2500 | 30 | 17 | – |
| L Tank: Sherman | 160 | 16 | 28 | 22 | 25 | 50 | 850 | 00:01:06 | 4 | 400 | 300 | 4 | – | 750 |
| Armoured V: BA-20 | 220 | 24 | 16 | 10 | 14 | 40 | 450 | 00:00:50 | 3 | 150 | 550 | 3 | – | 500 |
| Motorized Unit | 65 | 5 | 12 | 7 | 10 | 30 | 1400 | 00:00:23 | 2 | 30 | 50 | 2 | – | 120 |
| SPG: SU-76 | 140 | 22 | 32 | 36 | 50 | 1500 | 420 | 00:01:00 | 5 | 300 | 750 | 5 | – | 500 |
| Infantry | 100 | 9 | 10 | 10 | 18 | 20 | 300 | 00:00:20 | 1 | 10 | 100 | 1 | – | 70 |

## Outstanding Gaps

- Populate missing `Command Points` for naval and early-tier vehicles (pull from War2V mission/combat configs).
- Add `Steel` costs for high-tier units (currently blank in source CSV).
- Extend the roster with artillery, special forces, and alliance-exclusive units once decompiled data is parsed.
- Cross-verify training times and resource costs against War2V mission rewards to ensure cadence parity.
