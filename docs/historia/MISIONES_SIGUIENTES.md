# Misiones siguientes

Tres tickets futuros para Estudio 01. **No están en** `Assets/Resources/Content/missions.json`. El acto 1 no los usa. Cuando pidan «sigue la historia», el historiador los dramatiza en `docs/historia/ACTO_2.md` sin pegarlos al JSON.

Esquema: `docs/CONTRATO_CONTENIDO.md`. Acentos permitidos: `cyan`, `amber`, `violet`, `green`. Terminal: `scan`, `inspect`, `notes`, `report` (el juego también entiende `map`, `help`, `clear`; `simulate` sigue reservado al caso Orbital).

Estado: fichas listas para volcar. Prosa de acto todavía no escrita.

Al volcar, más adelante y en una pasada aparte: añadir los conceptos al arreglo `concepts` y estas misiones al final de `missions`, con `number` 05, 06 y 07. No reordenar las cuatro primeras.

## Conceptos nuevos

```json
[
  {
    "id": "timeline",
    "name": "Línea de tiempo",
    "category": "Observación",
    "definition": "Ordenar hechos ya registrados y separar lo que el registro muestra de lo que sigue siendo hipótesis.",
    "importance": "Un informe que rellena huecos con nombres convierte una hora en un culpable.",
    "defense": "Conservar registros con fecha, sistema y responsable, y dejar escrito lo que aún no se sabe."
  },
  {
    "id": "records",
    "name": "Registros que avisan",
    "category": "Sistemas",
    "definition": "Elegir eventos útiles y revisar si alguien se entera a tiempo.",
    "importance": "Un registro que nadie mira no cambia el curso de un fallo ya ocurrido.",
    "defense": "Definir qué eventos importan, quién los revisa y cómo se comprueba que el aviso llega."
  },
  {
    "id": "privilege",
    "name": "Mínimo privilegio",
    "category": "Identidad",
    "definition": "Conceder solo el acceso que el trabajo actual necesita.",
    "importance": "Un permiso de más sigue abierto aunque la persona ya no lo use.",
    "defense": "Revisar cada rol contra la tarea real y quitar lo que sobra."
  },
  {
    "id": "offboarding",
    "name": "Altas, cambios y bajas",
    "category": "Identidad",
    "definition": "El acceso acompaña la entrada, el cambio de función y la salida.",
    "importance": "Una baja a medias deja una puerta a nombre de quien ya no trabaja.",
    "defense": "Fecha de fin, responsable de la revisión y comprobación de que el acceso se cerró."
  },
  {
    "id": "segment",
    "name": "Segmentación",
    "category": "Sistemas",
    "definition": "Separar zonas para que un equipo de una zona no alcance por defecto los servicios de otra.",
    "importance": "Una red plana convierte un acceso de cortesía en alcance sobre sistemas internos.",
    "defense": "Dibujar las zonas, dejar en la de visitas solo lo que deben usar y revisar las excepciones."
  },
  {
    "id": "boundary",
    "name": "Frontera de confianza",
    "category": "Sistemas",
    "definition": "El límite donde deja de bastar compartir lugar para compartir acceso.",
    "importance": "El mismo edificio o la misma red de cortesía no son una autorización.",
    "defense": "Tratar cada zona como un alcance distinto y documentar qué puede cruzar."
  }
]
```

## 05 — Lo que ya pasó

**Prosa.** Taller Bruma fabrica maquetas para estudios pequeños. La semana pasada, dentro de la ficción, alguien copió de golpe la carpeta de bocetos desde un puesto que el papel daba por retirado. Hugo entrega el ticket como expediente ya cerrado en lo operativo: el hecho ocurrió, el analista no lo repite. Nuria quiere una frase con hora, máquina y hueco de control, sin nombre de culpable.

**Hugo:** «Bruma ya vivió la copia. Tú ordenas lo que quedó escrito. No busques cómo se haría otra vez.»

**Nuria:** «Si no hay nombre en el registro, tu informe tampoco inventa uno.»

