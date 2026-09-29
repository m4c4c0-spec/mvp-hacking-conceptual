# Entrega Linux · 29-09-2026

## Abrir el juego

Ejecutable: `Builds/Linux/AnalystAcademy.x86_64`.

Desde la raíz del proyecto:

```bash
bash scripts/play-linux.sh
```

También se puede entrar a `Builds/Linux` y ejecutar:

```bash
bash Iniciar.sh
```

No necesita tener Unity instalado. Es una aplicación Linux **x86_64**: no es un ejecutable ARM ni una aplicación de Windows. Necesita una sesión gráfica y un controlador gráfico compatible; la ejecución se comprobó en este equipo Fedora Linux 44, con X11 sobre XWayland y OpenGL 4.6 (Mesa / Zink, RTX 3070). No se ha verificado en todas las distribuciones.

## Transferir a otro computador

Paquete: `Builds/AnalystAcademy-linux-x86_64.tar.gz`.

Tamaño: 35.679.599 bytes (aproximadamente 34 MiB). Integridad gzip comprobada y contenido del TAR comparado con la carpeta entregada, sin diferencias. Para verificar después de transferir, copiar también el archivo `.sha256` y ejecutar en la carpeta que contiene ambos:

```bash
sha256sum -c AnalystAcademy-linux-x86_64.tar.gz.sha256
```

Extraer el archivo completo y conservar juntos el ejecutable, `AnalystAcademy_Data`, `UnityPlayer.so` y las bibliotecas incluidas. **No copiar solamente el archivo `.x86_64`.** El paquete incluye `Iniciar.sh`, instrucciones y el guion de presentación.

```bash
tar -xzf AnalystAcademy-linux-x86_64.tar.gz
cd Linux
bash Iniciar.sh
```

Si una copia manual pierde el permiso de ejecución, restaurarlo únicamente al archivo del juego:

```bash
chmod +x AnalystAcademy.x86_64
```

Ejecutar desde una carpeta con permisos de escritura para guardar `player.log`. Si una memoria USB impide ejecutar aplicaciones, extraer el paquete en una carpeta local del computador. No hace falta usar `sudo` para jugar.

## Antes de presentar

1. Ensayar el arranque en el computador de clase. Seleccionar «Entrar a la oficina» o «Continuar» y cerrar la introducción si aparece.
2. WASD y ratón para caminar y mirar; Shift al avanzar para correr; E para usar y G para tomar/devolver carpetas.
3. Esc abre el menú y libera el cursor. En Inicio se pueden ajustar sensibilidad y campo de visión, o volver a la entrada conservando el progreso.
4. Ensayar un recorrido de dos minutos: oficina → ventana → pizarra → archivo → escritorio. El guion de presentación se conserva localmente y no se publica en el repositorio.
5. No pulsar «Reiniciar demo» salvo que se quiera borrar la partida y se confirme esa decisión.

El juego no necesita conexión a Higgsfield: las texturas ya están integradas. Las imágenes de producción, incluida la ropa de personajes, se mantienen en esta compilación.

## Verificación de esta entrega

- Compilación `StandaloneLinux64` mediante Unity CLI y el método `EthicalLab.Editor.HubDesktopBuild.Linux`, con Unity 6000.6.3f1.
- Resultado del editor: `Build Finished, Result: Success`; salida del CLI: `success: true`, sin errores de editor reportados.
- Cabecera ELF x86-64 y permiso de ejecución comprobados. `ldd` resolvió las dependencias directas de `UnityPlayer.so` en el equipo de construcción.
- Se abrió el ejecutable nativo durante 30 segundos, con ventana 1280 × 720. Se verificó visualmente la bienvenida sobre la oficina 3D y la recuperación de progreso. No se registraron excepciones de ejecución ni errores de shader en esa prueba.
- La ventana de prueba se cerró mediante un límite deliberado de tiempo (`timeout`, código 124), no por un fallo observado del juego. El controlador emitió avisos de DPI y selección del modo de renderizado; no impidieron el arranque.
- Esta prueba breve confirma el arranque y el renderizado, **no** constituye una partida completa ni una medición de rendimiento. Las 20 pruebas PlayMode y 5 pruebas .NET de la entrega gráfica previa siguen disponibles; no se modificó el código del juego en esta entrega Linux.

Evidencia: [captura del ejecutable](previews/linux-startup.png), `Builds/Validation-2026-09-29/linux-build.log`, `linux-build-provenance.json` y `linux-player-smoke.log` en esa misma carpeta.

El build se hizo desde la copia de trabajo aislada usada para validar la entrega gráfica. Se comprobó que los assets existentes y los paquetes coincidían con los del proyecto; los assets adicionales de Academy son derivados del prototipo antiguo. El exportador incluye solamente las escenas Boot y Hub.

La versión Linux anterior se conserva en `Builds/Linux-anterior-2026-09-27`. La aplicación y el ZIP de Mac no se reemplazaron.

## Recompilar

Desde la raíz del proyecto, con el módulo Linux Build Support instalado y sin otro editor usando esa misma carpeta:

```bash
unity build . --target StandaloneLinux64 \
  --execute-method EthicalLab.Editor.HubDesktopBuild.Linux \
  --allow-dirty-build
```

El método del proyecto define el destino `Builds/Linux/AnalystAcademy.x86_64`. Si se desea conservar ese build antes de recompilar, hacer primero una copia de su carpeta. La skill `unity:unity-cli`, disponible en esta sesión, se utilizó para el flujo de compilación y su registro de procedencia.
