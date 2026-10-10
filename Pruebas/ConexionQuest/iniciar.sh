#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")"
godot_editor=${GODOT_BIN:-godot}
if [ -z "${GODOT_BIN:-}" ] && [ -x ../../.tools/godot-4.6.3/Godot ]; then
  godot_editor="$PWD/../../.tools/godot-4.6.3/Godot"
fi
if ! command -v "$godot_editor" >/dev/null 2>&1 && [ ! -x "$godot_editor" ]; then
  echo "Instala Godot 4.6 Standard o define GODOT_BIN con la ruta del ejecutable." >&2
  exit 2
fi
xr_mode=on
if [ "${1:-}" = "--desktop" ]; then
  xr_mode=off
  shift
fi
exec "$godot_editor" --path "$PWD" --xr-mode "$xr_mode" "$@"
