# Generar y jugar el escape room en Mac

Este documento describe la referencia Unity. Para la versión migrada consulta [la guía Godot](../Godot/docs/MAC_Y_BUILDS.md).

PC/Mac conserva la cámara en primera persona: WASD, mouse, E para usar, G para tomar/soltar, Esc para menú. Las escenas canónicas son Boot y Hub.

## Desde el repositorio, aunque no haya ejecutables

1. Instala Unity Hub y **Unity 6000.6.3f1**, la versión de `ProjectSettings/ProjectVersion.txt`.
2. En Hub → Installs → engranaje del editor → Add modules, instala **Mac Build Support (Mono)** si tu instalación lo ofrece o requiere. Usa una instalación que admita compilar StandaloneOSX.
3. Añade la carpeta completa del repositorio a Hub y ábrela. Espera la importación de paquetes. Si faltan recursos de texto, importa **Window → TextMeshPro → Import TMP Essential Resources**.
4. Abre `Assets/Scenes/Boot.unity` y pulsa **Play**: Unity renderiza la oficina 3D en la vista Game. Haz clic en ella y entra a la oficina. Para probar jugabilidad no necesitas generar un build.
5. Para crear el ejecutable, selecciona **EthicalLab → Build → Mac universal first person**. El código crea automáticamente `Builds/Mac/AnalystAcademy.app`, configura **Metal**, Mono y arquitectura universal Intel + Apple Silicon.
6. Desde Finder abre la app, o desde Terminal en la raíz del repo:

```bash
open Builds/Mac/AnalystAcademy.app
```

También puedes compilar desde Terminal con el editor cerrado y su licencia activada:

```bash
bash scripts/build-mac.sh
```

Si Unity está instalado en otra ubicación:

```bash
UNITY_EDITOR="/ruta/Unity.app/Contents/MacOS/Unity" bash scripts/build-mac.sh
```

El registro queda en `Builds/Mac/build.log`. Un error sobre módulo o arquitectura requiere revisar la instalación del editor; no basta crear una carpeta vacía.

## Qué incluye Builds en Git

Se versionan `Builds/README.md`, `Builds/Mac/.gitkeep` y `Builds/Quest/.gitkeep`. Las aplicaciones, APK y registros se generan localmente y se ignoran. Clonar el repositorio entrega el proyecto y las rutas de salida, no una app ya compilada.

Para transferir una compilación desde Mac, empaqueta la app completa conservando su estructura:

```bash
 ditto -c -k --sequesterRsrc --keepParent Builds/Mac/AnalystAcademy.app Builds/Mac/ProtocoloSalida-Mac.zip
```

La demo local no incluye firma Developer ID ni notarización. Si macOS solicita aprobación para esta app de procedencia conocida, usa la excepción por aplicación descrita por [Apple](https://support.apple.com/en-gb/102445). Comprueba el arranque en un Mac antes de presentar.

## Recorrido de prueba

Reinicia la demo desde Esc → Inicio si tienes una partida anterior. Acepta la primera fase en la pizarra, investiga correo/sesión/orden de bloqueo en las carpetas, clasifica observación y defensa y cierra el expediente. Activa el seguro 1 en el panel junto a la pared frontal. Repite contención, identidad y limpieza; activa los cuatro seguros en orden y usa la puerta roja de salida. Su apertura y el mensaje «HAS ESCAPADO» son el final de esta versión; no hay un segundo nivel exterior.
