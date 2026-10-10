# Cambios en vivo · 7 de octubre de 2026

Contexto de la noche para quien abra `docs/` más tarde. El árbol de trabajo tiene tres capas a la vez. La fecha de un archivo dice en cuál está.

Este archivo no es el canon. La historia que debe quedar escrita vive en `docs/historia/BIBLIA.md`. El reparto de quién puede editar qué vive en `docs/COORDINACION_AGENTES.md`. Los contratos de cada agente viven en `.cursor/agents/*-condor.md`. El plan de la operación está fuera del repositorio, en el plan «Cóndor Silencioso VR»; no se edita.

No hay commit de esta noche.

## Cómo saber qué estás leyendo

| Si el archivo dice… | Estás en… |
| --- | --- |
| Estudio 01, Blue / Red, `Assets/Resources/Content/missions.json` | El último commit, `9d0176d`. Unity. Ya no es la partida que arranca `scripts/play-godot.sh`. |
| Protocolo Salida, FBI, Nueva York, ductos, incendio para anular el lockdown | La reescritura de las 22:03. Canon intermedio, escrito antes de Cóndor. El código jugable de esa capa sigue siendo el prototipo de cuatro fases y quizzes. |
| UCD, Santiago, V. Soto, `7391`, CÓNDOR LIBRE | La operación en vivo. Eso es lo que los agentes están dejando esta noche. |

Si `docs/historia/BIBLIA.md` ya habla de la UCD, el agente de canon terminó la prosa. Si `Godot/Game/Features/Quests/condor.json` existe y `session.gd` expone `act()`, el catálogo ya sustituyó el ciclo de quizzes. Si faltan, la partida que arranca sigue siendo la oficina de cuatro fases.

## Capa 1 · último commit

`9d0176d` — pulido de la oficina en primera persona, texturas y exports de escritorio.

El juego commiteado es **Estudio 01**, consultora ficticia que forma analistas Blue / Red. El acto 1 cubre el hub y las misiones `recon`, `social`, `identity` y `web`. El catálogo de esa capa está en Unity (`Assets/Resources/Content/missions.json`). Público 18–32, una persona, offline, PC primero.

En el árbol de trabajo, Unity (`Assets/`, `ProjectSettings/`, `Packages/`, `web/`, stubs y scripts de .NET) figura borrado o retirado. El arranque vigente es Godot 4.6.3:

```bash
cd /home/cocus/Documentos/mvp_juegos/mvp_hacking_conceptual
bash scripts/play-godot.sh
```

## Capa 2 · migración Godot y Protocolo Salida

La migración a `Godot/Game/` está en el árbol y no está commiteada. Su revisión de estructura, guardado, horneado y paquete está en `docs/REVISION_EN_VIVO.md` (30 de septiembre: 195 comprobaciones de sesión, 62 del recorrido, 49.572 triángulos).

Hacia las 22:03 la prosa pasó de Estudio 01 a **Protocolo Salida**:

- Premisa escrita entonces: hackers con root toman el edificio del FBI en Nueva York, lockdown, amenaza de soltar los ascensores, fuga por ductos e incendio narrativo que anula el bloqueo.
- Quedó pendiente el nombre de los atacantes, el origen del acceso y el desenlace.
- `docs/diseno/07-lockdown-fbi.md` describe ese recorrido como propuesta. El path se conserva a propósito: Cóndor reescribe el cuerpo y no mueve el archivo, para no romper enlaces.
- El código no acompañó esa prosa. `session.gd` sigue en versión 2 (`escape-progress-v1`, misiones, pizarra, seguros). `missions.json` sigue siendo el catálogo que carga la sesión. `docs/CONTRATO_CONTENIDO.md` sigue describiendo el esquema de quizzes y aplicación de conceptos.

Esa capa es el punto de partida que Cóndor sustituye. No hace falta restaurar el FBI ni volver a añadir una quinta misión al JSON.

## Capa 3 · operación en vivo, Cóndor Silencioso

Desde las 22:24 el juego pasa a ser un escape room continuo, en primera persona, con las mismas reglas en PC y en OpenXR.

**Título.** Operación Cóndor Silencioso. Frase: «El enemigo ya tiene la llave.»

**Premisa.** El jugador es el equipo de respuesta a incidentes de la Unidad de Ciberdelitos (UCD), agencia ficticia en un edificio de Santiago. Custodian una lista ficticia de informantes. A las 22:00 una falsa prueba de emergencia cierra las puertas magnéticas. Un insider con root copia la lista. Quedan 60 minutos. La salida de emergencia del spawn permanece usable. La palabra de seguridad es **CÓNDOR LIBRE**: sube las luces, para el reloj y cierra el ejercicio sin castigo.

