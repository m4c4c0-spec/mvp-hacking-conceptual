# ELEMENTOS.md — Inventario del MVP EthicalLab

**Propósito**: Documentar cada elemento significativo del proyecto para que Marco pueda mantenerlo y extenderlo.

**Última actualización**: 2026-09-25 (STEP 3 — demo playable para cliente)

---

## 📋 Tabla de contenidos

1. [Domain (lógica pura C#)](#domain)
2. [Application (casos de uso)](#application)
3. [Infrastructure (persistencia y JSON)](#infrastructure)
4. [Presentation (Unity MonoBehaviours + UI)](#presentation)
5. [Content (datos de misiones y conceptos)](#content)
6. [Scenes (escenas Unity)](#scenes)
7. [Tests (EditMode y validación)](#tests)
8. [Scripts (automatización)](#scripts)

---

## Domain

**Ubicación**: `Assets/_Project/Domain/`  
**Asmdef**: `EthicalLab.Domain` (noEngineReferences, C# puro)  
**Propósito**: Modelos y reglas de negocio sin dependencias de Unity.

### MissionDefinition.cs
- **Clase**: `MissionDefinition`
- **Qué hace**: Representa una misión completa con su metadata (id, título, cliente, brief, alcance) y pasos.
- **Cómo extender**: Se crea desde datos JSON (ver `ContentMapper`), no manualmente.
- **Propiedades clave**:
  - `Steps`: Lista de `MissionStep` (clues, classify, quiz, report)
  - `Concepts`: IDs de conceptos que desbloquea
  - `Step(StepId)`: Busca un paso específico por ID

### MissionStep.cs
- **Clase**: `MissionStep`
- **Qué hace**: Representa un paso individual de una misión (recoger pista, clasificar, quiz, etc.)
- **Propiedades clave**:
  - `Kind`: `StepKind` enum (CollectClue, ClassifyItem, AnswerQuiz, WriteReportSection, UseTerminal)
  - `Prompt`, `Body`: Texto mostrado al jugador
  - `Options`, `CorrectIndex`: Para pasos de elección múltiple
  - `Unlocks`: Concepto que se desbloquea al completar (si aplica)

### StepKind.cs
- **Enum**: `StepKind`
- **Valores**: CollectClue, ClassifyItem, AnswerQuiz, WriteReportSection, UseTerminal
- **Propósito**: Tipo de cada paso de misión para que Presentation sepa cómo manejarlo

### MissionStepFactory.cs
- **Clase estática**: `MissionStepFactory`
- **Método principal**: `FromEvidence(evidence, reportPrompt, reportOptions, reportAnswer, quiz)`
- **Qué hace**: Convierte estructura de datos JSON (evidence, report, quiz) en lista de `MissionStep`
- **Patrón generado**:
  - Por cada evidencia: 3 pasos (`.clue`, `.observe`, `.defend`)
  - 1 paso de report
  - N pasos de quiz (`.quiz.0`, `.quiz.1`, etc.)
  - 1 paso terminal
- **Cómo extender**: Si se agrega nuevo tipo de paso, modificar aquí y en `StepKind`

### ConceptCard.cs
- **Clase**: `ConceptCard`
- **Qué hace**: Representa una ficha de glosario (definición, importancia, defensa)
- **Propiedades**: `Id`, `Name`, `Category`, `Definition`, `Importance`, `Defense`
- **Dónde se usa**: Glosario (notebook), se desbloquea al completar `.defend` de una evidencia

### PlayerProgress.cs
- **Clases**: `PlayerProgress`, `MissionProgress`, `StepProgress`
- **Qué hace**: Estado del jugador (misiones completadas, pasos completados, conceptos desbloqueados, scores)
- **Propiedades clave**:
  - `ActiveMission`: ID de misión en curso
  - `Missions`: Lista de `MissionProgress` (cada una con su estado y score)
  - `Unlocked`: Conceptos desbloqueados
  - `Reviewed`: Conceptos leídos en el cuaderno
- **Serialización**: Se guarda/carga como JSON via `JsonProgressStore`

### MissionCatalogRules.cs
- **Clase estática**: `MissionCatalogRules`
- **Qué hace**: Reglas de negocio para catálogo de misiones (validación, disponibilidad, scoring)
- **Métodos principales**:
  - `Start(missions, progress, id)`: Valida y arranca misión
  - `CompleteStep(mission, progress, stepId, choice)`: Valida respuesta de paso
  - `IsAvailable(missions, progress, id)`: Verifica si misión está desbloqueada
  - `Score(mission, progress)`: Calcula puntaje (0-100)
- **Cómo extender**: Modificar lógica de scoring o prerequisitos aquí

### TerminalVocabulary.cs
- **Clase estática**: `TerminalVocabulary`
- **Qué hace**: Define comandos ficticios permitidos en terminal narrativa
- **Comandos**: help, scan, map, inspect, simulate, notes, report, clear
- **Propósito**: Rechazar comandos reales (curl, nmap, etc.) para seguridad conceptual
- **Cómo extender**: Agregar comando a array `Commands`

### InvestigationReport.cs
- **Clase**: `InvestigationReport`
- **Qué hace**: Representa el reporte final de una misión con su elección y si fue aceptado
- **Propiedades**: `Mission`, `Prompt`, `Options`, `CorrectIndex`, `SelectedIndex`, `Accepted`

---

## Application

**Ubicación**: `Assets/_Project/Application/`  
**Asmdef**: `EthicalLab.Application` (depende de Domain + Shared)  
**Propósito**: Casos de uso que orquestan lógica de Domain.

### LabUseCases.cs
- **Clase**: `LabUseCases`
- **Qué hace**: Fachada principal que agrupa todos los casos de uso y el estado
- **Propiedades**:
  - `Catalog`: `IMissionRepository` (misiones y conceptos)
  - `Progress`: `PlayerProgress` (estado del jugador)
  - `StartMission`, `CompleteStep`, `UnlockConcept`, `SubmitReport`, `SaveLoad`: Casos de uso
  - `Active`: Misión actualmente en curso
- **Método**: `Tick(seconds)`: Actualiza tiempo de misión activa
- **Cómo usar**: Todas las interacciones del jugador pasan por estos casos de uso

### StartMission.cs
- **Clase**: `StartMission`
- **Método**: `Execute(MissionId)`
- **Qué hace**: Valida prerequisitos y marca misión como iniciada en `PlayerProgress`
- **Retorna**: `Result` (Ok o error con mensaje)

### CompleteStep.cs
- **Clase**: `CompleteStep`
- **Método**: `Execute(StepId, StepAnswer)`
- **Qué hace**: Valida respuesta de paso, actualiza progreso, desbloquea conceptos
- **Flujo**: Llama `MissionCatalogRules.CompleteStep` para validación
- **Retorna**: `Result`

### UnlockConcept.cs
- **Clase**: `UnlockConcept`
- **Método**: `Execute(ConceptId)`
- **Qué hace**: Marca concepto como leído (reviewed) en el cuaderno
- **Nota**: Conceptos se desbloquean automáticamente al completar pasos `.defend`, esto solo marca como "leído"

### SubmitReport.cs
- **Clase**: `SubmitReport`
- **Métodos**:
  - `Execute(conclusionIndex)`: Registra elección de reporte
  - `CloseIfReady()`: Cierra misión si report + todos los quiz están completos, calcula score
- **Scoring**: Llama `MissionCatalogRules.Score` para calcular 0-100

### SaveLoadProgress.cs
- **Clase**: `SaveLoadProgress`
- **Métodos**: `Load()`, `Save()`
- **Qué hace**: Persiste `PlayerProgress` via `IProgressStore`
- **Uso**: Se llama automáticamente cada 20s en `LabBootstrap.Update()` y al pausar/cerrar

### ClueWorkflow.cs
- **Clase estática**: `ClueWorkflow`
- **Propósito**: Lógica específica del flujo físico 3D de carpetas (clues)
- **Métodos**:
  - `ClueGroups(mission)`: Lista de grupos de evidencia (ej: `["lumen", "scope", "archive"]`)
  - `Collected(mission, progress, group)`: ¿Se leyó la pista?
  - `Documented(mission, progress, group)`: ¿Se completaron observe + defend?
  - `Pending(mission, progress, group)`: Siguiente paso sin completar (observe o defend)
  - `Clue(mission, group)`: Paso `.clue` del grupo
- **Cómo se usa**: `WorldBinder` pinta carpetas según este estado; `LabBootstrap` traduce interacciones físicas

### NarrativeTerminal.cs
- **Clase estática**: `NarrativeTerminal`
- **Método**: `Execute(LabUseCases, command)`
- **Qué hace**: Procesa comandos ficticios de terminal, rechaza herramientas reales
- **Retorna**: String con output ficticio (ej: "scan → found 3 assets")
- **Cómo extender**: Agregar casos para nuevos comandos en vocabulario

### Contracts.cs
- **Interfaces**: `IMissionRepository`, `IProgressStore`
- **Propósito**: Contratos de persistencia para inversión de dependencias
- **Implementaciones**: `JsonMissionRepository`, `JsonProgressStore` (Infrastructure)

---

## Infrastructure

**Ubicación**: `Assets/_Project/Infrastructure/`  
**Asmdef**: `EthicalLab.Infrastructure` (depende de Domain + Application + UnityEngine)  
**Propósito**: Persistencia, carga de JSON, ScriptableObjects.

### JsonMissionRepository.cs
- **Clase**: `JsonMissionRepository : IMissionRepository`
- **Qué hace**: Carga missions.json y lo convierte a modelos de Domain
- **Método estático**: `FromResources()`: Carga desde `Resources/Content/missions`
- **Dependencias**: `ContentMapper` para convertir DTOs → Domain models

### JsonProgressStore.cs
- **Clase**: `JsonProgressStore : IProgressStore`
- **Qué hace**: Guarda/carga `PlayerProgress` como JSON en `persistentDataPath`
- **Path**: `{persistentDataPath}/ethicallab-progress-v1.json`
- **Formato**: JSON plano con misiones, steps, unlocked, reviewed

### ContentDto.cs
- **DTOs**: `ContentDto`, `MissionDto`, `ConceptDto`, `EvidenceDto`, `ReportDto`, `QuizDto`
- **Qué hace**: Estructuras serializables para `JsonUtility` (Unity)
- **Clase**: `ContentMapper`
  - `ToConcept(ConceptDto)`: DTO → `ConceptCard`
  - `ToMission(MissionDto)`: DTO → `MissionDefinition` (usa `MissionStepFactory`)
- **Cómo extender**: Si se agrega campo al JSON, agregarlo al DTO y mapper

---

## Presentation

**Ubicación**: `Assets/_Project/Presentation/`  
**Asmdef**: `EthicalLab.Presentation` (depende de Application + UnityEngine)  
**Propósito**: MonoBehaviours, UI, input, construcción de mundo 3D.

### LabBootstrap.cs
- **Clase**: `LabBootstrap : MonoBehaviour`
- **Qué hace**: Punto de entrada principal, arranca automáticamente en Boot/Hub
- **Método estático**: `AutoStart()`: `[RuntimeInitializeOnLoadMethod]` crea instancia en Play si detecta escenas Boot/Hub y no existe "Analyst Academy"
- **Awake**:
  - Crea `LabUseCases` con repositorios
  - Llama `HubOffice.Build()` para crear oficina 3D
  - Crea `HubHud` (UI) y `WorldBinder` (sincroniza estado → objetos 3D)
  - Conecta eventos de `PcInteractor` (Used, Grabbed, Released) a casos de uso
- **Métodos de interacción**:
  - `OnUsed(InteractableId)`: Traduce E/click en objeto → caso de uso (ej: ticket → `StartMission`)
  - `OnGrabbed(InteractableId)`: Tomar carpeta (G) → leer pista si no leída
  - `OnReleased(InteractableId)`: Soltar carpeta → ocultar panel de lectura
- **Métodos de flujo**:
  - `AcceptTicket(missionId)`: `StartMission`, toast con alcance
  - `ReadClue(group)`: `CompleteStep` para `.clue`, muestra panel de lectura
  - `Classify(trayArg)`: Con carpeta en mano, `CompleteStep` para `.observe`/`.defend`
  - `OutOfScope()`: Mensaje educativo al interactuar con servidor Atlas
- **Update**: Autosave cada 20s
- **Cómo extender**: Agregar nuevos `InteractableId.Kind` y sus handlers en `OnUsed`

### HubOffice.cs
- **Clase estática**: `HubOffice`
- **Método principal**: `Build()`: Crea oficina 3D proceduralmente (sin assets externos)
- **Qué crea**:
  - **Sala principal** (x: -4..4): Escritorio, laptop, cuaderno, bandejas clasificación, pizarra tickets
  - **Archivo** (x: 4..8): Mesa con carpetas (pistas), puerta con bisagra
  - **Cajón**: Carpeta adicional (USB de utilería)
  - **Servidor Atlas**: Fuera de alcance (educativo)
  - **Jugador**: CharacterController + cámara first-person + `PcInteractor`
  - **Iluminación**: Luz direccional + point light en archivo + fog
- **Devuelve**: `HubScene` (referencias a objetos interactivos)
- **Primitivas**: Todo con `GameObject.CreatePrimitive` y materiales procedurales
- **Cómo extender**: Modificar geometría/posiciones aquí; para nuevos objetos, agregar a `HubScene` y `WorldBinder`

### HubScene.cs
- **Clase**: `HubScene`
- **Qué es**: DTO con referencias a todos los objetos interactivos del mundo
- **Propiedades**:
  - `Camera`, `Person` (PcInteractor), `Root`
  - `LaptopScreen`, `BoardTitle`, `TrayHeader`: TextMeshes para estado
  - `Tickets`, `Folders`, `Trays`: Listas de `InteractableView`
  - `OutOfScope`: Servidor Atlas
  - `Door`, `Drawer`: `HubMechanism` (bisagra/deslizante)

### PcInteractor.cs
- **Clase**: `PcInteractor : MonoBehaviour, IInteractor`
- **Qué hace**: Controlador first-person para PC (WASD, mouse, E, G)
- **Eventos**: `Used`, `Grabbed`, `Released`, `Hovered` (implementa `IInteractor`)
- **Input**:
  - WASD: movimiento (via `CharacterController`)
  - Mouse look: yaw/pitch (botón derecho presionado para lockear cursor)
  - E / Click izquierdo: Usar objeto enfocado
  - G: Tomar/soltar objeto grabbable
  - Shift: Correr
- **Estado**:
  - `MenuOpen`: Si es true, cursor visible y no procesa input físico
  - `Held`: `InteractableView` en mano (anclado a `hand` Transform)
  - `Prompt`: Texto mostrado en HUD
- **Raycasting**: 3m de alcance para detectar `InteractableView`
- **Cómo extender**: Ver `PcButtons` para agregar teclas

### IInteractor.cs
- **Interface**: `IInteractor`
- **Propósito**: Abstracción para PC + futuro VR (XrInteractor)
- **Eventos**: `Used`, `Grabbed`, `Released`, `Hovered`
- **Por qué**: `LabBootstrap` solo depende de `IInteractor`, no de `PcInteractor` específicamente

### XrInteractor.cs
- **Clase**: `XrInteractor : IInteractor` (stub)
- **Estado**: Solo esqueleto, no implementado (VR es opcional)
- **Propósito**: Placeholder para futura integración VR sin cambiar Domain/Application

### WorldBinder.cs
- **Clase**: `WorldBinder : MonoBehaviour`
- **Qué hace**: Pinta estado de Application/Domain sobre objetos 3D (read-only)
- **LateUpdate** llama:
  - `PaintTickets()`: Color/texto de tickets según `PlayerProgress` (verde=completo, cyan=en curso, amber=disponible, gris=bloqueado)
  - `PaintLaptop()`: Texto de pantalla con `CommandHint` de misión activa
  - `PaintFolders()`: Visibilidad, color, label de carpetas según `ClueWorkflow`
  - `PaintTrays()`: Visibilidad y opciones de bandejas según paso pendiente (observe/defend)
- **Colores**: `HubOffice.Navy`, `.Mint`, `.Amber`, `.Green`, `.Muted`, `.Red`
- **Cómo extender**: Para nuevos objetos dinámicos, agregar método `Paint<Objeto>()`

### HubHud.cs
- **Clase**: `HubHud : MonoBehaviour`
- **Qué hace**: UI principal (OnGUI) con tabs + panels
- **Páginas**: Office (mundo 3D), Home, Missions, Terminal, Case (expediente), Glossary
- **Métodos públicos**:
  - `Toast(text)`: Mensaje temporal en mundo 3D (feedback de acción física)
  - `Reading(text)`: Panel lateral de lectura de pista en mano
  - `Open(target)`: Abre panel específico (usado por `LabBootstrap`)
- **Páginas**:
  - **Office**: Vista first-person, crosshair, prompt, toast, reading panel
  - **Home**: Bienvenida, contador de misiones
  - **Missions**: Buzón de tickets (paralelo a pizarra 3D, útil para testing sin caminar)
  - **Terminal**: Terminal narrativa con input de comandos
  - **Case**: Expediente de misión activa con pasos y quiz
  - **Glossary**: Fichas de conceptos desbloqueadas
- **Nota**: Experiencia principal es Office (3D), otros panels son secundarios/menú (ESC)

### InteractableView.cs
- **Clase**: `InteractableView : MonoBehaviour`
- **Qué hace**: Componente para objetos interactivos (tickets, carpetas, bandejas, etc.)
- **Propiedades**:
  - `id`: String raw (ej: "ticket:recon", "clue:lumen")
  - `prompt`: Texto mostrado en HUD al enfocar
  - `grabbable`: Si se puede tomar con G
  - `sign`: `TextMesh` opcional para label en objeto
- **Métodos**:
  - `Focus(on)`: Highlight al enfocar (color más claro)
  - `Tint(color)`: Cambia color base
  - `Label(text)`: Actualiza sign
  - `Grab(hand)`, `Release()`: Ancla/desancla a mano, deshabilita colliders mientras held
- **InteractableId**: Property que parsea `id` string a struct tipado

### HubMechanism.cs
- **Clase**: `HubMechanism : MonoBehaviour`
- **Qué hace**: Animación simple para puerta (bisagra) y cajón (deslizante)
- **Enum**: `MechanismKind` (Door, Drawer)
- **Propiedades**: `kind`, `movingPart`, `openOffset`, `openAngle`
- **Método**: `Toggle()`: Alterna estado abierto/cerrado
- **Update**: Lerp smooth hacia estado target
- **Cómo extender**: Para nuevos mecanismos, agregar a enum y caso en Update

### PcButtons.cs
- **Clase estática**: `PcButtons`
- **Qué hace**: Abstracción de input con soporte para Input System (nuevo) y Input Manager (legacy)
- **Propiedades**: `Use`, `Click`, `Look`, `Escape`, `Drop`, `Sprint`, `Move`, `LookDelta`, `Pointer`
- **Compilación condicional**: `#if ENABLE_INPUT_SYSTEM` usa `Keyboard.current`/`Mouse.current`, else usa `Input.GetKey`
- **Cómo extender**: Agregar property similar para nueva tecla/acción

---

## Content

**Ubicación**: `Assets/Resources/Content/`  
**Propósito**: Datos de misiones, conceptos, vocabulario (JSON único).

### missions.json
- **Path**: `Assets/Resources/Content/missions.json`
- **Formato**: JSON con `title`, `version`, `concepts[]`, `missions[]`
- **Contiene**:
  - **8 concepts**: recon, scope, phishing, verification, passwords, mfa, surface, transport
  - **4 missions**:
    1. `recon` (Tutorial): 3 evidence, 3 quiz
    2. `social` (Personas): 4 evidence, 3 quiz
    3. `identity` (Identidad): 4 evidence, 3 quiz
    4. `web` (Sistemas): 4 evidence, 3 quiz
- **Por evidencia**: `id`, `label`, `kind`, `body`, `question`, `options[]`, `answer`, `reasonQuestion`, `reasons[]`, `reasonAnswer`, `feedback`, `concept`
- **Cómo agregar misión**: Agregar objeto a `missions[]`, regenerar ScriptableObjects si se usan (no requerido para EthicalLab)
- **Validación**: `scripts/validate-mission-flow.sh` verifica JSON válido

### Folders READMEs
- `Assets/Content/Missions/README.md`, `Glossary/README.md`, `Terminal/README.md`
- **Propósito**: Documentan que estos folders son legacy de Academy (ScriptableObjects generados), no fuente de verdad
- **Fuente única**: `missions.json`

---

## Scenes

**Ubicación**: `Assets/Scenes/`  
**Build settings**: Boot (0), Hub (1), Academy (deshabilitado)

### Boot.unity
- **Propósito**: Escena de arranque (opcional), delega a `LabBootstrap`
- **Contenido**: Vacía (solo GameObject "Boot" con Transform)
- **Ejecución**: `LabBootstrap.AutoStart()` detecta nombre "Boot" y crea mundo automáticamente
- **Cuándo usar**: Si se quiere splash screen o carga inicial antes de Hub

### Hub.unity
- **Propósito**: Escena principal del laboratorio
- **Contenido**: Vacía (solo GameObject "Hub" con Transform)
- **Ejecución**: `LabBootstrap.AutoStart()` detecta nombre "Hub" y crea oficina 3D proceduralmente
- **Experiencia**: First-person walkable office con objetos interactivos
- **Cómo jugar**: Play en Unity → WASD caminar, mouse mirar, E usar, G tomar/soltar, ESC menú

### Academy.unity
- **Estado**: Referencia congelada, no se modifica
- **Propósito**: Prototipo legacy con otro estilo (CharacterController, agachar, F inspeccionar)
- **Build**: Deshabilitado en EditorBuildSettings
- **Documentación**: Ver `UNITY_README.md` para detalles de Academy

---

## Tests

**Ubicación**: `Assets/Tests/EditMode/`  
**Asmdef**: `EthicalLab.Tests` (depende de Domain + Application + NUnit)

### UseCaseTests.cs
- **Propósito**: Pruebas de integración de flujo completo de misión sin Unity Play Mode
- **Tests**:
  1. `StartMission_CompleteSteps_UnlockConcept_SubmitReport_Scores`: Flujo completo tutorial, verifica score = 100
  2. `ClueWorkflow_DrivesPhysicalFolderThroughObserveThenDefend`: Workflow de carpetas 3D (.clue → .observe → .defend)
  3. `TerminalRejectsRealTools`: Terminal rechaza `curl`, acepta `inspect`
- **Helpers**:
  - `MemoryCatalog`: `IMissionRepository` en memoria para tests
  - `MemoryStore`: `IProgressStore` en memoria
- **Cobertura**: Domain + Application (sin Presentation/Unity)
- **Cómo ejecutar**:
  - Unity Test Runner (Window → General → Test Runner)
  - `scripts/test-dotnet.sh` (fuera de Unity, requiere .NET 8 SDK)

### Tools/EthicalLab.Pure
- **Propósito**: Proyecto .NET puro (netstandard2.1) con Shared + Domain + Application
- **Por qué**: Verifica que Domain/Application no usen APIs de Unity accidentalmente
- **Compilación**: `scripts/test-dotnet.sh` paso 1

### Tools/EthicalLab.PureTests
- **Propósito**: NUnit tests de `UseCaseTests.cs` corriendo en .NET 8 (no Unity)
- **Ejecución**: `scripts/test-dotnet.sh` paso 2

### Tools/EthicalLab.SyntaxCheck
- **Propósito**: Parsea todo `Assets/**/*.cs` con Roslyn para verificar sintaxis C# 9
- **Cobertura**: Presentation, Infrastructure, Editor (que sí usan `UnityEngine`)
- **Ejecución**: `scripts/test-dotnet.sh` paso 3

---

## Scripts

**Ubicación**: `scripts/`  
**Propósito**: Automatización de validación, tests, content sync.

### test-dotnet.sh
- **Qué hace**: Corre tests de Domain/Application fuera de Unity (.NET 8 SDK)
- **Pasos**:
  1. Compila `EthicalLab.Pure` (netstandard2.1)
  2. Ejecuta `EthicalLab.PureTests` con NUnit (3/3 tests)
  3. Valida sintaxis de todo Assets/ con Roslyn
- **Requisito**: .NET 8 SDK (`curl -fsSL https://dot.net/v1/dotnet-install.sh | bash -s -- --channel 8.0`)
- **Salida**: Errores de compilación o tests fallidos

### validate-mission-flow.sh
- **Qué hace**: Verifica que missions.json sea JSON válido y tenga estructura esperada
- **Validaciones**:
  - JSON válido (con python3)
  - Contiene `"id":"recon"` y `"id":"social"`
  - Tiene estructura `"evidence"`
- **Salida**: ✓ si todo OK, error exitoso

### validate-content.mjs
- **Qué hace**: Validación más exhaustiva de missions.json (Node.js)
- **Uso**: `node scripts/validate-content.mjs`

### test-session.mjs
- **Propósito**: Tests de sesión web (legacy de prototipo web/)
- **Estado**: Complementario, EthicalLab es la versión canónica

### test-scoring.mjs
- **Propósito**: Tests de scoring web
- **Estado**: Complementario

---

## Cómo extender

### Agregar nueva misión
1. Editar `Assets/Resources/Content/missions.json`
2. Agregar objeto a `missions[]` con estructura similar a existentes
3. Si usa nuevos conceptos, agregarlos a `concepts[]`
4. Ejecutar `scripts/validate-mission-flow.sh`
5. Play en Unity → ticket debería aparecer en pizarra

**No se requiere modificar código C# si la misión usa tipos de pasos existentes.**

### Agregar nuevo tipo de paso
1. Agregar valor a `StepKind` enum
2. Modificar `MissionStepFactory.FromEvidence` si el paso viene del JSON
3. Agregar handler en `LabBootstrap.OnUsed` o método específico
4. Agregar UI en `HubHud.DrawStep` si se necesita mostrar en panel

### Agregar nuevo objeto interactivo 3D
1. Crear en `HubOffice.Build()` con `Box()` y `Mark()`
2. Agregar referencia a `HubScene` si es dinámico
3. Si cambia con el estado del juego, agregar método `Paint<Objeto>()` en `WorldBinder`
4. Agregar handler de interacción en `LabBootstrap.OnUsed`

### Agregar nuevo comando de terminal
1. Agregar string a `TerminalVocabulary.Commands`
2. Agregar caso en `NarrativeTerminal.Execute`

---

## Decisiones de diseño

### ¿Por qué procedural en HubOffice.Build()?
- **Sin dependencias de assets externos**: Demo funciona sin importar paquetes 3D
- **Rápido de iterar**: Cambiar geometría es editar código, no scene
- **Entendible**: Toda la construcción en un archivo legible

### ¿Por qué escenas vacías (Boot/Hub)?
- `LabBootstrap.AutoStart()` las detecta y crea todo dinámicamente
- Permite versiones diferentes (Academy vs EthicalLab) sin conflictos de scene

### ¿Por qué Domain sin UnityEngine?
- **Testable**: Tests corren en .NET puro (más rápidos que Play Mode)
- **Portable**: Lógica reutilizable fuera de Unity (ej: servidor validación)
- **SOLID**: Dependency Inversion (Application depende de interfaces, no de MonoBehaviours)

### ¿Por qué IInteractor?
- **PC-first**: `PcInteractor` es implementación completa
- **VR opcional**: `XrInteractor` stub para futuro sin reescribir lógica
- **LabBootstrap** solo conoce `IInteractor`, no detalles de input

### ¿Por qué missions.json único?
- **Data-driven**: Agregar misión no requiere código C#
- **Versionable**: JSON en git, fácil de revisar cambios
- **Colaborativo**: Diseñador de contenido no necesita Unity abierto para editar

---

## Troubleshooting

### Error: "Falta Resources/Content/missions.json"
- **Causa**: `JsonMissionRepository.FromResources()` no encuentra el asset
- **Fix**: Verificar que `Assets/Resources/Content/missions.json` existe y está en carpeta Resources

### Tickets no aparecen en pizarra
- **Causa**: `WorldBinder` no encuentra misiones en catálogo
- **Debug**: Abrir Console, verificar errores de carga JSON
- **Fix**: Validar JSON con `scripts/validate-mission-flow.sh`

### Carpetas no son grabbables
- **Causa**: `InteractableView.grabbable` no está marcado
- **Fix**: En `HubOffice.Build()`, todas las carpetas usan `Mark(folder, id, prompt, true)` donde `true` = grabbable

### Tests fallan en .NET pero pasan en Unity
- **Causa**: Código Domain/Application usa API de Unity accidentalmente
- **Fix**: Remover `using UnityEngine` de archivos en Domain/Application

### Laptop screen no muestra commandHint
- **Causa**: `WorldBinder.PaintLaptop()` no encuentra misión activa
- **Debug**: Verificar que `app.StartMission.Execute()` retorna Ok

---

**Fin de ELEMENTOS.md** — Este documento se actualiza con cada cambio significativo al proyecto.
