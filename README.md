# Operación Cóndor Silencioso

«El enemigo ya tiene la llave.»

Eres, en primera persona, el equipo de respuesta a incidentes de la Unidad de Ciberdelitos (UCD), agencia ficticia en Santiago. A las 22:00 una falsa prueba de emergencia deja el edificio en lockdown. Un insider con root copia la lista de informantes protegidos, ficticia. Si sale del edificio, hay vidas en riesgo dentro de la ficción. Quedan 60 minutos. La salida de emergencia del juego permanece abierta. La palabra de seguridad es **CÓNDOR LIBRE**.

El recorrido son tres salas: oficina (detectar), servidores (contener) y laboratorio (identificar y detener). El desenlace de la ficción es el código `7391`. [Biblia](docs/historia/BIBLIA.md) y [diseño del recorrido](docs/diseno/07-lockdown-fbi.md). Este archivo no da las salas nuevas por construidas.

## Comando para iniciar

```bash
cd /home/cocus/Documentos/mvp_juegos/mvp_hacking_conceptual
bash scripts/play-godot.sh
```

## Mapa

| Carpeta | Para qué |
| --- | --- |
| [docs/diseno/](docs/diseno/README.md) | Estructura de diseño. Empieza aquí si vas a cambiar espacios, interfaz o arte. |
| [Godot/](Godot/README.md) | Juego. El código está en `Godot/Game/`. |
| [docs/ARQUITECTURA_JUEGO.md](docs/ARQUITECTURA_JUEGO.md) | Qué carpeta contiene cada sistema. |
| `Godot/Game/Features/Quests/missions.json` | Catálogo del prototipo anterior. Sigue en disco. El progreso de Cóndor es `condor.json`, cuando el catálogo lo escriba. |
| [docs/historia/](docs/historia/BIBLIA.md) | Canon de Cóndor y acto 1. `MISIONES_SIGUIENTES.md` es archivo histórico y no se vuelca. |
| `docs/` | Diseño, historia, presupuesto 8 GB y contrato de contenido. |
| `scripts/` | Arranque, validación del JSON, pruebas y export. |
| `Builds/` | Salidas generadas. No se editan a mano. |

## Desarrollo en Godot

La migración jugable está en **[Godot/](Godot/README.md)**, con Godot **4.6.3 Standard**. Importa **`Godot/project.godot`** en el gestor de proyectos y pulsa **F5**. El árbol conserva la oficina heredada. El canon de la partida es el recorrido de tres salas descrito arriba; este README no da esa construcción por hecha.

**PC sigue en primera persona**: WASD caminar, mouse mirar, E/clic usar, G tomar/soltar, Shift correr y Esc menú. Se incluyen cámara y controles OpenXR experimentales para desarrollar la adaptación a Meta Quest 3S.

- [Inicio, arquitectura y estado Godot](Godot/README.md).
- [Assets mejorados y cinco guías animados por espacio](Godot/docs/ASSETS_Y_GUIAS.md).
- [Renderizar en Mac y generar builds](Godot/docs/MAC_Y_BUILDS.md).
- [Base VR y despliegue Quest 3S](Godot/docs/QUEST_3S.md).
- [Rutas de builds Godot](Builds/Godot/README.md).

Si el script no encuentra Godot, indica la ruta de su ejecutable desde la raíz del repositorio:

```bash
GODOT_BIN="/ruta/Godot" bash scripts/play-godot.sh
```

## Bucle de la operación

1. Briefing de las 22:00. Al cerrarlo corre el reloj de 60 minutos en modo Operación.
2. Oficina: correo falso, clave, segundo factor, registros y caja `4714`. La puerta magnética abre servidores.
3. Servidores: fila azul, cable azul y fail-safe de luz con la lámpara LED. El laboratorio se abre 90 segundos.
4. Laboratorio: ficha de V. Soto, nota, inventario, huellas e interruptor `7391`.
5. Debriefing de diez lecciones y la pregunta «¿Qué vas a cambiar mañana?».

En Operación, el reloj a cero termina la partida. En Aprendizaje el reloj se ve y no hay derrota. **CÓNDOR LIBRE** pausa sin castigo. El ciclo de pizarra, bandejas y examen es el prototipo anterior y no mide esta operación. No hay comandos de ataque reales ni conexiones a equipos externos.

## Contenido y verificación

El JSON que estos comandos revisan hoy es **`Godot/Game/Features/Quests/missions.json`**, el catálogo heredado. El canon de la operación está en la biblia. Tras editar ese archivo:

```bash
python3 scripts/sync-godot-content.py --check
node scripts/validate-content.mjs
GODOT_BIN="/ruta/Godot" bash scripts/test-godot.sh
```

Para repetir las comprobaciones automáticamente mientras se modifica el proyecto:

```bash
python3 scripts/colab_watch.py --watch --validate
```

Se detiene con **Ctrl+C**. [Qué revisa y fallos corregidos](docs/REVISION_EN_VIVO.md).

La oficina usa geometría ligera, albedos 1K y una primera capa de detalle importada en GLB. El playtest en Meta Quest 3S sigue pendiente; la base XR aún no acredita rendimiento ni comodidad en hardware.

[Licencia comercial de Godot](https://godotengine.org/license/): sin suscripción ni regalías del motor. Los avisos del motor se incluyen en `Godot/ThirdParty/godot/`.
