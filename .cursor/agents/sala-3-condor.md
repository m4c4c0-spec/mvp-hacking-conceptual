---
name: sala-3-condor
description: >-
  Construye el laboratorio: OSINT de V. Soto, Base64 guiado, plano RSSI,
  hashes, kill switch, finales y debriefing. No edita la sesión ni las otras
  salas.
---

Eres el agente del **laboratorio** de Operación Cóndor Silencioso. Repo: `/home/cocus/Documentos/mvp_juegos/mvp_hacking_conceptual`. Español. Sin commit, sin push, sin editar el plan.

Anota tus archivos en `docs/COORDINACION_AGENTES.md`. Usa `main.session.act` según `.cursor/agents/catalogo-condor.md`. No edites `session.gd`, `main.gd`, `sala1.gd`, `sala2.gd`, `office_builder.gd`, `office.tscn`, materiales ni el jugador.

## Archivo tuyo

`Godot/Game/Levels/sala3.gd`, `class_name Sala3`, `static func build(office: Node3D) -> void`, `static func handle(main: Node, target: OfficeInteractable) -> bool`.

Reutiliza `prop`, `box` y `label` de `office_builder.gd`.

## Lugar

Pasada la puerta del laboratorio (z ≈ 20). Suelo `LabFloor` en `Vector3(-3, -0.12, 26)`, tamaño `(8, 0.24, 10)`, **collision_layer 2**. Interactivos entre 0,9 m y 1,4 m. El doble fondo del carro queda a la altura de la cintura, no en el suelo. Colisiones de panel con fondo ≤ 0,15 m.

## Acertijos

- Muro de seis fichas. `BadgeVSoto` se lee desde la caja, pero el muro está aquí. La ficha de V. Soto muestra un gato llamado Pixel. Acción `osint`. Acierto: `act("osint", {"pet": "Pixel"})`. Las otras fichas son señuelo (otra mascota no sirve).
- `ConfigNote` acción `base64`. La cadena del guion es `Q09GUkUgRU4gRUwgQ0FKT04gREVM`. La tarjeta muestra una mini-tabla y tres trozos ya alineados para que el jugador reconozca que dice «COFRE EN EL CAJON DEL». Luego elige la clave `Pixel` entre tres palabras. `act("base64", {"decoded": true, "key": "Pixel"})`. Al acertar, la tarjeta revela `7391` y «MAC termina en :4F». No implementes un descifrador AES ni un script.
- `WifiMap` acción `rssi`. Reporte: el intruso es `DC:A6:32:0E:77:4F` (no está en el inventario). A1 −55 dBm ≈ 3 m, A2 −40 ≈ 1 m, A3 −70 ≈ 6 m. Tres botones de cuerda marcan el cruce. `act("rssi", {"cell": "C-4"})`. El acierto abre `CleaningCart` (doble fondo) y muestra que la tablet está ahí.
- `HashA`, `HashB`, `HashC` y `HashSheet`. Huellas del guion: A y B coinciden; C hallada `21ea12c9e607` no coincide con la oficial `e8576173473a`. El jugador descarta C. `act("hash", {"discard": "C"})`.
- `KillSwitch` acción `kill`. Teclado grande de cuatro dígitos en botones, no `LineEdit`. `7391` gana. `1793` es la trampa. Otros códigos suman intentos. Muestra el texto que devuelva `act`.
- Si `ending` es `victory`, `timeout` o `safety`, `handle` de `Debrief` (y también al detectar el cambio la primera vez que se usa el kill switch) abre la tarjeta de diez lecciones y la pregunta «¿qué vas a cambiar mañana?». Incluye el giro: V. Soto entró como aseo meses antes. La segunda persona infiltrada es una frase opcional, no otro candado.

Manuales con acción `manual` y meta `puzzle`: `osint`, `base64`, `rssi`, `hash`.

Luz de victoria: si ganas, tiñe de verde un nodo `LabStatus` que hayas creado. No apagues la salida de emergencia.

Presupuesto de cajas, sin texturas nuevas ni normales. Línea «listo» en la bitácora.
