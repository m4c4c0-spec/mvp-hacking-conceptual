#!/usr/bin/env python3
"""Exporta medidas del hub procedural para blockout en Blender. Sin Unity."""
import json
import re
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
HUB = ROOT / "Assets/_Project/Presentation/HubOffice.cs"
OUT = ROOT / "Assets/Art/Office/blockout_dimensions.json"

# Posiciones y escalas de Box(...) en HubOffice.Build — mantener alineado al C#.
ANCHORS = [
    {"name": "player_spawn", "pos": [0.15, 0.08, -5.35], "size": None, "note": "CharacterController eye 1.62m. Entrada, lejos del muro frontal."},
    {"name": "desk", "pos": [-0.4, 0.85, -0.2], "size": [3.4, 0.14, 1.4]},
    {"name": "laptop", "pos": [-0.65, 0.98, -0.32], "size": [0.92, 0.06, 0.62]},
    {"name": "notebook", "pos": [-1.55, 0.97, -0.46], "size": [0.48, 0.06, 0.58]},
    {"name": "report_inbox", "pos": [1.05, 1.02, -0.58], "size": [0.46, 0.22, 0.5]},
    {"name": "board", "pos": [-0.2, 2.2, 5.72], "size": [3.8, 2.0, 0.08]},
    {"name": "atlas_rack", "pos": [-5.95, 0.9, -2.15], "size": [0.7, 1.8, 0.7]},
    {"name": "door_hinge", "pos": [5.5, 0, -0.35], "size": None, "note": "MECH_door hinge origin"},
    {"name": "archive_table", "pos": [8.4, 0.85, 0.6], "size": [2.8, 0.14, 1.15]},
    {"name": "drawer_root", "pos": [10.3, 1.1, -5.45], "size": None, "note": "MECH_drawer"},
    {"name": "floor_bounds", "pos": [2.35, -0.1, -0.5], "size": [18.1, 0.2, 13.0]},
]

TRAYS = [{"name": f"tray_{i}", "pos": [0.05 + i * 0.58, 0.95, 0.05], "size": [0.5, 0.06, 0.42]} for i in range(3)]
FOLDERS = [{"name": f"folder_{i}", "pos": [7.6 + i * 0.8, 0.97, 0.55], "size": [0.56, 0.07, 0.65]} for i in range(3)]
TICKETS = [{"name": f"ticket_{i}", "pos": [-1.35 + i * 0.8, 2.05, 5.655], "size": [0.55, 0.53, 0.025]} for i in range(4)]


def main():
    data = {
        "unit": "meters",
        "blender_scale": "1 Blender unit = 1 m",
        "unity_root": "Office · first person (local space)",
        "anchors": ANCHORS + TRAYS + FOLDERS + TICKETS,
        "source": "scripts/export-hub-blockout.py + HubOffice.cs",
    }
    OUT.parent.mkdir(parents=True, exist_ok=True)
    OUT.write_text(json.dumps(data, indent=2) + "\n", encoding="utf-8")
    print("OK", OUT.relative_to(ROOT), len(data["anchors"]), "anchors")


if __name__ == "__main__":
    main()
