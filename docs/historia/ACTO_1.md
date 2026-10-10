# Acto 1 · Operación Cóndor Silencioso

Recorrido jugable de una noche. Las cifras y los textos de acierto están cerrados en [la biblia](BIBLIA.md). Si algo de aquí discrepa, manda la biblia.

Quien juega camina en primera persona por la sede ficticia de la UCD, en Santiago. Es el equipo de respuesta a incidentes. Vera, Hugo, Nuria, el asistente técnico e Iriarte son grabaciones. Hablar con ellas no marca ningún obstáculo.

## Escena 0 — Briefing

**Dónde:** oficina, antes de que el reloj corra.

**Qué ocurre:** una tarjeta, con subtítulo, no un cuestionario. Al cerrarla empieza el tiempo.

**Texto de la tarjeta:** «Operación Cóndor Silencioso. El enemigo ya tiene la llave. A las 22:00 alguien lanzó una falsa prueba de emergencia. La sede está en lockdown. Un insider con root copia la lista de informantes. Quedan 60 minutos. La salida de emergencia del juego permanece abierta. La palabra es CÓNDOR LIBRE.»

**No romper:** el reloj visible parte de 60:00. En Aprendizaje se ve y no derrota. En Operación, el cero pierde la partida.

## Escena 1 — El corcho

**Dónde:** oficina, muro del corcho. Cuatro correos.

**Vera:** «En el corcho hay un aviso que apura. Compara el dominio con el de la casa.»

**Qué hace el jugador:** abre el correo falso, de `soporte-ti@ucd-ch1le.invalid`. Pide la clave y dice que caduca en diez minutos. Marca las tres señales: dominio, clave y urgencia.

Los otros tres no piden secretos. El de `soporte-ti@ucd-chile.invalid` avisa de una ventana de mantenimiento y dice que el cambio se hace en la consola, con el segundo factor. Recepción (`recepcion@ucd.invalid`) deja la lista de visitas de mañana. Edificio (`edificio@ucd.invalid`) recuerda la prueba de altavoz de las 21:55. Ninguno muestra el dígito.

**Tras el acierto:** el reverso del falso muestra el **4**. Puede aparecer una tarjeta experta. Cerrarla no abre nada más.

**No romper:** el 4 no está en el anverso ni en los correos legales.

## Escena 2 — El escritorio

**Dónde:** escritorio. Post-it «no olvidar: mi equipo + año», banderín «Cóndores», calendario «2026».

**Asistente técnico:** «La clave de esa consola está armada con lo que tienes delante. No la escribas de memoria: elige las dos fichas.»

**Qué hace el jugador:** en el inicio elige la ficha `Condores` y la ficha `2026`. El acierto es `Condores2026`.

**Tras el acierto:** queda a la vista el **7**.

**No romper:** no hay teclado libre. El acento del banderín no entra en la clave.

## Escena 3 — La rueda

**Dónde:** junto al escritorio. La pantalla dice «Último inicio de sesión: 22:17».

**Asistente técnico:** «El segundo factor no es la clave otra vez. Es un código que cambia. El minuto ya está escrito en la pantalla.»

**Qué hace el jugador:** gira la rueda hasta el minuto 17. El token de ese minuto es `492017`.

**Tras el acierto:** la puerta magnética puede abrirse. El rótulo de esa puerta dice «SALA DE SERVIDORES · PUERTA MAGNÉTICA».

**No romper:** el minuto 17 y el token `492017` van juntos. Otro minuto no sirve.

## Escena 4 — El parte

**Dónde:** la hoja de registro, doce líneas del día, de la madrugada a las 22:17.

**Nuria:** «No busques un nombre que el parte no tiene. Busca la hora en la que una cuenta de aseo aparece con un permiso que el aseo no usa.»

**Qué hace el jugador:** elige las 03:14. La línea dice: RACK-03, cuenta `aseo_turno_noche`, permiso ROOT. El minuto que se entrega es `14`.

