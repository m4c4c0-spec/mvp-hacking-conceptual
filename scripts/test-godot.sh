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
python3 scripts/sync-godot-content.py --check
python3 scripts/check-godot-structure.py
# Godot puede emitir errores de script con exit=0; revisar salida y marcador final.
log=$(mktemp)
trap 'rm -f "$log"' EXIT
if ! "$godot_editor" --headless --path "$PWD/Godot" --editor --import --quit >"$log" 2>&1; then
  cat "$log"
  exit 1
fi
if grep -Eq 'SCRIPT ERROR:|^ERROR:' "$log"; then
  cat "$log"
  exit 1
fi
echo "OK · importación Godot sin errores"
for test in test_session test_world test_xr; do
  if ! "$godot_editor" --headless --path "$PWD/Godot" --quit-after 1200 --script "tests/$test.gd" >"$log" 2>&1; then
    cat "$log"
    exit 1
  fi
  cat "$log"
  # Godot 4.6 headless cierra el renderer dummy con fugas de RID tras una escena 3D.
  # Es el apagado del motor, no un error de script. El resto de ERROR sigue fallando.
  if grep -E 'SCRIPT ERROR:|^ERROR:' "$log" | grep -Ev 'Pages in use exist at exit in PagedAllocator|RID allocations of type' | grep -q . \
    || ! grep -Eq 'checks / 0 failures' "$log"; then
    exit 1
  fi
done
