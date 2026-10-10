#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/.."
quest_apk=${1:-"$PWD/Builds/Godot/Quest/ProtocoloSalida.apk"}
quest_adb=${ADB_BIN:-adb}
if ! command -v "$quest_adb" >/dev/null 2>&1; then
  echo "Instala Android SDK Platform-Tools o define ADB_BIN con la ruta de adb." >&2
  exit 2
fi
if [ ! -f "$quest_apk" ]; then
  echo "No existe $quest_apk. Genera la APK con bash scripts/export-godot.sh Quest." >&2
  exit 2
fi
# -s permite seleccionar un visor cuando hay varios dispositivos Android conectados.
quest_device=()
if [ -n "${QUEST_SERIAL:-}" ]; then
  quest_device=(-s "$QUEST_SERIAL")
fi
if ! "$quest_adb" "${quest_device[@]}" get-state >/dev/null; then
  echo "Conecta el Quest por USB, activa modo desarrollador y acepta la depuración dentro del visor." >&2
  exit 2
fi
"$quest_adb" "${quest_device[@]}" install -r "$quest_apk"
"$quest_adb" "${quest_device[@]}" shell am start -n cl.protocolo.salida/com.godot.game.GodotApp
