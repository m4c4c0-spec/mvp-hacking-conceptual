# PC y Meta Quest 3S · Godot

## Implementado como base experimental

`main.gd` elige `DesktopPlayer` por defecto. Con el argumento de usuario `--xr`, o un export con custom feature `quest`, intenta usar el runtime OpenXR activo. Si no hay runtime, vuelve a PC y comunica el motivo.

`QuestPlayer` crea `XROrigin3D`, `XRCamera3D` y los controladores. Trigger derecho usa; grip derecho toma la carpeta y soltar la devuelve a su sitio. El stick derecho gira por pasos de 30°, sin giro continuo ni carrera. El click del stick izquierdo teletransporta al suelo de la capa 2 y exige espacio libre. El botón de menú emite `pause_requested`. Un rayo fino, sin sombra, sigue el apuntado del mando derecho. La cámara sale de `get_camera()`. OpenXR no se inicializa en este script ni en el arranque de escritorio.

Los menús reutilizan el mismo árbol Control. En VR el SubViewport es 1280×800 y el quad mide 2,00 × 1,25 m para que el cuerpo a 24 px se lea a unos 1,5 m; el puntero usa esas medidas. Los botones quedan a 22 px y 64 px de alto. El hint del objetivo va sobre placa oscura con texto blanco. `toast` rellena también el subtítulo de megafonía (texto, sin audio). Trigger derecho pulsa el control bajo el rayo. Esta ruta sigue sin certificar en el visor. El teclado de la terminal en VR es un pendiente; la operación se recorre con objetos del edificio, sin escribir comandos.

Para probar con runtime OpenXR de escritorio:

```bash
GODOT_BIN="/ruta/Godot" bash scripts/play-godot.sh --xr-mode on -- --xr
```

El runtime y su conexión al visor dependen del sistema de desarrollo. Si usas Mac, prioriza el APK Android autónomo por USB y no supongas que la conexión PCVR de escritorio está disponible.

En Linux, sigue [la conexión del proyecto con WiVRn por USB](LINUX_QUEST_WIVRN.md): preparación del visor, autorización ADB, emparejamiento y arranque con `scripts/play-vr.sh`.

## Preparar Android/Quest

1. Instala export templates 4.6.3, OpenJDK 17 y Android SDK; configura rutas en el editor.
2. Instala Android Build Template y crea un preset Android llamado **Quest 3S**, con Use Gradle Build, XR Mode **OpenXR**, ARM64 y custom feature `quest`.
3. Activa OpenXR al arrancar la variante Quest (`xr/openxr/enabled`) y conserva PC con XR apagado por defecto. Comprueba los ajustes específicos del preset y los action bindings de Touch.
4. Añade **Godot OpenXR Vendors** para las opciones específicas Meta según la versión compatible con 4.6.3. No se ha descargado ni habilitado este plugin automáticamente.
5. Exporta a `Builds/Godot/Quest/ProtocoloSalida.apk`, activa developer mode y autoriza USB debugging en el visor. Instala la demo por USB mediante herramientas oficiales o ADB.

Godot documenta la ruta [Android/OpenXR](https://docs.godotengine.org/en/stable/tutorials/xr/deploying_to_android.html) y el [rig XR](https://docs.godotengine.org/en/stable/tutorials/xr/setting_up_xr.html). Las recomendaciones del renderer cambian entre versiones; parte del perfil Compatibility del proyecto y mide ambos ojos y rendimiento en el dispositivo antes de cambiar a Mobile/Vulkan.

## Esta pasada de confort (código, sin certificar en el visor)

Quedó escrito en `game_ui.gd` y `xr_player.gd`. No hubo prueba en un Quest 3S y el APK no está certificado.

- Tarjetas: título 32 px, cuerpo y texto añadido 24 px, botones 22 px con alto mínimo 64 px y fondo claro. El orden sigue siendo título, cuerpo y después botones.
- Viewport espacial 1280×800 y quad 2,00×1,25 m (640 px/m). A 1,5 m, 24 px ocupan unos 1,4° y el panel unos 67° de ancho. `pointer` y la caja de colisión usan ese tamaño. `main.gd` sigue colocando el panel a 1,7 m; a esa distancia el mismo glifo baja a unos 1,3° y el ancho a unos 61°.
- `toast` rellena `subtitle` y `toast_label`. El subtítulo visible es texto claro a 28 px sobre placa oscura, para la megafonía. No hay audio.
- El objetivo (`hint`), el recordatorio de controles y el foco van en texto blanco sobre placa oscura. La carpeta en mano usa la misma placa, con el texto recortado al rectángulo.
- Rayo de la mano derecha: prisma de 1,2 cm y punta, sin sombra, del mando hasta el impacto. El modal se pulsa con el trigger de esa mano.
- Se mantienen el giro por pasos de 30°, el teletransporte al suelo (capa 2), trigger para usar, grip para tomar y soltar, y el botón menú emitiendo `pause_requested`. No hay carrera ni giro continuo.

## Sigue sin certificar en el visor

- Nada de esta pasada se jugó ni se midió en un Quest 3S. El APK no está certificado.
- Runtime OpenXR, action map y agarre reales con Touch.
- Arco o retícula de teletransporte, oclusión del panel, recenter y alcance sentado o de pie.
- Escritura del terminal en VR.
- Memoria, GPU y tasa de cuadros en el dispositivo. El presupuesto de render no certifica 72 Hz.
- Recorrido de las tres salas, guardar, reabrir, reiniciar y salir dentro del visor.

La oficina es inmersiva. Passthrough, manos sin controladores y distribución administrada de dispositivos son ampliaciones posteriores.
