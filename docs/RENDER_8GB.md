# Presupuesto de render a 8 GB

Protocolo Salida cabe en un computador de 8 GB y en unas Meta Quest 3S. En los dos sitios el límite es la RAM del sistema: el visor no tiene VRAM aparte, y el PC modesto se trata igual, sin contar con una tarjeta dedicada. Hay que dejar sitio para el sistema operativo y, en el visor, para el compositor. El proceso del juego se orienta a quedar por debajo de unos **2,5 GB**.

Ese número no es una medición hecha en un dispositivo. Es el techo con el que se eligieron los ajustes. `RenderBudget.warn_if_heavy()` mira `OS.get_static_memory_usage()`, que es la memoria estática del motor, no el RSS del proceso, y avisa si pasa de 2560 MB.

## Qué cambió en `project.godot`

Se tocaron solo las secciones `display`, `rendering` y `xr`. El nombre del proyecto, `run/main_scene` y la sección `physics` siguen como estaban. La física ya estaba en **72** ticks, alineada al objetivo de 72 Hz de Quest descrito en `docs/META_QUEST_3S.md`.

`[importer_defaults]` ya limitaba las texturas a 1024, pedía compresión de VRAM y desactivaba mallas de sombra, tangentes y LOD. No se reescribió.

### Display

Ventana y viewport de diseño a **1280×720**. El estirado sigue en `canvas_items` para que el texto de las lecciones no se escale como una textura borrosa. El aspecto es `expand`. La sincronía vertical queda activada (`vsync_mode=1`). En Quest, el runtime de OpenXR marca el ritmo; esta sincronía es la del escritorio.

### Rendering

| Ajuste | Escritorio | Móvil / Quest |
| --- | --- | --- |
| Renderer | `gl_compatibility` | `gl_compatibility` |
| Escala 3D | 1 | 0,8 (`.mobile`) |
| Modo de escala | bilinear (`0`) | el mismo |
| MSAA 3D | `1` (2×) | `0` (apagado) |
| Sombra direccional | 1024, 16 bits, filtro duro (`0`) | igual |
| Atlas posicional | 1024, 16 bits, filtro duro | igual |
| Anisotropía | `1` (2×) | la misma clave |
| Oclusión | `use_occlusion_culling=true` | la misma clave |
| Lambert en lugar de Burley | activado | activado |
| Luces Compatibility | 4 por objeto, 8 posicionales por fotograma | las mismas claves |

También quedan apagados el antialiasing de espacio de pantalla, el TAA, el debanding y el framebuffer HDR 2D. El recorte más estricto de casters de sombra queda activado.

El modo FSR no se usa: en Godot 4.6 solo tiene efecto con Forward+, y este proyecto conserva GL Compatibility.

### XR

`openxr/enabled` sigue en **false**. Si estuviera en true, Godot inicializaría OpenXR al arrancar y el escritorio pediría un visor. `startup_alert` está en false para no mostrar un modal si esa inicialización fallara. La foveación queda preparada en nivel **2** (medio: 0 ninguno, 1 bajo, 2 medio, 3 alto) y sin ajuste dinámico. `shaders/enabled` sigue en true para tener los shaders de XR cuando el arranque condicional sí los necesite.

En plataformas que no son Android, Godot desactiva la foveación si el MSAA 3D del proyecto está encendido. En el export móvil el MSAA queda en 0, así que la foveación puede aplicarse en el visor.

## Cómo el script separa PC y Quest

`Godot/scripts/quality_profile.gd` define `QualityProfile`. No conoce `EscapeSession`.

- Sin interfaz OpenXR inicializada, el perfil activo es **Desktop8Gb**: escala 3D 1 y MSAA 2×.
- Si la interfaz ya está inicializada, el perfil es **Quest3S**: escala 3D 0,8, sin MSAA, y la sombra direccional se acorta a 10 m (en PC queda en 18 m, una sola cascada ortogonal, desenfoque 0).
- El arranque de OpenXR es opt-in. Solo se llama a `initialize()` si el proceso recibió `--xr` o si el export trae la feature `quest`. Es la misma condición que ya usa la escena. Un arranque de escritorio no entra por ahí.
- Como máximo una luz direccional conserva la sombra. Omni y spot quedan sin sombra.
- En el `Environment` se apagan glow, niebla, niebla volumétrica, SSAO, SSIL, SDFGI y reflejos en espacio de pantalla. Godot 4.6 no tiene una clave de proyecto que apague el glow o la volumetría: viven en el recurso `Environment`.
- Los props cuyo AABB mide menos de 0,6 m en cada eje dejan de proyectar sombra.
- Cada `apply()` fija `Engine.physics_ticks_per_second` en 72. No se activa la interpolación física: el giro por pasos del visor tiene que seguir siendo instantáneo.

La escala 0,8 y el MSAA 0 del export Android también están en el proyecto, con el sufijo `.mobile`, aunque nadie haya llamado al script. La distancia de sombra y el tope de una sola luz solo ocurren cuando `QualityProfile.apply(tree)` corre.

No hay autoload. Registrarlo exigiría una sección distinta de display, rendering y xr. La fase 1 puede llamar a `apply(get_tree())` al inicio de `main._ready`, antes de crear al jugador.

## Claves que existen y claves que no

Comprobado contra `ProjectSettings` de Godot 4.6.3. No se inventaron nombres.

La oclusión activada es `rendering/occlusion_culling/use_occlusion_culling`. En la referencia de Godot 4.6 esa clave permite usar nodos `OccluderInstance3D` y advierte un coste de CPU. No hay otra clave de “oclusión por hardware” ni de occlusion queries de GPU. Sin occluders en los muros grandes, el flag no recorta la oficina.

No hay clave de proyecto para el número de luces que proyectan sombra ni para la distancia de la sombra direccional. Eso lo hace el script. Tampoco hay un interruptor de proyecto para glow o niebla volumétrica.

## Relación con el conteo de la escena

`RenderBudget` sigue midiendo triángulos, mapas de normales y texturas de más de 1024. El conteo distingue el tipo de malla: `PrimitiveMesh` usa `get_mesh_arrays`, `ArrayMesh` usa `surface_get_primitive_type` y el resto usa `get_faces()`. Así un `BoxMesh` no recibe un método que Godot solo expone en `ArrayMesh`. La cámara lejana de 40 m sigue siendo `RenderBudget.CAMERA_FAR`; este perfil no la pisa.
