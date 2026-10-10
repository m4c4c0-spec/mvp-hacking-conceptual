# Protocolo Salida · Escape room

Escape room en una oficina bloqueada por un hacker: descubre el phishing que permitió robar una sesión, contiene el incidente, recupera la identidad y limpia el controlador para escapar.

**PC sigue en primera persona para probar jugabilidad** (WASD/mouse/E/G). Cada misión recupera una fase; después activa su seguro físico en el panel junto a la pared frontal. Solo los cuatro seguros activados en orden permiten abrir la salida y ganar.

[Builds y render en Mac](docs/DEMO_MAC.md) · [Base y plan Meta Quest 3S](docs/META_QUEST_3S.md) · [Carpeta Builds](Builds/README.md)

## 🎮 Cómo jugar — Demo para cliente (PC, sin VR)

### Opción A: Unity EthicalLab (canónico, first-person walkable)

**Requisitos**:
- Unity 6000.6.3f1 (versión fijada por el proyecto)
- PC con teclado y mouse
- NO requiere headset VR ni paquetes XR

**Pasos**:
1. Abrir proyecto en Unity 6000.6.3f1
2. Abrir `Assets/Scenes/Boot.unity` y pulsar **Play**, o usar **EthicalLab → Play from Boot**.
3. Carga de oficina → bienvenida → **Entrar a la oficina** (o **Continuar**) → introducción inicial → primera persona.
4. Caminar por la oficina y resolver el tutorial + misión 01 **físicamente en 3D**. También se puede hacer Play directamente desde `Hub.unity` o `Hub_Art.unity`.

**Controles PC (WASD first-person)**:
- **WASD**: Caminar por la oficina
- **Mouse**: Mirar alrededor; el cursor se captura al entrar a la oficina y se libera con **ESC**.
- **E** o **Click izquierdo**: Usar objeto enfocado (pizarra, laptop, cuaderno, bandejas, buzón de informe)
- **G**: Tomar/soltar carpetas (pistas físicas)
- **Shift**: Correr
- **ESC**: Menú (Terminal, Glosario, Expediente)
- **H** o **F1**: Mostrar/ocultar ayuda de controles (esquina, modo Oficina)

**Ejecutable Linux**: versión actual en `Builds/Linux/AnalystAcademy.x86_64`; ejecutar `bash scripts/play-linux.sh`. Para transferirla, usar `Builds/AnalystAcademy-linux-x86_64.tar.gz`. Ver [inicio y verificación en Linux](docs/DEMO_LINUX.md). Los ejecutables se generan localmente y no se versionan en Git. Se recompila con **EthicalLab → Build → Linux first person**. El build incluye Boot → Hub, no el prototipo Academy. En Windows, usar **EthicalLab → Build → Windows first person** con el módulo correspondiente instalado.

**Mac Intel / Apple Silicon**: **EthicalLab → Build → Mac universal first person**, con el módulo Mac Build Support (Mono). Destino: `Builds/Mac/AnalystAcademy.app`; render Metal. La firma Developer ID y notarización requieren la cuenta Apple del distribuidor y no se sustituyen por una compilación local. Ver [presentación en Mac](docs/DEMO_MAC.md).

**Acabados**: la oficina incorpora materiales de superficie, biseles, antialiasing y ventanas transparentes con exterior 3D. Los albedos de roble, tejido y yeso están en `Assets/Art/Office/Resources/OfficeTextures/`.

**Comodidad y pausa**: ESC → Inicio permite ajustar sensibilidad, campo de visión (60–90°), invertir Y, volver a la entrada sin borrar el caso y guardar/salir. El movimiento tiene aceleración suave y velocidad diagonal normalizada; no hay balanceo de cámara ni salto. Al cambiar de ventana se pausa y hay que volver a Oficina o pulsar ESC para continuar. Las preferencias de cámara se conservan entre sesiones.

**Guía in-world**: en modo Oficina, una pista corta indica el siguiente paso físico (pizarra → archivo → bandejas → **buzón de informe**). Línea compacta de controles arriba-izquierda (H/F1). Toast de bienvenida la primera vez (WASD / mouse / E / G). El objeto enfocado **pulsa** (highlight) para ver qué recibe E/G. En la pizarra, zona inferior muestra el **objetivo** de la misión activa (o «Acepta un ticket»). Panel **PISTAS** en la pared: OK documentada / ○ leída / · pendiente. Beeps suaves al usar/tomar/toast/documentar; bandeja incorrecta = destello rojo + tono error; observación correcta = destello verde + beep éxito. Al cerrar el caso (informe + quiz) toast «MISIÓN CERRADA · score/100 · mira la pizarra» + beep éxito y la guía apunta de nuevo a la pizarra.

**Reiniciar demo** (pitch con cliente): ESC → Inicio → «Reiniciar demo» → confirmar «¿Seguro?» — borra progreso guardado, vuelve a Oficina y muestra de nuevo el tip de bienvenida.

