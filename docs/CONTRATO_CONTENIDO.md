# Contrato de contenido

Esquema **v3**. La operación jugable está solo en **`Godot/Game/Features/Quests/condor.json`**. Lo lee `Game/Infrastructure/catalog_store.gd`. La sesión (`EscapeSession`) no usa `missions.json` como progreso.

`Godot/Game/Features/Quests/missions.json` sigue en disco para no romper el filtro de export (`Game/Features/Quests/*.json`). Es herencia del prototipo anterior.

Hay **11 conceptos** y **13 pasos** de `act`. Los pasos `box` y `kill` repiten un concepto ya enseñado (clave débil e integridad) porque el candado y el código de parada son compuertas, no temas nuevos.

## Raíz

| Campo | Tipo | Uso |
| --- | --- | --- |
| `title` | string | Nombre de la operación |
| `subtitle` | string | Frase de la tarjeta inicial |
| `version` | number | `3` |
| `domain` | string | `soporte-ti@ucd-ch1le.invalid` |
| `ip` | string | `203.0.113.44` |
| `concepts[]` | Concept | Glosario, en el orden de abajo |
| `puzzles[]` | Puzzle | Los 13 pasos, en el orden de la tabla |
| `endings` | object | `victory`, `timeout`, `safety` |
| `closing` | object | Giro, diez lecciones, línea opcional y pregunta |

`endings.timeout` es exactamente `Descarga completa. La lista salió del edificio.`

`closing.question` es `¿Qué vas a cambiar mañana?` `closing.lessons` tiene 10 frases.

## Concept

`id`, `name`, `definition`, `defense`.

La `definition` es la primera frase del manual de cada paso que usa ese concepto, punto final incluido.

Ids, en orden: `phishing`, `passwords`, `mfa`, `logs`, `traffic`, `containment`, `failsafe`, `osint`, `obfuscation`, `inventory`, `integrity`.

## Puzzle

`id`, `room` (`oficina` | `servidores` | `laboratorio`), `concept`, `seconds` (número mayor que 0), `title`, `objective`, `manual`, `hints` (exactamente 3), `debrief`, `solution`.

La primera frase de `manual` define el concepto. Las pistas van de un empujón a la respuesta concreta. No hay payloads ni comandos de ataque.

## Soluciones

| id | `solution` | Bandera al acertar | Exige |
| --- | --- | --- | --- |
| `phish` | `signals`: dominio, clave, urgencia | `phish_digit` = `4` | — |
| `password` | `code` = `Condores2026` | `password_digit` = `7` | — |
| `mfa` | `token` = `492017` | `mfa_open` | — |
| `logs` | `minute` = `14` | `log_digits` = `14`, `insider` = `aseo_turno_noche` | — |
| `box` | `code` = `4714` | `box_open` | los tres dígitos anteriores |
| `traffic` | `color` = `azul` | `traffic_blue` | `box_open` |
| `cable` | `color` = `azul` | `cable_blue` | `traffic_blue` |
| `fire` | `synced` true | `lab_open_until` = elapsed + 90 | `cable_blue` |
| `osint` | `pet` = `Pixel` | `pixel_known` | `box_open` |
| `base64` | `key` = `Pixel` y `decoded` true | `kill_code` = `7391`, `mac_suffix` = `4F` | `pixel_known` |
| `rssi` | `cell` = `C-4` | `tablet_found` | `mac_suffix` |
| `hash` | `discard` = `C` | `hash_discarded` | `tablet_found` |
| `kill` | `code` = `7391` | `ending` = `victory` | `hash_discarded` |

Cable `verde`: `life_safety_cut`, +300 s, evacuación, la partida sigue. Cable `rojo`: +120 s. Cable `amarillo`: no avanza. Kill `1793`: +60 s y no cuenta entre los tres fallos. Otro código incorrecto suma `kill_attempts`; al tercero, `lockout_until` = elapsed + 90.

## Partida

Archivo `user://escape-progress-v1.json`, contenido versión 3. Campos: `mode` (`operacion` o `aprendizaje`), `elapsed`, `paused`, `manual_open`, `flags`, `hints` (0..3), `manuals_read`, `applied`, `ending` (`""`, `victory`, `timeout`, `safety`), `kill_attempts`, `lockout_until`, `note`, `pending_radio`.

En operación, `elapsed >= 3600` cierra con tiempo agotado. En aprendizaje el reloj se ve y no hay derrota; un manual abierto lo pausa. Una partida v1 o v2 no se convierte: hay aviso y `reset()` deja la versión 3. Un JSON dañado no pisa el estado ni borra el archivo. Si no hay archivo, `last_error` queda vacío.

## Límites

- Dominios en `.invalid`. La única IP escrita es `203.0.113.44`.
- El fail-safe es luz y una llave. No hay incendio real.
- La terminal solo acepta `help`, `scan`, `map`, `notes` y `clear`.
- Sin herramientas reales ni comandos de ataque.
