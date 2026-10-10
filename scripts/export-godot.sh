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
platform=${1:-Linux}
mode=${2:-release}
case "$platform" in
  Linux) output="$PWD/Builds/Godot/Linux/ProtocoloSalida.x86_64" ;;
  Windows) output="$PWD/Builds/Godot/Windows/ProtocoloSalida.exe" ;;
  Mac) output="$PWD/Builds/Godot/Mac/ProtocoloSalida.zip" ;;
  Quest) output="$PWD/Builds/Godot/Quest/ProtocoloSalida.apk"
    platform="Quest 3S"
    mode=${2:-debug}
    ;;
  *) echo "Uso: bash scripts/export-godot.sh Linux|Windows|Mac|Quest [debug|release]" >&2; exit 2 ;;
esac
case "$mode" in
  debug|release) ;;
  *) echo "El modo debe ser debug o release." >&2; exit 2 ;;
esac
python3 scripts/sync-godot-content.py --check
python3 scripts/check-godot-structure.py
mkdir -p "$(dirname "$output")"
log=$(mktemp)
trap 'rm -f "$log"' EXIT
run_checked() {
  if ! "$godot_editor" "$@" >"$log" 2>&1; then
    cat "$log"
    exit 1
  fi
  if grep -Eq 'SCRIPT ERROR:|^ERROR:' "$log"; then
    cat "$log"
    exit 1
  fi
}
run_checked --headless --path "$PWD/Godot" --editor --import --quit
run_checked --headless --path "$PWD/Godot" "--export-$mode" "$platform" "$output"
echo "Build Godot: $output"
