# Recorrido · Operación Cóndor Silencioso

El archivo conserva el nombre `07-lockdown-fbi.md` para no romper enlaces. El recorrido que describe es el de la UCD en Santiago.

Estado: canon confirmado el 2026-10-07. Las soluciones están cerradas en [la biblia](../historia/BIBLIA.md). Si una cifra de aquí discrepa, manda la biblia. Este diseño no declara las salas construidas.

Frase: «El enemigo ya tiene la llave.»

## Experiencia

Una persona, en primera persona, recorre una sede ficticia bajo lockdown. A las 22:00 una prueba de emergencia falsa cerró el edificio. Quien juega es el equipo de respuesta de la UCD. Un insider con root copia una lista de informantes, también ficticia. El reloj visible empieza en 60:00 al cerrar el briefing.

Cada obstáculo se resuelve en el mundo: mirar, relacionar, actuar y ver un cambio físico. El manual define el concepto en castellano llano. La radio ofrece hasta tres pistas: el objeto, la relación y la acción. Si pasa la mitad del tiempo previsto de un obstáculo sin pista pedida, la primera puede llegar sola por la radio. Esas duraciones las fija el catálogo; esta página no inventa los segundos de cada uno.

Leer el manual, el cuaderno o una tarjeta experta no completa el obstáculo. La tarjeta experta, si aparece, es posterior al acierto y se puede cerrar. La puerta siguiente no la comprueba. No hay examen como compuerta.

La salida de emergencia del corredor de entrada siempre está abierta. En la pausa, el botón **CÓNDOR LIBRE** hace lo mismo: pausa, sin castigo, y deja el debriefing a mano.

## Tres salas

| Sala | Fase del arco | Obstáculos | Cambio visible |
| --- | --- | --- | --- |
| Oficina | Detectar | Correo, clave, segundo factor, registro, caja `4714` | La puerta magnética abre a servidores cuando el segundo factor está resuelto |
| Servidores | Contener | Hoja de tráfico, cable azul, dos detectores | La puerta del laboratorio se abre 90 segundos |
| Laboratorio | Identificar y detener | Ficha de V. Soto, nota, plano, huellas, interruptor `7391` | La copia se detiene y la luz de estado pasa a verde |

```text
Oficina
  corcho → escritorio → rueda → parte → caja 4714
  puerta magnética  --------------------  salida de emergencia (siempre)
        |
Servidores
  hoja AZUL → cable azul → lámpara LED + llave
        |
Laboratorio (90 s, repetible)
  Pixel → nota → C-4 → descartar C → 7391
        |
Debriefing (victoria, tiempo agotado o CÓNDOR LIBRE)
```

La oficina existente es la primera sala. Servidores queda al otro lado del corredor. El laboratorio queda pasado su puerta. La planta es ficticia. No se agacha la cámara para usar un objeto: lo interactivo queda entre 0,9 m y 1,4 m, y el doble fondo del carro a la altura de la cintura.

## Orden que el acierto exige

| Acierto | Exige antes |
| --- | --- |
| Correo, clave, segundo factor, registro | Nada |
| Caja `4714` | Los dígitos 4, 7 y 14 ya revelados |
| Tráfico azul | Caja abierta |
| Cortar el cable azul como contención | Tráfico azul marcado |
| Detectores | Cable azul cortado |
| Pixel | Caja abierta |
| Nota con clave Pixel | Pixel elegido |
| Celda C-4 | La nota ya mostró el sufijo `:4F` |
| Descartar la huella C | Tablet encontrada en el carro |
| `7391` | Huella C descartada |

La puerta magnética mira el segundo factor, no la caja. Se puede entrar a servidores y encontrar la hoja muda hasta abrir la caja. La credencial y la tarjeta azul viajan dentro de la caja y se leen en la oficina; se usan en las salas siguientes.

Cortar verde, rojo o amarillo tiene efecto aunque el azul todavía no esté marcado. Solo el azul, después de la hoja, abre el manual del fail-safe.

## Oficina

Objetos del recorrido: radio de pistas, corcho con cuatro correos, inicio por fichas, rueda del token, hoja de doce líneas, caja, puerta magnética y salida de emergencia. El reloj de pared muestra `60:00` al empezar.

Correos:

1. Falso, `soporte-ti@ucd-ch1le.invalid`. Asunto: la clave caduca en diez minutos. Pide la clave actual y apura. Señales: dominio, clave, urgencia. El reverso enseña el 4 solo tras marcar las tres.
2. Legal, `soporte-ti@ucd-chile.invalid`. Ventana de mantenimiento. No pide claves. Dice que el cambio es en la consola, con el segundo factor.
3. `recepcion@ucd.invalid`. Visitas de mañana. No pide secretos.
4. `edificio@ucd.invalid`. Recuerda la prueba de altavoz de las 21:55.

Clave: post-it «no olvidar: mi equipo + año», banderín «Cóndores», calendario «2026». Fichas `Condores` y `2026`. Acierto `Condores2026`. Revela el 7.

Rueda: «Último inicio de sesión: 22:17». Minuto 17, token `492017`.

Parte, en orden del día. Se elige la línea 1; el minuto entregado es `14`.

1. 03:14 — RACK-03. Cuenta `aseo_turno_noche`. Permiso ROOT.
2. 03:16 — RACK-03. La copia hacia fuera sigue activa. Sin destino en esta hoja.
3. 03:40 — Canal de voz interno. Sin destino exterior.
4. 04:02 — Energía de los racks estable. Sin alarma de vida.
5. 18:02 — El carro de aseo entra al pasillo. Sin privilegio.
6. 20:03 — Tarjeta personal de V. Soto: DENEGADA.
7. 21:40 — Ronda de cierre de recepción. Sin novedad.
8. 21:55 — Prueba de altavoz. Sin fallo.
9. 22:00 — El panel acepta una prueba de emergencia.
10. 22:00 — Puertas de planta en lockdown.
11. 22:05 — El equipo de respuesta queda dentro.
12. 22:17 — Último inicio de sesión en la consola de guardia.

