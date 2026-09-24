# Blue / Red · Analyst Academy — Unity

Proyecto de un laboratorio narrativo en primera persona, con cuatro misiones conceptuales, quince evidencias y ocho conceptos. Versión de proyecto: **Unity 6, 6000.0.62f1**, Built-in Render Pipeline, uGUI e Input System. Todo el arte de la oficina se construye con geometría y materiales locales.

## Abrir y jugar

1. Instala Unity Hub y el editor **6000.0.62f1**, junto con Linux Build Support o Windows Build Support según el ejecutable que necesites.
2. En Hub, **Add → Add project from disk**, selecciona esta carpeta completa: `mvp_hacking_conceptual`.
3. Deja que Unity resuelva los paquetes de `Packages/manifest.json`. La primera importación requiere conexión. Si Input System ofrece habilitar su backend y reiniciar, acepta; el código también admite el input clásico.
4. Abre **Assets/Scenes/Academy.unity** o el menú **Academy → Abrir escena principal**.
5. Pulsa **Play**, haz clic en la vista Game y elige **Entrar a la oficina**.

La escena guardada tiene un bootstrap. La oficina, el jugador y el Canvas se crean al entrar en Play; por eso no verás el mobiliario en la escena antes de ejecutarla. No hay referencias que conectar a mano.

La importación inicial genera los ScriptableObjects y un material en `Assets/Resources/`, configura nombre/resolución del juego y registra la escena de build. Conserva los assets existentes si vuelves a importar.

## Primera persona

| Control | Acción |
| --- | --- |
| WASD / flechas | Caminar por oficina y archivo |
| Mouse | Mirar; se captura automáticamente al entrar al entorno |
| Shift izquierdo | Correr |
| Ctrl izquierdo | Agacharse; comprueba espacio antes de levantarse |
| Espacio | Saltar |
| E / clic izquierdo | Usar el objeto enfocado a un máximo de 2,7 m |
| G | Tomar objeto / devolverlo a su lugar |
| R + mouse | Girar horizontalmente el objeto en mano |
| F | Inspeccionar objeto o leer la evidencia asociada |
| Tab | Liberar/capturar cursor y mostrar/ocultar pestañas |
| Esc | Pausa; desde una interfaz también abre pausa |

El jugador usa `CharacterController`: colisiones con muebles, paredes, puerta y gravedad. Puede rodear el escritorio y cruzar al archivo. Los objetos tomados desactivan temporalmente sus colliders y se devuelven a su posición local de origen. En Pausa puedes silenciar sonidos y desactivar el balanceo al caminar.

## Interacciones del mapa

- **Laptop:** terminal ficticia; se puede tomar completa, con pantalla y teclado.
- **Buzón y pizarra:** misiones y seguimiento. Cada ticket físico abre su caso, respetando el desbloqueo secuencial.
- **Cuaderno:** notas de campo e historial de hallazgos.
- **Puerta del archivo:** abre y cierra con animación y colisión.
- **Interruptor:** enciende y apaga la luz de la oficina.
- **Carpetas del archivo:** evidencias 1–3 del caso activo; permiten inspección y manipulación.
- **Cajón:** se desliza para mostrar el USB de utilería; el USB enlaza la cuarta evidencia (o la última del tutorial).
- **Taza y credencial:** objetos inspeccionables y transportables con contexto narrativo.

La interfaz de expediente también permite leer las evidencias directamente, para que el aprendizaje no dependa de localizar objetos pequeños.

## Bucle del MVP

Acepta el ticket 01 → lee el alcance → inspecciona pistas → selecciona observación y defensa → valida el hallazgo → revisa las fichas desbloqueadas → entrega el informe → responde las tres preguntas → cierra el caso. Continúa con los casos 02, 03 y 04.

Los errores ofrecen feedback y permiten reintentos. La puntuación premia primeras decisiones correctas: evidencias 45, informe 15 (8 si se corrigió), quiz 25, glosario 10, tiempo 5 (3 al superar diez minutos por caso). El tiempo no bloquea el avance; se detiene en pausa o al perder foco. El score se fija al cerrar la misión.

