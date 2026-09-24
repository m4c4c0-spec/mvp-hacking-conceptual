# Bitácora de colaboración

El otro desarrollador y este prototipo comparten el mismo workspace. Esta bitácora registra **qué hizo el otro lado** y qué se adaptó aquí para no pisarnos.

Convención de rutas:

| Ruta | Dueño | Notas |
| --- | --- | --- |
| `Assets/Scripts/`, `Packages/`, `ProjectSettings/` | Otro desarrollador | Unity 6, `Academy.*`, oficina, interactables. |
| `Assets/Scenes/Academy.unity` | Otro desarrollador | Prototipo primera persona (CharacterController, agarrar). |
| `Assets/Resources/Content/missions.json` | Otro desarrollador | JSON canónico (antes en `content/`). |
| `Assets/_Project/` | Este lado | EthicalLab: Domain / Application / Infrastructure / Presentation / Shared. |
| `Assets/Scenes/Hub.unity`, `Boot.unity` | Este lado | Hub primera persona por `IInteractor`. |
| `Assets/Tests/EditMode/UseCaseTests.cs` | Este lado | StartMission → SubmitReport, sin Play Mode. |
| `web/` | Este lado | Cliente jugable que espeja las reglas de `AcademySession`. |
| `docs/`, `scripts/` | Este lado | Plan, contrato, Steam, bitácora, validación, firmas. |
| `unity-stub/` | Este lado | Solo el mapa web ↔ `InteractionAction`. |

Si aparece un archivo nuevo fuera de estas carpetas, se trata como cambio del otro desarrollador hasta que se clasifique.

---

## 2026-09-21 23:17 — Primer entregable del otro desarrollador

- **Archivo:** `content/missions.json`
- **Huella md5:** `3d552a642f9e1034c3159a968c79d0b6`
- **Tamaño:** 26 778 bytes
- **Versión de esquema:** `1`
- **Título de producto:** Blue / Red · Analyst Academy

### Qué construyó (contenido, no código de juego)

Base de datos ampliable al estilo ScriptableObject: 8 fichas de concepto y 4 misiones encadenadas. Cada pista tiene pregunta de clasificación, pregunta de defensa, feedback y enlace a un concepto. Cada misión cierra con informe + quiz de 3 preguntas + texto “aprendido”.

#### Conceptos (8)

| id | Nombre | Categoría |
| --- | --- | --- |
| recon | Reconocimiento | Observación |
| scope | Alcance y autorización | Ética |
| phishing | Ingeniería social | Personas |
| verification | Verificación independiente | Personas |
| passwords | Contraseñas únicas y gestores | Identidad |
| mfa | Autenticación multifactor | Identidad |
| surface | Superficie de ataque | Sistemas |
| transport | Protección del transporte | Sistemas |

Encaja con el criterio del MVP (6–8 conceptos con defensa).

#### Misiones (tutorial + 3)

| id | Nº | Tipo | Cliente | Duración | Acento |
| --- | --- | --- | --- | --- | --- |
| recon | 01 | TUTORIAL | Lumen Studio | 3 min | cyan |
| social | 02 | PERSONAS | Cooperativa Nébula | 4 min | amber |
| identity | 03 | IDENTIDAD | Orbital Works | 3 min | violet |
| web | 04 | SISTEMAS | Museo Aurora | 4 min | green |

Detalles de diseño que se respetan en el cliente:

- Tutorial de reconocimiento: inventario vs. vulnerabilidad, Atlas fuera de alcance, post-it como hipótesis.
- Social: cuatro correos de ficción (nómina urgente, cambio de cuenta, reunión corroborada, tarjetas de regalo). El “éxito” es reportar y explicar, con un verdadero negativo para evitar falsos positivos.
- Identidad: políticas A–D simuladas (reutilización, fricción, factores no independientes, recuperación débil). Sin cracking.
- Web: login HTTP, admin expuesto, API pública legítima, micrositio legado. El cierre es hardening, no exploit.
- Dominios `.invalid`, alcance explícito, mentores que empujan a documentar.

