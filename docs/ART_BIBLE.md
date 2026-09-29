# Art bible — Analyst Academy (18–32)

Oficina de consultora de ciberseguridad ficticia. El jugador es un analista junior; el mundo debe sentirse **profesional, contemporáneo y creíble**, no “curso escolar” ni “matrix hacker”.

## Referencias visuales

- Oficinas reales 2018–2026: open space sobrio, madera oscura o laminado gris, acentos en señalética, no neón.
- SOC / NOC: pantallas con datos ficticios legibles a distancia corta; cables ordenados, no sala de servidores cyberpunk.
- Simuladores serios (Flight Sim, PC Building Simulator): claridad funcional por encima del detalle ornamental.

## Paleta (alinear con `HubOffice` en código)

| Nombre | Uso | RGB aprox. (0–1) |
| --- | --- | --- |
| Navy | Suelo, techos, fondos de pantalla | 0.045, 0.08, 0.12 |
| Slate | Paredes | 0.12, 0.19, 0.23 |
| Graphite | Metal, laptop, marcos | 0.025, 0.033, 0.04 |
| Walnut | Madera (escritorio, puerta) | 0.38, 0.23, 0.15 |
| Paper | Texto en pizarra / documentos | 0.82, 0.84, 0.78 |
| Mint | Interactables activos, acento UI | 0.22, 0.83, 0.73 |
| Amber | Tickets disponibles, bandejas | 0.92, 0.68, 0.32 |
| Green | Completado / éxito | 0.36, 0.78, 0.42 |
| Muted | Bloqueado / inactivo | 0.30, 0.36, 0.40 |
| Rack red | Servidor Atlas (fuera de alcance) | 0.78, 0.32, 0.30 |

En Blender: PBR con roughness 0.55–0.85 en madera/pared; mint solo en props que el jugador debe notar (carpetas leídas, buzón listo).

## Anti-patrones (no usar)

- Terminal verde sobre negro estilo película de los 90.
- Calaveras, “ANONYMOUS”, máscaras, glitch excesivo.
- Confeti, estrellas, mascotas cartoon, voz infantil.
- Neón rosa/cyan en todo el entorno.
- Texto ilegible en pantallas (ruido sin jerarquía).

## Legibilidad y guía espacial

- **Luz**: key cálida desde ventana ficticia; fill fría suave; punto cálido en archivo.
- **Silueta**: puerta del archivo y rack Atlas reconocibles desde el spawn.
- **Contraste**: bandejas activas con emissive mint muy bajo (opcional en Unity).
- **Señalética**: “ARCHIVO →”, “ATLAS · NO ES TU CLIENTE”, “INFORME” en buzón.

## Escala y personaje

- 1 unidad = 1 metro.
- Ojos del jugador ~1.62 m (`PcInteractor` cámara).
- Raycast de uso: 3 m — interactables deben leerse a esa distancia.

## Presupuesto MVP

- Hub completo visible: **&lt; 150k tris** (LOD0).
- Texturas: 1K props mayoría; 2K solo laptop, pizarra, suelo si hace falta.
- Colliders: cajas en interactables; mesh collider solo en shell estático.

## Tono copy (coherente con arte)

- Español neutro, directo, sin jerga de ataque real.
- Errores = “relee la evidencia”, no “game over”.

## Entregables por módulo Blender

Ver [`Assets/Art/Office/README.md`](../Assets/Art/Office/README.md) y [`docs/HUB_ANCHORS.md`](HUB_ANCHORS.md).
