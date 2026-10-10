# Biblia · Operación Cóndor Silencioso

Frase de la operación: **«El enemigo ya tiene la llave.»**

Este archivo es el canon narrativo. Si un acto, una sala o un acertijo lo contradice, manda esta biblia. Aquí se fija la historia. No se declara el juego construido.

El recorrido jugable está en [Acto 1](ACTO_1.md). La planta y el orden de los obstáculos están en [el diseño del recorrido](../diseno/07-lockdown-fbi.md). El nombre de ese archivo se conserva para no romper enlaces; el cuerpo es esta operación.

## Premisa

A las 22:00 alguien lanzó una falsa prueba de emergencia en la sede de la Unidad de Ciberdelitos (UCD), agencia ficticia en Santiago. El edificio quedó en lockdown. El jugador, en primera persona, es el equipo de respuesta a incidentes de la UCD. Un insider con root copia la lista de informantes protegidos. Quedan 60 minutos.

La lista es ficticia. Si la copia sale del edificio, hay vidas en riesgo dentro de la ficción.

La sede, los controles y el personal son una representación inventada. No se reproduce una planta real, una dirección real ni un procedimiento de una agencia real. La UCD no existe fuera del juego.

Quedan fuera de este canon el edificio del FBI, Nueva York, la fuga por ductos y el incendio provocado por el equipo. El archivo [Misiones siguientes](MISIONES_SIGUIENTES.md) guarda tickets de una consultora anterior: es histórico, no se dramatiza y no se vuelca al catálogo.

## Cómo se avanza

El arco sigue un NIST simplificado, dicho en castellano dentro del mundo:

1. **Oficina — detectar.** Ver el anzuelo, la clave débil, el segundo factor y la línea del registro que no cuadra.
2. **Servidores — contener.** Señalar la fuga y cortar solo ese camino. El fail-safe de luz abre el laboratorio.
3. **Laboratorio — identificar y detener.** Saber quién es V. Soto, leer lo que el disfraz de texto no esconde, encontrar el equipo que no está en el inventario y parar la copia.

Cada concepto se define en castellano llano la primera vez que aparece. El manual de un obstáculo abre con esa definición. Leer el manual, pedir la radio, abrir el cuaderno o cerrar una tarjeta experta deja las puertas como estaban. El avance es el acierto sobre el objeto.

Tras cada acierto puede mostrarse una tarjeta experta: una observación más honda del mismo concepto. Se cierra cuando la persona quiere. La puerta siguiente no la espera. No hay examen como compuerta. La operación no cierra con una nota numérica.

## Reloj, modos y salida real

Al cerrar el briefing, el reloj visible parte de **60:00**. Es el tiempo que falta para que la lista salga del edificio.

- **Operación.** Si el reloj llega a cero, la partida se pierde. El texto es: «Descarga completa. La lista salió del edificio.»
- **Aprendizaje.** El reloj se ve. Abrir un manual lo pausa. Llegar a cero no derrota.

Tres códigos distintos de `7391` y de `1793`, escritos en el interruptor, bloquean ese panel **90 segundos**.

La salida de emergencia del juego está en el corredor de entrada y siempre se puede usar. El rótulo dice «SALIDA DE EMERGENCIA · SIEMPRE ABIERTA». En la pausa hay un botón visible, **CÓNDOR LIBRE**. Cualquiera de los dos detiene la operación, la pausa y no resta tiempo. El debriefing sigue disponible.

Esa salida es la seguridad de quien juega. No es un acertijo y no sustituye al interruptor.

## Cronología del día

Meses antes, V. Soto entra a la plantilla de aseo. La hoja de la oficina tiene doce líneas, en este orden. La que se marca es la primera. El minuto que se entrega es **14**.

