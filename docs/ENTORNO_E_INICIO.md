# Oficina 3D e inicio del videojuego

La dirección elegida es una oficina profesional y sobria: madera, grafito, luz cálida y acentos mint. El modelo se genera con geometría de Unity y se guarda como prefab editable; no requiere Blender ni paquetes de arte externos.

## Jugar

1. Abrir `Assets/Scenes/Boot.unity` y pulsar Play, o **EthicalLab → Play from Boot**.
2. Esperar la carga asíncrona y pulsar **Entrar a la oficina**. Si existe progreso, aparece **Continuar**.
3. Leer la introducción la primera vez. La bienvenida y la introducción bloquean movimiento e interacción.
4. Usar WASD para caminar, mouse para mirar, Shift para correr, E/click para interactuar y G para tomar/devolver pistas.
5. ESC abre el menú y libera el cursor. Volver a Oficina recupera la mirada en primera persona.
6. Empezar por la pizarra de tickets, recorrer el archivo y llevar las carpetas a las bandejas.

La entrada no borra la partida. El reinicio sigue disponible con confirmación en ESC → Inicio. El tiempo del caso se detiene en la bienvenida y el menú.

## Primera persona y ejecutable

WASD y las flechas mueven al jugador respecto a su orientación; ambos Shift permiten correr. El mouse gira horizontalmente el cuerpo y verticalmente la cámara, con límite de 80° para no voltear la vista. La cámara está dentro del jugador, sin balanceo artificial ni salto. El CharacterController resuelve suelo, paredes y muebles; la velocidad diagonal está normalizada y el movimiento acelera/frena suavemente.

ESC → Inicio incluye sensibilidad (0,25–2,5), campo de visión (60–90°), inversión vertical y restauración de cámara. Se guardan en PlayerPrefs, independientes del progreso del caso. Perder foco o liberar el cursor pausa el juego; no recaptura el mouse hasta reanudar. «Volver a la entrada» recupera una posición segura conservando la misión y devolviendo la carpeta a su lugar.

**EthicalLab → Build → Linux first person** produce `Builds/Linux/AnalystAcademy.x86_64`. Ejecutar `bash scripts/play-linux.sh`; el log queda en `Builds/Linux/player.log`. Para compartirlo, copiar la carpeta Linux completa, no solo el binario. El menú equivalente de Windows requiere su módulo de Unity. Ambos incluyen únicamente Boot y Hub.

`ProjectSettings.asset` fija ambos sistemas de entrada: el controlador canónico usa Input System y se conserva compatibilidad con el prototipo antiguo. Si Unity solicita reiniciar el editor después de importar esta configuración, aceptar antes de probar.

## Editar el entorno

- `Assets/Art/Office/Prefabs/Office_AnalystAcademy.prefab`: modelo compartido por `Hub` y `Hub_Art`, con colisiones, jugador, iluminación y referencias de interacción.
- `Assets/Art/Office/Materials/Generated/`: materiales editables y persistentes.
- `Hub_Art.unity`: escena para inspección y trabajo artístico. Los cambios al prefab compartido también llegan a `Hub`, que es la escena cargada desde Boot.
- **EthicalLab → Office → Prepare modeled scene**: regenera el prefab desde código y conecta la oficina al inicio. Regenerar reemplaza las modificaciones artísticas hechas directamente al prefab/materiales generados; para una variante manual, duplica el prefab antes.
- **EthicalLab → Office → Capture modeled office preview**: exporta imágenes de la oficina abierta en `Hub` o `Hub_Art` a `docs/previews/` usando el renderizador de Unity.

`HubOffice.cs` define arquitectura, anclas, jugador y objetos interactivos. `HubOfficeDressing.cs` modela ventanas escénicas, carpintería, sillas, archivo, plantas, lámparas y detalles. Una unidad equivale a un metro; la cámara está a 1,62 m sobre el origen del jugador. El techo queda a 3,35 m.

La decoración pequeña no tiene colisiones ni intercepta el raycast de uso. La envolvente, muebles principales, puerta y cajón sí mantienen colisiones. Las luces son hijas de la oficina y se incluyen en el prefab.

## Inicialización

`BootLoader` muestra el estado de carga y abre Hub. `LabBootstrap` atiende cada carga de escena, recupera el progreso local y conecta misiones, jugador e interfaz una sola vez. `HubSceneLoader` utiliza las referencias del modelo; si la escena no contiene el prefab, construye la misma oficina procedural.

El importador antiguo de Academy conserva la lista de escenas configurada. TextMesh Pro viene en uGUI para Unity 6; `HubProjectSetup` importa sus recursos esenciales cuando faltan.

## Verificación

- `bash scripts/test-dotnet.sh`: capas de lógica, pruebas NUnit y sintaxis de C#.
- `python3 UnityTools/validate_project.py`: contenido, ensamblados y orden Boot → Hub.
- Unity Test Runner → PlayMode → `EthicalLab.Tests.HubStartupTests`: transición desde Boot, instancia única, bienvenida sin movimiento, recarga de escena, acceso a laptop y paso por la puerta.
- Unity Test Runner → PlayMode → `EthicalLab.Tests.HubGameplayTests`: recorrido de las cuatro misiones mediante acciones e interfaz, bandejas, cajón, progresión, persistencia aislada, pausa, terminal y reinicio con confirmación. No sustituye el playtest manual de teclado y mouse.
- Unity Test Runner → PlayMode → `EthicalLab.Tests.FirstPersonInputTests`: teclado/mouse virtuales recorren PcButtons y Update; verifica WASD/flechas, sprint, diagonal, orientación, paredes, puerta, pausa, recuperación y preferencias de cámara. Los dispositivos y preferencias anteriores se restauran al finalizar.

El proyecto conserva la versión declarada 6000.0.62f1. Las comprobaciones locales pueden ejecutarse en una copia temporal con el editor instalado; la versión exacta y resultados se registran en `docs/previews/VALIDACION.md` cuando estén disponibles.
