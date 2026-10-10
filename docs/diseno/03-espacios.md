# 03 · Espacios

[Recorrido](07-lockdown-fbi.md). La sede es ficticia, de la UCD en Santiago. La oficina existente es la primera sala. Servidores y laboratorio son las dos salas siguientes. Este documento no las da por construidas.

| Espacio | Función | Elementos |
| --- | --- | --- |
| Oficina | Detectar | Corcho, escritorio, rueda, parte, caja `4714`, reloj `60:00` |
| Corredor de entrada | Salir de verdad | «SALIDA DE EMERGENCIA · SIEMPRE ABIERTA» |
| Puerta magnética | Pasar a servidores | Se abre con el segundo factor |
| Servidores | Contener | RACK-03, hoja de tráfico, cuatro cables, dos detectores, lámpara LED |
| Laboratorio | Identificar y detener | Seis fichas, nota, plano, carro en C-4, huellas, interruptor, debriefing |

Lo interactivo queda entre 0,9 m y 1,4 m. El doble fondo del carro queda a la altura de la cintura. Nada del recorrido pide agacharse. La escala de la oficina heredada sigue siendo 1 unidad = 1 metro, ojos cerca de 1,62 m, uso por raycast hasta unos 3 m.

Los ductos y un sector de incendio no forman parte de la planta. El fail-safe de servidores es luz y una llave.

## Oficina heredada

La escena de trabajo de esa sala es `Godot/Game/Levels/office.tscn`. El constructor que la regenera es `Godot/Game/Levels/office_builder.gd`. Las medidas del prefab Unity son referencia, no el layout que se edita ahora: [Anclas](../HUB_ANCHORS.md).

Las grabaciones siguen en sus sitios. Hablan de la UCD y no resuelven. Roles previstos en `Godot/Game/Levels/office_guide.gd`; el texto canónico es el del acto 1.

| Zona | Guía | De qué habla | Objetos del recorrido |
| --- | --- | --- | --- |
| Recepción | Vera | El corcho y el dominio | Correos |
| Pizarra | Hugo | El reloj y la caja | Reloj, caja |
| Archivo | Nuria | La línea de las 03:14 | Hoja de registro |
| Escritorio | Asistente técnico | Las dos fichas y la rueda | Inicio, token, laptop, cuaderno |
| Recuperación y salida | Iriarte | La puerta magnética y CÓNDOR LIBRE | Puerta magnética, salida de emergencia |

## Qué hace cada objeto del canon

| Objeto | Acción |
| --- | --- |
| Radio | Pide una pista. No marca el acierto |
| Correo falso | Marca dominio, clave y urgencia |
| Inicio | Elige `Condores` y `2026` |
| Rueda | Se detiene en el minuto 17 |
| Parte | Elige el minuto 14 |
| Caja | `4714` |
| Puerta magnética | Entra a servidores si el segundo factor está resuelto |
| Salida de emergencia | Detiene la operación sin castigo |
| Laptop | Terminal local, opcional |
| Cuaderno | Conceptos ya acertados |

Bandejas, buzón de expediente, pizarra de fases y seguros heredados no son el progreso. El diseño de la sala 1 los aparta de la vista y de la colisión. Este documento no afirma que ese apartado ya esté en la escena.

El prefijo Unity `INT_` / `MECH_` / `ENV_` queda para cuando se exporte el arte detallado. En Godot el nombre del nodo es el ancla.

## Detalles todavía abiertos

- Planta en metros de la oficina Godot dibujada encima del blockout actual.
- Qué objetos son hero y cuáles siguen siendo caja.
- RACK-03 pertenece a la sala de servidores y es el origen de la fuga. Su posición exacta la cierra quien construya esa sala, dentro del recorrido ya escrito.
