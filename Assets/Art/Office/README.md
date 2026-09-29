# Office art pipeline (Blender → Unity)

## Modelo disponible

`Prefabs/Office_AnalystAcademy.prefab` contiene la oficina modular completa, con mobiliario, luminarias, ventanas escénicas, archivo e interacciones. `Hub.unity` y `Hub_Art.unity` usan el mismo prefab. Los materiales persistentes están en `Materials/Generated/`.

El modelo actual se genera en Unity, sin depender de Blender. **EthicalLab → Office → Prepare modeled scene** lo reconstruye desde `HubOffice.cs` + `HubOfficeDressing.cs`. Duplica el prefab antes de hacer una variante manual si quieres conservarla al regenerar.

Capturas reales y controles: [`docs/ENTORNO_E_INICIO.md`](../../../docs/ENTORNO_E_INICIO.md). El pipeline Blender descrito a continuación queda disponible para sustituir o refinar módulos.

## Carpetas

| Carpeta | Contenido |
| --- | --- |
| `Models/` | FBX exportados desde Blender (`INT_*`, `MECH_*`, `ENV_*`) |
| `Materials/` | Materiales Standard / URP Lit |
| `Textures/` | Albedo, normal, ORM (1K / 2K) |
| `Prefabs/` | Prefabs Unity con colliders + `InteractableView` |
| `blockout_dimensions.json` | Anclas en metros (generar con `python3 scripts/export-hub-blockout.py`) |

## Blockout en Blender (Fase 0–1)

1. Importar o alinear grid 1 m.
2. Cargar `blockout_dimensions.json` como guía (Add Empty por anchor o script de import).
3. Módulos: `Office_Shell`, `Office_Furniture`, `Interactables_Hero`, `Atlas_Rack`, `SetDressing` (baja densidad).
4. Pivote puerta en bisagra `(4, 0, -0.45)`; cajón en `(7.3, 1.1, -2.25)`.
5. Export FBX: Apply Transform, Y-up, scale 1.0.

Plantilla de colecciones Blender (crear manualmente):

```
Office_Blockout/
  Shell/
  Furniture/
  INT_Hero/
  MECH/
  ENV_Dressing/
```

Ver [`docs/ART_BIBLE.md`](../../../docs/ART_BIBLE.md) y [`docs/HUB_ANCHORS.md`](../../../docs/HUB_ANCHORS.md).

## Producción PBR (Fase 2)

Ver [`PBR_CHECKLIST.md`](PBR_CHECKLIST.md) y [`EXPORT_FBX.md`](EXPORT_FBX.md).

## Unity

- Greybox procedural empaquetado: menú **EthicalLab → Office → Bake procedural greybox prefab**.
- Escena art: `Assets/Scenes/Hub_Art.unity` (referencias `HubSceneRefs`).
- Runtime: `LabBootstrap` prioriza `HubSceneRefs` en escena; fallback `HubOffice.Build()`.
