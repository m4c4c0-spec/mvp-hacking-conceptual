#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/.."

if [ "${1:-}" = "--help" ]; then
  echo "Uso: bash scripts/connect-quest-usb.sh"
  echo "Abre WiVRn en un Quest conectado por USB y autorizado para depuración."
  echo "Antes: abre wivrn-dashboard e instala su cliente en el visor."
  exit 0
fi
if [ "$#" -ne 0 ]; then
  echo "Este comando no acepta argumentos. Usa --help para ver las instrucciones." >&2
  exit 2
fi
if ! command -v adb >/dev/null 2>&1; then
  echo "Falta ADB. En Fedora: sudo dnf install android-tools" >&2
  exit 2
fi

# -d selecciona un dispositivo USB; no actúa sobre conexiones ADB por Wi-Fi.
if ! state=$(adb -d get-state 2>&1) || [ "$state" != "device" ]; then
  echo "No hay un único visor USB autorizado: $state" >&2
  echo "Conecta un cable de datos, activa modo desarrollador y acepta la depuración USB en el visor." >&2
  adb devices -l >&2
  exit 2
fi
model=$(adb -d shell getprop ro.product.model)
model=${model//$'\r'/}
if [[ "$model" != *Quest* ]]; then
  echo "El dispositivo USB es '$model'; conecta un Meta Quest para continuar." >&2
  exit 2
fi

client_package=""
for candidate in org.meumeu.wivrn org.meumeu.wivrn.github; do
  if package_path=$(adb -d shell pm path "$candidate" 2>/dev/null) && [[ "$package_path" == package:* ]]; then
    client_package=$candidate
    break
  fi
done
if [ -z "$client_package" ]; then
  echo "No se encontró el cliente WiVRn de Meta Store o GitHub en '$model'." >&2
  echo "Instálalo desde el asistente de WiVRn; cliente y servidor deben tener la misma versión." >&2
  exit 2
fi
if ! pgrep -x wivrn-server >/dev/null; then
  echo "Abre wivrn-dashboard y arranca su servidor antes de conectar el visor." >&2
  exit 2
fi

adb -d reverse tcp:9757 tcp:9757
launch_output=$(adb -d shell am start -W -a android.intent.action.VIEW -d "wivrn+tcp://localhost" "$client_package" 2>&1)
printf '%s\n' "$launch_output"
if [[ "$launch_output" == *"Error:"* || "$launch_output" == *"Error type"* ]]; then
  echo "Android no pudo abrir WiVRn; comprueba la instalación del cliente." >&2
  exit 1
fi
echo "WiVRn abierto en $model por USB. Completa el emparejamiento dentro del visor."
echo "Cuando WiVRn confirme la conexión, ejecuta: bash scripts/play-vr.sh"
