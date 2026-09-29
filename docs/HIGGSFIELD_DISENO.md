# Higgsfield · diseños de Analyst Academy

## Estado y alcance

El conector oficial se utilizó realmente el **29-09-2026** para generar tres texturas de 1024 × 1024: roble, tejido y yeso. Las imágenes originales están incorporadas al proyecto, no son referencias externas ni imágenes de ejemplo. La geometría 3D y los shaders siguen siendo recursos nativos editables de Unity.

Higgsfield se usa como herramienta de producción, no como dependencia del ejecutable. Una referencia 2D no reemplaza un entorno transitable con colisiones, perspectiva y profundidad.

## Configuración

Priorizar el conector oficial ya conectado. No instalar servidores de terceros ni duplicar la conexión. Para otro cliente, se incluyen ejemplos sin secretos en `integrations/higgsfield/`; deben fusionarse con su configuración existente, no sustituirla.

- Servidor oficial: `https://mcp.higgsfield.ai/mcp` ([Higgsfield MCP](https://higgsfield.ai/mcp)).
- Codex CLI: usar el bloque TOML de ejemplo y completar `codex mcp login higgsfield` mediante OAuth. La configuración está documentada en [Codex MCP](https://developers.openai.com/codex/mcp).
- Cursor: el JSON es una plantilla para fusionar con `mcpServers` y autenticar desde el cliente.
- No escribir claves en `Assets`, el control de versiones ni el build. No publicar proyectos, sitios ni vídeos sin solicitarlo.

Para generación ordinaria se usan las herramientas directas, no recetas ni creación de sitios. En esta entrega se verificaron los parámetros de resolución y calidad de `gpt_image_2_5`; se estimó el coste antes de enviar un único lote de tres imágenes. El servidor notificó tres trabajos completados, sin reintentos. No inventar nombres de modelo ni repetir un trabajo cuyo estado sea desconocido.

## Procedencia de las imágenes incorporadas

Generadas mediante **Higgsfield MCP**, modelo `gpt_image_2_5` (motor OpenAI, variante `flare`), calidad `medium`, resolución `1k`, proporción `1:1`, fondo opaco. Estimación previa: **0,5 créditos por imagen; 1,5 créditos en total**. No se utilizó otro servicio de generación directamente ni se hicieron compras o cambios de plan.

| Archivo | Trabajo completado | Uso |
| --- | --- | --- |
| `oak.png` | `c2aa86f9-9e67-42f7-95b3-1fc8fe528a47` | Muebles y acabados de roble/nogal |
| `fabric.png` | `a10d8639-d766-4600-9703-00611cb0e1fa` | Tapicería, alfombra y ropa de los personajes |
| `plaster.png` | `e5345f0c-98fe-4ee3-92d3-a3c190eb25b7` | Paredes y superficies de yeso |

Los prompts exactos, parámetros, identificador de proyecto y SHA-256 de cada PNG están en [`texture-provenance.json`](../integrations/higgsfield/texture-provenance.json). No se guardan tokens, enlaces de descarga firmados ni credenciales. Los archivos PNG descargados no se retocaron; las adaptaciones de escala, color y detalle ocurren en Unity. El uso queda sujeto a las condiciones de la cuenta y el servicio, sin atribuirles una licencia CC0 ni exclusividad no comprobadas.

## Dirección de arte

Oficina de analistas profesional, sobria y cálida. Mantener distribución, escala humana, madera de roble, metal grafito, textiles discretos y acentos menta/ámbar. Conservar los cuatro personajes estilizados. Evitar neón cyberpunk, carteles ilegibles, logotipos reales, suciedad excesiva y contrastes que oculten los objetos interactivos.

### Referencia de ambiente (no es una captura del juego)

> Professional analyst office, restrained warm architectural interior, human eye height 1.62 m, oak desks with subtle rounded edges, graphite powder-coated metal, charcoal woven carpet, warm neutral plaster, understated mint and amber accents. Four tall windows on the left reveal a real mid-rise neighbourhood, courtyard trees and a quiet street below, layered depth and soft afternoon sky. Warm practical pendant lights balanced with cool window daylight, readable work surfaces, clean purposeful props. Respect the supplied in-game layout. No neon, no fisheye, no motion blur, no logos, no invented signage. Game environment art-direction reference, not a claim of an implemented scene.

### Albedos repetibles

Generar cada material por separado, cuadrado 1024 × 1024, vista ortográfica frontal, iluminación plana, repetición sin costuras en ambos ejes. Sin perspectiva, brillos, sombras horneadas, bordes, objetos ni texto. Preferir tonos neutros claros: Unity aplica después el tinte de cada material.

- `oak.png`: pale neutral oak grain, very subtle pores, fine understated long grain, no knots, no planks, seamless PBR base-color only.
- `fabric.png`: neutral light grey fine woven office upholstery, subtle even weave, no seams or folds, seamless PBR base-color only.
- `plaster.png`: light neutral matte fine plaster, barely visible micrograin, no cracks or stains, seamless PBR base-color only.

## Incorporación a Unity

1. Revisar el resultado a tamaño real y en mosaico 2 × 2; rechazar costuras o iluminación incorporada. Registrar modelo, prompt, fecha y condiciones de uso junto al archivo.
2. Guardar los PNG aprobados en `Assets/Art/Office/Resources/OfficeTextures/` con los nombres anteriores. No guardar respuestas JSON ni credenciales en Resources.
3. `OfficeTextureImporter` configura sRGB, mipmaps, repetición, filtrado trilineal, anisotropía 4, compresión y límite 1024. Descarta alpha porque son albedos opacos. `OfficeSurface` carga los acabados locales; sin ellos mantiene sus acabados procedurales.
4. Regenerar desde **EthicalLab → Office → Prepare modeled scene** (o ejecutar `EthicalLab.Editor.HubProjectSetup.PrepareModeledScene`). Comprobar oficina, archivo y ventana antes del build.
5. Volver a ejecutar las pruebas y generar **EthicalLab → Build → Mac universal first person**. No sustituir una entrega validada por un diseño sin comprobar.

### Escala y personajes

Los shaders usan proyección triplanar en metros locales del objeto para evitar imágenes estiradas sobre paredes largas, bordes biselados o brazos curvos. La textura acompaña a los muebles móviles. La normalización con el último mip conserva el tinte artístico existente, evitando oscurecer dos veces los materiales. El roble se repite cada 0,8 m, la tela de tapicería cada 0,25 m y el yeso cada 1 m; el detalle de yeso es deliberadamente suave.

`CelShading` aplica `fabric.png` a materiales identificados como `jacket`, `shirt` y `trousers`: chaquetas y pantalones cada 0,25 m, camisas cada 0,167 m. Se conservan el sombreado cel, los contornos, el rim y las sombras del personaje. Piel, ojos, cabello, calzado y manos en primera persona mantienen sus pigmentos originales, sin textura de tela o madera.

### Revisión realizada

Se comprobaron los tres PNG a tamaño real y en mosaico 2 × 2: sin texto, objetos, bordes ni perspectiva; roble de veta longitudinal, tejido uniforme y yeso de micrograno discreto. Los mosaicos se usan solo como revisión temporal; el juego referencia los PNG originales. La validación final de shaders, capturas del entorno y ejecutable corresponde al proceso de pruebas/build del proyecto.
