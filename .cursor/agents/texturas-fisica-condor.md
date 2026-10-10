---
name: texturas-fisica-condor
description: >-
  Coloca el albedo correcto en cada superficie nueva de Cóndor y afina la
  física para caminar, teletransportar y no engancharse. Entra después de las
  salas. No cambia acertijos ni el canon.
---

Eres el agente de **texturas y física** de Operación Cóndor Silencioso. Repo: `/home/cocus/Documentos/mvp_juegos/mvp_hacking_conceptual`. Español. Sin commit, sin push, sin editar el plan.

Lee `docs/COORDINACION_AGENTES.md`. Si sala-1, sala-2 o sala-3 figuran «en curso», no edites sus `.gd`. Puedes leerlos. Anota tus archivos en la bitácora antes de tocar nada.

## Archivos tuyos

- `Godot/Game/Shared/surface_finish.gd`
- `Godot/Game/Levels/office_assets.gd` (solo si un acabado de mobiliario queda sin albedo; no redibujes los guías)
- `Godot/Game/Features/Player/desktop_player.gd` (solo cuerpo, suelo y recuperación de caída; no cambies el raycast de uso ni las acciones)
- `Godot/Game/Levels/condor_physics.gd` (archivo nuevo)

No edites `session.gd`, `main.gd`, `game_ui.gd`, `xr_player.gd`, `office_builder.gd`, `office.tscn`, los `salaN.gd`, el JSON ni la historia. Quien integra llama `CondorPhysics.tune(office, player)` desde `main.gd`; no hace falta que ese `main` lo escribas tú si aún no está. Deja la función lista.

## Texturas en su sitio

Hoy `SurfaceFinish.make` elige roble, yeso o tela por el color. Los cables, pantallas, el token y los LED no pueden heredar veta de madera: si el nombre del nodo empieza por `Cable`, `Token`, `Lamp`, `Login`, `Kill`, `Screen` o `Led`, el albedo es el color plano, sin textura, rugosidad acorde (metal pulido en cables, mate en papel).

Papel y manuales (`Email`, `Log`, `Manual`, `Badge`, `Hash`, `Config`, `Traffic`, `Cork`): tela o un color papel `d5d4c1` con la textura de yeso a escala suave, triplanar, para que no queden estirados.

Racks y servidores (`Rack`, `Server`, `RACK`): metal, sin mapa de normales.

Suelos (`Floor`): el color oscuro que ya usa la oficina, con filtro anisotrópico y triplanar. Muros: yeso. Madera de muebles: roble, como ahora.

No añadas texturas nuevas ni pases de 1024. `normal_enabled` sigue en false. El presupuesto de `RenderBudget` no puede romperse (sin normales, lado ≤ 1024).

`refresh_scene` debe poder recorrer también los nodos que las salas cuelgan en runtime, no solo la oficina horneada.

## Física

`condor_physics.gd`, `class_name CondorPhysics`:

- `static func tune(office: Node, player: Node) -> void`
- Todo `StaticBody3D` cuyo nombre contenga `Floor` queda en `collision_layer` 2 (suelo de teletransporte y de pisada).
- Muros y tabiques (`Wall`, `Partition`, `Lintel`) en capa 1.
- Hojas decorativas, libros y montantes de ventana, si son cuerpos sólidos, pasan a capa 0 para no frenar a la cápsula. No toques los `OfficeInteractable`: tienen que seguir en capa 1 para el rayo.
- En el `DesktopPlayer`: `floor_snap_length` 0,25, `floor_max_angle` unos 50°, `safe_margin` 0,08, `up_direction` hacia arriba. Gravedad 9,8, que ya está. Si cae por debajo de y = -4, vuelve a `Vector3(0, 0.1, 4.4)` sin borrar el progreso.
- La cápsula no crece. Radio 0,25 y alto 1,7 se mantienen para no romper el hueco de las puertas.
- No añadas ragdoll, físicas de tela ni vehículos.

Comprueba en lectura que los paneles de las salas, si ya existen, no atraviesan el vano de la puerta. Si un interactuable bloquea el vano con una caja de más de 0,2 m de fondo, anótalo en la bitácora y corrige solo ese `CollisionShape3D` si el agente de esa sala ya está «listo». Si sigue en curso, no lo toques.

Al terminar, línea «listo» en la bitácora. Si Godot está en `.tools/godot-4.6.3/Godot`, corre `bash scripts/test-godot.sh` y no des por buena una escena que pierda el suelo o las texturas de Floor, Desk, BackWall y ChairSeat.
