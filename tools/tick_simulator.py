#!/usr/bin/env python3
"""
Tiny tick simulator for Victory Command prototypes.
Loads sample seeds and simulates resource production and simple training queue progression.
"""
import json
import time
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
SAMPLES = ROOT / 'docs' / 'samples'


def load_json(name):
    with open(SAMPLES / name) as f:
        return json.load(f)


def simulate_ticks(ticks=10, tick_seconds=1):
    player = load_json('seed_player.json')
    city = load_json('seed_city.json')
    unit = load_json('seed_unit_infantry.json')

    # Simplified production per tick (configurable)
    prod_per_tick = { 'Food': 5 }

    print('Starting simulation: player=', player['id'], 'city=', city['id'])
    for t in range(ticks):
        # Produce resources
        for r, amt in prod_per_tick.items():
            player['resources'][r] = player['resources'].get(r, 0) + amt

        # Simulate upkeep deduction (per unit)
        upkeep = unit.get('upkeep', {})
        for r, u in upkeep.items():
            cost = u
            player['resources'][r] = max(0, player['resources'].get(r, 0) - cost)

        print(f'Tick {t+1}: resources=', {k: player['resources'].get(k) for k in ['Food','Steel','Oil']})
        time.sleep(0 if tick_seconds == 0 else min(0.1, tick_seconds))


if __name__ == '__main__':
    simulate_ticks(ticks=10, tick_seconds=0)
