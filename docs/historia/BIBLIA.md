# Biblia — Blue / Red · Analyst Academy

Canon narrativo del laboratorio. Los casos ya jugables viven en `Assets/Resources/Content/missions.json`. Esta biblia no los sustituye: fija premisa, voces y reglas para que un acto nuevo no las rompa.

## Estado del serial

- Acto escrito: `docs/historia/ACTO_1.md` (hub actual, misiones 01–04 ya publicadas).
- Fichas futuras, todavía fuera del JSON: `rastro`, `permisos`, `fronteras` en `docs/historia/MISIONES_SIGUIENTES.md`.
- Próximo archivo si piden «sigue la historia»: `docs/historia/ACTO_2.md`, dramatizando esas tres fichas en la misma oficina.
- Quién escribe: el agente `.cursor/agents/historiador.md`.

## Premisa

Estudio 01 es una consultora ficticia que forma analistas. El jugador llega el primer día con una credencial Blue / Red y una sola oficina. No hay red real, no hay víctima y no hay herramienta de intrusión.

El trabajo consiste en aceptar un ticket autorizado, leer fichas locales, separar hecho de hipótesis y cerrar con un informe que un equipo podría aplicar: qué se observó, qué queda fuera, qué control falta.

Se gana por comprender el concepto y defenderlo. El edificio es de mentira. El criterio con el que se redacta el informe es la parte seria del juego.

Público 18–32. Una persona, offline, PC primero. La misma oficina podrá servir más tarde a un visor; la historia no depende de eso.

## Tono

La imagen se mueve hacia un cell shading caricaturesco: contornos claros, siluetas legibles, gestos un poco teatrales. Vera sella papeles en el aire. El coach tiene la chaqueta menta demasiado recta. Hugo abre los tickets como un abanico. Nuria sostiene la bandeja del informe con las dos manos, como si pesara.

La frase pedagógica no se caricaturiza. Español neutro, concreto, sin jerga de ataque. Cuando el jugador se equivoca, el mundo pide releer la evidencia. No hay game over, calaveras, terminal de película ni voz infantil.

`docs/ART_BIBLE.md` describe todavía el look sobrio anterior. Los píxeles siguen a cargo de ese documento y de quien modele. Esta biblia manda en las palabras.

## Elenco

Nadie del elenco pide una contraseña, un comando ni “probar el fallo”. Hablan de alcance, señales y redacción.

### El analista (jugador)

Junior de primer día. Sin nombre fijo: la credencial dice Blue / Red y el cargo. Mira en primera persona, camina la oficina y deja su voz en el informe. La síntesis correcta es suya.

### Coach Iriarte

Figura junto a la entrada, rótulo **COACH**, chaqueta menta. Enseña el cuerpo en la sala: caminar, mirar, correr, señalar una silla, tomarla y soltarla. Cuando la lección termina, se aparta. No comenta los casos. Si alguien le pregunta por un ticket, señala la pizarra con la barbilla.

Las seis líneas que el juego ya muestra son ley:

1. Camina con WASD un par de metros.
2. Mueve el mouse para mirar la oficina.
3. Mantén Shift mientras caminas para correr.
4. Mira una silla hasta que se resalte.
5. Pulsa G para tomar la silla.
6. Pulsa G otra vez. La silla se queda donde cae.

### Vera Solís — recepción

Recibe en la pantalla de entrada y en el cartel de primer día. Frases cortas, como sellos. Repite el nombre del estudio y la frase ya pintada en el juego: «Tu primer día empieza aquí.» También deja claro qué sala es esta: investigar, documentar y defender conceptos. La sala no simula ataques reales.

### Hugo Pellicer — casos

Vive en la pizarra. Entrega un ticket cuando el anterior está cerrado. Lee el alcance en voz alta, incluido lo que queda fuera. Su muletilla: «Acepta antes de tocar.» Trata a Atlas como un mueble con letrero, no como un caso.

### Nuria Quintero — informe

Vive en el buzón del escritorio. No abre el expediente hasta que el checklist de la pared está documentado. Exige una frase que la evidencia pueda sostener. Descarta el cierre catastrofista y el cierre vacío. Después del informe pide el quiz, tres preguntas, y devuelve al analista a la pizarra.

## Qué es Blue y qué es Red

Son dos lentes de la misma credencial, no dos equipos ni dos finales.

**Blue** es la lente de lo que hay. El analista inventaría activos, nombra responsables, marca el alcance autorizado y elige un control proporcionado: completar un inventario, verificar por un canal ya conocido, separar una cuenta, exigir un transporte protegido, retirar lo que ya no tiene dueño.