```json
{
  "id": "rastro",
  "number": "05",
  "type": "REGISTROS",
  "title": "Lo que ya pasó",
  "subtitle": "Línea de tiempo y avisos",
  "client": "Taller Bruma",
  "duration": "4 min",
  "accent": "cyan",
  "concepts": ["timeline", "records"],
  "brief": "En Taller Bruma la copia de una carpeta de bocetos ya ocurrió. Ordena los registros ficticios, separa lo que se sabe de lo que se sospecha y señala el control que no estaba.",
  "mentor": "Un registro muestra una hora y un puesto. El nombre de una persona, si no está escrito, no se añade.",
  "scope": "Expediente local del incidente ya cerrado en la ficción: aviso, ficha del puesto y nota de avisos. No se accede a equipos ni se reproducen acciones.",
  "objective": "Redacta una línea de tiempo respaldada y el control de registro que faltó.",
  "commandHint": "scan · inspect aviso · notes · report",
  "evidence": [
    {
      "id": "aviso",
      "label": "Aviso del lunes",
      "kind": "REGISTRO",
      "icon": "archive",
      "source": "Carpeta del incidente / aviso-lunes.txt",
      "body": "Lunes, 08:40. El archivo de bocetos registra una copia masiva hacia el puesto BRUMA-VIEJA. El aviso no nombra a ninguna persona. El taller detuvo el uso de esa carpeta ese mismo día.",
      "question": "¿Qué puedes afirmar con este aviso?",
      "options": [
        "Quedó registrada una copia masiva hacia BRUMA-VIEJA, sin identidad de persona.",
        "El aviso demuestra quién copió la carpeta.",
        "Como el incidente ya se detuvo, no hace falta documentarlo."
      ],
      "answer": 0,
      "reasonQuestion": "¿Qué harías con el hueco del nombre?",
      "reasons": [
        "Completarlo con el primer trabajador que estuvo en el edificio.",
        "Dejarlo como dato ausente hasta que otro registro lo sostenga.",
        "Borrar la hora para que el informe quede más simple."
      ],
      "reasonAnswer": 1,
      "feedback": "La hora y el puesto son hechos del registro. La identidad, si no aparece, sigue siendo una hipótesis.",
      "concept": "timeline"
    },
    {
      "id": "puesto",
      "label": "Ficha de BRUMA-VIEJA",
      "kind": "ACTIVO",
      "icon": "map",
      "source": "Inventario / puestos de trabajo",
      "body": "BRUMA-VIEJA figura como retirado hace seis meses. Sigue en la lista de equipos conectados. La ficha no tiene responsable desde la baja en papel.",
      "question": "¿Qué problema describe la ficha?",
      "options": [
        "Un puesto dado de baja en el inventario sigue conectado y sin dueño.",
        "Cualquier puesto antiguo demuestra una intrusión.",
        "Tener un nombre de equipo confirma quién lo usó."
      ],
      "answer": 0,
      "reasonQuestion": "¿Qué control corresponde?",
      "reasons": [
        "Asignar responsable, confirmar si aún hace falta y retirarlo de forma controlada si no se usa.",
        "Cambiar el nombre del puesto y dar por cerrada la baja.",
        "Dejarlo conectado porque el incidente ya pasó."
      ],
      "reasonAnswer": 0,
      "feedback": "La baja en papel y la baja real son controles distintos. El informe pide comprobar las dos.",
      "concept": "timeline"
    },
    {
      "id": "alerta",
      "label": "Nota de avisos",
      "kind": "PISTA",
      "icon": "note",
      "source": "Pizarra interna / qué se mira",
      "body": "La nota del taller dice que nadie recibió un aviso aquella mañana. La copia masiva no estaba entre los eventos que el estudio revisa. El registro existía; no había una persona asignada a mirarlo.",
      "question": "¿Qué faltó, según la nota?",
      "options": [
        "Un evento útil definido y alguien responsable de enterarse.",
        "Un registro más largo con el mismo silencio.",
        "Ocultar el puesto para que no vuelva a salir en la lista."
      ],
      "answer": 0,
      "reasonQuestion": "¿Qué defensa propones?",
      "reasons": [
        "Elegir los eventos que importan, asignar quién los revisa y comprobar que el aviso llega.",
        "Guardar todos los eventos y no designar a nadie.",
        "Apagar los registros para reducir ruido."
      ],
      "reasonAnswer": 0,
      "feedback": "Un registro que no avisa deja el fallo para cuando alguien lo encuentra por otro lado.",
      "concept": "records"
    },
    {
      "id": "rumor",
      "label": "Nota del becario",
      "kind": "PISTA",
      "icon": "note",
      "source": "Taza / papel suelto",
      "body": "Un papel sin fecha dice: «Seguro que fue el becario». No hay registro de acceso a su nombre ni horario que lo sitúe a las 08:40.",
      "question": "¿Cómo entra ese papel en el informe?",
      "options": [
        "Como rumor, separado de los hechos del registro.",
        "Como identidad confirmada del incidente.",
        "Como prueba de que los registros sobran."
      ],
      "answer": 0,
      "reasonQuestion": "¿Qué necesita el rumor para cambiar de categoría?",
      "reasons": [
        "Otro registro que lo sostenga, revisado con el responsable.",
        "Que varias personas repitan la misma frase.",
        "Que el informe quede más claro con un nombre."
      ],
      "reasonAnswer": 0,
      "feedback": "La línea de tiempo no se completa con sospechas. Se completa con registros.",
      "concept": "timeline"
    }
  ],
  "report": {
    "prompt": "Selecciona el cierre que mejor respalda la evidencia.",
    "options": [
      "El becario copió los bocetos; hay que despedir y borrar los registros.",
      "El lunes 08:40 quedó una copia masiva hacia un puesto dado de baja y aún conectado, sin identidad ni aviso. Asignar dueño, retirar lo que no se use y definir qué eventos deben avisar.",
      "No hay incidente porque el taller ya dejó de usar la carpeta."
    ],
    "answer": 1
  },
  "quiz": [
    {
      "question": "¿Qué convierte una hora en un hecho del informe?",
      "options": [
        "Que conste en un registro, no en un rumor.",
        "Que la historia resulte verosímil.",
        "Que alguien en la sala esté de acuerdo."
      ],
      "answer": 0,
      "explanation": "La línea de tiempo se apoya en lo registrado. Lo demás se etiqueta como hipótesis."
    },
    {
      "question": "Un puesto figura como retirado y sigue conectado. ¿Qué toca hacer?",
      "options": [
        "Confirmar la necesidad, asignar responsable y retirarlo con control si ya no se usa.",
        "Suponer que quien lo encienda es el responsable del incidente.",
        "Ignorarlo porque el papel de baja ya existe."
      ],
      "answer": 0,
      "explanation": "El ciclo de vida del activo incluye comprobar que la baja ocurrió de verdad."
    },
    {
      "question": "¿Para qué sirve un registro que debe avisar?",
      "options": [
        "Para que una persona designada se entere de un evento elegido a tiempo.",
        "Para guardar el mayor volumen posible sin que nadie lo lea.",
        "Para sustituir el inventario de equipos."
      ],
      "answer": 0,
      "explanation": "El valor está en el evento útil y en quien tiene la tarea de mirarlo."
    }
  ],
  "learned": "Ordenar lo ya ocurrido exige hora, sistema y huecos honestos. Un aviso sin destinatario llega tarde por diseño."
}
```