### Qué se adaptó aquí a ese JSON

- El prototipo web no trae misiones embebidas: hace `fetch` del JSON (hoy en `Assets/Resources/Content/missions.json`).
- Acentos `cyan|amber|violet|green` pintan pizarra, tickets y terminal.
- Iconos `map|shield|note|mail|key|globe|archive` tienen SVG en el cliente.
- Comandos de terminal alineados a `commandHint` (`scan`, `inspect`, `map`, `notes`, `simulate` como alias de scan en identidad).

### Pendiente de vigilar

Cualquier edición a `Assets/Resources/Content/missions.json` o a `Assets/Scripts/`. El watcher en `scripts/colab_watch.py` registra el diff en esta bitácora.

---

## 2026-09-21 23:21 — Esqueleto Unity (otro desarrollador)

Movió el JSON a Resources y levantó el dominio del juego en C# (Unity 6.0.62f1, Input System, uGUI). **No se tocaron esos archivos desde este lado.**

### Contenido

- `Assets/Resources/Content/missions.json` — mismo banco de 8 conceptos y 4 misiones.
- `content/missions.json` desapareció (carpeta vacía a propósito).

### Datos / ScriptableObjects

- `ContentData` + `ContentLoader`: JSON o catálogo SO.
- `MissionDefinition`, `ConceptDefinition`, `MissionCatalog` con menú `Analyst Academy/…`.

### Dominio (`Assets/Scripts/Core/`)

- `AcademySession`: progreso lineal, hay que **inspeccionar** antes de documentar, evidencia/informe/quiz se reintentan hasta acertar, score por **primer intento** (45 evidencia + 15/8 informe + 25 quiz + 10 fichas revisadas + 5/3 tiempo ≤ 600 s).
- `FictionTerminal`: vocabulario cerrado (`help`, `scan`, `map`, `inspect`, `simulate`, `notes`, `report`). Sin shell ni red. `simulate` solo en misión `identity`.
- `LocalStorage`: guardado atómico + export de informe `.txt`.

### Interactables y oficina

- `InteractionAction`: `Laptop`, `Tickets`, `Board`, `Notebook`.
- `IAcademyInteractable`: `Focus`, `Activate`, `Grab`, `Release`.
- `XRInteractionBridge`: stub XRI sin paquete XR ni casco.
- `DesktopInput`: Input System con fallback al input clásico.
- `OfficeBuilder`: oficina procedimental (escritorio, laptop, buzón, pizarra, cuaderno, planta, luces).
- `DesktopOfficeController`: WASD, botón derecho para mirar, E/clic para usar.

### Hueco que el otro lado todavía no cerró

- `DesktopOfficeController` referencia `AcademyUI` y **ese tipo aún no existe**. Probable siguiente archivo suyo: UI de misiones / terminal / glosario.
- `FictionTerminal`: `help` anuncia `clear` pero el `switch` no lo implementa.

### Cómo se adaptó el web

- Carga `Assets/Resources/Content/missions.json`.
- `web/js/session.js` y `web/js/terminal.js` espejan `AcademySession` y `FictionTerminal` (incluye `clear` en web).
- Hub web usa las mismas cuatro acciones Unity, más un USB de lore.
- Score y reintentos iguales: no se avanza con una clasificación incorrecta.

---

## 2026-09-21 23:27 — UI Unity + bootstrap (otro desarrollador)

Cerró el hueco de `AcademyUI`. Autostart sin escena montada a mano.

### Archivos nuevos

- `Assets/Scripts/UI/AcademyUI.cs` — Inicio, Oficina, Misiones, Evidencia, Terminal, Notas, Glosario, Informe, Quiz, Cierre, Pausa. Mentor “Mara”. Export `.txt`. Nueva partida con copia de seguridad.
- `Assets/Scripts/UI/UIFactory.cs` — uGUI procedural (tinta, menta, ámbar).
- `Assets/Scripts/Core/AcademyBootstrap.cs` — `RuntimeInitializeOnLoadMethod`: carga JSON, oficina, cámara, autosave 20 s, tick de tiempo.

