#!/bin/bash
# Valida condor.json: 11 conceptos, 13 pasos, pistas, soluciones y sin herramientas reales.
set -euo pipefail
cd "$(dirname "$0")/.."

echo "== Validando condor.json"
python3 scripts/sync-godot-content.py --check
if command -v node >/dev/null 2>&1; then
  node scripts/validate-content.mjs
fi
echo "✓ Catálogo v3 de la operación"