**Arco**, en un solo edificio y sin pantalla de carga:

1. Oficina (la escena que ya existe, 16 × 12 m): detectar. Phishing, contraseña, MFA, logs.
2. Servidores, blockout al otro lado del corredor (`CorridorEnd` en z = 9): contener. Tráfico, cable, fail-safe.
3. Laboratorio, pasado z ≈ 20: identificar y detener. OSINT, Base64 guiado, plano, hashes, kill switch.

**Giro**, solo en el debriefing. V. Soto entró como personal de aseo meses antes. A las 20:03 su tarjeta fue denegada; a las 03:14 entró como `aseo_turno_noche` con ROOT en RACK-03. Había dos personas de aseo. La segunda es una línea opcional del cierre, no un acertijo.

**Cómo se aprende.** Cada acertijo lleva un manual en el mundo. La primera frase define el concepto en castellano llano. El acierto cambia el lugar. El error explica la señal y deja reintentar. La radio da tres pistas. Al resolver, el concepto queda aplicado para el debriefing. El cuaderno deja de ser una compuerta de examen.

**Modos.** Operación: si el reloj llega a 60 minutos, la partida termina con «Descarga completa». Aprendizaje: el reloj se ve, no hay derrota, y se pausa mientras el manual está abierto. Operación es el modo del guion. Aprendizaje existe porque el público declarado no ha hecho ciberseguridad.

**Qué se conserva.** Rig de escritorio y de OpenXR, agarre, oficina horneada, guardado atómico, presupuesto de 8 GB, perfiles Desktop8Gb y Quest3S. OpenXR no arranca en escritorio.

**Qué deja de gobernar la partida.** El ciclo pizarra → carpetas → bandejas → buzón → quiz → cuatro seguros. `missions.json` permanece en disco para no romper el filtro de export; la sesión nueva no lo usa como progreso. Las partidas v1 y v2 no se convierten: al cargar, se avisa y se empieza desde el briefing. Un JSON dañado no pisa el estado ni borra el archivo.

**Ficción.** Dominios en `.invalid`. La dirección de documentación es `203.0.113.44`. La lámpara, el humo y el CO₂ son luz y sonido. Los ascensores son un temporizador de la ficción que el kill switch detiene. La terminal, si queda, solo muestra el estado del incidente. `curl` y cualquier comando fuera de `help`, `scan`, `map`, `notes` y `clear` se rechazan. La terminal no hace falta para terminar.

## Dónde queda cada cosa cuando la operación aterrice

| Qué | Dónde |
| --- | --- |
| Canon, acto y archivo histórico de misiones viejas | `docs/historia/BIBLIA.md`, `ACTO_1.md`, `MISIONES_SIGUIENTES.md` |
| Recorrido de las tres salas (mismo path de siempre) | `docs/diseno/07-lockdown-fbi.md` |
| Producto, bucle, espacios, interfaz, decisiones | `docs/diseno/01` a `04` y `06`, más el README de esa carpeta |
| Arranque y premisa corta | `README.md` (los comandos de arranque se quedan) |
| Catálogo de los once acertijos | `Godot/Game/Features/Quests/condor.json` |
| Máquina de banderas, reloj, pistas, guardado | `Godot/Game/Features/Quests/session.gd` (`class_name EscapeSession`) |
| Quién carga el JSON | `Godot/Game/Infrastructure/catalog_store.gd` |
| Oficina, servidores, laboratorio | `sala1.gd`, `sala2.gd`, `sala3.gd` en `Godot/Game/Levels/` |
| Textos de los cinco guías | `office_guide.gd`, solo `ROLES` |
| Integración de salas, HUD y pausa | `Godot/Game/Bootstrap/main.gd` y `terminal.gd` |
| Lectura en visor, rayo, teletransporte, giro de 30° | `game_ui.gd`, `xr_player.gd`, apartado en `Godot/docs/QUEST_3S.md` |
| Albedo por nombre de nodo y pisada | `surface_finish.gd`, `condor_physics.gd`, cuerpo en `desktop_player.gd` |
| Esquema v3 | `docs/CONTRATO_CONTENIDO.md` |
| Pruebas | `Godot/tests/test_session.gd`, `Godot/tests/test_world.gd` |

`main.gd` adjunta cada sala solo si el script existe, llama `build(office)` y, si existe `condor_physics.gd`, llama `CondorPhysics.tune(office, player)` después de crear al jugador. `use_object` pregunta `handle` a cada sala. La pausa incluye modo, cuaderno, volver a la entrada `Vector3(0, 0.1, 4.4)`, reiniciar, guardar, salir y un botón **CÓNDOR LIBRE**.

