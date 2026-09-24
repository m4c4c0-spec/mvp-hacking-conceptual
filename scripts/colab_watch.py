#!/usr/bin/env python3
"""Detecta cambios del otro desarrollador sin pisar web/docs/scripts/unity-stub."""

from __future__ import annotations

import hashlib
import json
import os
from datetime import datetime
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
STATE_PATH = ROOT / "docs" / ".colab-state.json"
LOG_PATH = ROOT / "docs" / "COLABORACION.md"
OURS = {
    "web",
    "docs",
    "unity-stub",
    "scripts",
    "README.md",
    ".gitignore",
}

OURS_PREFIXES = (
    "Assets/_Project/",
    "Assets/Scenes/Hub.unity",
    "Assets/Scenes/Boot.unity",
    "Assets/Tests/EditMode/UseCaseTests.cs",
    "Assets/Tests/EditMode/EthicalLab.Tests.asmdef",
    "Assets/Content/Missions/README.md",
    "Assets/Content/Glossary/README.md",
    "Assets/Content/Terminal/README.md",
)


def rel(path: Path) -> str:
    return str(path.relative_to(ROOT))


def digest_file(path: Path) -> str:
    h = hashlib.sha256()
    h.update(path.read_bytes())
    return h.hexdigest()[:16]


def snapshot() -> dict[str, str]:
    out: dict[str, str] = {}
    for dirpath, dirnames, filenames in os.walk(ROOT):
        dirnames[:] = [d for d in dirnames if d not in {".git", "node_modules"}]
        base = Path(dirpath)
        for name in filenames:
            path = base / name
            rel_path = rel(path)
            top = rel_path.split("/", 1)[0]
            if top in OURS or rel_path in OURS:
                continue
            if any(rel_path.startswith(prefix) for prefix in OURS_PREFIXES):
                continue
            if rel_path.startswith("docs/."):
                continue
            out[rel_path] = digest_file(path)
    return out


def load_state() -> dict[str, str]:
    if STATE_PATH.exists():
        return json.loads(STATE_PATH.read_text(encoding="utf-8"))
    return {}


def append_log(lines: list[str]) -> None:
    stamp = datetime.now().strftime("%Y-%m-%d %H:%M")
    block = [f"\n## {stamp} — Cambios detectados (otro desarrollador)\n", *lines, ""]
    with LOG_PATH.open("a", encoding="utf-8") as fh:
        fh.write("\n".join(block))


def main() -> int:
    current = snapshot()
    previous = load_state()
    STATE_PATH.parent.mkdir(parents=True, exist_ok=True)
    STATE_PATH.write_text(json.dumps(current, indent=2, sort_keys=True), encoding="utf-8")

    added = sorted(set(current) - set(previous))
    removed = sorted(set(previous) - set(current))
    changed = sorted(key for key in current if key in previous and current[key] != previous[key])

    if not previous:
        print("COLAB_WATCH_INIT files=%d" % len(current))
        return 0
    if not (added or removed or changed):
        print("COLAB_WATCH_IDLE")
        return 0

    lines = []
    for path in added:
        lines.append(f"- Añadido: `{path}` (`{current[path]}`)")
    for path in changed:
        lines.append(f"- Editado: `{path}` (`{previous[path]}` → `{current[path]}`)")
    for path in removed:
        lines.append(f"- Eliminado: `{path}`")
    append_log(lines)
    print("AGENT_LOOP_WAKE_colab " + json.dumps({"prompt": "El otro desarrollador cambió archivos. Lee docs/COLABORACION.md, adapta el cliente si el JSON cambió, y no pises content/.", "added": added, "changed": changed, "removed": removed}, ensure_ascii=False))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
