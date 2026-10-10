# Protocolo Salida · Godot

Escape room en **Godot 4.6.3 Standard**, con GDScript y renderer Compatibility. El proyecto es autónomo: copia esta carpeta completa para abrirlo en otro equipo. El código y el catálogo están en `Game/`; los avisos del motor, en `ThirdParty/godot/`.

## Abrir y jugar

1. Descarga Godot 4.6.3 **Standard** desde [el archivo oficial](https://godotengine.org/download/archive/4.6.3-stable/). No se requiere la edición .NET ni una licencia de pago.
2. En el gestor de proyectos elige **Importar** y selecciona `Godot/project.godot`.
3. Espera la importación y pulsa **F6** con `Game/Levels/main.tscn` abierta, o **F5** para ejecutar el proyecto.
4. Entra a la oficina. El reloj de la operación empieza al cerrar el briefing.

En este workspace se preparó una copia local del editor en `.tools/godot-4.6.3/Godot`, excluida de Git. El script de inicio la detecta; después de clonar en otro equipo instala Godot o define `GODOT_BIN`. En Mac también detecta `/Applications/Godot.app`.

Para iniciar desde la raíz del repositorio:

```bash
bash scripts/play-godot.sh
```

Si el ejecutable tiene otra ubicación:

```bash
GODOT_BIN="/ruta/Godot" bash scripts/play-godot.sh
```

## Juego migrado

- PC en primera persona: **WASD**, mouse, **Shift**, **E/clic**, **G**, **Esc**.
- Oficina y archivo con colisiones, puerta animada, escritorio, estantes, ventana estilizada y corredor de salida.
- Mobiliario con bordes biselados y detalles: sillas con ruedas, estantes abiertos, teclado, carpetas, plantas y manillas.
- Cinco guías animados de asistencia grabada, uno por espacio; **E/clic** abre explicación y ayuda según tu progreso.
- Trece pasos y once conceptos en tres salas, cargados desde `Game/Features/Quests/condor.json`.
- Banderas, reloj de 60 minutos, pistas por radio y salida de emergencia CÓNDOR LIBRE.
- Terminal local de estado: solo `help`, `scan`, `map`, `notes` y `clear`. No hace falta para terminar.
- Cuaderno con los conceptos ya aplicados. La ayuda de un guía no cambia banderas.
- Guardado local versión 3, reanudación, copia `.bak` y reinicio con confirmación. Una partida v1 o v2 no se convierte.

La historia es Operación Cóndor Silencioso: la UCD, agencia ficticia en Santiago, con un insider de aseo y la lista de informantes. El interruptor de victoria es `7391`. La salida de emergencia y la palabra CÓNDOR LIBRE detienen el ejercicio sin castigo. Véase [el recorrido](../docs/diseno/07-lockdown-fbi.md).

## Desarrollo

| Archivo | Función |
| --- | --- |
| `Game/Levels/main.tscn` | Entrada del juego y oficina instanciada |
| `Game/Levels/office.tscn` | Oficina horneada, editable en el editor |
| `Game/Bootstrap/main.gd` | Conecta objetos, reglas y UI. No elige las clases concretas |
| `Game/Factories/` | Fábricas del jugador (PC u OpenXR), de las salas y de los interactuables |
| `Game/Features/Quests/terminal.gd` | Estado del incidente; rechaza cualquier comando fuera de help, scan, map, notes y clear |
| `Game/Features/Quests/session.gd` | Banderas, reloj, pistas, modos y guardado de la operación |
| `Game/Features/Quests/condor.json` | Catálogo v3: once conceptos y trece pasos |
| `Game/Features/Player/desktop_player.gd` | Movimiento y raycast PC |
| `Game/Features/Player/xr_player.gd` | Base OpenXR experimental |
| `Game/Shared/interactable.gd` | Identidad, agarre y resaltado |
| `Game/Shared/game_ui.gd` | Interfaz PC y viewport espacial |
| `Game/Levels/office_builder.gd` | Construcción de la oficina |
| `Game/Infrastructure/progress_store.gd` | Guardado de la partida |
| `tools/bake_office.gd` | Regenera la escena desde el constructor |

Edita la oficina directamente en `Game/Levels/office.tscn`. Regenerar con el constructor sobrescribe esos cambios y debe reservarse para reconstruir el layout.

La oficina es geometría ligera con albedos de roble, yeso y tejido en `Game/Shared/Textures/`. [Cómo editar assets y guías](docs/ASSETS_Y_GUIAS.md).

## Contenido y partidas

El catálogo jugable es `Game/Features/Quests/condor.json` (esquema v3): once conceptos y trece pasos de `act`, cada uno con manual, tres pistas, debrief y solución. `missions.json` sigue en la carpeta para el filtro de exportación y la sesión ya no lo usa como progreso.

```bash
python3 scripts/sync-godot-content.py --check
node scripts/validate-content.mjs
bash scripts/validate-mission-flow.sh
```

El bucle es una máquina de banderas. `act` devuelve cadena vacía cuando el paso es correcto. En Operación el reloj dura 60 minutos; al llegar, la lista sale del edificio. En Aprendizaje el reloj se ve y no hay derrota. La terminal acepta solo `help`, `scan`, `map`, `notes` y `clear`.

Guardado Godot: `user://escape-progress-v1.json`. En Linux normalmente está en `~/.local/share/godot/app_userdata/Protocolo Salida/`; en Mac en `~/Library/Application Support/Godot/app_userdata/Protocolo Salida/`. El nombre del archivo se conserva para encontrar partidas anteriores. El contenido usa versión 3. Una partida v1 o v2 no se convierte: la sesión avisa y empieza una operación nueva. Un JSON dañado no se borra ni pisa la partida en memoria. Antes de reemplazar un guardado válido se deja una copia `.bak`.

## Pruebas y exports

```bash
GODOT_BIN="/ruta/Godot" bash scripts/test-godot.sh
GODOT_BIN="/ruta/Godot" bash scripts/export-godot.sh Linux
GODOT_BIN="/ruta/Godot" bash scripts/export-godot.sh Mac
```

Exportar requiere las **export templates de la misma versión**: Editor → Manage Export Templates → Download and Install. Los presets Desktop ya están en `export_presets.cfg`. Ver [Mac y Builds](docs/MAC_Y_BUILDS.md).

[Estado de validación y capturas](docs/VALIDACION.md).

## Meta Quest 3S

Se incluyen rig, controladores y una UI espacial inicial, conectados a las mismas reglas que PC. Es código experimental: **todavía no probado en visor, sin APK certificado ni rendimiento medido**. Consulta [la ruta de pruebas y despliegue](docs/QUEST_3S.md). PC es el modo por defecto; OpenXR se activa al arrancar la variante VR.

## Licencia del motor

Godot permite uso comercial con licencia MIT, sin suscripciones ni regalías del motor. El contenido del juego conserva sus propios derechos. Se incluyen `ThirdParty/godot/GODOT_LICENSE.txt` y `ThirdParty/godot/GODOT_COPYRIGHT.txt` con los avisos del motor y sus dependencias. [Fuente oficial](https://godotengine.org/license/).
