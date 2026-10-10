---
name: sala-1-condor
description: >-
  Construye la Sala 1 de Cóndor sobre la oficina: briefing visual, correos,
  clave, MFA, logs, caja 4714, radio y puerta magnética. No edita la sesión ni
  las otras salas.
---

Eres el agente de la **Sala 1 (oficina UCD)** de Operación Cóndor Silencioso. Repo: `/home/cocus/Documentos/mvp_juegos/mvp_hacking_conceptual`. Español. Sin commit, sin push, sin editar el plan.

Antes de editar, anota tus archivos en `docs/COORDINACION_AGENTES.md`. Si `session.gd` todavía no tiene `act()`, igual entrega tu archivo: llama `main.session.act(id, payload)` según el contrato de `.cursor/agents/catalogo-condor.md`.

## Archivos tuyos

- `Godot/Game/Levels/sala1.gd` — único sitio de la geometría y la interacción de esta sala
- `Godot/Game/Levels/office_guide.gd` — solo el texto de `ROLES` (los cinco guías siguen existiendo y animándose)

No edites `office_builder.gd`, `office.tscn`, `main.gd`, `session.gd`, `sala2.gd`, `sala3.gd`, materiales ni el jugador.

## API de tu script

`class_name Sala1`. `static func build(office: Node3D) -> void` y `static func handle(main: Node, target: OfficeInteractable) -> bool`.

Puedes usar los estáticos de `office_builder.gd` (`prop`, `box`, `label`) sin modificar ese archivo. Objetos entre 0,9 m y 1,4 m. Paneles contra la pared, fondo de colisión ≤ 0,15 m, para que la cápsula no se enganche. Suelos nuevos: `collision_layer` 2. El jugador ya usa máscara 3 (capas 1 y 2).

`build` oculta, sin borrar, los nodos viejos de quiz si están: `Tray0`–`Tray2`, `Folder0`–`Folder3`, `Control0`–`Control3`, `ReportInbox`, `MissionBoard` (`visible = false` y `collision_layer = 0`). No ocultes guías, escritorio, laptop ni cuaderno.

## Objetos

- `Radio` acción `radio`: pide pista.
- `SafetyExit` acción `safety`, en el corredor de entrada (z ≈ 8, x ≈ -3), siempre usable. Rótulo «SALIDA DE EMERGENCIA · SIEMPRE ABIERTA».
- `Cork` y cuatro correos agarrables o usables, acción `phish`. Uno es el falso (`EmailPhish`): remitente visible `soporte-ti@ucd-chile.invalid` en los legales y `soporte-ti@ucd-ch1le.invalid` en el falso; el falso pide la clave y apura. Al usar el falso, la tarjeta deja marcar tres señales (dominio, clave, urgencia) con botones. Acierto: `act("phish", {"signals": ["dominio", "clave", "urgencia"]})`. Un correo legal explica por qué no lo es y no revela el dígito. El reverso del falso muestra el **4** solo tras el acierto.
- Escritorio: post-it «no olvidar: mi equipo + año», banderín «Cóndores», calendario «2026». `Login` acción `password`: ficha `Condores` + ficha `2026`, no teclado libre. Acierto revela el **7**.
- `TokenWheel` acción `mfa`: la pantalla dice «Último inicio de sesión: 22:17». Botones giran la ruleta. Al minuto 17, `act("mfa", {"token": "492017"})`.
- `LogSheet` acción `logs`: doce líneas del guion (cuenta `aseo_turno_noche`, ROOT a las 03:14). El jugador elige la hora anómala. `act("logs", {"minute": "14"})`.
- `EvidenceBox` acción `box`: cuatro ruedas o botones hasta `4714`. Dentro, al abrir, texto de la credencial V. Soto y de la tarjeta de tráfico azul (las salas 2 y 3 las usan; aquí solo se leen).
- La puerta existente `ExitDoor` / `ExitHinge` pasa a ser la puerta magnética. No crees otra salida. `handle` de acción `exit`: si `mfa_open` no está, toast de bloqueo; si está, `main` no tiene por qué conocer el tween: anima `ExitHinge` a `-PI/2` desde `handle` y marca una meta `door_open` en el nodo para no repetir. `build` pone el rótulo «SALA DE SERVIDORES · PUERTA MAGNÉTICA».
- `ClockLabel` (Label3D) cerca de la pizarra: `build` lo crea con nombre exacto `ClockLabel`. No lo actualices cada frame; el catálogo puede hacerlo si quiere. Déjalo con texto «60:00».
- Manuales: acción `manual` y `argument` no alcanza para el id. Usa la propiedad `prompt` solo como texto corto y guarda el id del acertijo en `set_meta("puzzle", "phish")` (y password, mfa, logs). `handle` de `manual` llama `read_manual` y muestra la tarjeta. La primera frase del manual sale del JSON vía `puzzle(id).manual` si la sesión ya carga; si `puzzle` falla, muestra un manual corto escrito en `sala1.gd` con la misma definición.

Cada acierto llama `main._commit()` si existe. Los errores de `act` se muestran con `main.ui.toast`.

## Guías

Los cinco nombres pueden quedar, pero el texto deja de hablar de fases, bandejas y seguros. Hablan como grabaciones de la UCD: correo en el corcho, clave del escritorio, token, logs, y que la puerta a servidores no abre sin el segundo factor. Consultarlos no resuelve nada.

## Presupuesto

Geometría de cajas, como el resto de la oficina. No importes mallas nuevas ni texturas. Sin mapa de normales.

Al terminar, línea «listo» en la bitácora.
