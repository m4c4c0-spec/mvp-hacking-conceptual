#!/usr/bin/env python3
"""Detecta cambios de Godot y puede validar continuamente mientras se edita."""
from __future__ import annotations

import argparse
import hashlib
import json
import os
import subprocess
import time
from datetime import datetime
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
STATE_PATH = ROOT / "docs" / ".colab-state.json"
LOG_PATH = ROOT / "docs" / "COLABORACION.md"
WATCH_ROOTS = (ROOT / "Godot", ROOT / "scripts")
IGNORED_DIRS = {".godot", ".android", "__pycache__", "docs", "node_modules", "build"}


def snapshot() -> dict[str, str]:
    files = {}
    for folder in WATCH_ROOTS:
        for directory, subdirs, names in os.walk(folder):
            subdirs[:] = [name for name in subdirs if name not in IGNORED_DIRS]
            for name in names:
                path = Path(directory) / name
                # Godot genera estos metadatos al importar: no son cambios de código.
                if path.suffix in {".uid", ".import", ".pyc", ".tmp"} or name == ".export_credentials.cfg":
                    continue
                try:
                    files[str(path.relative_to(ROOT))] = hashlib.sha256(path.read_bytes()).hexdigest()[:16]
                except FileNotFoundError:
                    # Una operación de mover archivos puede ocurrir durante el recorrido.
                    continue
    for path in [ROOT / ".gitignore"]:
        if path.exists():
            files[str(path.relative_to(ROOT))] = hashlib.sha256(path.read_bytes()).hexdigest()[:16]
    return files


def load_state() -> dict[str, str]:
    try:
        data = json.loads(STATE_PATH.read_text(encoding="utf-8"))
        return data.get("files", {}) if data.get("version") == 2 else {}
    except (FileNotFoundError, json.JSONDecodeError):
        return {}


def save_state(files: dict[str, str], result: bool | None) -> None:
    STATE_PATH.parent.mkdir(parents=True, exist_ok=True)
    data = {"version": 2, "files": files, "checked_at": datetime.now().isoformat(timespec="seconds"), "validation_ok": result}
    temporary = STATE_PATH.with_suffix(".tmp")
    temporary.write_text(json.dumps(data, indent=2, sort_keys=True), encoding="utf-8")
    temporary.replace(STATE_PATH)


def validate() -> bool:
    commands = [
        ["node", "scripts/validate-content.mjs"],
        ["bash", "-n", "scripts/play-godot.sh", "scripts/test-godot.sh", "scripts/export-godot.sh", "scripts/validate-mission-flow.sh"],
        ["bash", "scripts/validate-mission-flow.sh"],
        ["bash", "scripts/test-godot.sh"],
    ]
    for command in commands:
        try:
            result = subprocess.run(command, cwd=ROOT, stdin=subprocess.DEVNULL, capture_output=True, text=True, timeout=180)
        except (OSError, subprocess.TimeoutExpired) as error:
            print(f"VALIDACIÓN FALLÓ: {error}", flush=True)
            return False
        output = result.stdout + result.stderr
        if result.returncode:
            print(output, flush=True)
            print("VALIDACIÓN FALLÓ: " + " ".join(command), flush=True)
            return False
        for line in output.splitlines():
            if line.startswith(("OK", "ok", "SESSION:", "WORLD:", "✓")):
                print(line, flush=True)
    print("VALIDACIÓN OK · contenido, rutas, importación y recorrido", flush=True)
    return True


def record(previous: dict[str, str], current: dict[str, str], result: bool | None) -> None:
    added = sorted(current.keys() - previous.keys())
    removed = sorted(previous.keys() - current.keys())
    changed = sorted(path for path in current.keys() & previous.keys() if current[path] != previous[path])
    if previous and (added or removed or changed):
        print("AGENT_LOOP_WAKE_colab " + json.dumps({
            "prompt": "Revisa los cambios de Godot/Game y scripts, sus recursos y el catálogo. Lee docs/COORDINACION_AGENTES.md antes de corregir archivos compartidos.",
            "added": added, "changed": changed, "removed": removed,
        }, ensure_ascii=False), flush=True)
    if result is not None or (previous and (added or removed or changed)):
        lines = [f"\n## {datetime.now():%Y-%m-%d %H:%M:%S} — Revisión Godot\n"]
        for label, paths in [("Añadido", added), ("Editado", changed), ("Eliminado", removed)]:
            if previous:
                lines.extend(f"- {label}: `{path}`" for path in paths)
        if result is not None:
            lines.append("- Validación: " + ("OK" if result else "FALLÓ · revisar salida del monitor"))
        with LOG_PATH.open("a", encoding="utf-8") as file:
            file.write("\n".join(lines) + "\n")


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--watch", action="store_true", help="Vigila continuamente hasta Ctrl+C")
    parser.add_argument("--validate", action="store_true", help="Ejecuta validaciones y tests ante cada cambio")
    parser.add_argument("--interval", type=float, default=3.0, help="Segundos entre revisiones, mínimo 1")
    args = parser.parse_args()
    interval = max(1.0, args.interval)
    previous = load_state()
    first = True
    if args.watch:
        print(f"VIGILANCIA GODOT ACTIVA · PID {os.getpid()} · Ctrl+C para detener", flush=True)
    while True:
        current = snapshot()
        if current != previous or (first and args.validate):
            if args.watch:
                # Espera una escritura estable para no validar una mudanza a medio guardar.
                while True:
                    time.sleep(interval)
                    settled = snapshot()
                    if settled == current:
                        break
                    current = settled
            result = validate() if args.validate else None
            record(previous, current, result)
            save_state(current, result)
            previous = current
            if not args.watch:
                print("COLAB_WATCH_CHECKED", flush=True)
                return 1 if result is False else 0
        elif not args.watch:
            print("COLAB_WATCH_IDLE", flush=True)
            return 0
        first = False
        time.sleep(interval)


if __name__ == "__main__":
    try:
        raise SystemExit(main())
    except KeyboardInterrupt:
        print("Vigilancia detenida.", flush=True)