Caja `4714`. Dentro, la credencial de V. Soto (aseo, alta de hace meses, denegada a las 20:03, ficha de convivencia en el laboratorio) y la tarjeta «Fuga AZUL. RACK-03 hacia 203.0.113.44. 2,3 GB, repetida.»

Las bandejas, el buzón, la pizarra de fases y los seguros heredados no son el avance. El diseño de la oficina los aparta de la vista y de la colisión. Consultar una grabación no marca banderas.

## Servidores

Hoja de diez filas. La fuga ocupa la 3 y la 5.

| # | Recorrido | Volumen | Palabra |
| --- | --- | --- | --- |
| 1 | RACK-01 → impresora.ucd.invalid | 12 MB | GRIS |
| 2 | RACK-02 → reloj.interno.invalid | 1 MB | GRIS |
| 3 | RACK-03 → 203.0.113.44 | 2,3 GB | AZUL, flecha |
| 4 | RACK-04 → voz.interno.invalid | 40 MB | AMARILLO |
| 5 | RACK-03 → 203.0.113.44 | 2,3 GB | AZUL, flecha |
| 6 | RACK-05 → archivo.interno.invalid | 80 MB | GRIS |
| 7 | RACK-01 → energia.interno.invalid | 4 MB | ROJO |
| 8 | RACK-02 → vida.interno.invalid | 2 MB | VERDE |
| 9 | RACK-04 → tablero.interno.invalid | 15 MB | GRIS |
| 10 | RACK-05 → correo.ucd.invalid | 30 MB | GRIS |

Cables, con palabra e icono: AZUL ▲ externo, VERDE ■ vida, ROJO ● energía, AMARILLO ◆ voz.

- Azul: contención. Aparece el manual.
- Verde: evacuación dramática, −5 min, se sigue. La copia continúa.
- Rojo: −2 min. La fuga sigue.
- Amarillo: la voz se corta. No abre paso.

Fail-safe: un tabique impide ver los dos detectores a la vez. La lámpara LED se deja en el primero y la llave se usa en el segundo dentro de los tres segundos siguientes. La puerta «LABORATORIO · 90 s» se abre. Si vence, se repite. No hay fuego.

## Laboratorio

Muro, de izquierda a derecha:

| Ficha | Función | Mascota |
| --- | --- | --- |
| V. Soto | aseo | gato Pixel |
| A. Ruiz | guardia | perro Luna |
| M. Palma | recepción | pez Nube |
| C. León | servidores | gato Sol |
| R. Díaz | laboratorio | loro Mia |
| J. Peña | archivo | hámster Rocky |

El acierto es Pixel. Sol no sirve.

La nota muestra `Q09GUkUgRU4gRUwgQ0FKT04gREVM`, una tabla breve y tres trozos ya alineados: `COFRE EN EL CAJON DEL`. Entre tres palabras, la clave es Pixel. Entonces se revelan `7391` y «MAC termina en :4F».

El plano nombra al extraño `DC:A6:32:0E:77:4F`, ausente del inventario. A1 −55 dBm ≈ 3 m, A2 −40 dBm ≈ 1 m, A3 −70 dBm ≈ 6 m. Tres marcas de distancia se cruzan en C-4. El carro abre el doble fondo y muestra la tablet.

Huellas: A y B igualan la oficial `e8576173473a`. C, hallada, es `21ea12c9e607`. Se descarta C.

El interruptor es un teclado de botones, cuatro dígitos. `7391` gana. `1793` resta un minuto y no entra en la cuenta del bloqueo. Otros tres códigos errados bloquean el panel 90 segundos.

Textos de cierre:

- Victoria: «Copia detenida. La lista sigue en el edificio.» Luz de estado en verde. Los ascensores, dentro de la ficción, quedan en parada segura. El juego no describe frenos.
- Tiempo agotado, solo en Operación: «Descarga completa. La lista salió del edificio.»
- Salida de emergencia: «Salida de emergencia. La operación se detiene sin castigo.»

## Debriefing

En los tres cierres, y en este orden: el párrafo de V. Soto (alta de aseo hace meses, tarjeta denegada a las 20:03, ROOT de `aseo_turno_noche` a las 03:14), las diez lecciones de la biblia, la frase opcional de la segunda persona de aseo, y la pregunta «¿Qué vas a cambiar mañana?». La pregunta acepta el blanco. No puntúa y no abre una puerta.

## Reloj

| Suceso | Efecto |
| --- | --- |
| Cerrar el briefing | El visible corre desde 60:00 |
| Cable verde | +5 min consumidos, evacuación, se sigue |
| Cable rojo | +2 min consumidos |
| `1793` | +1 min consumido |
| Tres códigos ajenos a `7391` y a `1793` | Bloqueo del panel 90 s |
| Cero en Operación | Derrota |
| Cero en Aprendizaje | El reloj sigue siendo visible y no hay derrota |
| Manual abierto en Aprendizaje | El reloj se pausa |
| CÓNDOR LIBRE o la puerta de emergencia | Pausa, sin castigo |

## Qué queda fuera de este recorrido

Los ductos, el vestíbulo como ruta y un incendio provocado por el equipo no forman parte de la operación. Los tickets de la consultora anterior siguen en `docs/historia/MISIONES_SIGUIENTES.md` como archivo histórico: no se construyen como continuación.

La terminal es local y opcional: `help`, `scan`, `map`, `notes`, `clear`. No termina un obstáculo.
