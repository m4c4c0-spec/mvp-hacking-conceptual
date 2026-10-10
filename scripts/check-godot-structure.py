#!/usr/bin/env python3
"""Comprueba referencias y recursos de exportación después de mover carpetas."""
from pathlib import Path
import configparser
import fnmatch
import os
import re
import sys

ROOT = Path(__file__).resolve().parent.parent
PROJECT = ROOT / "Godot"
RESOURCE = re.compile(r'["\'](res://[^"\'\n]+)["\']')
CLASS = re.compile(r"^class_name\s+(\w+)", re.MULTILINE)
REQUIRED_EXPORT = (
    "Game/Features/Quests/missions.json",
    "Game/Features/Quests/condor.json",
    "ThirdParty/godot/GODOT_LICENSE.txt",
    "ThirdParty/godot/GODOT_COPYRIGHT.txt",
)


def check() -> list[str]:
    errors = []
    classes = {}
    for directory, subdirs, files in os.walk(PROJECT):
        subdirs[:] = [name for name in subdirs if name not in {".godot", ".android", "docs"}]
        for name in files:
            file = Path(directory) / name
            if file.suffix not in {".gd", ".tscn", ".tres", ".godot"}:
                continue
            content = file.read_text(encoding="utf-8")
            for reference in RESOURCE.findall(content):
                # Las rutas con formato corresponden a salidas de capturas.
                if "%" in reference:
                    continue
                target = PROJECT / reference.removeprefix("res://")
                if not target.is_file():
                    errors.append(f"{file.relative_to(ROOT)}: recurso ausente {reference}")
            for name in CLASS.findall(content):
                if name in classes:
                    errors.append(f"Clase duplicada {name}: {classes[name]} y {file.relative_to(ROOT)}")
                classes[name] = str(file.relative_to(ROOT))

    config = configparser.ConfigParser(interpolation=None)
    config.read(PROJECT / "export_presets.cfg", encoding="utf-8")
    presets = [section for section in config.sections() if re.fullmatch(r"preset\.\d+", section)]
    if not presets:
        errors.append("No hay presets de exportación")
    for preset in presets:
        include = config[preset].get("include_filter", "").strip('"').split(",")
        exclude = config[preset].get("exclude_filter", "").strip('"').split(",")
        for resource in REQUIRED_EXPORT:
            if not (PROJECT / resource).is_file():
                errors.append(f"Recurso necesario ausente: {resource}")
            if not any(fnmatch.fnmatchcase(resource, pattern.strip()) for pattern in include):
                errors.append(f"{preset}: include_filter no garantiza {resource}")
            if any(fnmatch.fnmatchcase(resource, pattern.strip()) for pattern in exclude if pattern.strip()):
                errors.append(f"{preset}: exclude_filter elimina {resource}")
    return errors


if __name__ == "__main__":
    failures = check()
    if failures:
        for failure in failures:
            print("ERROR: " + failure, file=sys.stderr)
        raise SystemExit(1)
    print("OK · rutas de Godot, clases y recursos de exportación")