Terminal: `help`, `scan`, `map`, `inspect <id>`, `simulate`, `notes [texto]`, `report`, `clear`. Son comandos locales del juego; no invocan shell, redes o herramientas externas.

## Progreso e informes

Guardado JSON en `Application.persistentDataPath`, después de cada decisión, cada veinte segundos y al salir/perder foco. En Linux, con la configuración inicial, normalmente corresponde a:

`~/.config/unity3d/BlueRedAcademy/Blue Red Analyst Academy/`

Los cierres permiten exportar informes `.txt` dentro de `Reports/`. La nueva partida requiere un segundo clic y conserva una copia del guardado. El feedback al finalizar se escribe localmente; no se envía a un servicio externo. No se ha conectado una página Steam ni un botón real de wishlist.

## Contenido ampliable

`Assets/Resources/Content/missions.json` contiene la versión inicial. En la primera apertura, el menú de importación genera:

- `Assets/Content/Missions/*.asset`: `MissionDefinition`.
- `Assets/Content/Concepts/*.asset`: `ConceptDefinition`.
- `Assets/Resources/MissionCatalog.asset`: orden de misiones y catálogo de conceptos.

Después de esa importación, **los ScriptableObjects del catálogo son la fuente usada por el juego**. Edita esos assets desde Inspector. Cambiar el JSON no sobrescribe los assets existentes: la reimportación agrega lo que falte y conserva el trabajo del diseñador. Si el catálogo no existe, el runtime puede leer el JSON directamente.

Para agregar una misión: crea un `MissionDefinition` en **Create → Analyst Academy → Misión**, completa su `data`, agrega conceptos y vincula los assets en el catálogo. Usa IDs únicos, opciones con índices válidos y al menos una evidencia y un quiz. `answer` y `reasonAnswer` son índices desde cero. El orden del catálogo define la progresión.

## Compilar

Menú **Academy → Build → Linux x86_64 / Windows x86_64**. Requiere el módulo de plataforma correspondiente instalado en Hub. Los resultados aparecen en `Builds/`.

Ejemplo batch, reemplazando `/ruta/al/Unity` por el ejecutable real:

```bash
/ruta/al/Unity -batchmode -quit -projectPath "$PWD" \
  -executeMethod Academy.Editor.AcademyProjectTools.BuildLinux \
  -logFile build-linux.log
```

## Pruebas y estado de verificación

En Unity: **Window → General → Test Runner**, ejecuta EditMode y PlayMode. Se incluyen pruebas de bloqueo secuencial, corrección de errores, puntuación, persistencia serializada, terminal local, creación de oficina y contrato de interacción.

También hay controles estáticos específicos de Unity en `UnityTools/validate_project.py`:

```bash
python3 UnityTools/validate_project.py
# Opcional, para comprobar sintaxis C# además del contenido:
python3 -m pip install tree-sitter tree-sitter-c-sharp
python3 UnityTools/validate_project.py --csharp
```

**En el entorno donde se escribió este proyecto no hay Unity instalado.** La validación estática no sustituye la compilación, los tests de Unity ni una prueba visual de la cámara y las colisiones. No se entregó un ejecutable compilado ni se afirma haber completado un playtest.

## XR después

`IAcademyInteractable` y `AcademyInteractable.Activate/Grab/Release` son el contrato compartido. `XRInteractionBridge` expone HoverEnter/HoverExit/Select/Deselect/Activate para conectar eventos de XR Interaction Toolkit. Escritorio y stub XR llegan a las mismas acciones del dominio.

El bridge es un **stub**, no una implementación VR probada. Para Quest faltan XR Origin, OpenXR, entrada de manos/controladores, UI world-space, configuración Android, rendimiento y pruebas con visor. La arquitectura permite conservar contenido y reglas.

## Alcance pendiente de publicación

El proyecto incluye la demo en código, documentación y tests. Quedan pendientes: verificarlo en Unity, ajustar presentación a partir del playtest, grabar capturas/trailer reales y publicar/configurar Steam o itch. El tiempo 10–15 minutos es una meta de diseño, no una medición con usuarios.

Referencia del editor: https://unity.com/releases/editor/whats-new/6000.0.62f1