1. 03:14 — RACK-03. Cuenta `aseo_turno_noche`. Permiso ROOT.
2. 03:16 — RACK-03. La copia hacia fuera sigue activa. Sin destino en esta hoja.
3. 03:40 — Canal de voz interno. Sin destino exterior.
4. 04:02 — Energía de los racks estable. Sin alarma de vida.
5. 18:02 — El carro de aseo entra al pasillo. Sin privilegio.
6. 20:03 — Tarjeta personal de V. Soto: DENEGADA.
7. 21:40 — Ronda de cierre de recepción. Sin novedad.
8. 21:55 — Prueba de altavoz. Sin fallo.
9. 22:00 — El panel acepta una prueba de emergencia. Es falsa.
10. 22:00 — Puertas de planta en lockdown.
11. 22:05 — El equipo de respuesta queda dentro.
12. 22:17 — Último inicio de sesión en la consola de guardia.

`03:14` es la madrugada de este mismo día, anterior al simulacro de las 22:00. La denegación de las 20:03 se lee y no es el acierto. El minuto 17 de las 22:17 es el de la rueda del segundo factor.

## El giro

V. Soto se empleó como aseo meses antes. A las 20:03 su tarjeta personal fue denegada. A las 03:14 entró al registro como `aseo_turno_noche`, con ROOT en RACK-03.

El parte de aseo de esa noche tenía dos personas. La segunda es una sola frase opcional del debriefing. No tiene nombre que descubrir, no tiene código y no abre una puerta.

## Personajes

- **Quien juega** es el equipo de respuesta, en un solo cuerpo y en primera persona. La operación es de una persona. No hay multijugador.
- **V. Soto** es el insider. En la ficha: aseo, alta de hace meses, tarjeta denegada a las 20:03, gato llamado **Pixel**.
- **Vera, Hugo, Nuria, el asistente técnico e Iriarte** son grabaciones de asistencia de la UCD. Orientan hacia el objeto. Consultarlos no resuelve el obstáculo.
- Las otras cinco fichas del muro del laboratorio son señuelo de mascota. No son la segunda persona de aseo.

## Soluciones cerradas

El código usa estas soluciones. No inventa otras.

| Obstáculo | Acierto | Qué deja abierto |
| --- | --- | --- |
| Correo | El falso es `soporte-ti@ucd-ch1le.invalid`. Hay que marcar dominio, clave y urgencia. | El dígito **4**, al reverso, solo tras el acierto. |
| Clave | `Condores2026`, con las fichas `Condores` y `2026`. | El dígito **7**. |
| Segundo factor | Token `492017` con la rueda en el minuto 17. | La puerta magnética hacia servidores. |
| Registro | La hora `03:14`, minuto `14`, cuenta `aseo_turno_noche`. | Los dígitos **14** y el nombre de esa cuenta. |
| Caja | `4714`. Exige que 4, 7 y 14 ya se hayan revelado. | Credencial de V. Soto y tarjeta de la fuga azul. |
| Tráfico | La fuga `RACK-03 → 203.0.113.44`, 2,3 GB, palabra **AZUL** y flecha. En la hoja de diez filas ocupa la 3 y la 5. Exige la caja abierta. | Poder cortar el cable azul como contención. |
| Cable | Cortar el **AZUL** (externo). Exige haber marcado el azul. | El manual del fail-safe. |
| Fail-safe | Lámpara LED en un detector y la llave en el otro dentro de los tres segundos siguientes. Exige el cable azul. | La puerta del laboratorio, 90 segundos. |
| Ficha | La mascota **Pixel**, en la ficha de V. Soto. Exige la caja abierta. | Poder usar esa palabra en la nota. |
| Nota | El texto ya alineado dice `COFRE EN EL CAJON DEL`. La clave es `Pixel`. | El código `7391` y «MAC termina en :4F». |
| Plano | La celda **C-4**. La MAC completa es `DC:A6:32:0E:77:4F`. | El carro de aseo y la tablet. |
| Huellas | Descartar **C**. | El interruptor. |
| Interruptor | `7391`. | Victoria. |

Detalles que no se sustituyen:

