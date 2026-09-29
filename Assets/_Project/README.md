# _Project (EthicalLab) — implementación canónica

Capas: Shared → Domain → Application → Infrastructure → Presentation. `Editor/` solo en el editor (sincroniza contenido).

- Domain y Application no referencian `UnityEngine`.
- Misión nueva = editar `Assets/Resources/Content/missions.json`. Nada más. Los ScriptableObjects de Academy se regeneran desde ahí (`EthicalLab/Contenido`).
- Primera persona: `PcInteractor` implementa `IInteractor`. `XrInteractor` es stub de la misma puerta.
- El mundo ejecuta el bucle: `WorldBinder` pinta el estado del dominio en los objetos; `LabBootstrap` traduce cada uso físico a un caso de uso; `ClueWorkflow` (puro, con test) decide qué paso toca a una carpeta.
- Play: `Hub.unity` (preferido) o `Boot.unity` (splash → Hub). WASD, Shift, botón derecho, E usar, G tomar/devolver, ESC menú.

Recorrido: pizarra (ticket) → puerta del archivo → carpeta (leer = pista) → bandeja del escritorio (observación) → bandeja (defensa, desbloquea ficha) → cuaderno → informe y quiz (ESC o laptop). "Atlas" está fuera de alcance a propósito.

Firmas y decisiones: `docs/ARQUITECTURA.md`.
