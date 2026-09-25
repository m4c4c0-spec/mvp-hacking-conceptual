# Blue / Red · Analyst Academy

Laboratorio narrativo: ganas por entender el concepto y defenderlo, no por “hackear de verdad”.

## 🎮 Cómo jugar — Demo para cliente (PC, sin VR)

### Opción A: Unity EthicalLab (canónico, first-person walkable)

**Requisitos**:
- Unity 6000.0.62f1 (o compatible)
- PC con teclado y mouse
- NO requiere headset VR ni paquetes XR

**Pasos**:
1. Abrir proyecto en Unity 6000.0.62f1
2. Play en `Assets/Scenes/Hub.unity` o `Boot.unity`
3. Caminar por la oficina y resolver el tutorial + misión 01 **físicamente en 3D**

**Controles PC (WASD first-person)**:
- **WASD**: Caminar por la oficina
- **Mouse**: Mirar alrededor (mantén **botón derecho** para lockear cursor)
- **E** o **Click izquierdo**: Usar objeto enfocado (pizarra, laptop, cuaderno, bandejas)
- **G**: Tomar/soltar carpetas (pistas físicas)
- **Shift**: Correr
- **ESC**: Menú (Terminal, Glosario, Expediente)

**Guía in-world**: en modo Oficina, una pista corta abajo-izquierda indica el siguiente paso físico (pizarra → archivo → bandejas → informe). Toast de bienvenida la primera vez (WASD / botón derecho / E / G).

**Flujo de juego 3D interactivo** (NO lectura de páginas):
1. **Pizarra**: E sobre ticket → acepta misión (brief + alcance en toast)
2. **Archivo**: Camina a través de la puerta, E para abrir
3. **Carpetas**: E para leer pista, G para tomar carpeta
4. **Bandejas del escritorio**: Con carpeta en mano, E sobre bandeja correcta → observación, luego defensa
5. **Cuaderno**: E para ver ficha desbloqueada con concepto aprendido
6. **Laptop**: E para terminal narrativa (comandos conceptuales, no reales)
7. **Panel ESC**: Informe final + quiz cuando todas las pistas están documentadas
8. **Servidor "Atlas" rojo**: Fuera de alcance (lección de autorización, sin castigo)

**Tutorial**: Misión 01 "Antes de tocar nada" (Lumen Studio, 3 pistas, 3 min)  
**Misión 01**: Misión 02 "No todo es lo que parece" (Nébula, 4 mensajes de email, 4 min)

### Opción B: Web (prototipo sin Unity, útil para pitch rápido)

**Sin instalar nada**:
```bash
python3 -m http.server 8765
```
Abrir [http://127.0.0.1:8765/web/](http://127.0.0.1:8765/web/)

**Nota**: Prototipo web es funcional pero más simple (lectura de páginas). La versión Unity EthicalLab es la experiencia canónica para el cliente (walkable 3D).

### ⚠️ Academy.unity (NO usar para demo cliente)

`Assets/Scenes/Academy.unity` es un prototipo congelado de referencia. Ver `UNITY_README.md` si necesitas explorarlo, pero **NO** es la versión para mostrar al cliente.

**Verificar sin Unity (desarrollo)**: 
- `scripts/test-dotnet.sh`: Compila Shared/Domain/Application (netstandard2.1, C# 9), corre tests EditMode con NUnit (3/3), verifica sintaxis de `Assets/**/*.cs`. Requiere .NET 8 SDK.
- `scripts/validate-mission-flow.sh`: Verifica que missions.json sea válido y tenga estructura esperada.
- Complementan: `python3 UnityTools/validate_project.py` y `node scripts/test-session.mjs`

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
