# Revisión de la reorganización Godot

La operación de la noche del 7 de octubre de 2026 (Protocolo Salida y, encima, Operación Cóndor Silencioso) está en [Cambios en vivo](CAMBIOS_EN_VIVO.md). Este archivo cubre solo la reorganización del 30 de septiembre.

El juego usa `Godot/Game/`, el catálogo único `Game/Features/Quests/missions.json` y avisos del motor en `Godot/ThirdParty/godot/`. La revisión incluye reglas, guardados, escena, recursos, importación y exportación.

## Fallos encontrados y corregidos

| Fallo | Corrección y evidencia |
| --- | --- |
| Presets con filtros de las carpetas retiradas `content/` y `licenses/` | Filtros nuevos para Quests y ThirdParty. El PCK inicialmente omitía ambos avisos de licencia; después de corregirlo se comprobaron los tres archivos necesarios dentro del paquete. |
| JSON truncado del guardado confundido con partida ausente | El store comunica el error a la sesión; bootstrap deshabilita autosave y conserva el archivo. Pruebas con JSON truncado, vacío y `null`. |
| Detalle GLB duplicado al hornear una instancia y sus hijos | La copia horneada conserva los hijos como geometría editable. Bajó de 69.132 a 49.572 triángulos; desaparecieron los errores del editor al importar. El horneado comprueba la geometría tras empaquetar/instanciar antes de guardar. |
| Conversor del arte dependiente de `HEAD` y del índice de trabajo | Origen fijado en `SOURCE_REV`; enumera el árbol de esa revisión. Se reconstruyó en `/tmp` y se verificó que el GLB fuera idéntico al existente. Es una herramienta de desarrollo, no una dependencia del juego. |
| Documentación con rutas anteriores a la reorganización | Rutas de escenas, guías e interfaz actualizadas en documentación operativa y diseño. |

## Comprobaciones

- **195** comprobaciones de sesión y **62** del recorrido físico, sin fallos.
- Importación del editor sin errores; los scripts de tests y export detectan errores Godot aunque el proceso devuelva exit=0.
- Validadores del catálogo y del orden de las cuatro fases: correctos.
- Escena bajo el presupuesto de 150.000 triángulos; texturas hasta 1K y sin mapas de normales.
- Paquete PCK cargado fuera del árbol fuente; arranque independiente comprobado.

## Vigilancia continua

Desde una terminal en la raíz del repositorio:

```bash
python3 scripts/colab_watch.py --watch --validate
```

Requiere Python 3, Node.js y Godot instalado o detectado en `.tools/`. También admite `GODOT_BIN` para elegir el editor.

El monitor observa el código, escenas, catálogo, arte, configuración y scripts. Espera un intervalo de archivos estables y ejecuta contenido, sintaxis shell, estructura, importación y tests. Excluye `.godot`, metadatos generados, credenciales, capturas y builds para evitar revisiones provocadas por sus propias pruebas.

Muestra **VALIDACIÓN OK** o **VALIDACIÓN FALLÓ** en la terminal, y registra resultado y archivos cambiados en `docs/COLABORACION.md`. El último estado queda en `docs/.colab-state.json`, ignorado por Git. Se detiene con **Ctrl+C**.

El monitor detecta fallos; no edita el código ni sustituye una revisión humana. Una validación solo cubre el estado de archivos comprobado. Las exportaciones ejecutables Mac/Android y el visor Quest 3S siguen requiriendo templates, SDK y pruebas de hardware.
