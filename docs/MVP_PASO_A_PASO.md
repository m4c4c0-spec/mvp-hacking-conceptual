# Cómo se construyó este repo

Orden para reproducir **EthicalLab**, el producto de PC. La escena `Assets/Scenes/Academy.unity` queda en el árbol como prototipo congelado: no es el entregable y no se le agregan misiones. Fuentes de este recorrido: `docs/COLABORACION.md`, `docs/ARQUITECTURA.md`, `docs/ENTORNO_E_INICIO.md`, `docs/CONTRATO_CONTENIDO.md`, `README.md` y el código en `Assets/_Project/`.

## 1. Entorno

El editor con el que este equipo compiló, corrió las 17 pruebas PlayMode y generó el Linux es **Unity 6000.6.3f1**. En esta máquina el comando `unity-editor` es un enlace a `/home/cocus/Unity/Hub/Editor/6000.6.3f1/Editor/Unity`. Ese enlace no está en el repo.

`ProjectSettings/ProjectVersion.txt` hoy declara `6000.6.3f1`. `README.md`, `UNITY_README.md`, `docs/ENTORNO_E_INICIO.md` y `UnityTools/validate_project.py` todavía nombran `6000.0.62f1`. El validador afirma que esa cadena está en `ProjectVersion.txt`; con el archivo actual, esa afirmación no se cumple.

Si abres el proyecto desde una **plantilla** de Unity, omite `ProjectVersion.txt`: no lo copies de la plantilla ni de otro proyecto. Lo escribe el editor al abrir la carpeta. La versión a usar es la del editor instalado (aquí, 6000.6.3f1), no un `ProjectVersion.txt` pegado a mano.

Paquetes fijados en `Packages/manifest.json`: Input System `1.20.0`, uGUI `2.6.0` (TextMesh Pro va dentro de uGUI en Unity 6), Test Framework `1.8.0`. Pipeline de render: Built-in.

Pruebas de lógica fuera del editor: SDK de **.NET 8**. El script lo dice así:

```bash
bash scripts/test-dotnet.sh
```

Si `dotnet` no está en `PATH`, el mismo script indica:

```bash
curl -fsSL https://dot.net/v1/dotnet-install.sh | bash -s -- --channel 8.0
```

