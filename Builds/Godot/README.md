# Salidas Godot

Versionamos las rutas con .gitkeep. Ejecutables locales excluidos de Git.

`bash scripts/export-godot.sh Linux|Windows|Mac` requiere Godot 4.6.3 Standard y export templates compatibles. El ZIP Mac contiene la app universal. El APK Quest necesita completar el preset Android/OpenXR y probar el visor.

[Proyecto Godot](../../Godot/README.md) · [Mac](../../Godot/docs/MAC_Y_BUILDS.md) · [Quest](../../Godot/docs/QUEST_3S.md).

Se generó y verificó `Linux/ProtocoloSalida.pck` como paquete de desarrollo. Para ejecutarlo necesitas Godot 4.6.3, por ejemplo `godot --main-pack /ruta/ProtocoloSalida.pck`. Los ejecutables independientes requieren instalar export templates y usar los presets.