## Acertijos que el código toma del contrato

El detalle está congelado en `.cursor/agents/catalogo-condor.md`. Resumen para leer una sala sin reabrir el rol:

| id | Acierto | Bandera | Sala |
| --- | --- | --- | --- |
| phish | señales dominio, clave y urgencia en el correo `soporte-ti@ucd-ch1le.invalid` | `phish_digit` = 4 | 1 |
| password | `Condores2026` (equipo + año del calendario) | `password_digit` = 7 | 1 |
| mfa | token `492017`, ruleta en el minuto 17 | `mfa_open` | 1, abre la puerta magnética |
| logs | minuto 14, cuenta `aseo_turno_noche` | `log_digits` = 14 | 1 |
| box | `4714`, exige los tres dígitos anteriores | `box_open` | 1 |
| traffic | color azul, `RACK-03` hacia `203.0.113.44`, exige la caja | `traffic_blue` | 2 |
| cable | azul (externo). Verde resta 5 min y sigue. Rojo resta 2 min. Amarillo no avanza | `cable_blue` | 2 |
| fire | lámpara LED y llave a la vez; la puerta del laboratorio queda abierta 90 s | `lab_open_until` | 2 |
| osint | mascota `Pixel` | `pixel_known` | 3 |
| base64 | clave `Pixel` y trozo ya alineado | `kill_code` = 7391, MAC termina en `4F` | 3 |
| rssi | cruce `C-4`, doble fondo del carro a la cintura | `tablet_found` | 3 |
| hash | descartar la huella C | `hash_discarded` | 3 |
| kill | `7391`. La trampa `1793` adelanta el reloj 1 min. Tres fallos bloquean 90 s | `ending` = victory | 3 |

Objetos de uso entre 0,9 m y 1,4 m. Nada que exija agacharse. Suelos nuevos en `collision_layer` 2. Paneles con fondo de colisión de 0,15 m como máximo. Cada cable lleva color, icono y palabra.

Al cerrar, una tarjeta con diez lecciones y la pregunta «¿qué vas a cambiar mañana?».

## Quién escribe, y el cruce de esta noche

El tablero está en `docs/COORDINACION_AGENTES.md`, sección «Cóndor Silencioso · dueños». Cada agente edita solo su lista.

Hay dos sesiones lanzando los mismos roles a la vez (una planificó y creó los agentes; otra pidió ejecutar los pendientes). La bitácora ya tiene dos entradas de VR sobre los mismos archivos. Antes de abrir un script, lee la bitácora: si figura «en curso», no es tuyo.

El agente de texturas espera a que las salas dejen de figurar en curso. El de catálogo es el único que reescribe `session.gd` y `main.gd`. Las salas llaman `act()` aunque la sesión todavía sea la de quizzes: el contrato ya está escrito para que no inventen otra API.

## Foto al redactar este archivo

Cerca de las 22:44 del 7 de octubre de 2026:

- Existían los siete roles en `.cursor/agents/*-condor.md` y el reparto en la bitácora.
- Canon había marcado en curso la biblia, el acto, el diseño y el README (dos líneas, 22:36 y 22:37). En disco, `BIBLIA.md`, `ACTO_1.md` y `docs/diseno/07-lockdown-fbi.md` seguían con el texto de Protocolo Salida de las 22:03.
- `CONTRATO_CONTENIDO.md` seguía en el esquema de quizzes.
- No estaban `condor.json`, `sala1.gd`, `sala2.gd`, `sala3.gd` ni `condor_physics.gd`.
- `session.gd` seguía en la máquina de misiones versión 2.
- VR había anotado `game_ui.gd`, `xr_player.gd` y `Godot/docs/QUEST_3S.md` como en curso, también por duplicado. El visor Quest no se certifica en esta pasada.

Cuando retomes, contrasta esta foto con la bitácora. Una línea «listo» manda sobre este párrafo.

## Al volver a leer el juego

1. `docs/historia/BIBLIA.md` — premisa que manda.
2. `docs/diseno/07-lockdown-fbi.md` — recorrido de las tres salas.
3. `.cursor/agents/catalogo-condor.md` — API y soluciones.
4. `docs/COORDINACION_AGENTES.md` — qué archivo está ocupado.
5. `docs/REVISION_EN_VIVO.md` — solo la revisión Godot del 30 de septiembre, anterior a Cóndor.
6. `docs/RENDER_8GB.md` — techo que esta operación no sube.
