# Protocolo Salida · Escape room

Hackers con acceso root toman el edificio del FBI en Nueva York y activan un lockdown con el equipo dentro. Amenazan con soltar todos los ascensores. La historia central sigue una fuga por ductos y un incendio que anula el bloqueo. Nombres de atacantes, origen del acceso y desenlace pendientes de confirmación.

El juego se rediseña como un escape room continuo, local y en primera persona. [Diseño del recorrido, espacios y acertijos](docs/diseno/07-lockdown-fbi.md). La demo actual cubre la oficina; ductos, ascensores y emergencia aún no son jugables.

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
| `Godot/Game/Features/Quests/missions.json` | Único catálogo de misiones. |
| [docs/historia/](docs/historia/BIBLIA.md) | Premisa, acto 1 y misiones que todavía no están en el catálogo. |
| `docs/` | Diseño, historia, presupuesto 8 GB y contrato de contenido. |
| `scripts/` | Arranque, validación del JSON, pruebas y export. |
| `Builds/` | Salidas generadas. No se editan a mano. |

## Desarrollo en Godot

La migración jugable está en **[Godot/](Godot/README.md)**, con Godot **4.6.3 Standard**. Importa **`Godot/project.godot`** en el gestor de proyectos y pulsa **F5**. El proyecto incluye oficina editable, cuatro fases, quince pistas, clasificación física, expedientes, seguros de salida y guardado.

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

## Bucle del escape room

1. Pizarra: aceptar una fase.
2. Archivo: abrir la puerta y leer/tomar las carpetas.
3. Escritorio: elegir bandeja de observación y después bandeja de defensa.
4. Cuaderno: revisar conceptos desbloqueados.
5. Buzón: validar diagnóstico y comprobaciones del expediente.
6. Panel de salida: resolver dos situaciones nuevas, justificar las decisiones y activar el seguro. Leer el cuaderno o terminar el quiz no basta.
7. Repetir las cuatro fases y usar la puerta para entrar a la corredor de la oficina.

Fases actuales del prototipo de oficina: **Edificio en lockdown · Cortar el hilo · Recuperar nuestra identidad · Abrir la oficina**. Las pruebas confirman control root y bloqueo, sin atribuir todavía el método de acceso inicial. Superar la oficina no termina la historia. No hay comandos de ataque reales ni conexiones a equipos externos.

## Contenido y verificación

El catálogo está en **`Godot/Game/Features/Quests/missions.json`**. Tras editarlo:

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

## Ajuste de texturas y materiales · 2026-10-07

Se reutilizan los tres albedos 1K existentes. Se ajustaron el tinte multiplicado por la textura, la escala de repetición y la mezcla triplanar para hacer legibles madera, yeso y tejido. Las sillas tienen acabado textil en asiento y respaldo; los metales usan reflejos moderados. El filtrado anisotrópico y los mipmaps conservan detalle en superficies vistas en ángulo. La luz ambiental es más neutra y las luces permiten leer la respuesta especular.

Los ajustes se guardan en `office.tscn` y en el constructor. Para volver a aplicar solo acabados e iluminación a la escena editada, sin reconstruir el layout:

```bash
.tools/godot-4.6.3/Godot --headless --path Godot --script tools/refresh_materials.gd
```

El actualizador conserva geometría, colisiones y colores de origen mediante metadatos. Repetirlo no vuelve a aclarar los materiales. La escena mantiene 49.572 triángulos, texturas de hasta 1024 px y ningún mapa normal.

[Referencia de materiales de Godot](https://docs.godotengine.org/en/stable/classes/class_basematerial3d.html). Capturas actuales: [oficina](previews/office-pc.png), [materiales de cerca](previews/materials-pc.png) y [aplicación de conceptos](previews/application-pc.png).
