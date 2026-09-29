# Entrega gráfica y Mac · 29-09-2026

## Resultado

Aplicación: `Builds/Mac/AnalystAcademy.app`.

Paquete para transferir: `Builds/Mac/AnalystAcademy-mac-universal.zip`, aproximadamente 44 MiB. Contiene la aplicación completa y `LEEME.txt`. ZIP comprobado sin errores. El ejecutable Mach-O contiene **x86_64 y arm64**, comprobado tanto por el exportador como por la herramienta `file`.

SHA-256 del ZIP:

```text
be8bc63e085da270f2de7730374a02f30789c65d32f2a1f413960ed8d353f7b3
```

Unity 6000.6.3f1, Mac Build Support (Mono), Metal, macOS mínimo 12.0. No se ha ejecutado en un Mac físico. La compilación incluye firma local del proceso de Unity, pero **no firma Apple Developer ID ni notarización**. Ver [guía de presentación](DEMO_MAC.md) y probar el arranque en el equipo de clase.

## Revisión de cambios y mejoras

Se conservaron los cambios previos del proyecto: oficina ampliada, cuatro personajes estilizados, manos en primera persona, coach de movimiento, sillas físicas y flujo narrativo. No se regresó a la distribución antigua.

- Acabados de madera, yeso y tejido; biseles en mobiliario con mallas persistidas en el prefab.
- Tres PNG originales generados mediante Higgsfield MCP: roble, tejido y yeso, 1024 × 1024. Aplicados a oficina y ropa de los personajes, preservando piel, ojos y cabello. Prompts, trabajos y hashes en [procedencia](../integrations/higgsfield/texture-provenance.json).
- Texturizado triplanar en escala métrica, mipmaps, filtrado y compresión; sin dependencia de Higgsfield durante el juego.
- Iluminación suave del entorno, luz cálida de lámparas, relleno frío de ventanas y antialiasing 4×. Los personajes mantienen su sombreado cel.
- Cuatro ventanas transparentes con huecos reales en el muro, marcos, tiradores, persianas y colisión de contención.
- Exterior 3D: calle inferior, acera, árboles, edificios en varias capas, azoteas y cielo con variación lenta de nubes. La profundidad cambia al caminar: no es una imagen plana pegada al vidrio.
- Exportación Mac universal: se corrigió el uso de arquitectura para Unity 6.6 y se añadió validación del binario resultante. No basta con llamar al archivo «universal».

Geometría capturada: **87.080 triángulos / 896 renderers**, frente a 60.552 / 703 en la captura previa de esta sesión. No se ha medido rendimiento en el Mac de destino.

## Evidencia visual

- [Oficina antes](previews/before-office-preview.png) / [oficina después](previews/office-preview.png).
- [Detalle de materiales](previews/materials-preview.png).
- [Ropa de personaje](previews/character-preview.png).
- [Vista por la ventana](previews/window-preview.png).
- [Archivo](previews/archive-preview.png).

Son capturas renderizadas en Unity, no imágenes conceptuales generadas. El prefab final está en `Assets/Art/Office/Prefabs/Office_AnalystAcademy.prefab`; Hub y Hub_Art lo comparten.

## Verificación

- **20/20 PlayMode**: inicio Boot → Hub, cámara y teclado/ratón, sprint, pausa, recuperación, puertas, cajón, cuatro casos, guardado, ventanas, persistencia de geometría y texturas en oficina/personajes.
- **5/5 NUnit .NET**: casos de uso puros. El restaurador notificó que no pudo consultar el índice de vulnerabilidades NuGet; esto no fue una auditoría de dependencias.
- Validadores Python/Node: contenido, referencias del prefab, fuentes, escenas y sesión correctos.
- Capturas revisadas; sin errores de compilación de shaders en los registros finales de capturas/build.
- Compilación Mac final: `Build Finished, Result: Success`; `MAC UNIVERSAL VERIFIED: x86_64 + arm64`.
- Evidencias completas en `Builds/Validation-2026-09-29/`.

Las pruebas antiguas usaban posiciones de la oficina pequeña y suponían sprint lateral. Se actualizaron para usar las anclas actuales, acercamiento humano al cajón y sprint hacia delante, sin eliminar las comprobaciones de colisión o interacción.

## Integraciones y límites

El agente de texturas sí recibió las herramientas de Higgsfield y completó los tres trabajos. La skill de Unity del enlace facilitado no apareció en el catálogo accesible; no se afirma haberla utilizado. El modelado, renderizado, pruebas y compilación se realizaron directamente con el editor Unity instalado.

Según la [documentación de plugins de OpenAI](https://learn.chatgpt.com/docs/plugins), las skills y herramientas recién instaladas se cargan en un chat o sesión nuevos. El identificador del enlace no permitió identificar ni inspeccionar ese plugin Unity; para continuar ese diagnóstico hace falta el nombre mostrado en su ficha o el mensaje exacto de error.

Antes de reemplazar los assets generados se guardó una copia local de la oficina y las escenas previas en `/tmp/analyst-before-delivery.6UEk2s/`. Es una copia temporal, no un respaldo permanente. No se borraron los builds Linux existentes.
