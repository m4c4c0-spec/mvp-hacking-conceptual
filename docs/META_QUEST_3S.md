# Adaptación a Meta Quest 3S

Este documento describe la referencia Unity. Para la versión migrada consulta [la guía Godot](../Godot/docs/QUEST_3S.md).

## Estado entregado

La versión jugable principal sigue siendo PC/Mac en primera persona. El escape room tiene reglas puras (`EscapeRoomRules`), controles físicos con IDs `escape:<fase>` y progreso persistente; no depende de un visor. `XrInteractor` permite emitir Use/Grab/Release/Hover con los mismos IDs, pero sigue siendo un puente de eventos: no proporciona tracking, locomoción ni rig. No se entrega aún una experiencia VR validada ni un APK.

El menú **EthicalLab → Quest → Install XR foundation** instala mediante Package Manager los paquetes `com.unity.xr.management`, `com.unity.xr.openxr` y `com.unity.xr.interaction.toolkit`, resolviendo versiones compatibles con el editor. Es una preparación opcional; instalar paquetes no activa automáticamente VR.

## Ruta de implementación

1. Instalar Android Build Support con SDK, NDK y OpenJDK desde Unity Hub. Crear un Build Profile Android separado del escritorio.
2. Instalar la base XR con el menú anterior. En Project Settings → XR Plug-in Management → Android, activar OpenXR; habilitar soporte Meta Quest e interacción de controladores y resolver OpenXR Project Validation. Ver [guía oficial Meta](https://developers.meta.com/horizon/documentation/unity/unity-tutorial-hello-vr/) y [ajustes OpenXR Quest](https://developers.meta.com/horizon/documentation/unity/unity-openxr-settings-quest/).
3. Crear un prefab XR Origin con cámara seguida por el visor y controladores izquierdo/derecho. Desactivar cámara, manos decorativas y entrada de `PcInteractor` cuando XR esté realmente activo. Mantener un modo Desktop independiente para probar exactamente el mismo contenido.
4. Adaptar `LabBootstrap`, `HubHud` y `WorldBinder`, que hoy usan `PcInteractor` directamente, a un contexto de jugador compartido: eventos `IInteractor`, objeto sostenido, menú y posición de retorno. Conectar select/grip a Grab/Release, trigger a Use y hover al resaltado de `InteractableView`. Mantener un único objeto sostenido y limpiar suscripciones al salir de escena.
5. Convertir expedientes, lectura y menú a Canvas World Space con Tracked Device Graphic Raycaster e input XR; colocarlos delante del usuario y probar legibilidad. No dejar un Canvas Screen Space Overlay como única forma de empezar o finalizar una misión.
6. Añadir teletransporte a zonas despejadas y giro por pasos como modo inicial. Movimiento continuo opcional; evitar correr, balanceo y giro de cámara por mouse en VR. La altura se obtiene del tracking, con recenter y comprobación de colisiones.
7. Mantener la oficina como entorno virtual inmersivo. Carpetas y controles a altura alcanzable, interacción directa cercana y rayos a distancia. El exterior de ventanas es decoración, no zona caminable. Passthrough/mixed reality y manos sin controladores son fases opcionales posteriores.
8. Configurar Android con IL2CPP y ARM64, identificador propio y salida `Builds/Quest/ProtocoloSalida.apk`. Elegir Vulkan o OpenGLES3 según validación del render actual y las extensiones; comprobar ambos ojos y shaders WorldText/superficies. La escena debe contener o crear el rig XR antes de ofrecer un APK.
9. Activar developer mode, conectar por USB y autorizar depuración; instalar el APK mediante Meta Quest Developer Hub. Mac puede compilar y desplegar Android por USB. No confundir el build macOS con el APK autónomo del visor.

## Verificación del entorno virtual

Medir en Quest 3S real: tracking, escala en metros, colisiones, agarre/soltado, todas las bandejas, controles del panel, guardado/reanudación, interfaz y final. Perfil inicial de 72 Hz como objetivo a medir, no rendimiento garantizado. El Quest 3S tiene 8 GB compartidos. En Android, `OfficeRenderQuality` aplica solo ese perfil: una sombra dura, MSAA 2× y el mismo albedo de la oficina, sin cambiar el arte de escritorio. Para verlo en el editor: **EthicalLab → Office → Preview 8 GB render**.

El simulador ayuda a revisar input y flujo, pero no acredita rendimiento ni comodidad del dispositivo. [Meta documenta que XR Simulator no emula el hardware ni Android](https://developers.meta.com/horizon/documentation/unity/xrsim-intro/).
