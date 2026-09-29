# Acto 1 — El primer día en Estudio 01

Cubre el hub jugable y las cuatro misiones ya publicadas (`recon`, `social`, `identity`, `web`). Nada de este acto pide un archivo nuevo en `missions.json`.

Lugar único: la oficina de Estudio 01. El analista no sale del edificio. Los cuatro guías ocupan su sitio; no caminan detrás del jugador.

## Escena 1 — Recepción

**Dónde:** pantalla de entrada, antes de soltar el cursor dentro de la oficina. Rótulo «ESTUDIO 01 / FORMACIÓN DE ANALISTAS». Título «BLUE / RED». Subtítulo «ANALYST ACADEMY».

**Presentes:** Vera Solís. El analista, todavía del otro lado del cristal de la interfaz.

**Qué ocurre:** Si no hay partida, el progreso dice que la primera misión espera en la pizarra y el botón es «Entrar a la oficina». Si ya hay casos cerrados, Vera cambia el botón a «Continuar en la oficina» y cuenta los cierres. Salir del juego está en la misma lámina. Abajo, la tarjeta de controles: WASD, mouse, E, G, Shift, ESC.

**Vera:** «Tu primer día empieza aquí. Explora la oficina, investiga casos y convierte cada evidencia en una decisión informada. Una persona. Guardado en esta máquina.»

**Sintetiza el jugador:** Entra al estudio para formarse como analista, con el cuerpo y con el expediente.

**No romper:** El texto de esta lámina ya está en el juego. Vera no añade una amenaza ni un tutorial de herramientas.

## Escena 2 — El contrato de la sala

**Dónde:** cartel de primer día, encima de la oficina aún en penumbra.

**Presentes:** Vera.

**Qué ocurre:** Una sola vez, Vera lee el contrato en voz de recepción. El botón es «Entendido · a la oficina».

**Vera:** «Eres un analista junior en una consultora ficticia. Investigas, documentas y defiendes conceptos de ciberseguridad. Esta sala no es un simulador de ataques reales: no hay exploits, Kali ni herramientas de intrusión. Empieza en la pizarra: acepta un ticket y sigue las pistas en el archivo y las bandejas del escritorio.»

**Sintetiza el jugador:** El trabajo del día es criterio y redacción, dentro de un edificio inventado.

**No romper:** El cartel ya está escrito en el onboarding del hub. No se reescribe para sonar más “hacker”.

## Escena 3 — Los pies, antes que el ticket

**Dónde:** entrada de la oficina. Coach Iriarte, chaqueta menta, de cara al analista.

**Presentes:** Coach Iriarte.

**Qué ocurre:** Antes de hablar de casos, el coach hace cumplir las seis lecciones, en orden, y se calla cuando la última silla queda en el suelo. Si la lección ya estaba hecha, no la repite. En un reinicio de demo, vuelve al paso uno junto con el tip de bienvenida.

**Coach:** «Primero los pies. El caso espera en la pared.» Luego, solo las seis líneas canónicas, una por vez: caminar, mirar, correr, resaltar una silla, tomarla con G, soltarla con G.

**Sintetiza el jugador:** Puede habitar la oficina. Todavía no ha aceptado un trabajo.

**No romper:** Iriarte no enseña conceptos de seguridad. La silla es una silla.

## Escena 4 — La sala y el mueble rojo

**Dónde:** oficina. Escritorio con laptop, cuaderno y tres bandejas. Buzón de informe. Pizarra al fondo. Puerta con rótulo hacia el archivo. Panel de pistas en la pared. Rack rojo a un lado.

**Presentes:** Hugo, a distancia, junto a la pizarra. Nuria, junto al buzón, en silencio hasta que haya un expediente. El coach ya no habla.

**Qué ocurre:** Un tip breve, la primera vez: caminar, mirar, usar, tomar una carpeta, y mirar la pizarra. La guía corta de la sala apunta al ticket. El analista puede acercarse al rack antes de aceptar nada.

**Hugo, si miran a Atlas:** «Ese servidor no es de tu cliente. Sin autorización explícita no se investiga, aunque esté al alcance de la mano. Atlas es el recordatorio rojo. No es un sobre.»

**Sintetiza el jugador:** Estar cerca de un sistema no abre permiso para evaluarlo.

**No romper:** Atlas es el proveedor de pagos excluido del caso Lumen y, en el espacio, el interactuable de fuera de alcance. Tocarlo muestra el alcance del ticket activo, o avisa de que no hay ticket. No hay castigo ni contenido oculto dentro del rack.

## Escena 5 — Cuatro sobres, uno abierto

**Dónde:** pizarra de tickets.

**Presentes:** Hugo Pellicer.

**Qué ocurre:** Cuatro tarjetas, en el orden del catálogo. Solo la primera acepta E. Las otras esperan el cierre anterior. Sin misión activa, el objetivo pintado abajo dice que hay que aceptar un ticket.

**Hugo:** «Cuatro sobres. Uno abierto. Acepta antes de tocar. Cuando Nuria cierre el informe, el siguiente sobre se destapa solo.»

