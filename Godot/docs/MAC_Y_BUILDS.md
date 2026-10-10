# Mac y carpeta de builds · Godot

## Renderizar y probar en Mac

1. Instala Godot **4.6.3 Standard** para macOS, compatible con Intel y Apple Silicon.
2. Importa `Godot/project.godot` desde el gestor de proyectos; pulsa F5.
3. Godot carga `Game/Levels/main.tscn`. La oficina renderiza con Compatibility; conserva primera persona WASD/mouse/E/G para probar el recorrido completo.

Esta ruta funciona desde el código fuente y no depende de que exista un ejecutable en `Builds`.

## Exportar una aplicación

1. En el editor instala **Manage Export Templates → Download and Install**, usando la misma versión del editor.
2. Abre **Project → Export**. El preset **Mac** ya está definido como universal Intel + Apple Silicon.
3. Crea la carpeta `Builds/Godot/Mac` en la raíz del repo y exporta allí `ProtocoloSalida.zip`.
4. Extrae el ZIP y abre la aplicación completa. No copies solo su ejecutable interno.

Desde Terminal con Godot en `/Applications`:

```bash
GODOT_BIN="/Applications/Godot.app/Contents/MacOS/Godot" bash scripts/export-godot.sh Mac
```

Desde Linux también se puede generar el ZIP Mac con las templates instaladas. Una exportación cruzada no acredita el arranque en Mac; compruébalo en el equipo del cliente.

La firma Developer ID y notarización están desactivadas en este preset de demo. Distribuir sin firma puede requerir aprobación manual de macOS; para una entrega empresarial sin avisos de procedencia habrá que preparar la firma con la identidad del distribuidor. Los costos de identidad o tiendas son independientes de la licencia gratuita de Godot.

## Rutas versionadas

```text
Builds/Godot/
  README.md
  Linux/.gitkeep
  Windows/.gitkeep
  Mac/.gitkeep
  Quest/.gitkeep
```

Los binarios se generan y no se guardan en Git. Los presets de PC están preparados; el preset Android/Quest se configura después de instalar SDK/JDK y validar OpenXR, según `QUEST_3S.md`.

El comando `export-godot.sh` genera Linux, Windows o Mac y crea sus directorios. No presupone que existan las templates ni entrega un APK todavía.