**Red** es la lente de lo que la ficción ya dio por sucedido. El analista lee el síntoma, el impacto y el control que faltó, para que el informe explique por qué ese control importa. Red no otorga permiso, no describe el camino para repetir el fallo y no convierte al jugador en atacante.

En un mismo ticket se usan las dos. Blue pregunta «¿qué está autorizado y qué control corresponde?». Red pregunta «¿qué quedó registrado del fallo y qué hueco lo hizo posible?». La respuesta siempre sale por el buzón de Nuria.

## La oficina (hub único)

Una sala y un archivo, al otro lado de una puerta. El recorrido de cada caso es físico:

1. Pizarra: E en el ticket disponible. Hugo suelta brief y alcance.
2. Puerta del archivo, carpetas (E leer, G tomar). La cuarta pista, si existe, es el USB del cajón.
3. Bandejas del escritorio: observación y después defensa.
4. Cuaderno: ficha del concepto desbloqueado.
5. Laptop: terminal narrativa. Vocabulario permitido: `help`, `scan`, `map`, `inspect`, `simulate`, `notes`, `report`, `clear`. `scan` lista las fichas del ticket. No hay escaneo de red.
6. Buzón de informe, cuando todas las pistas están documentadas: informe y quiz. Toast de misión cerrada y vuelta a la pizarra.

El rack rojo del rincón es **Atlas**. El letrero dice que no es el cliente. Tocarlo enseña la frase de alcance; no abre un quinto caso ni castiga.

Los tickets se abren en orden. Completar el anterior desbloquea el siguiente.

## Casos publicados (no contradecir)

| Número | Id | Título | Cliente | Conceptos | Síntesis que Nuria acepta |
| --- | --- | --- | --- | --- | --- |
| 01 | `recon` | Antes de tocar nada | Lumen Studio | reconocimiento, alcance | Tres activos, una dependencia excluida (Atlas, pagos) y una pista por confirmar. Completar inventario y responsables. |
| 02 | `social` | No todo es lo que parece | Cooperativa Nébula | ingeniería social, verificación | Reportar nómina y regalo; verificar el cambio bancario por un canal conocido; mantener la reunión corroborada. |
| 03 | `identity` | Una llave no basta | Orbital Works | contraseñas, autenticación multifactor | Claves únicas con gestor, factores resistentes al phishing y recuperación protegida. |
| 04 | `web` | Menos puertas abiertas | Museo Aurora | superficie, transporte | HTTPS en el acceso, limitar la administración, revisar el micrositio sin dueño y mantener el catálogo público bajo control. |

Hechos fijos de esos casos:

- Lumen: portal de lectura, área de empleados y catálogo. La ficha de responsables solo nombra el portal. El post-it del catálogo antiguo es hipótesis. Atlas queda expresamente fuera.
- Nébula: cuatro mensajes de ficción. Los enlaces no se abren. Urgencia y secreto no bastan para obedecer; un recordatorio corroborado tampoco se denuncia por reflejo.
- Orbital: políticas simuladas. No hay claves reales ni pruebas de cracking. Dos contraseñas no son dos factores. La recuperación débil deshace el acceso fuerte.
- Aurora: direcciones `.invalid` decorativas. HTTP en el acceso del personal deja las credenciales sin protección de transporte. Una API pública de títulos, en solo lectura y con responsable, puede ser exposición intencional. El micrositio de la exposición cerrada no tiene dueño. HTTPS no declara honesto un sitio ni vivo un activo abandonado.

Conceptos ya ocupados: `recon`, `scope`, `phishing`, `verification`, `passwords`, `mfa`, `surface`, `transport`.

## Reglas de escritura

- Un fallo se cuenta en pasado de ficción: qué se vio, a quién afectó dentro del caso, qué control no estaba. Sin pasos para reproducirlo.
- Dominios `.invalid`. Sin correos enviados, sin peticiones a esos hosts, sin credenciales copiables.
- La visibilidad no es autorización. Registrar una dependencia no es evaluarla.
- Una pista no es una vulnerabilidad confirmada. Una hipótesis espera al propietario.
- El informe nombra lo que la evidencia sostiene y el control proporcionado. Ni “apagarlo todo” ni “no hay riesgo porque no probamos”.
- Los personajes nuevos, si algún acto los necesita, entran como personal del estudio o como cliente ficticio con ticket. No entra un antagonista operativo.
- El acto 2 y los siguientes siguen en Estudio 01, con los mismos cuatro guías, hasta que esta biblia diga lo contrario en una revisión pedida por quien dirige el juego.

## Cómo se sigue

Otra sesión puede decir: «Usa el subagente historiador y sigue la historia.» El agente lee esta biblia, el último acto y las fichas, y escribe el siguiente acto sin volcar `missions.json`.
