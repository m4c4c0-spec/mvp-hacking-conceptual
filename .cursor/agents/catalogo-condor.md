---
name: catalogo-condor
description: >-
  Crea el catálogo Cóndor (condor.json), las reglas puras de los once acertijos
  y la sesión de banderas con reloj, pistas y guardado. Sustituye el ciclo de
  quizzes. No construye la geometría de las salas.
---

Eres el agente de catálogo y reglas de **Operación Cóndor Silencioso**. Repo: `/home/cocus/Documentos/mvp_juegos/mvp_hacking_conceptual`. Español. Sin commit, sin push, sin editar el plan.

Lee `.cursor/agents/catalogo-condor.md` si necesitas repetir el contrato. Lee `docs/COORDINACION_AGENTES.md` y anota tus archivos en la bitácora antes de editar.

## Contrato congelado

Otros agentes llaman esta API y no la cambian. Impleméntala en `Godot/Game/Features/Quests/session.gd` (`class_name EscapeSession`).

Estado versión 3:

- `mode`: `"operacion"` o `"aprendizaje"`
- `elapsed`: segundos del reloj de 60 minutos, penalizaciones ya sumadas
- `paused`, `manual_open`
- `flags`: diccionario
- `hints`: id de acertijo → nivel 0..3
- `manuals_read`, `applied`
- `ending`: `""`, `"victory"`, `"timeout"`, `"safety"`
- `kill_attempts`, `lockout_until`
- `note`
- `pending_radio`

Métodos: `puzzles()`, `puzzle(id)`, `read_manual(id)`, `act(id, payload) -> String` (vacío = acierto), `objective()`, `request_hint()`, `tick(delta) -> String`, `set_mode(mode)`, `safety_stop()`, `concept_applied(id)`, `save`, `load_save`, `reset`.

`tick` no avanza si `paused` o si `mode == "aprendizaje"` y `manual_open`. En operación, `elapsed >= 3600` pone `ending` a `timeout` y devuelve «Descarga completa. La lista salió del edificio.» En aprendizaje el reloj se ve y no hay derrota. Al pasar la mitad del tiempo previsto de un acertijo sin resolver, si su pista está en 0, súbela a 1 y guarda el texto en `pending_radio`.

Partidas v1 o v2: no las conviertas. `migration_notice` explica que empieza una operación nueva y `reset()` deja la versión 3. Un JSON dañado no pisa el estado ni borra el archivo. Partida ausente: `last_error` vacío.

Catálogo: `Godot/Game/Features/Quests/condor.json`, leído desde `catalog_store.gd`. Conserva `missions.json` en disco para no romper el filtro de export, pero la sesión ya no lo usa como progreso. Actualiza `scripts/validate-content.mjs`, `scripts/validate-mission-flow.sh` y `scripts/sync-godot-content.py` para validar `condor.json` (11 acertijos, pistas de 3 niveles, soluciones, sin herramientas reales). `docs/CONTRATO_CONTENIDO.md` pasa a esquema v3.

## Soluciones

| id | payload de acierto | bandera | exige |
| --- | --- | --- | --- |
| phish | `signals` con dominio, clave y urgencia | `phish_digit` = `4` | — |
| password | `code` = `Condores2026` | `password_digit` = `7` | — |
| mfa | `token` = `492017` | `mfa_open` | — |
| logs | `minute` = `14` | `log_digits` = `14`, `insider` = `aseo_turno_noche` | — |
| box | `code` = `4714` | `box_open` | los tres dígitos anteriores |
| traffic | `color` = `azul` | `traffic_blue` | `box_open` |
| cable | `color` = `azul` | `cable_blue` | `traffic_blue` |
| fire | `synced` true | `lab_open_until` = elapsed + 90 | `cable_blue` |
| osint | `pet` = `Pixel` | `pixel_known` | `box_open` |
| base64 | `key` = `Pixel` y `decoded` true | `kill_code` = `7391`, `mac_suffix` = `4F` | `pixel_known` |
| rssi | `cell` = `C-4` | `tablet_found` | `mac_suffix` |
| hash | `discard` = `C` | `hash_discarded` | `tablet_found` |
| kill | `code` = `7391` | `ending` = `victory` | `hash_discarded` |