**Sintetiza el jugador:** El orden del aprendizaje es el orden de la pared.

**No romper:** Los ids y clientes de los cuatro sobres son los publicados. Hugo no adelanta el contenido de una ficha que el archivo aún no ha entregado.

## Escena 6 — Lumen: antes de tocar nada

**Dónde:** ticket `recon`, archivo, bandejas, cuaderno, laptop, buzón.

**Presentes:** Hugo al aceptarlo. Nuria al cerrarlo. El analista en el recorrido completo, que los casos siguientes repetirán sin volver a describir cada paso.

**Qué ocurre:** E en el ticket. Toast con el brief y el alcance. La puerta del archivo se cruza. Tres carpetas (y el cajón, si se abre, no esconde una cuarta pista en este caso). Cada carpeta se lee, se lleva al escritorio y se clasifica: primero qué se puede afirmar, después qué defensa corresponde. El cuaderno gana la ficha del concepto. La laptop, si se usa, responde a `scan`, `map`, `inspect`, `notes` y `report` sobre fichas locales. Con el checklist en verde, E en el buzón abre el expediente: una frase de cierre y tres preguntas.

**Hugo, al soltar el ticket:** «Lumen prepara el archivo digital. Tu trabajo es ordenar las pistas y distinguir lo autorizado. Solo el archivo ficticio y sus documentos locales. El proveedor de pagos, Atlas, está excluido. El primer hallazgo de un buen analista suele ser una buena pregunta.»

**Pistas, en el orden del archivo:**

- Mapa de bienvenida. Tres piezas: portal de lectura, área de empleados, catálogo. La ficha de responsables solo menciona el portal. Hugo, si el analista duda: «Un inventario describe lo observado. No convierte una sospecha en una vulnerabilidad confirmada.»
- Carta de autorización. Se pueden analizar los documentos simulados de Lumen. Atlas queda fuera. Hugo: «La visibilidad no otorga permiso. Anota la dependencia y déjala quieta. Si hiciera falta más, se pide una ampliación explícita.»
- Post-it: «El antiguo catálogo quizá siga publicado.» Sin fecha ni dueño. Hugo: «Eso es una hipótesis. La confirma el propietario, no tu prisa.»

**Nuria, en el buzón:** «Elige la frase que las tres carpetas pueden sostener.»

**Sintetiza el jugador:** Se identificaron tres activos, una dependencia excluida y una pista por confirmar. Completar inventario y responsables.

**No romper:** No se “descubre” que el archivo está comprometido ni que hay que apagarlo. No haber hecho pruebas tampoco demuestra que no haya riesgos: demuestra que el mapa todavía es el trabajo.

## Escena 7 — Nébula: no todo es lo que parece

**Dónde:** el mismo recorrido. Ticket `social`. Cuatro mensajes; el cuarto puede salir del cajón como USB de utilería.

**Presentes:** Hugo y Nuria.

**Qué ocurre:** El buzón de la cooperativa, dentro de la ficción, dejó cuatro mensajes. El analista clasifica sin abrir enlaces y sin contestar. La guía de la sala vuelve a señalar archivo, bandeja y, al final, el buzón.

**Hugo:** «La urgencia es una señal, no una sentencia. Explica la decisión y elige un canal de verificación que ya conocieras antes del mensaje. Los enlaces de estas hojas son texto muerto. No se envían respuestas ni reportes fuera del estudio.»

**Pistas:**

- Nómina, 09:12, dominio de adorno. Pide contraseña y código, con reloj y secreto. Nuria, adelantando el criterio: «Reportar. La mezcla de secretos, presión y aislamiento es la señal. No se contesta “solo el código”.»
- Cambio de cuenta de un proveedor habitual, 09:24. El remitente coincide con un contacto; el cambio no estaba previsto; el teléfono nuevo viene en el mismo correo. Hugo: «Verifica con el número que ya estaba en el directorio. El canal escrito en el mensaje dudoso no es independiente.»
- Recordatorio de reunión, 09:31. Está en el calendario interno ya abierto. Mismos participantes, misma sala, sin adjuntos ni petición de datos. Nuria: «Sigue el flujo habitual. Detectar también es no denunciar un recordatorio corroborado.»
- Regalo del director, 09:42. Tarjetas, códigos, que no se hable con administración, urgente. Hugo: «Autoridad de papel, prisa y un control que te piden saltar. Se reporta y se corrobora por un canal interno conocido.»

**Sintetiza el jugador:** Reportar nómina y regalo; verificar el cambio bancario por un canal conocido; mantener el flujo habitual de la reunión corroborada.

**No romper:** Ningún mensaje se trata como correo real. El “reporte” es una práctica dentro del juego. No aparece un enlace que el jugador deba visitar.

## Escena 8 — Orbital: una llave no basta

**Dónde:** ticket `identity`. Las “pistas” son políticas simuladas, no cuentas vivas.

**Presentes:** Hugo y Nuria.

**Qué ocurre:** Orbital quiere reemplazar reglas de acceso. El analista elige una política viable para el trabajo diario, incluida la recuperación. En la laptop, `simulate` solo tiene sentido en este ticket y solo relata el escenario.