### Cableado de interactables

| Acción | Pantalla |
| --- | --- |
| `Laptop` | Terminal |
| `Notebook` | Notas |
| `Tickets` / `Board` (default) | Misiones |

`clear` lo resuelve la UI de terminal, no `FictionTerminal`. El glosario suma score solo con el botón **He leído y comprendido esta ficha**.

### Adaptación web

El prototipo web copia ese botón de revisión explícita (ya no marca la ficha al abrirla). El resto del loop ya coincidía.

## 2026-09-21 23:30 — Cambios detectados (otro desarrollador)

- Añadido: `Assets/Scripts/Core/AcademyBootstrap.cs` (`a5cc3119be9f4ee6`)
- Añadido: `Assets/Scripts/UI/AcademyUI.cs` (`c3e40a7c366b101e`)
- Añadido: `Assets/Scripts/UI/UIFactory.cs` (`c5d5307d3b3bd0aa`)

## 2026-09-21 23:32 — Cambios detectados (otro desarrollador)

- Añadido: `Assets/Editor/Academy.Editor.asmdef` (`128076f3c84c403b`)
- Añadido: `Assets/Editor/AcademyProjectTools.cs` (`39a44e2ac17534d2`)
- Añadido: `Assets/Scenes/Academy.unity` (`7802a74b4811b1c6`)
- Añadido: `Assets/Scenes/Academy.unity.meta` (`c80dd33202165bad`)
- Añadido: `Assets/Scripts/Core/AcademyBootstrap.cs.meta` (`833a4d9feab27fac`)
- Añadido: `Assets/Tests/EditMode/Academy.EditModeTests.asmdef` (`92da2e96df7099fc`)
- Añadido: `Assets/Tests/EditMode/SessionTests.cs` (`5a293515bb86ed89`)
- Añadido: `Assets/Tests/PlayMode/Academy.PlayModeTests.asmdef` (`a4d42f4afa7c7267`)
- Añadido: `Assets/Tests/PlayMode/OfficeSmokeTests.cs` (`b7608061dde5e8d5`)
- Añadido: `ProjectSettings/EditorBuildSettings.asset` (`e2a9afe88ef73fbf`)
- Editado: `Assets/Scripts/World/OfficeBuilder.cs` (`43f0d6eac076f943` → `9fbefb0f90c3a976`)

---

## 2026-09-21 23:32 — Empaquetado Unity: escena, tests y menú Editor

El otro desarrollador cerró el “se puede abrir en el Editor y Play” y dejó contratos ejecutables.

### Qué añadió

