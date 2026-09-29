# Export FBX (Blender → Unity)

## Por archivo

1. Seleccionar solo el módulo a exportar (`INT_laptop`, etc.).
2. **File → Export → FBX**
   - Scale: 1.0
   - Apply Scalings: FBX All
   - Forward: -Z Forward, Up: Y Up (default Unity)
   - Apply Unit
3. Guardar en `Assets/Art/Office/Models/INT_laptop.fbx`

## Batch (opcional)

```bash
# Desde Blender headless, ajustar rutas:
# blender Office_Blockout.blend --background --python Assets/Art/Office/blender_export_batch.py
```

Script stub: [`blender_export_batch.py`](blender_export_batch.py) — lista colecciones `INT_*`, `MECH_*`, `ENV_*`.

## Import Unity

- Model: Scale Factor 1, Mesh Compression Off (MVP).
- Generate Colliders: **Off** (colliders en prefab).
- Materiales: usar `Materials/` del proyecto, no embed automático si duplica.

## Prefab

1. Arrastrar FBX a escena.
2. Añadir BoxCollider + `InteractableView` según [`docs/HUB_ANCHORS.md`](../../../docs/HUB_ANCHORS.md).
3. Guardar en `Prefabs/INT_laptop.prefab`.
4. Actualizar `HubSceneRefs` en `Office_Art` root.