**Hugo:** «La seguridad también depende de lo fácil que sea hacer lo correcto. Aquí no se piden contraseñas reales y no se rompe ninguna clave. Se leen consecuencias ficticias.»

**Pistas:**

- Una clave para todo. Si se filtra en el proveedor, correo y archivo quedan en el mismo riesgo. Defensa: largas, únicas, en un gestor.
- Reglas que estorban: claves cortas, cambios periódicos sin indicios de compromiso, pegado bloqueado. El equipo inventa patrones y evita el gestor. Defensa: longitud, unicidad, permitir el gestor, bloquear claves ya conocidas como comprometidas. El cambio de credencial corresponde cuando hay evidencia de compromiso.
- Dos contraseñas memorizadas. Siguen siendo un solo tipo de factor: algo que se sabe. Defensa: passkeys con verificación de la persona, o llaves de seguridad bien configuradas.
- La recuperación: el acceso pide un segundo factor, y soporte lo quita a quien diga el nombre de la cuenta. Nuria: «Revisa esa puerta. Verificar la identidad en la recuperación y custodiar los códigos. Apagar el segundo factor para que “todas las rutas sean iguales” empeora el ciclo.»

**Sintetiza el jugador:** Claves únicas con gestor, factores resistentes al phishing y recuperación protegida.

**No romper:** No hay ejemplo de contraseña, ni lista de claves filtradas, ni método para probarlas.

## Escena 9 — Aurora: menos puertas abiertas

**Dónde:** ticket `web`. Cuatro fichas de inventario. Direcciones `.invalid` pintadas en el papel.

**Presentes:** Hugo y Nuria.

**Qué ocurre:** El museo va a renovar la exposición digital. El analista marca solo los riesgos que el inventario respalda y entrega un plan de reducción de exposición. La laptop no llama a esos nombres.

**Hugo:** «Que una API sea pública no demuestra un fallo. Mira qué publica, quién la necesita y qué controles describe la ficha.»

**Pistas:**

- Acceso del personal por HTTP, sin canal protegido ni redirección. Uso interno. Lo respaldado: las credenciales en tránsito carecen de protección. Mejora: exigir HTTPS y cuidar certificados y redirecciones. Hugo: «El canal protegido no convierte el sitio en honesto ni sustituye la autenticación.»
- Panel de administración público, usado solo desde la oficina, con ganas de abrirlo a toda la plantilla. Exposición de más y permisos más anchos que la tarea. Defensa: quien lo necesita, autenticación reforzada, mínimo privilegio. Un nombre distinto no es un control.
- Catálogo público: solo lectura, HTTPS, sin datos personales, responsable y revisión, necesario para la web. Nuria: «Exposición intencional. Se queda en el inventario y se revisa. No se borra por ser visible.»
- Micrositio de una exposición cerrada hace dos años, sin responsable, fuera del mantenimiento, sin confirmación de que aún haga falta. Se registra como activo sin dueño y posiblemente obsoleto. Acción: asignar responsable, confirmar la necesidad y retirarlo con control si ya no se usa.

**Sintetiza el jugador:** Proteger el login con HTTPS, limitar la administración, revisar el micrositio y mantener el catálogo público bajo control.

**No romper:** No se declara comprometido ningún sitio. No se “cierra toda la web” porque exista una API pública.

## Escena 10 — Cuatro sobres en verde

**Dónde:** pizarra, después del quiz de Aurora.

**Presentes:** Nuria, luego Hugo. Vera, solo como eco del contador de la recepción: cuatro de cuatro.

**Qué ocurre:** Cada cierre ya mostró el toast de misión cerrada, la puntuación y la flecha de vuelta a la pizarra. Al cuarto, no queda ticket bloqueado ni ticket abierto. El cuaderno tiene las ocho fichas: reconocimiento, alcance, ingeniería social, verificación, contraseñas, autenticación multifactor, superficie, transporte. El rack rojo sigue rojo.

**Nuria:** «Cuatro informes. Cada uno cabe en sus carpetas. Eso es el primer día.»

**Hugo:** «La pared se ha quedado en verde. El estudio sigue teniendo archivo, bandejas y buzón. Cuando llegue otro sobre, se acepta igual: antes de tocar, se lee el alcance.»

**Sintetiza el jugador:** Completó el mapa, los mensajes, las políticas y el inventario del museo sin salir del permiso de cada ticket.

**No romper:** El juego, hoy, no entrega todavía una credencial nueva ni abre una segunda sala. El acto termina en la oficina conocida. El deseo de más casos queda como gancho, no como contenido ya publicado en el JSON.

## Gancho

En algún cajón de la recepción, Vera tiene tres sobres sin colgar: un taller al que ya le ocurrió una copia, una editorial con una llave compartida, un puerto de trabajo donde la visita y el archivo comparten sitio. No están en la pizarra. Son el acto 2, y solo existen en `docs/historia/MISIONES_SIGUIENTES.md` hasta que alguien pida volcarlos al catálogo.
