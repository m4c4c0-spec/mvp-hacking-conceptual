# Adaptación del equipo — migración Unity → Godot

## Estado verificado · 2026-09-30

La primera migración jugable está implementada. `Godot/scenes/main.tscn` y `office.tscn` existen; PC conserva primera persona y las cuatro fases se completan mediante pizarra, carpetas, bandejas, expediente/quiz y controles de salida. Se incluyen onboarding, pausa, terminal narrativa, cuaderno y persistencia independiente. La geometría es una reconstrucción ligera editable; importar el arte detallado queda pendiente.

Las pruebas de reglas y persistencia pasan 182 comprobaciones; el recorrido por UI y objetos físicos pasa 62, incluyendo cinco guías animados con ayuda contextual. La oficina tiene assets geométricos mejorados y suma 30.012 triángulos. [Assets y guías](../Godot/docs/ASSETS_Y_GUIAS.md) documenta su edición y las capturas renderizadas con Godot. `scripts/play-godot.sh`, `test-godot.sh`, `export-godot.sh` y `sync-godot-content.py` están disponibles. Mac tiene preset universal e instrucciones; exportar ejecutables requiere templates del editor. Rig OpenXR y UI espacial son experimentales y no están validados en un Quest 3S.

El README raíz dirige el desarrollo a Godot. Unity queda como referencia y conserva su catálogo canónico durante la transición. Documentación de uso: [Godot/README.md](../Godot/README.md), [Mac y Builds](../Godot/docs/MAC_Y_BUILDS.md) y [Quest 3S](../Godot/docs/QUEST_3S.md). Las fases inferiores describen el plan de transición original; este estado verificado actualiza el inventario inicial.


Documento operativo para personas y agentes de IA. Complementa la arquitectura jugable descrita en [`ARQUITECTURA.md`](ARQUITECTURA.md), el contrato de datos en [`CONTRATO_CONTENIDO.md`](CONTRATO_CONTENIDO.md) y el inventario Unity en [`ELEMENTOS.md`](ELEMENTOS.md).