## 06 — La llave de todos

**Prosa.** Editorial Cauce cierra números con una cuenta compartida y una lista de accesos que nadie revisa al terminar una colaboración. Hugo pide mínimo privilegio y una baja de verdad. Nuria no acepta “una clave más larga para toda la redacción”: eso ya lo vio, con otro cliente, en Orbital, y aquí el fallo es otro.

**Hugo:** «Tres personas, una cuenta, un cuaderno. El problema es que el informe no puede decir quién hizo qué.»

**Nuria:** «Y el catálogo de solo lectura que sí está bien no se sacrifica para que el cierre suene más grave.»

```json
{
  "id": "permisos",
  "number": "06",
  "type": "ACCESO",
  "title": "La llave de todos",
  "subtitle": "Privilegios y bajas",
  "client": "Editorial Cauce",
  "duration": "4 min",
  "accent": "amber",
  "concepts": ["privilege", "offboarding"],
  "brief": "Cauce quiere ordenar quién entra a la redacción. Revisa cuentas compartidas, una baja sin cerrar y una aprobación que se concede a sí misma. Elige controles que dejen rastro de persona.",
  "mentor": "El acceso justo es el que la tarea de hoy necesita. La salida de alguien también es una tarea.",
  "scope": "Listados ficticios de la editorial. No se muestran ni se piden secretos. No se prueban accesos.",
  "objective": "Separar identidades, cerrar la baja pendiente y partir la aprobación del pedido.",
  "commandHint": "scan · inspect compartida · notes · report",
  "evidence": [
    {
      "id": "compartida",
      "label": "La cuenta de redacción",
      "kind": "IDENTIDAD",
      "icon": "key",
      "source": "Listado de cuentas / redacción",
      "body": "La cuenta «redaccion» la usan tres personas. El secreto está anotado en un cuaderno del estudio, no en un gestor. El registro de cambios solo muestra esa cuenta, nunca a quién de las tres.",
      "question": "¿Qué falla en este arreglo?",
      "options": [
        "Una identidad compartida impide saber qué persona actuó.",
        "Tres personas en un equipo siempre son un riesgo.",
        "Anotar un secreto en papel lo hace único por cada servicio."
      ],
      "answer": 0,
      "reasonQuestion": "¿Qué cambio eliges?",
      "reasons": [
        "Cuentas nominativas y un gestor. Retirar el cuaderno compartido.",
        "Una clave nueva, también compartida, escrita en el mismo cuaderno.",
        "Dar la cuenta de redacción a toda la editorial para que nadie quede fuera."
      ],
      "reasonAnswer": 0,
      "feedback": "Sin identidad individual, el mínimo privilegio y la baja no tienen a quién aplicarse.",
      "concept": "privilege"
    },
    {
      "id": "baja",
      "label": "Colaboración de verano",
      "kind": "IDENTIDAD",
      "icon": "shield",
      "source": "Listado de altas y bajas / verano",
      "body": "La colaboración de verano terminó hace dos meses según el acuerdo ficticio. El acceso a maquetas sigue activo. Nadie firmó la comprobación de cierre.",
      "question": "¿Qué registra el informe?",
      "options": [
        "Una baja acordada que no se comprobó en el acceso.",
        "Que toda colaboración temporal es un incidente.",
        "Que el acuerdo en papel basta aunque el acceso siga abierto."
      ],
      "answer": 0,
      "reasonQuestion": "¿Qué control cierra el hueco?",
      "reasons": [
        "Fecha de fin, responsable y una comprobación de que el acceso quedó cerrado.",
        "Esperar a la siguiente auditoría anual sin mirar esta ficha.",
        "Compartir la cuenta de redacción con quien se fue, para no perder archivos."
      ],
      "reasonAnswer": 0,
      "feedback": "La salida forma parte del mismo ciclo que el alta. El papel y el acceso tienen que coincidir.",
      "concept": "offboarding"
    },
    {
      "id": "aprobacion",
      "label": "Quien pide y quien aprueba",
      "kind": "ROL",
      "icon": "shield",
      "source": "Tabla de roles / maquetas",
      "body": "El rol «coordinación de número» puede solicitar acceso a un dossier y aprobar esa misma solicitud. No hay una segunda persona en el recorrido.",
      "question": "¿Qué riesgo respalda la tabla?",
      "options": [
        "La misma función pide y concede el acceso.",
        "Aprobar accesos es inseguro en cualquier organización.",
        "Un rol con nombre largo equivale a mínimo privilegio."
      ],
      "answer": 0,
      "reasonQuestion": "¿Qué defensa propones?",
      "reasons": [
        "Separar quien solicita de quien aprueba, con el acceso justo para cada tarea.",
        "Dejar la doble función y añadir un mensaje de confirmación.",
        "Eliminar las aprobaciones y dejar los dossier abiertos."
      ],
      "reasonAnswer": 0,
      "feedback": "Partir el pedido de la aprobación evita que un solo rol se conceda lo que no le corresponde.",
      "concept": "privilege"
    },
    {
      "id": "lectura",
      "label": "Catálogo de solo lectura",
      "kind": "ROL",
      "icon": "note",
      "source": "Tabla de roles / catálogo público interno",
      "body": "El rol de consulta abre el catálogo de títulos en solo lectura. No edita, no aprueba y no ve dossieres. Tiene responsable y revisión trimestral en la ficha.",
      "question": "¿Cómo lo clasificas?",
      "options": [
        "Acceso acotado a la tarea. La ficha no muestra un exceso.",
        "Inseguro porque existe un rol de consulta.",
        "Hay que darle edición para que el equipo no pida excepciones."
      ],
      "answer": 0,
      "reasonQuestion": "¿Qué recomiendas?",
      "reasons": [
        "Mantenerlo y seguir revisándolo con su responsable.",
        "Fundirlo con la cuenta compartida de redacción.",
        "Quitarlo porque cualquier acceso es de más."
      ],
      "reasonAnswer": 0,
      "feedback": "Mínimo privilegio también es reconocer el acceso que sí corresponde.",
      "concept": "privilege"
    }
  ],
  "report": {
    "prompt": "Elige el plan de acceso que sostienen las fichas.",
    "options": [
      "Una clave compartida nueva para toda la editorial y la baja del verano en el próximo año.",
      "Cuentas nominativas, comprobar la baja de verano, separar solicitud y aprobación, y mantener el catálogo de solo lectura.",
      "Cerrar todos los roles, incluido el de consulta, porque hubo una cuenta compartida."
    ],
    "answer": 1
  },
  "quiz": [
    {
      "question": "¿Por qué estorba una cuenta usada por tres personas?",
      "options": [
        "Porque el rastro no distingue qué persona actuó.",
        "Porque tres es un número impar.",
        "Porque un gestor no puede guardar cuentas de equipo."
      ],
      "answer": 0,
      "explanation": "Sin identidad individual no hay privilegio justo ni baja atribuible."
    },
    {
      "question": "Una colaboración terminó y el acceso sigue activo. Eso es…",
      "options": [
        "Una baja sin comprobar.",
        "Una prueba de que el acuerdo era falso.",
        "Un detalle que el inventario puede ignorar."
      ],
      "answer": 0,
      "explanation": "El fin del acuerdo tiene que verse también en el cierre del acceso."
    },
    {
      "question": "¿Qué significa separar solicitud y aprobación?",
      "options": [
        "Que no sea la misma función quien pide el acceso y quien lo concede.",
        "Que cada persona apruebe sus propias solicitudes dos veces.",
        "Que no exista forma de pedir un acceso nuevo."
      ],
      "answer": 0,
      "explanation": "El control está en el segundo criterio, no en repetir el mismo rol."
    }
  ],
  "learned": "El privilegio justo nombra a una persona, cabe en su tarea y se cierra cuando la tarea termina."
}
```

