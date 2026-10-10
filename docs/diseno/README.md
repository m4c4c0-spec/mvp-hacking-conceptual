# Estructura de diseño · Operación Cóndor Silencioso

Aquí se decide el diseño. El código no es la biblia: si una ficha de esta carpeta y una escena discrepan, la ficha dice lo que queremos y la escena dice lo que hay hoy.

La dirección vigente es [Operación Cóndor Silencioso](07-lockdown-fbi.md): una noche en la UCD, tres salas y desenlace cerrado en la [biblia](../historia/BIBLIA.md). El archivo del recorrido conserva su nombre anterior para no romper enlaces.

## Orden de lectura

1. [Producto](01-producto.md) — a quién va, qué promete y qué no entra.
2. [Bucle y fases](02-bucle-y-fases.md) — el recorrido de una partida.
3. [Espacios](03-espacios.md) — oficina, servidores y laboratorio.
4. [Interfaz](04-interfaz.md) — reloj, tarjetas, terminal y CÓNDOR LIBRE.
5. [Dirección de arte](05-direccion-de-arte.md) — paleta, materiales y techo de 8 GB.
6. [Decisiones](06-decisiones.md) — lo que ya está cerrado.
7. [Recorrido de la operación](07-lockdown-fbi.md) — salas, acertijos, reloj y debriefing.

## Dónde vive cada cosa

| Qué | Dónde | Se edita |
| --- | --- | --- |
| Diseño (esta carpeta) | `docs/diseno/` | Sí, al cambiar una decisión de juego o de arte |
| Historia y acto | `docs/historia/` | Sí, prosa. La biblia manda sobre las cifras |
| Archivo histórico | `docs/historia/MISIONES_SIGUIENTES.md` | No se vuelca al catálogo |
| Catálogo heredado | `Godot/Game/Features/Quests/missions.json` | Permanece en disco. No es el progreso de esta operación |
| Escena de la oficina | `Godot/Game/Levels/office.tscn` | Sí, en el editor. Regenerar con `office_builder.gd` pisa los cambios manuales |
| Detalle geométrico importado | `Godot/Game/Levels/office_detail.glb` | Arte convertido; procedencia y regeneración en `Godot/docs/ASSETS_Y_GUIAS.md` |
| Presupuesto de render | `docs/RENDER_8GB.md` | Lo fija el perfil 8 GB; el diseño no lo sube |

## Regla al diseñar

El recorrido se diseña como tres salas y aciertos encadenados. Un cuestionario no abre una puerta. Las tarjetas expertas van después del acierto y no son requisito. Los tickets históricos no se incorporan. No se inventan comandos reales, payloads ni conexiones a equipos externos. La terminal sigue en `help`, `scan`, `map`, `notes` y `clear`.