Las doce líneas son las de la biblia, de las 03:14 a las 22:17. La copia sigue a las 03:16 sin destino escrito; el carro entra a las 18:02 sin privilegio; la tarjeta de V. Soto es denegada a las 20:03; la prueba de emergencia se acepta a las 22:00. Ninguna de esas otras líneas es el acierto.

**Tras el acierto:** quedan a la vista los dígitos **14** y la cuenta `aseo_turno_noche`.

**No romper:** la línea que se marca es la de las 03:14. La denegación de las 20:03 se lee y no es el acierto. La segunda persona de aseo no está en esta hoja.

## Escena 5 — La caja

**Dónde:** caja de evidencia. Cuatro ruedas.

**Hugo:** «El candado junta lo que ya encontraste. Si te falta un dígito, vuelve al objeto que lo enseña.»

**Qué hace el jugador:** marca `4714` cuando ya tiene el 4, el 7 y el 14.

**Dentro:** la credencial «V. Soto. Aseo. Alta de hace meses. 20:03, tarjeta personal denegada. La ficha de convivencia está en el laboratorio.» Y la tarjeta de tráfico: «Fuga AZUL. RACK-03 hacia 203.0.113.44. 2,3 GB, repetida.»

**No romper:** la credencial no nombra a Pixel. Ese nombre está en el muro del laboratorio.

## Escena 6 — Dos puertas

**Dónde:** la puerta magnética del fondo y, en el corredor de entrada, la salida de emergencia.

**Iriarte:** «La magnética espera el segundo factor. La de emergencia no espera nada: siempre está abierta. Si quieres parar de verdad, la palabra es CÓNDOR LIBRE.»

**Qué hace el jugador:** con el segundo factor ya acertado, abre la magnética y entra a servidores. La salida de emergencia se puede usar en cualquier momento. No resta tiempo y lleva al mismo cierre que el botón de la pausa.

**No romper:** abrir la magnética no detiene la copia. Superar la oficina no termina la operación.

## Escena 7 — La hoja de tráfico

**Dónde:** sala de servidores, al otro lado del corredor. Hay racks. Uno está rotulado RACK-03.

**Qué ocurre:** una hoja de diez filas. La misma fuga aparece dos veces: `RACK-03 → 203.0.113.44`, 2,3 GB, palabra AZUL e icono de flecha. Las otras filas son internas y pequeñas, con las palabras GRIS, AMARILLO, ROJO o VERDE. La fila verde es un aviso de vida, corto, dentro de la casa.

**Qué hace el jugador:** señala el azul. Hace falta haber abierto la caja. Si la caja sigue cerrada, la hoja lo dice y no marca la fuga.

**No romper:** la dirección no se contacta. Es de documentación.

## Escena 8 — El panel de cables

**Dónde:** el panel, con cuatro cables a la altura de las manos.

**Qué hace el jugador:** corta el AZUL, rotulado «AZUL · externo» con el icono ▲.

Si corta el VERDE («VERDE · vida», ■), el edificio anuncia una evacuación, el reloj pierde cinco minutos y se puede seguir. Si corta el ROJO («ROJO · energía», ●), pierde dos minutos y la fuga sigue. Si corta el AMARILLO («AMARILLO · voz», ◆), la voz interna se calla y no se abre nada.

**Tras cortar el azul:** aparece el manual del fail-safe.

**No romper:** contener es cortar el azul. El verde es vida. La palabra y el icono acompañan al color.

## Escena 9 — Dos detectores

**Dónde:** el mismo sector, partido por un tabique. Un detector no se ve desde el otro. La lámpara es un prop LED.

**Qué dice el manual:** dos detectores a la vez abren la puerta del laboratorio durante 90 segundos.

