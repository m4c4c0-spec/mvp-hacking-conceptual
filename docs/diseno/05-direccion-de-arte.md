# 05 · Dirección de arte

Referencias largas: [Art bible](../ART_BIBLE.md) y [presupuesto 8 GB](../RENDER_8GB.md).

## Intención

Sede ficticia de la UCD en Santiago, sobria y contemporánea, durante la Operación Cóndor Silencioso. La oficina de analistas conserva madera y materiales existentes como Sala 1. Servidores y laboratorio son blockout del mismo edificio: metal grafito en racks, yeso en muros, un acento menta en lo que se puede usar y ámbar en lo que está pendiente. Sin neón, sin glitch, sin máscaras, sin texto ilegible en pantallas. Cada cable lleva icono y palabra, no solo color.

Los tres albedos aprobados son de 1024, sin mapa de normales: roble, tejido y yeso. Están en `Godot/Game/Shared/Textures/` y la oficina horneada los usa en madera, yeso y tejido.

## Estados visuales propuestos

- Oficina: mobiliario cotidiano, corcho, escritorio y puerta magnética. La amenaza se lee en el reloj y en los monitores, no en un HUD de examen.
- Servidores: racks de cajas, panel de cuatro cables con icono y palabra, tabique entre los dos detectores. La lámpara es un LED; el humo, si aparece, es luz y no tapa la lectura.
- Laboratorio: fichas, mesa forense, plano y carro de aseo con el doble fondo a la altura de la cintura. Al ganar, una luz del laboratorio pasa a verde. La salida de emergencia del corredor sigue visible y abierta.

Prioridad de modelado: los interactivos de las tres salas, entre 0,9 m y 1,4 m. Los nuevos recursos reutilizan materiales del proyecto y respetan el presupuesto vigente. No hay ductos ni vestíbulo de ascensores en este recorrido.

## Techo que el diseño no puede romper

- Menos de 150 000 triángulos en la escena visible. El blockout Godot está muy por debajo; el prefab Unity de referencia ronda 87 000.
- Texturas de 1K. Sin normales ni tangentes.
- Una sola luz con sombra: direccional, dura, mapa 1024. El resto no proyecta.
- La escena en juego tiene que quedar holgada dentro de unos 2 GB para que el proceso entre en 8 GB de RAM.

## Paleta de trabajo

La [art bible](../ART_BIBLE.md) fija navy, slate, graphite, walnut, paper, mint, amber, green, muted y el rojo del rack. Esa tabla manda hasta que [Decisiones](06-decisiones.md) cierre si el ambiente cálido sustituye al navy de suelo y techo.

## A definir

- Silueta reconocible desde el spawn: corcho, reloj y puerta magnética hacia servidores.
- Qué color lleva cada material del blockout cuando los cables, la lámpara y el interruptor no hereden veta de madera.