## 07 — Dos redes, una cafetería

**Prosa.** Puerto Calma alquila mesas a visitas y guarda el archivo de los socios detrás del mismo mostrador. Un martes, en la ficción, alguien de la zona de visitas abrió la página de ajustes de la impresora del archivo. El caso no explica cómo se llegó: describe que la frontera no existía. Hugo separa cortesía y autorización. Nuria conserva el calendario público, que sí debe verse desde fuera.

**Hugo:** «Compartir el local no es compartir el archivo. El martes ya ocurrió. Tu trabajo es la frontera que faltaba.»

**Nuria:** «No me cierres el calendario para castigar a la impresora.»

```json
{
  "id": "fronteras",
  "number": "07",
  "type": "REDES",
  "title": "Dos redes, una cafetería",
  "subtitle": "Segmentación y confianza",
  "client": "Puerto Calma",
  "duration": "4 min",
  "accent": "violet",
  "concepts": ["segment", "boundary"],
  "brief": "Puerto Calma mezcla visitas y archivo en un solo plano. Una ficha cuenta que la página de ajustes de la impresora interna ya fue vista desde la zona de visitas. Distingue lo que debe seguir siendo público y dibuja la frontera que faltó.",
  "mentor": "La cortesía del espacio no es una autorización. Cada zona tiene su propio alcance.",
  "scope": "Planos y notas locales del espacio de trabajo. Las direcciones .invalid son decorativas. No se contactan equipos ni se describe cómo cruzar una zona.",
  "objective": "Proponer zonas separadas para visitas y archivo, y conservar lo público que el plano marca como necesario.",
  "commandHint": "scan · inspect plano · notes · report",
  "evidence": [
    {
      "id": "plano",
      "label": "Un solo plano",
      "kind": "MAPA",
      "icon": "map",
      "source": "Recepción / plano de red ficticio",
      "body": "El plano une en la misma zona la red de visitas, los puestos de socios, la impresora del archivo y el archivo de trabajos. No hay una línea que separe la cafetería de los dossieres.",
      "question": "¿Qué observas en el plano?",
      "options": [
        "Visitas y archivo comparten zona. No hay frontera dibujada.",
        "El plano demuestra que alguien ya copió los dossieres.",
        "Una cafetería con red propia es siempre un incidente."
      ],
      "answer": 0,
      "reasonQuestion": "¿Qué defensa cabe en este dibujo?",
      "reasons": [
        "Separar la zona de visitas de la impresora interna y del archivo.",
        "Quitar el plano para que el problema deje de verse.",
        "Poner una contraseña común escrita en la pizarra de la cafetería."
      ],
      "reasonAnswer": 0,
      "feedback": "Segmentar es dibujar zonas y dejar en cada una solo lo que esa zona debe alcanzar.",
      "concept": "segment"
    },
    {
      "id": "martes",
      "label": "Nota del martes",
      "kind": "INCIDENTE YA OCURRIDO",
      "icon": "note",
      "source": "Libro de turno / martes",
      "body": "El martes, una persona de la zona de visitas comentó en recepción que había visto la página de ajustes de la impresora del archivo. El turno anotó el hecho y apagó la impresora. La nota no explica pasos ni herramientas: solo el síntoma y que esa página no era para visitas.",
      "question": "¿Qué aporta la nota al informe?",
      "options": [
        "Un síntoma ya ocurrido: desde visitas se alcanzó un ajuste interno.",
        "Una guía para volver a abrir esa página.",
        "La prueba de que la persona de la visita es responsable de un robo."
      ],
      "answer": 0,
      "reasonQuestion": "¿Qué control faltó?",
      "reasons": [
        "Una frontera que dejara la impresora del archivo fuera de la zona de visitas.",
        "Ocultar el nombre de la impresora y mantenerla en la misma zona.",
        "Pedir a las visitas que no miren lo que el plano ya les pone delante."
      ],
      "reasonAnswer": 0,
      "feedback": "El fallo se cuenta como síntoma y como zona mal dibujada. No se convierte en instrucciones.",
      "concept": "boundary"
    },
    {
      "id": "archivo",
      "label": "Archivo de socios",
      "kind": "ACTIVO",
      "icon": "globe",
      "source": "https://archivo.puertocalma.invalid · Responsable: Socios",
      "body": "El archivo guarda trabajos de socios. Usa un canal protegido y tiene responsable. El plano lo sitúa en la misma zona que las visitas. La ficha dice que las visitas no deberían abrirlo.",
      "question": "¿Qué tensión hay entre la ficha y el plano?",
      "options": [
        "El destino del archivo es interno, pero la zona dibujada lo trata como si estuviera al alcance de las visitas.",
        "HTTPS basta para compartir el archivo con quien pase por la cafetería.",
        "Tener responsable elimina la necesidad de una frontera."
      ],
      "answer": 0,
      "reasonQuestion": "¿Qué recomiendas?",
      "reasons": [
        "Sacar el archivo de la zona de visitas y revisar quién debe cruzar, con autorización explícita.",
        "Publicar el archivo porque el canal ya está protegido.",
        "Mover solo el letrero de la puerta y dejar el plano igual."
      ],
      "reasonAnswer": 0,
      "feedback": "El canal protegido cuida el transporte. La zona decide quién puede llegar al servicio.",
      "concept": "segment"
    },
    {
      "id": "calendario",
      "label": "Calendario de salas",
      "kind": "ACTIVO",
      "icon": "globe",
      "source": "https://salas.puertocalma.invalid · Responsable: Recepción",
      "body": "El calendario de salas libres es público a propósito. Solo muestra horarios de mesas, sin nombres de socios ni trabajos. Tiene responsable y una revisión marcada.",
      "question": "¿Cómo lo clasificas?",
      "options": [
        "Exposición intencional. Puede seguir en la zona que las visitas necesitan.",
        "Hay que apagarlo porque el martes hubo un incidente.",
        "Público significa que el resto de sistemas también puede serlo."
      ],
      "answer": 0,
      "reasonQuestion": "¿Qué relación tiene con la frontera?",
      "reasons": [
        "Es una excepción documentada: cruza hacia visitas porque ese es su uso.",
        "Obliga a dejar también la impresora y el archivo en la misma zona.",
        "Demuestra que las fronteras son innecesarias."
      ],
      "reasonAnswer": 0,
      "feedback": "Una excepción escrita no abre el resto de la casa. Se queda en el inventario y se revisa.",
      "concept": "boundary"
    }
  ],
  "report": {
    "prompt": "¿Qué plan de zonas respalda el plano y la nota del martes?",
    "options": [
      "Cerrar calendario, archivo e impresora porque una visita vio una página.",
      "Separar visitas de la impresora interna y del archivo, y mantener el calendario público de salas.",
      "Dejar el plano como está y pedir a recepción que vigile las pantallas."
    ],
    "answer": 1
  },
  "quiz": [
    {
      "question": "¿Qué es una zona en este caso?",
      "options": [
        "Un conjunto de sistemas que comparten quién puede alcanzarlos.",
        "El color del cable en el plano.",
        "El horario de la cafetería."
      ],
      "answer": 0,
      "explanation": "Segmentar agrupa por alcance, no por el mueble más cercano."
    },
    {
      "question": "La nota del martes sirve para…",
      "options": [
        "Mostrar un síntoma ya ocurrido y el control de frontera que faltaba.",
        "Enseñar cómo abrir la página de ajustes.",
        "Identificar a la visita como autora de un robo."
      ],
      "answer": 0,
      "explanation": "Red, en esta ficción, lee el fallo sucedido. No entrega el camino para repetirlo."
    },
    {
      "question": "Un calendario público y necesario, ¿qué pide?",
      "options": [
        "Dejarlo como excepción documentada y no usarlo de excusa para abrir el archivo.",
        "Cerrarlo junto con todo lo demás.",
        "Copiar su exposición al archivo de socios."
      ],
      "answer": 0,
      "explanation": "La frontera distingue lo que debe cruzar de lo que debe quedarse dentro."
    }
  ],
  "learned": "Compartir mesa no comparte autorización. Las zonas se dibujan, las excepciones se escriben y el fallo ya ocurrido solo cuenta el síntoma."
}
```
