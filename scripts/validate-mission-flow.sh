#!/bin/bash
# Valida que missions.json sea JSON válido y tenga la estructura esperada
set -e

echo "== Validando missions.json"
JSON_FILE="Assets/Resources/Content/missions.json"

if [ ! -f "$JSON_FILE" ]; then
  echo "ERROR: $JSON_FILE no encontrado"
  exit 1
fi

# Verificar que sea JSON válido
if command -v python3 &> /dev/null; then
  python3 -c "import json; json.load(open('$JSON_FILE'))" && echo "✓ JSON válido"
else
  echo "⚠ python3 no disponible, omitiendo validación JSON"
fi

# Verificar estructura básica
if grep -q '"id":"recon"' "$JSON_FILE" && \
   grep -q '"id":"social"' "$JSON_FILE" && \
   grep -q '"evidence"' "$JSON_FILE"; then
  echo "✓ Estructura esperada presente (recon, social, evidence)"
else
  echo "ERROR: Falta estructura esperada en missions.json"
  exit 1
fi

echo "✓ missions.json válido para demo playable"