**Flujo de juego 3D interactivo** (NO lectura de páginas):
1. **Pizarra**: E sobre ticket → acepta misión (brief + alcance en toast)
2. **Archivo**: Camina a través de la puerta, E para abrir
3. **Carpetas**: E para leer pista, G para tomar carpeta
4. **Bandejas del escritorio**: Con carpeta en mano, E sobre bandeja correcta → observación, luego defensa
5. **Cuaderno**: E para ver ficha desbloqueada con concepto aprendido
6. **Laptop**: E para terminal narrativa (comandos conceptuales, no reales)
7. **Buzón de informe** (escritorio): cuando todas las pistas están documentadas, E abre el Expediente (informe + quiz)
8. **Cierre**: al completar informe + quiz → vuelta a Oficina con toast de misión cerrada; pizarra para el siguiente ticket
9. **Panel ESC**: mismo Expediente / Terminal / Glosario si prefieres el menú
10. **Servidor "Atlas" rojo**: Fuera de alcance (lección de autorización, sin castigo)

**Fases**: ¿Cómo nos encerró? → Cortar el hilo → Recuperar nuestra identidad → Limpiar y abrir la salida.

### ⚠️ Academy.unity (NO usar para demo cliente)

`Assets/Scenes/Academy.unity` es un prototipo congelado de referencia. Ver `UNITY_README.md` si necesitas explorarlo, pero **NO** es la versión para mostrar al cliente.

**Entorno 3D modelado**: oficina profesional con losetas, ventanas escénicas y persianas, revestimiento de madera, sillas, estanterías, plantas, lámparas y detalles de escritorio. La puerta, el cajón, las carpetas y los equipos conservan sus interacciones. El prefab editable es `Assets/Art/Office/Prefabs/Office_AnalystAcademy.prefab`; `Hub` y `Hub_Art` comparten ese modelo. **EthicalLab → Office → Prepare modeled scene** regenera el modelo y sus materiales. Ver [modelado e inicio](docs/ENTORNO_E_INICIO.md), `docs/ART_BIBLE.md` y `docs/HUB_ANCHORS.md`.

**Verificar sin Unity (desarrollo)**: 
- `scripts/test-dotnet.sh`: Compila Shared/Domain/Application (netstandard2.1, C# 9), corre tests EditMode con NUnit (3/3), verifica sintaxis de `Assets/**/*.cs`. Requiere .NET 8 SDK.
- `scripts/validate-mission-flow.sh`: Verifica que missions.json sea válido y tenga estructura esperada.
- Complementa: `python3 UnityTools/validate_project.py`

**Contenido:** `Assets/Resources/Content/missions.json` es la **única fuente** (data-driven). Los ScriptableObjects de Academy se regeneran desde el JSON si es necesario (menú `EthicalLab/Contenido`), pero EthicalLab carga JSON directamente. Ver `docs/CONTRATO_CONTENIDO.md`, `docs/ARQUITECTURA.md`, y **`docs/ELEMENTOS.md`** (inventario completo de elementos).

## 🏗️ Arquitectura EthicalLab

**Documentación completa**:
- `docs/ARQUITECTURA.md`: Capas, decisiones, firmas de contrato
- **`docs/ELEMENTOS.md`**: Inventario de cada elemento (Domain, Application, Infrastructure, Presentation, Content, Scenes, Tests) — **lee esto para mantener/extender el proyecto**

**Capas (Clean Architecture, SOLID)**:
- **Domain** (C# puro, sin `UnityEngine`): MissionDefinition, MissionStep, ConceptCard, PlayerProgress, MissionCatalogRules
- **Application**: Casos de uso (StartMission, CompleteStep, UnlockConcept, SubmitReport, ClueWorkflow, NarrativeTerminal)
- **Infrastructure**: Persistencia (JsonMissionRepository, JsonProgressStore)
- **Presentation**: Unity MonoBehaviours (LabBootstrap, HubOffice procedural, PcInteractor, WorldBinder, HubHud, InteractableView)

**PC-first**: `PcInteractor` es first-class (WASD/mouse/E/G). `XrInteractor` es stub opcional para futuro VR.

**Agregar misión nueva** = editar `Assets/Resources/Content/missions.json`. Si hay que tocar C# para añadir una misión, se rompió el diseño.

## ⛔ Qué NO entra (hard constraints)

- **Exploits reales**: Solo conceptos educativos, no payloads/malware/herramientas Kali
- **Comandos reales de ataque**: Terminal narrativa solo acepta `scan`, `inspect`, `notes`, `report` (vocabulario ficticio)
- **VR obligatorio**: PC con teclado/mouse es versión principal, VR es capa opcional futura
- **Netcode**: Single-player offline
- **Steam/monetización**: Fuera de scope del MVP

**Validación**: `NarrativeTerminal` rechaza `curl`, `nmap`, `metasploit`, etc. Tests verifican esto.
