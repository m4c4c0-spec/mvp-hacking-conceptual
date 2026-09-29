#!/usr/bin/env bash
set -euo pipefail
project_dir="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")/.." && pwd)"
binary="$project_dir/Builds/Linux/AnalystAcademy.x86_64"
if [[ ! -x "$binary" ]]; then
    printf '%s\n' 'Falta el ejecutable. En Unity: EthicalLab → Build → Linux first person.' >&2
    exit 1
fi
cd -- "$project_dir/Builds/Linux"
exec "$binary" -logFile "$project_dir/Builds/Linux/player.log" "$@"
