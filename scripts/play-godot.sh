#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/.."
godot_editor=${GODOT_BIN:-godot}
if [ -z "${GODOT_BIN:-}" ]; then
  if [ -x "$PWD/.tools/godot-4.6.3/Godot" ]; then
    godot_editor="$PWD/.tools/godot-4.6.3/Godot"
  elif [ -x /Applications/Godot.app/Contents/MacOS/Godot ]; then
    godot_editor=/Applications/Godot.app/Contents/MacOS/Godot
  fi
fi
if ! command -v "$godot_editor" >/dev/null 2>&1 && [ ! -x "$godot_editor" ]; then
  echo "Instala Godot 4.6.3 Standard o define GODOT_BIN con la ruta del ejecutable." >&2
  exit 2
fi
python3 scripts/sync-godot-content.py --check
python3 scripts/check-godot-structure.py
exec "$godot_editor" --path "$PWD/Godot" "$@"
