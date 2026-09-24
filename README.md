# Blue / Red · Analyst Academy

Laboratorio narrativo: ganas por entender el concepto y defenderlo, no por “hackear de verdad”.

## Cómo jugar

**Web (sin Unity):** `python3 -m http.server 8765` → [http://127.0.0.1:8765/web/](http://127.0.0.1:8765/web/)

**Unity, EthicalLab (canónica, capas):** Play en `Assets/Scenes/Hub.unity` o `Boot.unity`. WASD, Shift, botón derecho mirar, E usar, G tomar/devolver, ESC menú. El caso se resuelve en el mundo: pizarra (ticket) → archivo (carpetas = pistas) → bandejas del escritorio (observación, luego defensa) → cuaderno (ficha) → informe y quiz en el panel. El servidor "Atlas" del rincón está fuera de alcance a propósito.

**Unity 6, prototipo Academy (referencia congelada):** abre `UNITY_README.md`. Play en `Assets/Scenes/Academy.unity` (CharacterController, agachar, G tomar, F inspeccionar, TAB cursor). No se le agregan misiones ni reglas; es donante de detalle 3D/UI hacia EthicalLab.

**Verificar sin Unity:** `scripts/test-dotnet.sh` compila Shared/Domain/Application (netstandard2.1, C# 9), corre los tests EditMode con NUnit y revisa la sintaxis de todo `Assets/**/*.cs`. Necesita .NET 8 SDK (`curl -fsSL https://dot.net/v1/dotnet-install.sh | bash -s -- --channel 8.0`). Complementa a `python3 UnityTools/validate_project.py` y `node scripts/test-session.mjs`.

**Contenido:** `Assets/Resources/Content/missions.json` es la única fuente. Los ScriptableObjects de Academy se regeneran desde el JSON (menú `EthicalLab/Contenido`). Ver `docs/CONTRATO_CONTENIDO.md` y `docs/ARQUITECTURA.md` → "Decisiones cerradas".

## Arquitectura EthicalLab

`docs/ARQUITECTURA.md` — Domain puro, Application (StartMission / CompleteStep / UnlockConcept / SubmitReport), Infrastructure (JSON), Presentation (`PcInteractor` = `IInteractor`, `XrInteractor` stub).

Misión nueva = datos en `Assets/Resources/Content/missions.json`. Si hay que tocar C# para añadir una misión, se rompió el diseño.

## Qué no entra

Exploits, Kali, netcode, VR obligatorio. Terminal narrativa: `scan`, `inspect`, `notes`, `report`.