Eso instala el SDK en `~/.dotnet`. El script compila `Tools/EthicalLab.Pure` (netstandard2.1, C# 9), corre `Tools/EthicalLab.PureTests` y revisa la sintaxis de `Assets/**/*.cs` con `Tools/EthicalLab.SyntaxCheck`.

Abrir el proyecto y pulsar Play es una operación de editor. El camino escrito es: abrir `Assets/Scenes/Boot.unity` y pulsar Play, o el menú **EthicalLab → Play from Boot** (`HubOfficeBakeTool.PlayFromBoot`).

## 2. Contenido, antes del cliente 3D

El 2026-09-21 el banco de misiones ya existía. Hoy la única fuente es `Assets/Resources/Content/missions.json` (esquema versión 1, título «Blue / Red · Analyst Academy»).

Contenido actual:

- 8 conceptos: `recon`, `scope`, `phishing`, `verification`, `passwords`, `mfa`, `surface`, `transport`.
- 4 misiones: `recon` (3 pistas), `social` (4), `identity` (4), `web` (4). Cada una cierra con informe y 3 preguntas de quiz.

El contrato de campos está en `docs/CONTRATO_CONTENIDO.md`. Dominios de ficción en `.invalid`. No hay campos para payloads.

Comprobar el JSON con lo que el repo ya trae:

```bash
bash scripts/validate-mission-flow.sh
node scripts/validate-content.mjs
python3 UnityTools/validate_project.py
```

`validate-mission-flow.sh` exige JSON válido y la presencia de `recon`, `social` y `evidence`. `validate_project.py` exige 4 misiones, 8 conceptos alcanzables y 12 preguntas de quiz, además de ensamblados y el orden Boot → Hub.

## 3. Prototipo Academy (congelado)

El mismo 2026-09-21 quedó el esqueleto en `Assets/Scripts/` (`AcademySession`, `FictionTerminal`, `OfficeBuilder`, `AcademyUI`) y la escena `Assets/Scenes/Academy.unity`. En `EditorBuildSettings` esa escena está **deshabilitada**. El build canónico no la incluye.

`UNITY_README.md` documenta el menú **Academy → Build** y un ejemplo batch con `-executeMethod Academy.Editor.AcademyProjectTools.BuildLinux`. Ese método empaqueta el prototipo congelado. Para el producto EthicalLab se usa el menú del paso 8.

## 4. Capas

A continuación, en `Assets/_Project/`, las capas con ensamblado propio. El mapa está en `docs/ARQUITECTURA.md` y `Assets/_Project/README.md`.

| Asmdef | Carpeta | Referencias | Motor |
| --- | --- | --- | --- |
| `EthicalLab.Shared` | `Shared/` | ninguna | `noEngineReferences` |
| `EthicalLab.Domain` | `Domain/` | Shared | `noEngineReferences` |
| `EthicalLab.Application` | `Application/` | Domain, Shared | `noEngineReferences` |
| `EthicalLab.Infrastructure` | `Infrastructure/` | Domain, Application, Shared | con Unity |
| `EthicalLab.Presentation` | `Presentation/` | Domain, Application, Infrastructure, Shared, uGUI, TextMesh Pro, Input System | con Unity |
| `EthicalLab.Editor` | `Editor/` | `Academy.Runtime`, Presentation, TextMesh Pro | solo Editor |
| `EthicalLab.Tests` | `Assets/Tests/EditMode/` | Domain, Application, Shared | Editor |

Orden de dependencia: Shared → Domain → Application. Infrastructure lee `missions.json` (`JsonMissionRepository.FromResources`) y guarda `ethicallab-progress-v1.json` (`JsonProgressStore`). Presentation no mete reglas en los botones: `LabBootstrap` traduce el uso físico a `StartMission`, `CompleteStep`, `UnlockConcept` y `SubmitReport`.

`Assets/Tests/EditMode/UseCaseTests.cs` cubre el recorrido de misión, el flujo de carpetas, el rechazo de herramientas reales en la terminal, el router de páginas y el reinicio de demo. Esas cinco pruebas son las que corre el paso 2 de `scripts/test-dotnet.sh`.

Los ScriptableObjects de Academy son una vista generada. Operación de editor: **EthicalLab → Contenido → Regenerar ScriptableObjects desde JSON (sobrescribe)** y **Verificar coherencia JSON ↔ ScriptableObjects** (`ContentSync` en `Assets/_Project/Editor/ContentSync.cs`). Al guardar el JSON dentro del editor, `ContentSync` también regenera.

## 5. Escenas Boot y Hub

Build settings (`ProjectSettings/EditorBuildSettings.asset`):

1. `Assets/Scenes/Boot.unity` habilitada.
2. `Assets/Scenes/Hub.unity` habilitada.
3. `Hub_Art.unity` deshabilitada (inspección de arte).
4. `Academy.unity` deshabilitada.

`BootLoader` (`RuntimeInitializeOnLoadMethod`) solo actúa si la escena activa se llama `Boot`: muestra el estado de carga y abre `Hub`. `LabBootstrap` solo arma la oficina si la escena se llama `Hub`.

## 6. Oficina: prefab y fallback procedural

`HubSceneLoader.Resolve()` busca un `HubSceneRefs` válido. Si lo hay, usa esa instancia. Si no, llama a `HubOffice.BuildProceduralLegacy()`, que delega en `HubOffice.Build()`.

El prefab compartido es `Assets/Art/Office/Prefabs/Office_AnalystAcademy.prefab`. `Hub` y `Hub_Art` lo instancian. Los materiales persistentes quedan en `Assets/Art/Office/Materials/Generated/`.

Operaciones de editor (menús en `Assets/_Project/Editor/`):

| Menú | Método | Efecto |
| --- | --- | --- |
| EthicalLab → Office → Bake modeled office prefab | `HubOfficeBakeTool.BakePrefab` | Ejecuta `HubOffice.Build()`, guarda materiales y escribe el prefab. |
| EthicalLab → Office → Create or refresh Hub_Art scene | `HubOfficeBakeTool.CreateArtScene` | Hornea el prefab y lo coloca en `Hub_Art.unity`. |
| EthicalLab → Office → Prepare modeled scene | `HubProjectSetup.PrepareModeledScene` | Crea o refresca `Hub_Art` y vuelve a colocar el prefab en `Hub` (`RefreshPlayableHub`). |
| EthicalLab → Office → Capture modeled office preview | `HubPreviewCapture` | Exporta capturas a `docs/previews/` desde `Hub` o `Hub_Art`. |

`Prepare modeled scene` reemplaza cambios artísticos hechos a mano sobre el prefab y los materiales generados. `docs/ENTORNO_E_INICIO.md` indica duplicar el prefab antes si se quiere conservar una variante.

Medidas: 1 unidad = 1 metro, cámara a 1,62 m, techo a 3,35 m. Anclas en `docs/HUB_ANCHORS.md`. El JSON de blockout se genera con el comando que ya está en `Assets/Art/Office/README.md`:

```bash
python3 scripts/export-hub-blockout.py
```

El modelado Blender (`BLOCKOUT_BLENDER.md`, `EXPORT_FBX.md`, `PBR_CHECKLIST.md`) es el paso posterior. El prefab actual no depende de un FBX.

## 7. Interacción de primera persona

`PcInteractor` mueve con `CharacterController`: WASD y flechas, Shift para correr, mouse para mirar con tope de 80°, E o clic para usar, G para tomar o soltar. ESC abre el menú. Las preferencias de cámara van a `PlayerPrefs`.

Las sillas salen de `HubOfficeDressing`: un `BoxCollider` de volumen, `Rigidbody` con rotación X/Z congelada, e `InteractableView` con id `chair`. Al soltar, el cuerpo físico se queda en el mundo. Las carpetas de pista vuelven a su sitio.

`MovementCoach` guía caminar, mirar, correr y tomar una silla. El paso queda en `PlayerPrefs` (`EthicalLab.MovementCoachStep`). La figura del coach es geometría en `HubOfficeDressing.MovementCoachFigure`. `FirstPersonHands` cambia el agarre entre carpeta y silla.

Puerta y cajón son `HubMechanism` (bisagra y deslizamiento). El servidor Atlas usa el id `scope` y no dispara un caso de uso: muestra el alcance de la misión.

## 8. UI

Al cargar el editor, `HubProjectSetup` (`InitializeOnLoad`) importa **TMP Essential Resources** del paquete uGUI si falta `TMP Settings`. El build Linux aborta con un error explícito si esos recursos no están (`HubDesktopBuild`).

La interfaz de juego está en `HubHud`, `HubMenuUi`, `HubOfficeOverlayUi`, `HubOnboardingOverlay` y `HubStartScreen`. Páginas: Oficina, Inicio, Misiones, Terminal, Expediente, Glosario.

## 9. Build Linux

Operación de editor: **EthicalLab → Build → Linux first person**.

El método público es `EthicalLab.Editor.HubDesktopBuild.Linux`. Arma solo `Assets/Scenes/Boot.unity` y `Assets/Scenes/Hub.unity`, en ventana 1600×900, y escribe `Builds/Linux/AnalystAcademy.x86_64`. El repo no documenta un `-executeMethod` para ese método. El ejemplo batch de `UNITY_README.md` apunta al build de Academy.

Para ejecutarlo, el script del repo:

```bash
bash scripts/play-linux.sh
```

Exige el binario con permiso de ejecución, cambia a `Builds/Linux` y escribe el log en `Builds/Linux/player.log`. Hay que copiar la carpeta Linux completa. `Builds/` no es fuente: se regenera con el menú.

El menú **EthicalLab → Build → Windows first person** (`HubDesktopBuild.Windows`) pide el módulo de Windows de ese editor. En la validación del 2026-09-27 ese módulo no estaba instalado.

`docs/previews/VALIDACION.md` registra el build Linux del 2026-09-27 con 6000.6.3f1: éxito, Boot y Hub, carpeta de unos 93 MB, arranque sin excepciones en ese equipo.

## 10. Verificación

Sin editor:

```bash
bash scripts/test-dotnet.sh
bash scripts/validate-mission-flow.sh
python3 UnityTools/validate_project.py
node scripts/test-session.mjs
```

`node scripts/test-session.mjs` y `scripts/test-scoring.mjs` cubren el prototipo web, no el hub 3D.

Dentro del editor, Test Runner → PlayMode, como lista `docs/ENTORNO_E_INICIO.md`:

- `EthicalLab.Tests.HubStartupTests`
- `EthicalLab.Tests.HubGameplayTests`
- `EthicalLab.Tests.FirstPersonInputTests`

El 2026-09-27 esas baterías sumaron 17 pruebas aprobadas (`docs/previews/VALIDACION.md`). El checklist humano sigue pendiente en `UnityTools/PLAYTEST_ETHICALLAB.md`.

## Orden corto

1. Instalar Unity 6000.6.3f1 y .NET 8. Si naces de una plantilla, no copies `ProjectVersion.txt`.
2. Dejar `missions.json` como única fuente y validarlo.
3. Dejar Academy congelada y fuera del build.
4. Armar Shared → Domain → Application → Infrastructure → Presentation.
5. Escenas Boot (carga) y Hub (oficina).
6. Hornear el prefab con **EthicalLab → Office → Prepare modeled scene**, con fallback procedural si no hay referencias.
7. Primera persona, sillas físicas y coach encima de esa oficina.
8. UI uGUI/TMP; el editor importa los recursos TMP si faltan.
9. **EthicalLab → Build → Linux first person** y `bash scripts/play-linux.sh`.
10. `scripts/test-dotnet.sh`, `validate_project.py` y las tres clases PlayMode. El playtest de teclado y mouse real queda como pasada humana.
