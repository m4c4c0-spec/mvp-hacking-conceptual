# Anclas del hub — prefab ↔ InteractableId

Fuente de medidas procedural: [`HubOffice.Build()`](../Assets/_Project/Presentation/HubOffice.cs).  
JSON exportable: [`Assets/Art/Office/blockout_dimensions.json`](../Assets/Art/Office/blockout_dimensions.json).

## Convención de nombres (Blender / Unity)

| Prefijo | Rol |
| --- | --- |
| `INT_` | Lleva `InteractableView` (id en prefab o pintado por `WorldBinder`) |
| `MECH_` | `HubMechanism` (puerta, cajón) |
| `COL_` | Solo collider estático |
| `SIGN_` | `TextMesh` o canvas world-space (opcional) |
| `ENV_` | Decoración sin interact |

## Tabla de interactables

| Objeto Unity / prefab | `InteractableId` | `grabbable` | Collider | Prompt base |
| --- | --- | --- | --- | --- |
| `INT_laptop` (+ pantalla) | `laptop` | no | Box | LAPTOP · E para terminal narrativa |
| `INT_notebook` | `notebook` | no | Box | CUADERNO · E para glosario |
| `INT_board` | `board` | no | Box | PIZARRA DE TICKETS |
| `INT_ticket_0..3` | `ticket:{missionId}` | no | Box | Pintado por `WorldBinder` |
| `INT_tray_0..2` | `tray:0`, `tray:1`, `tray:2` | no | Box | Opción de clasificación |
| `INT_report_inbox` | `report` | no | Box | BUZÓN DE INFORME |
| `INT_clue_folder_0..2` | `clue:{group}` | sí | Box | Carpeta de pista activa |
| `INT_clue_usb` | `clue:{group}` | sí | Box | USB en cajón |
| `INT_atlas_rack` | `scope` | no | Box | SERVIDOR AJENO |
| `MECH_door` (bisagra en hinge) | `door` | no | Box en hoja | PUERTA DEL ARCHIVO |
| `MECH_drawer` | `drawer` | no | Box en frente | CAJÓN |

## TextMeshes dinámicos (`HubScene`)

| Campo | Ubicación aprox. (m, espacio mundo) | Quién escribe |
| --- | --- | --- |
| `LaptopScreen` | (-0.25, 1.3, 0.64) | `WorldBinder.PaintLaptop` |
| `BoardTitle` | (0.9, 2.85, 3.07) | fijo o `PaintBoard` |
| `BoardObjective` | (0.9, 1.42, 3.07) | `WorldBinder` |
| `TrayHeader` | (1.0, 1.02, 1.02) | `WorldBinder.PaintTrays` |
| `ClueChecklist` | (-2.55, 2.45, 3.07) | `WorldBinder.PaintChecklist` |

## Mecanismos

| Componente | `kind` | `movingPart` | Notas |
| --- | --- | --- | --- |
| Puerta archivo | `Door` | Transform bisagra en x=4, z=-0.45 | `openAngle` ≈ -100° |
| Cajón | `Drawer` | Root del cajón en (7.3, 1.1, -2.25) | `openOffset` z ≈ 0.55 |

## Spawn jugador

- Posición: `(0, 0.08, -2.85)`
- `CharacterController`: height 1.8, radius 0.28, center (0, 0.9, 0)

## Integración en escena

1. Colocar prefab `Office_Art` con componente [`HubSceneRefs`](../Assets/_Project/Presentation/HubSceneRefs.cs).
2. Asignar referencias (menú **EthicalLab → Office → Bake procedural greybox prefab**).
3. `LabBootstrap` usa `HubSceneLoader` si `HubSceneRefs.IsValid`; si no, `HubOffice.Build()`.

## Orden de tickets / carpetas

- Tickets: índice 0..3 = orden de `missions.json`.
- Carpetas: índices 0..2 = primeras pistas del caso; índice 3 = USB (cuarta pista si existe).