**Qué hace el jugador:** deja la lámpara en el primer detector y usa la llave en el segundo antes de que pasen tres segundos. La puerta, rotulada «LABORATORIO · 90 s», se abre. Si se cierra, se repiten la lámpara y la llave.

**No romper:** no hay fuego. Luz y sonido. La puerta no se queda abierta para siempre.

## Escena 10 — El muro

**Dónde:** laboratorio. Seis fichas. La de V. Soto muestra un gato llamado Pixel. C. León también tiene un gato; se llama Sol. Las otras mascotas son Luna, Nube, Mia y Rocky.

**Qué hace el jugador:** elige Pixel. Hace falta la caja abierta, porque la credencial es la que ata el nombre V. Soto a esa ficha.

**No romper:** la especie no basta. La palabra es Pixel. Ningún señuelo es la segunda persona de aseo.

## Escena 11 — La nota

**Dónde:** una nota de configuración en el laboratorio.

**Qué ve el jugador:** la cadena `Q09GUkUgRU4gRUwgQ0FKT04gREVM`, una tabla breve y tres trozos ya alineados que leen `COFRE EN EL CAJON DEL`. Elige la clave entre tres palabras. La que sirve es Pixel.

**Tras el acierto:** la tarjeta muestra `7391` y «MAC termina en :4F».

**No romper:** la frase recuperada va sin tilde, tal como sale de esa cadena. No hay un descifrador que la persona tenga que operar.

## Escena 12 — El plano y el carro

**Dónde:** plano de intensidad y, a la altura de la cintura, el carro de aseo con doble fondo.

**Qué dice el reporte:** el intruso es `DC:A6:32:0E:77:4F` y no está en el inventario. A1 −55 dBm, unos 3 m. A2 −40 dBm, cerca de 1 m. A3 −70 dBm, unos 6 m.

**Qué hace el jugador:** marca el cruce en la celda C-4. El doble fondo se abre y la tablet está ahí.

**No romper:** la celda es C-4. El plano ya trae las distancias escritas.

## Escena 13 — Las huellas

**Dónde:** tres sellos y una hoja. A y B muestran `e8576173473a`, el mismo sello oficial. C, el hallado en la tablet, muestra `21ea12c9e607`.

**Qué hace el jugador:** descarta C.

**No romper:** se descarta C. A y B coinciden con la oficial.

## Escena 14 — El interruptor

**Dónde:** un teclado grande de cuatro dígitos, hecho de botones.

**Qué hace el jugador:** marca `7391`.

Si marca `1793`, el reloj pierde un minuto y la copia sigue. Ese código es la trampa y no entra en la cuenta de los tres errores. Cualquier otro código suma un error. Al tercero, el panel se bloquea 90 segundos.

**Victoria:** «Copia detenida. La lista sigue en el edificio.» La luz de estado del laboratorio pasa a verde. Dentro de la ficción, los ascensores quedan en parada segura. La salida de emergencia sigue abierta.

**No romper:** el juego no explica cómo se fuerzan unos frenos. El interruptor detiene la copia y devuelve los ascensores a esa parada.

## Si el reloj se acaba

Solo en modo Operación, al llegar a cero: «Descarga completa. La lista salió del edificio.» El debriefing se abre igual.

## Si la persona se va

La puerta del corredor o el botón **CÓNDOR LIBRE**: «Salida de emergencia. La operación se detiene sin castigo.» El debriefing se abre igual.

## Debriefing

**Dónde:** el laboratorio, en cualquiera de los tres cierres.

**Qué ocurre:** primero el giro de V. Soto, con las horas 20:03 y 03:14. Después las diez lecciones de la biblia, en ese orden. Después, si se muestra, la frase de que el parte de aseo tenía dos personas y solo una cuenta obtuvo ROOT. Al final, la pregunta «¿Qué vas a cambiar mañana?». Se puede dejar en blanco.

**No romper:** la segunda persona no añade un candado. La pregunta no se corrige. Ninguna lección vuelve a cerrar una puerta.