**Plan Cursor oficial encontrado:** en `~/.cursor/plans/` solo existe [`mvp_3d_blender_9be06f0b.plan.md`](file:///home/cocus/.cursor/plans/mvp_3d_blender_9be06f0b.plan.md) (hub modelado **en Unity**, ya completado). **No hay** un archivo de plan titulado “migración Godot” en el repo ni en `~/.cursor/plans`. La migración a Godot está materializada en la carpeta [`Godot/`](../Godot/) y en este documento, que fija criterios hasta que se publique un plan formal equivalente.

---

## Principios que no cambian

Estos límites aplican en Unity legacy y en Godot por igual:

| Principio | Fuente |
| --- | --- |
| **Ética y pedagogía** | Solo conceptos educativos; sin exploits, payloads, malware ni herramientas ofensivas reales. |
| **Terminal narrativa** | Vocabulario ficticio (`help`, `scan`, `map`, `inspect`, `simulate`, `notes`, `report`, `clear`). Rechazar `curl`, `nmap`, `metasploit`, etc. |
| **Contenido data-driven** | [`Assets/Resources/Content/missions.json`](../Assets/Resources/Content/missions.json) es la **única fuente canónica** de misiones y conceptos. Una misión nueva es un cambio en JSON (+ validación), no lógica ad hoc en el motor. |
| **Offline single-player** | Sin netcode ni servicios en runtime. |
| **Bucle 3D** | El mundo ejecuta el aprendizaje: ticket → archivo → carpetas → bandejas (observar/defender) → cuaderno → expediente/quiz → seguros físicos → salida. |
| **Alcance (Atlas)** | Interactuable fuera de alcance autorizado; enseña scope sin castigo punitivo. |
| **Blue / Red** | Lentes de la misma persona en un caso autorizado; no bandos ni instrucciones de ataque. Narrativa: [`docs/historia/BIBLIA.md`](historia/BIBLIA.md). |

---

## Qué se porta de Unity y qué se descarta

### Se porta (comportamiento y datos)

- **Reglas de dominio y casos de uso** (hoy en C# puro bajo `Assets/_Project/Domain/` y `Application/`): catálogo, progreso, flujo de pistas, informe, quiz, puntuación, escape room (seguros ordenados + salida).
- **`missions.json`** (mismo esquema que [`CONTRATO_CONTENIDO.md`](CONTRATO_CONTENIDO.md)).
- **IDs de interactables** del hub (`ticket:`, `clue:`, `tray:`, `scope`, `door`, `drawer`, `laptop`, `notebook`, buzón de informe, `escape:<fase>`) — ver [`HUB_ANCHORS.md`](HUB_ANCHORS.md) y [`ELEMENTOS.md`](ELEMENTOS.md).
- **Arte 3D** exportable (FBX/GLTF desde `Assets/Art/Office/`, Blender) como **assets importados**, no como escena Unity embebida.
- **Documentación de producto** (README, playtest, demos Linux/Mac); actualizar referencias de motor cuando Godot sea jugable.

### Se descarta o congela (no ampliar como canónico)

| Área | Tratamiento |
| --- | --- |
| **`Assets/Scenes/Academy.unity`** | Prototipo congelado; no añadir misiones ni reglas. |
| **ScriptableObjects de Academy** | Vista generada desde JSON; irrelevantes en Godot. |
| **Presentation Unity** (`LabBootstrap`, `HubHud`, `PcInteractor`, shaders URP/Built-in, menús `EthicalLab/*`) | Legacy de referencia hasta paridad Godot; luego solo mantenimiento mínimo de builds si hace falta. |
| **XR Unity** (`XrInteractor`, paquetes OpenXR en editor Unity) | Sustituido por ruta OpenXR nativa Godot 4 (`Godot/project.godot` ya reserva sección `[xr]`). Ver [`META_QUEST_3S.md`](META_QUEST_3S.md) como **intención**, no como implementación Godot. |
| **HubOffice procedural** (`HubOffice.Build`) | Fallback histórico; el hub modelado (`Office_AnalystAcademy`) es la referencia visual a reimportar. |

---

## Estado actual en Godot (inventario)

Carpeta raíz del motor: **`/home/cocus/Documentos/mvp_juegos/mvp_hacking_conceptual/Godot/`**

| Artefacto | Estado |
| --- | --- |
| `project.godot` | Godot **4.6.3**, nombre *Protocolo Salida*, versión `0.2.0-godot`, renderer **GL Compatibility** (PC modesto + base móvil/VR), OpenXR desactivado, 72 ticks física. |
| `run/main_scene` | Apunta a `res://scenes/main.tscn` — **creada**; instancia oficina horneada e inicializa juego y UI. |
| `scripts/session.gd` | Clase `EscapeSession`: carga catálogo, estado de partida, flujo misión/pistas/bandejas/informe/quiz, seguros, escape, guardado `user://escape-progress-v1.json`. Equivalente funcional a gran parte de `MissionCatalogRules` + progreso + `EscapeRoomRules` en Unity. |
| `content/missions.json` | Copia byte-a-byte del canónico Unity (misma talla que `Assets/Resources/Content/missions.json`). **Duplicado de despliegue Godot**, no segunda fuente de verdad. |
| Escenas, input, UI, 3D, terminal, tests GDScript | Primera versión jugable implementada; arte detallado y prueba de visor pendientes. |

Los scripts de inicio, sincronización, pruebas y exportación Godot están en `scripts/`. README raíz apunta a Godot.

---

## Correspondencias Unity → Godot

| Concepto | Unity (canónico hoy) | Godot (objetivo) |
| --- | --- | --- |
| **Dominio sin motor** | `EthicalLab.Domain` + `Application` (C#, asmdef `noEngineReferences`) | GDScript puro o C# en proyecto Godot **sin** nodos en reglas: p. ej. ampliar `EscapeSession` y extraer módulos (`catalog.gd`, `escape_rules.gd`, `terminal_vocab.gd`) |
| **Casos de uso** | `StartMission`, `CompleteStep`, `ClueWorkflow`, `SubmitReport`, `NarrativeTerminal`, `SaveLoadProgress`, `LabUseCases` | Métodos en sesión/servicios; la escena solo traduce input → llamada → feedback |
| **Contenido** | `Resources.Load` → `JsonMissionRepository` | `FileAccess` / `res://content/missions.json`; **editar siempre** el JSON bajo `Assets/Resources/Content/` y copiar o automatizar sync a `Godot/content/` |
| **Persistencia** | `JsonProgressStore` (ruta usuario OS) | `EscapeSession.save()` → `user://escape-progress-v1.json` |
| **Hub / oficina** | `HubSceneRefs`, `WorldBinder`, `InteractableView`, prefab `Office_AnalystAcademy` | Escena `office.tscn` + nodos con metadata `interactable_id`; script “binder” que tinta/activa según estado de `EscapeSession` |
| **Bootstrap** | `LabBootstrap` | Autoload o nodo raíz que instancia sesión, carga guardado, enlaza UI y mundo |
| **Primera persona** | `PcInteractor`, `CharacterController` | `CharacterBody3D` + cámara; mismas teclas WASD, E, G, ESC |
| **Terminal narrativa** | `NarrativeTerminal` + `TerminalVocabulary` | Panel UI + parser que rechaza comandos desconocidos |
| **UI menú / expediente** | `HubHud`, `HubMenuUi`, uGUI/TMP | `Control`/`CanvasLayer`; world-space para VR más adelante |
| **Guía in-world** | `HubGuide` | `hint()` ya en `EscapeSession`; conectar a label/HUD |
| **Escape room** | `EscapeRoomRules`, `EscapeRoomWorld`, IDs `escape:*` | `restore()`, `escape()`, `CONTROL_NAMES` en `session.gd` |
| **Tests** | `scripts/test-dotnet.sh`, EditMode NUnit | Tests GDScript (`GUT` o escenas de prueba) + seguir validando JSON con `scripts/validate-mission-flow.sh` |
| **VR (futuro)** | Unity OpenXR + `XrInteractor` | Godot OpenXR; mismo contrato `IInteractor` conceptual (Use/Grab/Hover por ID) |

---

## Fases de migración (orden recomendado)

Hasta que exista un plan Cursor dedicado, el equipo adopta **este orden**. Las fases 1–3 tienen una primera implementación jugable validada en Godot; la importación del arte detallado y el playtest de VR siguen pendientes.

### Fase 0 — Alineación (hecho / en curso)

- Carpeta `Godot/` con `project.godot` y `EscapeSession`.
- Copia de `missions.json` en `Godot/content/`.
- Este documento.

### Fase 1 — Núcleo jugable sin 3D complejo

- Crear `scenes/main.tscn` (placeholder: UI de debug o hub greybox mínimo).
- Paridad de reglas: tests que reproduzcan casos de `EscapeRoomTests` y flujo misión 01 contra `EscapeSession`.
- Script de sync documentado: tras editar JSON canónico, actualizar copia Godot (o enlace simbólico en entorno dev).
- **No** duplicar reglas en Unity y Godot sin test equivalente.

### Fase 2 — Hub 3D e interactables

- Importar layout oficina (GLB/FBX) o greybox con **mismos IDs** que Unity.
- Raycast / áreas interactuables → `start`, `collect`, `classify`, `report`, `answer_quiz`, `restore`.
- Puerta/cajón como animación o `AnimatableBody3D`; sin lógica de negocio en el nodo.

### Fase 3 — UX paridad PC

- Onboarding, toasts, panel ESC, buzón de informe, cuaderno, laptop/terminal.
- Guardado/carga y “reiniciar demo”.
- Build export Linux (y Mac si aplica) desde Godot; carpeta `Builds/` análoga a la actual.

### Fase 4 — Render y presupuesto 8 GB (en curso)

Reglas fijadas en `Godot/scripts/render_budget.gd` y en los valores por defecto de `project.godot`:

- Menos de **150.000 triángulos** en la oficina. `office_builder.gd` y el horneado lo comprueban; si se supera, el horneado no guarda la escena.
- Texturas importadas a **1K** como máximo, comprimidas en VRAM, con mipmaps. `compress/normal_map` queda en 0.
- **Sin normales:** los materiales Lambert no activan normal map y los FBX/glTF no generan tangentes ni LODs extra.
- Una sombra dura de 1024 px desde la luz de ventana. Los focos del techo no tienen mapa de sombras. GL Compatibility se mantiene; MSAA no aplica en ese renderer.
- La escena en juego debe quedarse **bajo unos 2 GB** de memoria estática para que el proceso completo quepa en 8 GB. Al arrancar se avisa si se cruza ese umbral.
- OpenXR sigue apagado hasta que el playtest de PC pase. Ver intención en [`META_QUEST_3S.md`](META_QUEST_3S.md).

### Fase 5 — Declarar Godot canónico

- README y demos apuntan a Godot como build principal.
- Unity EthicalLab: congelado salvo hotfix de contenido compartido vía JSON.

---

## Prioridad para agentes: Godot vs Unity legacy

1. **Pregunta explícita del usuario:** si pide Godot, Quest, 8 GB o “migración”, trabaja en `Godot/` y docs asociados; **no** abras Unity ni refactors grandes en `Presentation/` Unity salvo sync de JSON o extracción de reglas de referencia.
2. **Contenido narrativo:** editar solo `Assets/Resources/Content/missions.json` (contrato); historiador en `docs/historia/` no vuelca misiones sin orden. Tras cambios, sincronizar copia Godot si existe.
3. **Reglas de juego:** implementar primero en Godot (`EscapeSession` o módulos GDScript); Unity C# Domain/Application sirve de **especificación** hasta paridad; evitar divergencias sin test.
4. **Arte:** Blender → export → Godot; no reimplementar `HubOffice.Build()` en Godot.
5. **Archivos calientes:** un agente por área (`Godot/scripts/`, `Godot/scenes/`, `missions.json`, shaders/render) para no pisar migración en curso.
6. **Validación mínima:** `scripts/validate-mission-flow.sh` tras tocar JSON; tests de sesión Godot cuando existan; no exigir Unity Play Mode para validar reglas.

---

## Cuándo deja de ser canónico el código Unity

| Momento | Unity |
| --- | --- |
| **Ahora** | Jugable principal; referencia de paridad y demos cliente. |
| **Tras Fase 3 Godot** (paridad PC misión 01–04 + guardado) | Presentation Unity deja de recibir features; solo fixes críticos. |
| **Tras Fase 5** | `Assets/_Project` queda archivo histórico + tests dotnet opcionales; README declara Godot. |

Domain/Application C# pueden mantenerse como **biblioteca de especificación** y tests cruzados hasta que Godot tenga cobertura equivalente.

---

## Referencias rápidas

| Tema | Ruta |
| --- | --- |
| Proyecto Godot | `Godot/project.godot` |
| Lógica sesión Godot | `Godot/scripts/session.gd` |
| JSON canónico | `Assets/Resources/Content/missions.json` |
| Plan Unity 3D (completado) | `~/.cursor/plans/mvp_3d_blender_9be06f0b.plan.md` |
| Roadmap producto (Unity-centric) | `docs/ROADMAP.md` — revisar al declarar Godot |
| Agente historiador | `.cursor/agents/historiador.md` |

---

*Última revisión: 2026-09-29 — alineado al esqueleto Godot 0.2.0-godot en repo.*
