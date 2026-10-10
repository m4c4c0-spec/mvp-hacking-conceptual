#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/.."
# El flag del motor habilita OpenXR antes de crear el renderer; --xr selecciona el rig.
exec bash scripts/play-godot.sh --xr-mode on "$@" -- --xr