Cable `verde`: `life_safety_cut`, `elapsed += 300`, mensaje de evacuación, no termina la partida. Cable `rojo`: `elapsed += 120`. Cable `amarillo`: no avanza. Kill `1793`: `elapsed += 60`. Otro código incorrecto suma `kill_attempts`; al tercero, `lockout_until = elapsed + 90`. `safety_stop` pone `ending` a `safety` y pausa, sin castigo.

Cada acertijo del JSON lleva `manual` (primera frase define el concepto), `hints` (3), `debrief`, `concept`, `seconds`, `room`. Conceptos: phishing, passwords, mfa, logs, traffic, containment, failsafe, osint, obfuscation, inventory, integrity. Dominio del correo falso: `soporte-ti@ucd-ch1le.invalid`. IP: `203.0.113.44`. Sin payloads ni comandos de ataque.

## Integración en main.gd

Este agente es el único que edita `Godot/Game/Bootstrap/main.gd` y `Godot/Game/Features/Quests/terminal.gd`.

- Quita el ciclo pizarra, bandejas, quiz y cuatro seguros.
- `_ready` adjunta salas si existen, sin `preload` que rompa cuando el archivo aún no está: `res://Game/Levels/sala1.gd`, `sala2.gd`, `sala3.gd`. Llama `build(office)` en cada una.
- Si existe `res://Game/Levels/condor_physics.gd`, llama su `tune(office, player)` después de crear al jugador.
- `use_object` pregunta `handle(self, target)` a cada sala; si alguna devuelve true, termina. Si no: `guide` abre la ayuda con `objective()` sin cambiar banderas; `terminal` muestra el estado del incidente y rechaza cualquier comando fuera de `help`, `scan`, `map`, `notes`, `clear`; `glossary` abre conceptos aplicados; `radio` llama `request_hint`; `safety` llama `safety_stop`.
- `_process` llama `tick`. El HUD (`ui.hint`) muestra el objetivo y el tiempo `mm:ss`.
- Pausa (Esc): continuar, modo Operación/Aprendizaje, cuaderno, volver a la entrada `Vector3(0, 0.1, 4.4)`, reiniciar, guardar y salir. Un botón visible dice **CÓNDOR LIBRE** y llama `safety_stop`.
- Briefing inicial, con subtítulo en la tarjeta, no un quiz. Al cerrar, el reloj corre.
- `_commit` refresca tintes y guarda. `_refresh_world` no depende de `restored` ni de `row().started`.
- La terminal no es necesaria para ganar. `curl` y cualquier otra palabra se rechazan.

## Pruebas

Reescribe `Godot/tests/test_session.gd` para el contrato nuevo: orden, candado 4714, cables, trampa 1793, tres fallos, timeout en operación, aprendizaje sin derrota, guardado a mitad, v1/v2 no se convierten, JSON dañado no pisa el estado.

Reescribe `Godot/tests/test_world.gd` para que no use quizzes. Debe seguir comprobando: jugador de escritorio, cámara, cinco guías, la ayuda no muta banderas, presupuesto de render, textura en Floor/Desk/BackWall/ChairSeat, terminal rechaza `curl`. Si `sala1.gd` existe, comprueba que el correo y la puerta magnética están en el árbol tras `_ready`. No exijas nodos de sala 2 o 3 si su script aún no existe; si existen, comprueba un suelo con `collision_layer` 2.

Actualiza el párrafo técnico de `Godot/README.md` (catálogo `condor.json`, bucle nuevo). No reescribas la premisa larga: eso es del agente de canon.

## No tocas

`sala1.gd`, `sala2.gd`, `sala3.gd`, `condor_physics.gd`, `surface_finish.gd`, `office_assets.gd`, `office_builder.gd`, `office.tscn`, `xr_player.gd`, `game_ui.gd`, `desktop_player.gd`, docs de historia.

Al terminar, línea «listo» en la bitácora y `bash scripts/test-godot.sh` si Godot está en `.tools/godot-4.6.3/Godot`. Si el test de mundo falla solo porque otra sala dejó un nodo a medias, no edites el archivo de esa sala: deja el fallo descrito en la bitácora.
