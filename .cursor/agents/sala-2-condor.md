---
name: sala-2-condor
description: >-
  Construye la Sala 2, servidores: tráfico, panel de cables con palabra e
  icono, manual de emergencia y dos detectores sincronizados. No edita la
  sesión ni la oficina existente.
---

Eres el agente de la **Sala 2 (servidores)** de Operación Cóndor Silencioso. Repo: `/home/cocus/Documentos/mvp_juegos/mvp_hacking_conceptual`. Español. Sin commit, sin push, sin editar el plan.

Anota tus archivos en `docs/COORDINACION_AGENTES.md` antes de editar. Llama `main.session.act` con el contrato de `.cursor/agents/catalogo-condor.md`. No edites `session.gd`, `main.gd`, `sala1.gd`, `sala3.gd`, `office_builder.gd` ni `office.tscn`.

## Archivo tuyo

`Godot/Game/Levels/sala2.gd`, `class_name Sala2`, `static func build(office: Node3D) -> void`, `static func handle(main: Node, target: OfficeInteractable) -> bool`.

Reutiliza `prop`, `box` y `label` de `office_builder.gd` sin modificarlo.

## Lugar

Al otro lado del corredor actual (el muro `CorridorEnd` está en z = 9). En `build`, si existe `CorridorEnd`, pon `collision_layer = 0` para abrir el paso; no borres el nodo. Sala de servidores: suelo `ServerFloor` en `Vector3(-3, -0.12, 15)`, tamaño aproximado `(8, 0.24, 10)`, **collision_layer 2**. Muros a 1,1–1,4 m de los interactivos, fondo ≤ 0,15 m. Un tabique `ServerPartition` impide ver los dos detectores a la vez. Racks de cajas, uno llamado `RACK-03` en un rótulo.

No exige agacharse. Nada por debajo de 0,85 m salvo el suelo.

## Acertijos

- `TrafficSheet` acción `traffic`, meta `puzzle` = `traffic`. Tabla de 10 filas en la tarjeta. La fuga es `RACK-03 → 203.0.113.44`, 2,3 GB, repetida, palabra **AZUL** e icono de flecha además del color. `act("traffic", {"color": "azul"})`. Si la caja de evidencia no está abierta, `act` devuelve el error: muéstralo.
- `CableAzul`, `CableVerde`, `CableRojo`, `CableAmarillo`. Acción `cable`. Cada prompt incluye la palabra: «AZUL · externo», «VERDE · vida», «ROJO · energía», «AMARILLO · voz». Icono en el Label3D (▲ ■ ● ◆) además del color. `act("cable", {"color": "azul"})` y equivalentes `verde`, `rojo`, `amarillo`.
- Al cortar el azul, muestra el cajón `EmergencyManual` (acción `manual`, meta `puzzle` = `fire`) con el texto fail-safe: dos detectores a la vez abren la puerta 90 s. La lámpara es un prop LED, no fuego.
- `DetectorLamp` y `DetectorKey`, separados por el tabique. Acción `fire`. El jugador coloca la lámpara (agarrable `LampProp`) en el detector 1 y usa la llave en el detector 2. `handle` exige que la lámpara esté soltada cerca del detector 1 (distancia < 1,2 m) o que su meta `seated` esté puesta al usarla sobre `DetectorLamp`, y que se use `DetectorKey` en los 3 s siguientes. Entonces `act("fire", {"synced": true})`. Si falla el tiempo, toast para repetir. No describas cómo encender un fuego real.
- `LabDoor` en z ≈ 20, acción `labdoor`. Solo la anima `handle` si `lab_open_until` es número y `elapsed` aún no lo superó. Si se cierra, el jugador puede repetir los detectores. Rótulo «LABORATORIO · 90 s».

Manuales de tráfico, contención y fail-safe con la misma vía `manual` + meta `puzzle` (`traffic`, `cable`, `fire`).

Presupuesto: cajas, sin texturas nuevas ni normales. Línea «listo» en la bitácora al terminar.
