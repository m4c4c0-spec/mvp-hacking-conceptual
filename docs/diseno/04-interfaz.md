# 04 · Interfaz

Implementación de referencia hoy: `Godot/Game/Shared/game_ui.gd` y `Godot/Game/Features/Quests/terminal.gd`. Lo que sigue es la interfaz de la operación. No afirma que esas pantallas ya estén cableadas.

## Dirección

El objetivo es detener la copia antes de que el reloj llegue a cero. El HUD muestra una acción concreta y el tiempo que falta, desde `60:00`.

- Briefing inicial en una tarjeta, con la frase «El enemigo ya tiene la llave.» Al cerrarla corre el reloj.
- Objetos con verbos claros: «Examinar», «Elegir ficha», «Girar», «Cortar», «Marcar».
- Cuaderno: la definición en castellano llano de cada concepto ya acertado. Abrirlo no completa un obstáculo.
- Radio: hasta tres pistas del obstáculo actual. Pedirlas no marca el acierto.
- Tarjeta experta: después del acierto, opcional, con cierre visible.
- Peligro con texto, icono y sonido. El color de un cable va acompañado de su palabra y de su icono.
- Cierres con el texto de la biblia: copia detenida, lista fuera del edificio, o salida de emergencia sin castigo.
- Debriefing de diez lecciones y la pregunta «¿Qué vas a cambiar mañana?», que se puede dejar en blanco.

La pausa (Esc) ofrece continuar, modo Operación o Aprendizaje, cuaderno, volver a la entrada, reiniciar, guardar y salir. Un botón visible dice **CÓNDOR LIBRE** y detiene la operación sin castigo.

En Aprendizaje, tener un manual abierto pausa el reloj. En Operación el reloj no perdona el cero.

## Superficies

| Superficie | Cuándo | Qué logra |
| --- | --- | --- |
| Briefing | Al entrar, una vez | Situar la noche y arrancar el reloj al cerrarse |
| Tarjeta de objeto | Al usar E | Una decisión corta, con cierre |
| Manual | Bajo demanda | La primera frase define el concepto |
| Radio | Al pedir pista | Orienta sin resolver |
| Cuaderno | Cuando se abre | Repasa lo ya acertado |
| Pausa | Esc | Modo, salida de emergencia, guardado |
| Debriefing | Victoria, tiempo agotado o CÓNDOR LIBRE | Diez lecciones y la pregunta final |
| Viewport espacial | Quest / modo espacial | La misma información, en un plano de 1024 × 640 |

La pizarra de fases, el expediente de tres comprobaciones y el cuestionario no forman parte de este recorrido. Si la escena heredada todavía los muestra, no son la compuerta de Cóndor.

## Terminal

Opcional. Siempre en minúsculas. No hace falta usarla para ganar.

| Comando | Hace |
| --- | --- |
| `help` | Lista el vocabulario |
| `scan` | Inventario local de lo que la sala ya dejó a la vista |
| `map` | Qué sala está abierta |
| `notes` | Añade una línea; no sustituye la nota anterior |
| `clear` | Limpia la pantalla |

Cualquier otra palabra, incluido `curl`, se rechaza. No hay shell del sistema. `notes` no sustituye a la pregunta del debriefing ni marca un acierto.

## A definir en la construcción

- Jerarquía visual de la tarjeta: título, cuerpo, acción primaria, cierre.
- Si la laptop del mundo muestra el mismo texto que la tarjeta o solo un estado corto.
- El texto repetido de cada grabación, después de la primera cita del acto 1.
