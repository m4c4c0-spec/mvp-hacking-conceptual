# Conectar este proyecto a Meta Quest 3S desde Linux

El juego corre en el PC Linux. WiVRn envía la imagen al visor y recibe el seguimiento de cabeza y mandos; Godot se comunica con WiVRn mediante OpenXR. Esta prueba usa el proyecto de escritorio y no requiere exportar un APK del juego.

## Preparación del PC

En Fedora:

```bash
sudo dnf install wivrn wivrn-dashboard android-tools
wivrn-dashboard
```

Completa el asistente y arranca el servidor desde esa ventana. Si el dashboard ya tiene un servidor abierto, no inicies además `wivrn-server` o el servicio systemd: ambos intentarían usar el mismo puerto.

El script del proyecto selecciona la copia local de Godot 4.6.3 si existe. Para otro ejecutable, define `GODOT_BIN` al iniciar el juego.

## Preparación de los Quest

1. Instala **WiVRn** en el visor desde Meta Store o el asistente del dashboard. Comprueba que cliente y servidor tienen la misma versión. Una diferencia de versiones puede impedir la conexión.
2. Activa **Modo desarrollador** desde los ajustes del visor en la aplicación **Meta Horizon** del teléfono. Si no aparece la opción, completa primero el registro de desarrollador que indica Meta.
3. Enciende el visor y conéctalo a un puerto USB del PC con un cable USB-C de datos.
4. Ponte el visor y acepta **Permitir depuración USB**. Puedes marcar **Permitir siempre desde este ordenador**. La autorización para acceder a archivos es un aviso distinto.
5. En una terminal del PC ejecuta:

```bash
adb devices -l
```

Debe aparecer una fila con el estado `device`. Si aparece `unauthorized`, falta aceptar el aviso dentro del visor. Una lista vacía requiere revisar cable, puerto, visor encendido y modo desarrollador. Si aparece `no permissions`, revisa las reglas udev del paquete `android-tools`; evita iniciar ADB como root.

## Conexión USB y arranque del juego

Desde la raíz de este repositorio, con el servidor WiVRn abierto:

```bash
bash scripts/connect-quest-usb.sh
```

El comando comprueba que el dispositivo USB es un Quest autorizado, encuentra el cliente WiVRn de Meta Store o GitHub, redirige el puerto 9757 y abre la aplicación en el visor. No instala aplicaciones ni cambia el firewall.

Para el primer emparejamiento, habilítalo en la ventana de WiVRn. También puedes abrir un intervalo de cinco minutos desde la terminal:

```bash
wivrnctl pair -d 5
```

Completa el PIN o la confirmación que muestre WiVRn dentro del visor. Espera a que el dashboard indique que el visor está conectado. Puedes apagar el Wi-Fi del visor durante la prueba para comprobar que la transmisión usa USB.

Después ejecuta:

```bash
bash scripts/play-vr.sh
```

Este comando activa OpenXR en el motor y pasa `--xr` al juego para seleccionar su jugador VR. WiVRn configura el runtime activo al conectar el visor. Si el juego muestra «OpenXR no está activo» y abre el modo PC, primero corrige la conexión del visor y vuelve a iniciar el juego.

Para usar otro Godot:

```bash
GODOT_BIN="/ruta/Godot" bash scripts/play-vr.sh
```

El renderer actual es Compatibility/OpenGL. Godot recomienda Mobile para XR; si hay problemas de OpenGL, prueba temporalmente lo siguiente y revisa iluminación y materiales en ambos ojos:

```bash
bash scripts/play-vr.sh --rendering-method mobile --rendering-driver vulkan
```

Este argumento no cambia el renderer guardado del proyecto. En equipos con dos GPU, WiVRn y Godot deben utilizar la misma GPU; evita que uno use la integrada y el otro la dedicada.

## Estado comprobado en este equipo el 9 de octubre de 2026

- Fedora 44, WiVRn y dashboard 26.9, ADB instalado y Godot local 4.6.3.
- El servidor nativo reconoció y emparejó los Quest 3S, pero las sesiones fallaron con `Failed to find a suitable video encoder`.
- La RTX 3070 utiliza nouveau/NVK. No está instalado el controlador NVIDIA que proporciona CUDA/NVENC. Antes de usar el encoder NVENC de WiVRn hay que resolver ese controlador; renderizar con Vulkan no demuestra que la codificación de vídeo funcione.
- El paquete de WiVRn observado incluye NVENC, pero tiene el encoder de software x264 deshabilitado. No se puede asumir que x264 esté disponible como alternativa en este paquete.
- Linux ya detecta un Quest 3S por USB (`2833:5010`), pero `adb devices -l` sigue vacío: falta disponer de la interfaz de depuración y autorizarla.
- Se instaló WiVRn 26.9 de Flathub y se conservó el emparejamiento del visor. Su servidor estableció una sesión usando x264/H.264 con [la configuración del laboratorio independiente](../../Pruebas/ConexionQuest/README.md#primera-prueba-con-codificación-por-cpu), sin cambiar el controlador gráfico.
- El laboratorio se inició en esa sesión: Godot 4.6.3 inicializó OpenXR con WiVRn 26.9 y Vulkan/Mobile en la RTX 3070. No se observaron errores de scripts en su registro de arranque.
- La imagen en ambos ojos, los mandos y el rendimiento siguen pendientes de comprobar dentro del visor.

## Controles y verificación

Apunta con el mando derecho y pulsa trigger para usar objetos o botones. Grip toma/suelta; el stick derecho gira por pasos de 30 grados. El click del stick izquierdo inicia el teletransporte y al soltarlo confirma el destino; el botón de menú pausa.

Comprueba primero imagen en ambos ojos, seguimiento de cabeza, puntero y menú. Después prueba giro, teletransporte, agarre y recorrido. Una conexión correcta no acredita la comodidad ni el rendimiento de la adaptación VR.

## Referencias

- [WiVRn: instalación, USB y emparejamiento](https://github.com/WiVRn/WiVRn#usage).
- [Configuración de encoders WiVRn](https://github.com/WiVRn/WiVRn/blob/master/docs/configuration.md).
- [Meta: configurar el visor para desarrollo](https://developers.meta.com/vr/documentation/native/android/mobile-device-setup/).
- [Godot: configurar OpenXR](https://docs.godotengine.org/en/stable/tutorials/xr/setting_up_xr.html).
- [NVIDIA: requisitos de NVENC](https://docs.nvidia.com/video-technologies/video-codec-sdk/13.0/nvenc-video-encoder-api-prog-guide/).
