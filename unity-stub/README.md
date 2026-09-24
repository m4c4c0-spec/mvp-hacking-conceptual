# Interactables (web ↔ Unity)

La implementación canónica está en el proyecto Unity:

- `Assets/Scripts/Interaction/AcademyInteractable.cs` — `Focus`, `Activate`, `Grab`, `Release`
- `Assets/Scripts/Interaction/XRInteractionBridge.cs` — hover/select/activate hacia las mismas acciones
- `Assets/Scripts/World/OfficeBuilder.cs` — escritorio procedimental
- `Assets/Scripts/World/DesktopOfficeController.cs` — WASD + clic / E

## Acciones compartidas (`InteractionAction`)

| Unity | Web `data-id` | Efecto |
| --- | --- | --- |
| `Laptop` | `laptop` | Abrir terminal / laboratorio |
| `Tickets` | `mailbox`, `ticket:<id>` | Elegir misión |
| `Board` | `board` | Pizarra de progreso |
| `Notebook` | `notes` | Notas y glosario |

En PC el clic llama `use`. En Quest, XRI llamará `Grab` / `Activate` sobre el mismo componente. No hace falta casco para el demo de escritorio.
