#!/usr/bin/env python3
"""Offline tick simulator for the reconstructed core loop."""
import json
import time
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
SEED_PATH = ROOT / 'content' / 'seed' / 'game_state.json'


def load_seed():
    with open(SEED_PATH, 'r', encoding='utf-8') as handle:
        return json.load(handle)


def simulate_ticks(ticks=3, tick_seconds=0):
    seed = load_seed()
    cities = seed['cities']

    print(f"Loaded {len(cities)} cities from {SEED_PATH}")
    for tick in range(1, ticks + 1):
        print(f"\n=== Tick {tick} ===")
        for city in cities:
            morale = city.get('morale', 1.0)
            morale_factor = morale if morale >= 0.9 else max(0.35, morale)

            # Production
            produced_summary = {}
            for prod in city.get('production', []):
                amount = int(prod['fields'] * prod['outputPerField'] * morale_factor)
                produced_summary[prod['resourceType']] = produced_summary.get(prod['resourceType'], 0) + amount

            print(f"{city['displayName']}: morale={morale:.2f} production={produced_summary}")

            # Training progress (deterministic)
            for queue in city.get('trainingQueues', []):
                queue['secondsRemaining'] = max(0, queue.get('secondsRemaining', queue['durationSeconds']) - 60)
                if queue['secondsRemaining'] == 0:
                    print(f"  Training complete: {queue['quantity']} {queue['unitType']}")
                    queue['secondsRemaining'] = queue['durationSeconds']

        if tick_seconds:
            time.sleep(min(tick_seconds, 0.1))


if __name__ == '__main__':
    simulate_ticks()