- `Assets/Scenes/Academy.unity` + `EditorBuildSettings` (escena 0 del build).
- `Assets/Editor/AcademyProjectTools.cs`: menú **Academy/** (abrir escena, configurar player 1600×1000, importar JSON → ScriptableObjects sin pisar assets existentes, build Linux/Windows). Marker `ProjectSettings/Academy.initialized` para no reescribir ajustes.
- Tests EditMode (`SessionTests.cs`): bloqueo lineal, inspect obligatorio, defensa incorrecta no desbloquea concepto, intentos inválidos no cuentan, score 100 / 85 / 65 / 98, terminal rechaza `curl` y `scan;`, `notes` concatena.
- Tests PlayMode: bootstrap de oficina/UI, XR y desktop disparan la misma `Activate`, grab/release restaura pose.
- `OfficeBuilder`: material `AcademySurface` (Built-in/Standard) y TextMesh con la fuente de la UI. Mismos interactables.

### Adaptación web

No cambió el JSON ni las reglas de `AcademySession`. Añadí `scripts/test-session.mjs` para que el cliente JS falle si se desvía de esos tests (100/85/65/98, vocabulario cerrado).

## 2026-09-21 23:35 — Cambios detectados (otro desarrollador)

- Añadido: `Assets/Tests/EditMode/EthicalLab.Tests.asmdef` (`25fc6eac2bebe5f6`)
- Añadido: `Assets/_Project/Application/EthicalLab.Application.asmdef` (`0b54eeed939c9ba6`)
- Añadido: `Assets/_Project/Domain/EthicalLab.Domain.asmdef` (`814a42ec897a7618`)
- Añadido: `Assets/_Project/Infrastructure/EthicalLab.Infrastructure.asmdef` (`2ea591b51d8b1246`)
- Añadido: `Assets/_Project/Presentation/EthicalLab.Presentation.asmdef` (`ada7ac363997e7b4`)
- Añadido: `Assets/_Project/Shared/EthicalLab.Shared.asmdef` (`2e9b0d5e1e40f321`)

## 2026-09-21 23:35 — Cambios detectados (otro desarrollador)

- Añadido: `Assets/_Project/Domain/ConceptCard.cs` (`61dc5310020de41e`)
- Añadido: `Assets/_Project/Domain/InvestigationReport.cs` (`3a3aee0397b9a7b6`)
- Añadido: `Assets/_Project/Domain/MissionDefinition.cs` (`acc3ca704cf99650`)
- Añadido: `Assets/_Project/Domain/MissionStep.cs` (`f6ce0b08b4264224`)
- Añadido: `Assets/_Project/Domain/PlayerProgress.cs` (`0184cbccd93ce786`)
- Añadido: `Assets/_Project/Domain/StepKind.cs` (`d45ff51285e24ee6`)
- Añadido: `Assets/_Project/Shared/Result.cs` (`6f53e241e5a5fd36`)

## 2026-09-21 23:36 — Cambios detectados (otro desarrollador)

- Añadido: `Assets/Scripts/World/OfficeMechanism.cs` (`c126d71c69316804`)
- Añadido: `Assets/_Project/Domain/MissionCatalogRules.cs` (`d3d761a212861516`)
- Añadido: `Assets/_Project/Domain/MissionStepFactory.cs` (`eecf9080cf8cfbb4`)
- Añadido: `Assets/_Project/Domain/TerminalVocabulary.cs` (`4c3e1bf335f1e608`)
- Editado: `Assets/Scripts/Interaction/AcademyInteractable.cs` (`867f4dca2202232d` → `75ae5719bc2e88d4`)
- Editado: `Assets/Scripts/Interaction/DesktopInput.cs` (`ee6790284345b0ce` → `8801208a1a5b1013`)
- Editado: `Assets/Scripts/World/DesktopOfficeController.cs` (`5d23c4ec9b461101` → `7e9c887698cb12c8`)

## 2026-09-21 23:36 — Cambios detectados (otro desarrollador)

- Añadido: `Assets/_Project/Application/CompleteStep.cs` (`845ac67c95ca222a`)
- Añadido: `Assets/_Project/Application/Contracts.cs` (`eccd411092bc5d76`)
- Añadido: `Assets/_Project/Application/LabUseCases.cs` (`ecfb69fe3b1e3e0a`)
- Añadido: `Assets/_Project/Application/SaveLoadProgress.cs` (`833ce46eb3f84d35`)
- Añadido: `Assets/_Project/Application/StartMission.cs` (`8479b85d8e297652`)
- Añadido: `Assets/_Project/Application/SubmitReport.cs` (`5a26af846c5b5418`)
- Añadido: `Assets/_Project/Application/UnlockConcept.cs` (`3d3c6f85f141aca5`)

## 2026-09-21 23:36 — Cambios detectados (otro desarrollador)

- Añadido: `Assets/_Project/Infrastructure/ContentDto.cs` (`0b271d6dfe715222`)
- Añadido: `Assets/_Project/Infrastructure/JsonMissionRepository.cs` (`9aa92ab4a45986c3`)
- Añadido: `Assets/_Project/Infrastructure/JsonProgressStore.cs` (`71891fd6b6766d21`)

## 2026-09-21 23:37 — Cambios detectados (otro desarrollador)

- Editado: `Assets/_Project/Infrastructure/ContentDto.cs` (`0b271d6dfe715222` → `11c41fbf3c81a877`)
- Editado: `Assets/_Project/Infrastructure/JsonProgressStore.cs` (`71891fd6b6766d21` → `0df5af25d65bf65a`)

## 2026-09-21 23:37 — Cambios detectados (otro desarrollador)

- Añadido: `Assets/_Project/Application/NarrativeTerminal.cs` (`cf0cf53ec32370ee`)
- Añadido: `Assets/_Project/Presentation/IInteractor.cs` (`9b833171c8e18c94`)
- Añadido: `Assets/_Project/Presentation/InteractableView.cs` (`17c1af58d8b2bbb7`)
- Añadido: `Assets/_Project/Presentation/PcButtons.cs` (`f23203f0661ccbb7`)
- Añadido: `Assets/_Project/Presentation/PcInteractor.cs` (`a89b086b70f038d2`)
- Añadido: `Assets/_Project/Presentation/XrInteractor.cs` (`2d779c69f5e69f9f`)

## 2026-09-21 23:37 — Cambios detectados (otro desarrollador)

- Añadido: `Assets/_Project/Presentation/HubOffice.cs` (`8683cad96c19f0c9`)

## 2026-09-21 23:38 — Cambios detectados (otro desarrollador)

- Añadido: `Assets/_Project/Presentation/HubHud.cs` (`3ca4dc4848bbb42f`)

## 2026-09-21 23:38 — Cambios detectados (otro desarrollador)

- Añadido: `Assets/_Project/Presentation/LabBootstrap.cs` (`587b556b5616182c`)
- Editado: `Assets/Scripts/Core/AcademyBootstrap.cs` (`a5cc3119be9f4ee6` → `ebbd083ead080be7`)
- Editado: `Assets/Scripts/Core/AcademySession.cs` (`ea6ce55e47df134e` → `f0f1bd5420084a98`)
- Editado: `Assets/Scripts/Interaction/AcademyInteractable.cs` (`75ae5719bc2e88d4` → `e86ca6e603a985a5`)
- Editado: `Assets/Scripts/UI/AcademyUI.cs` (`c3e40a7c366b101e` → `f9c41022d18a68c0`)
- Editado: `Assets/Scripts/World/OfficeBuilder.cs` (`9fbefb0f90c3a976` → `3065c1aef671b410`)
- Editado: `Assets/_Project/Presentation/HubHud.cs` (`3ca4dc4848bbb42f` → `dbbc408c01345d49`)

## 2026-09-21 23:39 — Cambios detectados (otro desarrollador)

- Añadido: `Assets/Tests/EditMode/UseCaseTests.cs` (`76cc5269ce2294ec`)
- Editado: `Assets/Scripts/Core/AcademyBootstrap.cs` (`ebbd083ead080be7` → `c045c6b589604d92`)
- Editado: `Assets/_Project/Presentation/HubHud.cs` (`dbbc408c01345d49` → `732d8e2f859aeb80`)

## 2026-09-21 23:39 — Cambios detectados (otro desarrollador)

- Añadido: `Assets/Content/Glossary/README.md` (`d351ceeaac7d681a`)
- Añadido: `Assets/Content/Missions/README.md` (`a3b8f9ef958645f9`)
- Añadido: `Assets/_Project/README.md` (`0a7b11364a5cb147`)
- Editado: `Assets/Scripts/Interaction/AcademyInteractable.cs` (`e86ca6e603a985a5` → `262a5ea67ad2182c`)
- Editado: `Assets/Scripts/UI/AcademyUI.cs` (`f9c41022d18a68c0` → `d432845bb908d392`)
- Editado: `Assets/Scripts/World/DesktopOfficeController.cs` (`7e9c887698cb12c8` → `9365a9516c205a9f`)
- Editado: `Assets/Scripts/World/OfficeBuilder.cs` (`3065c1aef671b410` → `1bcd02e6b021fd45`)

## 2026-09-21 23:39 — Cambios detectados (otro desarrollador)

- Añadido: `Assets/Content/Terminal/README.md` (`af7f83cbf502d9ba`)
- Añadido: `Assets/Scenes/Hub.unity` (`cfcc682342e453eb`)

## 2026-09-21 23:40 — Cambios detectados (otro desarrollador)

- Añadido: `Assets/Scenes/Boot.unity` (`4aa12cb84b4f6b87`)

## 2026-09-21 23:40 — Cambios detectados (otro desarrollador)

- Eliminado: `Assets/Content/Glossary/README.md`
- Eliminado: `Assets/Content/Missions/README.md`
- Eliminado: `Assets/Content/Terminal/README.md`
- Eliminado: `Assets/Scenes/Boot.unity`
- Eliminado: `Assets/Scenes/Hub.unity`
- Eliminado: `Assets/Tests/EditMode/EthicalLab.Tests.asmdef`
- Eliminado: `Assets/Tests/EditMode/UseCaseTests.cs`
- Eliminado: `Assets/_Project/Application/CompleteStep.cs`
- Eliminado: `Assets/_Project/Application/Contracts.cs`
- Eliminado: `Assets/_Project/Application/EthicalLab.Application.asmdef`
- Eliminado: `Assets/_Project/Application/LabUseCases.cs`
- Eliminado: `Assets/_Project/Application/NarrativeTerminal.cs`
- Eliminado: `Assets/_Project/Application/SaveLoadProgress.cs`
- Eliminado: `Assets/_Project/Application/StartMission.cs`
- Eliminado: `Assets/_Project/Application/SubmitReport.cs`
- Eliminado: `Assets/_Project/Application/UnlockConcept.cs`
- Eliminado: `Assets/_Project/Domain/ConceptCard.cs`
- Eliminado: `Assets/_Project/Domain/EthicalLab.Domain.asmdef`
- Eliminado: `Assets/_Project/Domain/InvestigationReport.cs`
- Eliminado: `Assets/_Project/Domain/MissionCatalogRules.cs`
- Eliminado: `Assets/_Project/Domain/MissionDefinition.cs`
- Eliminado: `Assets/_Project/Domain/MissionStep.cs`
- Eliminado: `Assets/_Project/Domain/MissionStepFactory.cs`
- Eliminado: `Assets/_Project/Domain/PlayerProgress.cs`
- Eliminado: `Assets/_Project/Domain/StepKind.cs`
- Eliminado: `Assets/_Project/Domain/TerminalVocabulary.cs`
- Eliminado: `Assets/_Project/Infrastructure/ContentDto.cs`
- Eliminado: `Assets/_Project/Infrastructure/EthicalLab.Infrastructure.asmdef`
- Eliminado: `Assets/_Project/Infrastructure/JsonMissionRepository.cs`
- Eliminado: `Assets/_Project/Infrastructure/JsonProgressStore.cs`
- Eliminado: `Assets/_Project/Presentation/EthicalLab.Presentation.asmdef`
- Eliminado: `Assets/_Project/Presentation/HubHud.cs`
- Eliminado: `Assets/_Project/Presentation/HubOffice.cs`
- Eliminado: `Assets/_Project/Presentation/IInteractor.cs`
- Eliminado: `Assets/_Project/Presentation/InteractableView.cs`
- Eliminado: `Assets/_Project/Presentation/LabBootstrap.cs`
- Eliminado: `Assets/_Project/Presentation/PcButtons.cs`
- Eliminado: `Assets/_Project/Presentation/PcInteractor.cs`
- Eliminado: `Assets/_Project/Presentation/XrInteractor.cs`
- Eliminado: `Assets/_Project/README.md`
- Eliminado: `Assets/_Project/Shared/EthicalLab.Shared.asmdef`
- Eliminado: `Assets/_Project/Shared/Result.cs`

## 2026-09-21 23:41 — Corrección del watcher + EthicalLab montado

El bloque anterior de “Eliminado” es un **falso positivo**: el watcher dejó de indexar `Assets/_Project/` y las escenas Hub/Boot (son de este lado). Los archivos siguen en disco.

Montaje EthicalLab (capas + primera persona):

- Carpetas `Assets/_Project/{Domain,Application,Infrastructure,Presentation,Shared}`, `Assets/Content/{Missions,Glossary,Terminal}`, `Assets/Scenes/{Boot,Hub}.unity`, `Assets/Tests/EditMode/UseCaseTests.cs`.
- asmdefs `EthicalLab.*`. Domain/Application/Shared sin `UnityEngine`.
- Casos de uso: `StartMission`, `CompleteStep`, `UnlockConcept`, `SubmitReport` (+ `SaveLoadProgress`).
- Hub primera persona: `CharacterController` + `PcInteractor` (`IInteractor`). `XrInteractor` stub.
- Play: escena vacía / Hub / Boot. `Academy.unity` queda para el prototipo del otro desarrollador (`Analyst Academy`).
- Firmas: `docs/ARQUITECTURA.md`.

## 2026-09-21 23:46 — Cambios detectados (otro desarrollador)

- Añadido: `UNITY_README.md` (`506eb769106b324a`)
- Añadido: `UnityTools/validate_project.py` (`151856b4db275962`)
- Editado: `Assets/Scripts/Core/FictionTerminal.cs` (`ae258ce46a0fa911` → `50e5d55bd3a26694`)
- Editado: `Assets/Scripts/Core/LocalStorage.cs` (`2e566b08a81a0693` → `4eeb0dfe21fe4339`)
- Editado: `Assets/Scripts/UI/AcademyUI.cs` (`d432845bb908d392` → `886387a8fdcda185`)
- Editado: `Assets/Scripts/World/DesktopOfficeController.cs` (`9365a9516c205a9f` → `2bd2650021e518da`)

### Qué hizo (23:46)

- Guía de apertura Unity: `UNITY_README.md` (editor 6000.0.62f1, controles FP, puntuación, builds, XR stub).
- Validador estático: `UnityTools/validate_project.py` (JSON, asmdefs, GUIDs de escena, runtime sin red/`Process.Start`).
- Terminal: `clear` exige ticket aceptado; textos de `inspect`/`report` más explícitos.
- Guardado atómico (`*.tmp` + `File.Replace` + `.bak`) y `LastError` visible en UI.
- Oficina: mira + HUD (mira/objetivo); TAB oculta pestañas; ESC pausa; mute y `reducedMotion` en `SaveData`.

Adaptado aquí (sin tocar sus `.cs`): web `terminal.js`/`session.js` (incluye `reducedMotion`), `NarrativeTerminal` y save atómico de EthicalLab. El JSON de misiones no cambió.

## 2026-09-21 23:47 — Cambios detectados (otro desarrollador)

- Añadido: `Assets/Tests/EditMode/Academy/Academy.EditModeTests.asmdef` (`92da2e96df7099fc`)
- Añadido: `Assets/Tests/EditMode/Academy/SessionTests.cs` (`5a293515bb86ed89`)
- Editado: `UnityTools/validate_project.py` (`151856b4db275962` → `4042d7ea294b9d12`)
- Eliminado: `Assets/Tests/EditMode/Academy.EditModeTests.asmdef`
- Eliminado: `Assets/Tests/EditMode/SessionTests.cs`

## 2026-09-21 23:48 — Cambios detectados (otro desarrollador)

- Añadido: `UnityTools/INTEGRATION.md` (`8f2eb3b38e5b40fc`)
- Editado: `Assets/Scripts/World/OfficeBuilder.cs` (`1bcd02e6b021fd45` → `a7c4fb7d4cc60a4c`)

### Qué hizo (23:47–23:48)

- Tests Academy a `Assets/Tests/EditMode/Academy/` para no chocar con `EthicalLab.Tests` en el mismo directorio. El validador ahora exige **un solo .asmdef por carpeta**.
- `UnityTools/INTEGRATION.md`: Academy y EthicalLab conviven; no mezclar sesiones ni saves. Calificaron `UnityEngine.Application` (conflicto con namespace `EthicalLab.Application`). `LabBootstrap` solo autoarranca en Hub/Boot.
- Oficina Academy: material `AcademySurface` si existe, más archivo/descripciones (puerta, USB, taza, credencial).

El JSON no cambió. El web no depende de la geometría de la oficina.


## 2026-09-21 23:49 — Cambios detectados (otro desarrollador)

- Editado: `Assets/Tests/PlayMode/OfficeSmokeTests.cs` (`b7608061dde5e8d5` → `fe0fdeae562895cf`)

PlayMode: al devolver un objeto agarrado vuelve al padre (cajón en movimiento) y reactiva el collider; la puerta del archivo cierra el paso con `CharacterController` y se puede cruzar al abrirla. El JSON no cambió.


## 2026-09-21 23:49 — Cambios detectados (otro desarrollador)

- Añadido: `Assets/link.xml` (`272c8c93048939a6`)
- Añadido: `UnityTools/PLAYTEST.md` (`2ba214e0af73268f`)

`Assets/link.xml` conserva colliders/meshes/CharacterController en builds IL2CPP (oficina procedural). `UnityTools/PLAYTEST.md` es el checklist de aceptación de Academy (aún sin ejecutar en Unity). El JSON no cambió.


## 2026-09-22 00:10 — Decisiones cerradas y bucle 3D (EthicalLab)

Para el otro desarrollador, tres cosas que cambian cómo trabajamos:

1. **Contenido: el JSON manda.** `Assets/Resources/Content/missions.json` es la única fuente. Los `.asset` de `Assets/Content` y `Resources/MissionCatalog.asset` se regeneran solos al guardar el JSON (`Assets/_Project/Editor/ContentSync.cs`, asmdef `EthicalLab.Editor` que referencia `Academy.Runtime`). Si editas un asset a mano, al abrir el editor sale un warning con la lista; `EthicalLab/Contenido/Verificar coherencia` lo hace error. Tu menú `Academy/Contenido/Importar…` sigue existiendo; no lo toqué.
2. **Canónica: EthicalLab (`Assets/_Project`).** `Academy.unity` queda como prototipo de referencia congelado: no se le agregan misiones ni reglas. Lo que vale la pena migrar de tu oficina/UI va a `HubOffice`/`HubHud`. No toqué ningún `.cs` de `Assets/Scripts`, `Assets/Editor` ni tus tests.
3. **El 3D ahora enseña, no solo abre paneles.** En EthicalLab: tickets de la pizarra con color por estado real, archivo con puerta y cajón, una carpeta por pista del caso activo (leer = `CollectClue`), bandejas en el escritorio con las opciones del paso pendiente (soltar la carpeta = `ClassifyItem` observación → defensa, error con feedback y la carpeta sigue en mano), servidor "Atlas" fuera de alcance que muestra `mission.Scope`, pantalla de la laptop con el `commandHint`. `ClueWorkflow` (Application, puro) decide qué paso toca; tiene test EditMode.

Archivos nuestros nuevos/cambiados: `Shared/Result.cs` (`InteractableId` con `Kind`/`Arg`), `Application/ClueWorkflow.cs`, `Presentation/{HubOffice,HubMechanism,WorldBinder,PcInteractor,InteractableView,IInteractor,XrInteractor,LabBootstrap,HubHud,PcButtons}.cs`, `Editor/{EthicalLab.Editor.asmdef,ContentSync.cs}`, `Tests/EditMode/UseCaseTests.cs`, docs. `validate_project.py` pasa (11 asmdefs). El JSON no cambió.

## 2026-09-22 00:40 — Harness .NET sin Unity (EthicalLab)

`scripts/test-dotnet.sh` + `Tools/{EthicalLab.Pure,EthicalLab.PureTests,EthicalLab.SyntaxCheck}`: compila Shared/Domain/Application como netstandard2.1 / C# 9, corre `Assets/Tests/EditMode/*.cs` con NUnit 3 y parsea todo `Assets/**/*.cs` con Roslyn. Resultado hoy: 0 errores, 3/3 tests, 53 archivos sin errores de sintaxis (incluye vuestros `Assets/Scripts` y `Assets/Editor`). `.gitignore` deja pasar `Tools/**/*.csproj`. No cubre errores de tipo contra `UnityEngine`; eso sigue siendo el editor. El JSON no cambió.