- El correo legal de soporte es `soporte-ti@ucd-chile.invalid`. El falso cambia la `i` de `chile` por un `1`. Todos los dominios de la ficción terminan en `.invalid`.
- El banderín dice «Cóndores», con acento. La clave no lleva acento: `Condores2026`. El post-it dice «no olvidar: mi equipo + año». El calendario dice «2026».
- La pantalla de la rueda dice «Último inicio de sesión: 22:17».
- `203.0.113.44` es una dirección de documentación. No es un equipo real y no se contacta.
- La cadena escrita en la nota es `Q09GUkUgRU4gRUwgQ0FKT04gREVM`. La tarjeta muestra una tabla breve y tres trozos ya alineados. La persona reconoce la frase; no ejecuta un descifrado.
- Huella oficial, y la de A y la de B: `e8576173473a`. Huella C, la hallada: `21ea12c9e607`. Son sellos abreviados de la ficción.
- En el plano, A1 −55 dBm ≈ 3 m, A2 −40 dBm ≈ 1 m, A3 −70 dBm ≈ 6 m. El cruce es C-4. El doble fondo del carro queda a la altura de la cintura.
- El muro tiene seis fichas. Solo V. Soto tiene el gato Pixel. Los señuelos son A. Ruiz (perro Luna), M. Palma (pez Nube), C. León (gato Sol), R. Díaz (loro Mia) y J. Peña (hámster Rocky). Otro gato no sirve: la palabra es Pixel.

### Cables y códigos que no ganan

| Acción | Efecto |
| --- | --- |
| Cable **VERDE**, vida | Evacuación dramática, el reloj pierde 5 minutos, se puede seguir. La copia no se detiene. |
| Cable **ROJO**, energía | El reloj pierde 2 minutos. La fuga sigue. |
| Cable **AMARILLO**, voz | La voz interna se corta. No abre nada. |
| Interruptor `1793` | Trampa. El reloj pierde 1 minuto. No cuenta entre los tres errores del bloqueo. |
| Otro código de cuatro dígitos | Suma un error. Al tercero, el panel queda bloqueado 90 segundos. |

Cada cable se nombra con palabra e icono, además del color: AZUL ▲ externo, VERDE ■ vida, ROJO ● energía, AMARILLO ◆ voz.

## Los once conceptos

La primera frase de cada manual es la definición. El cuaderno la repite cuando el obstáculo ya se acertó.

| Id | Nombre en el mundo | Primera frase |
| --- | --- | --- |
| `phishing` | Anzuelo | Un anzuelo, o phishing, es un mensaje que finge ser de confianza para que entregues un secreto. |
| `passwords` | Clave débil | Una clave débil es la que alguien arma con lo que ya está a la vista. |
| `mfa` | Segundo factor | El segundo factor, o MFA, es otra prueba distinta de la clave. |
| `logs` | Registro | Un registro es la lista de lo que el sistema ya anotó: hora, cuenta y permiso. |
| `traffic` | Tráfico | El tráfico es el movimiento de datos entre equipos; una fuga es una copia que sale hacia fuera. |
| `containment` | Contención | Contener es cortar el camino por donde el daño sigue saliendo, y dejar intacto lo que mantiene a salvo a las personas. |
| `failsafe` | Fail-safe | Un fail-safe es un seguro que abre un paso cuando dos avisos coinciden; aquí es luz y una llave, no un incendio. |
| `osint` | Lo que ya se ve | OSINT es observar lo que una persona ya dejó a la vista, sin entrar en ningún sistema. |
| `obfuscation` | Disfraz de texto | Ofuscar es cambiar el aspecto de un texto; Base64 disfraza letras y no las protege. |
| `inventory` | Inventario | El inventario es la lista de equipos que sí pertenecen a la casa; una MAC ausente es un extraño. |
| `integrity` | Huella | La integridad se comprueba con una huella: si no coincide, el archivo cambió. |

## Debriefing

Se abre en la victoria, en el tiempo agotado y en la salida de emergencia. El orden es fijo.

1. El giro, en este párrafo: «V. Soto entró a la plantilla de aseo meses antes. A las 20:03 su tarjeta personal fue denegada. A las 03:14 la cuenta aseo_turno_noche ya tenía ROOT en RACK-03.»
2. Las diez lecciones de abajo.
3. Una línea opcional, que se puede omitir: «El parte de aseo de esa noche tenía dos personas. Solo una cuenta obtuvo ROOT.»
4. La pregunta, sin respuesta correcta y con la posibilidad de dejarla en blanco: **«¿Qué vas a cambiar mañana?»**

