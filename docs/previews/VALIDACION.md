# Validación del entorno y el inicio

Fecha: 27 de septiembre de 2026.

Se compiló y ejecutó una copia temporal del proyecto con Unity **6000.6.3f1**, el editor instalado en este equipo. El proyecto original conserva su versión declarada **6000.0.62f1**; no se verificó con ese editor exacto. La copia de prueba actualizó automáticamente sus paquetes a uGUI 2.6.0, Input System 1.20.0 y Test Framework 1.8.0; los pins del proyecto original se conservaron.

- 5 pruebas NUnit de las capas de lógica: aprobadas.
- 17 pruebas PlayMode: aprobadas. Las 3 de `HubStartupTests` cubren Boot → Hub, instancia única, fuentes disponibles, bloqueo de controles en bienvenida/introducción, entrada al juego, recarga de oficina, raycast a laptop y colisión/paso por puerta.
- Las 7 de `HubGameplayTests` cubren las cuatro misiones completas con 100/100, desbloqueo y revisión de los ocho conceptos, recuperación del progreso desde JSON, rechazo de bandejas incorrectas, bloqueo de tickets e informes prematuros, USB del cajón, pausa del cronómetro, envío con Enter en la terminal narrativa y reinicio con confirmación.
- Las 7 de `FirstPersonInputTests` envían eventos de teclado y mouse virtuales a Input System, a través de PcButtons y Update. Verifican bienvenida bloqueada, WASD/flechas, ambos ejes de cámara, sprint, diagonal normalizada, frenado, paredes, uso de E en la puerta y cruce al archivo, ESC, pérdida de foco, preferencias y recuperación tras caída.
- 73 archivos C#: comprobación de sintaxis C# 9 aprobada.
- Contenido: 4 misiones, 15 pistas, 8 conceptos, 12 preguntas de quiz.
- Validación de prefab, referencias a scripts, fuentes y conexión de Hub/Hub_Art: aprobada.
- Geometría de la oficina: **21.652 triángulos**, **459 renderers**. Esto describe el modelo; no constituye una medición de rendimiento en hardware objetivo.

Resultados de la última ejecución: [first-person-results.xml](first-person-results.xml), 17 aprobadas y 0 fallidas. El informe anterior de 10 pruebas se conserva en [gameplay-results.xml](gameplay-results.xml). Las pruebas utilizan carpetas de guardado temporales independientes y restauran preferencias y dispositivos; no reinician la partida del jugador.

Durante el recorrido se corrigieron el envío de la terminal con Enter (antes comprobaba la tecla de interacción E), un símbolo de completado ausente en la fuente, los nombres internos de pasos que aparecían en el expediente y el anclaje del texto de estado.

`office-preview.png` y `archive-preview.png` se renderizaron desde las cámaras de la escena modelada. `welcome-preview.png` se capturó durante la prueba de arranque, con la interfaz real. También se revisaron el [expediente a 1920×1080](case-playtest-1080.png) y la [terminal a 1280×720](terminal-playtest-720.png). Son capturas del renderizador de Unity, no ilustraciones.

Las cuatro misiones de `HubGameplayTests` despachan acciones y callbacks de la interfaz; la prueba de Enter invoca el evento de envío del campo. La nueva batería `FirstPersonInputTests` sí recorre la entrada de teclado/mouse y el controlador real, con el entorno de dispositivos aislados de Unity. En batchmode no existe una ventana donde capturar físicamente el cursor: esa condición se omite solo en batchmode. La pérdida de foco se simula mediante el callback de Unity. La comodidad, el mouse físico y Alt-tab requieren todavía una pasada humana; no se ha medido FPS ni probado el editor original 6000.0.62f1.

## Ejecutable de primera persona

Build Linux x86_64 con Unity 6000.6.3f1: **Success**, escenas Boot y Hub, carpeta completa de aproximadamente 93 MB en `Builds/Linux/`. Se abrió el ejecutable gráfico en este equipo: motor, backend X11, renderizador y carga de escena se inicializaron, sin excepciones registradas durante la comprobación del arranque. Aparecen avisos del controlador de pantalla sobre DPI y ajuste de modo de renderizado; no se probó en una máquina limpia.

Ejecutar `bash scripts/play-linux.sh`. El log de la aplicación queda en `Builds/Linux/player.log`. El build está ignorado por Git; se reproduce con **EthicalLab → Build → Linux first person**. El módulo Windows no está instalado y no se generó ese ejecutable.

Capturas del recorrido: [dentro del archivo en primera persona](first-person-archive.png) y [ajustes de cámara](first-person-settings.png).

Referencias de implementación: [Unity: captura y liberación del cursor](https://docs.unity.cn/6000.1/Documentation/ScriptReference/Cursor-lockState.html) y [Unity: eventos de dispositivos virtuales](https://docs.unity.cn/Packages/com.unity.inputsystem%401.13/manual/Events.html).