Lecciones:

1. **Anzuelo.** El dominio llevaba un 1 donde iba una i, el mensaje pedía la clave y apuraba. Esas tres señales bastan para no entregar nada.
2. **Clave a la vista.** El nombre del equipo y el año estaban en el escritorio. Una clave se protege cuando no se arma con el banderín ni con el calendario.
3. **Segunda prueba.** La clave no alcanza. El código que cambia, situado en el minuto del último inicio, es la otra prueba. Sin ella la puerta magnética sigue cerrada.
4. **La línea del registro.** A las 03:14 la cuenta de aseo tuvo ROOT en RACK-03. El registro muestra hora, cuenta y permiso. Una sospecha no se escribe como si fuera esa línea.
5. **La fuga.** La misma copia grande sale de RACK-03 hacia 203.0.113.44, marcada AZUL. Contener es cortar ese camino. Esa dirección es de documentación: en el juego no apunta a un equipo real.
6. **El cable de vida.** El verde sostiene a las personas. Cortarlo anuncia la evacuación y resta cinco minutos. El rojo quita energía y resta dos. El amarillo calla la voz y no abre nada. El cable de la fuga es el azul.
7. **Luz, no incendio.** Dos detectores a la vez abren el laboratorio durante noventa segundos. La lámpara es LED: luz y sonido.
8. **El nombre que ya estaba escrito.** La ficha de V. Soto nombra al gato Pixel. Otra mascota, aunque sea otro gato, no abre el cofre.
9. **Letras disfrazadas.** Base64 cambia el aspecto. La tarjeta ya separa los trozos: `COFRE EN EL CAJON DEL`. La palabra que falta es Pixel. Ahí aparecen el 7391 y la MAC que termina en :4F.
10. **Lista, sello e interruptor.** La MAC `DC:A6:32:0E:77:4F` no está en el inventario; el plano la sitúa en el carro, celda C-4. La huella C no coincide con la oficial y se descarta. El 7391 detiene la copia. El 1793 adelanta el reloj un minuto.

## Desenlaces

- **Victoria.** `7391` detiene la copia. La lista sigue en el edificio. Dentro de la ficción, los ascensores vuelven a una parada segura. Una luz del laboratorio pasa a verde. La salida de emergencia permanece abierta. Texto: «Copia detenida. La lista sigue en el edificio.»
- **Tiempo agotado.** Solo en modo Operación. Texto: «Descarga completa. La lista salió del edificio.»
- **Salida de emergencia.** `CÓNDOR LIBRE` o la puerta siempre abierta. Texto: «Salida de emergencia. La operación se detiene sin castigo.»

Mientras la copia sigue, la ficción trata los ascensores como parte del mismo control hostil. El interruptor los devuelve a esa parada segura. El juego no explica frenos ni cómo forzarlos.

## Límites de la ficción

- Dominios solo en `.invalid`.
- La única dirección escrita es `203.0.113.44`, de documentación.
- El fail-safe es una lámpara LED y una llave. Luz y sonido. No hay fuego, no hay materiales y no hay una receta.
- La terminal del mundo es local. Acepta `help`, `scan`, `map`, `notes` y `clear`. No hace falta usarla para terminar. Cualquier otra palabra se rechaza.
- Los obstáculos se resuelven con fichas, ruedas y botones del propio objeto. No hay comandos, payloads ni tiempos de ataque.
- Las tarjetas expertas llegan después del acierto y nunca son requisito.

## Estado del canon

Canon escrito el 2026-10-07. Cubre una noche, un acto y un desenlace cerrado. No hay acto 2 en preparación.

Este documento no afirma que las tres salas, el catálogo de la operación ni la sesión nueva estén construidos. `missions.json` puede seguir en disco como herencia; no es el progreso de esta historia.

La próxima prosa, si alguien pide seguir, es otra operación posterior y solo con pedido explícito. No reabre a V. Soto como acertijo, no convierte a la segunda persona de aseo en un candado y no devuelve el canon retirado.
