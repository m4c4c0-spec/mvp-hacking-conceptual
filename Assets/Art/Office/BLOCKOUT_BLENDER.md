# Blockout Blender — paso a paso

1. **Nuevo archivo** `Office_Blockout.blend` en esta carpeta.
2. **Unidades**: Scene Properties → Unit System Metric, 1 m.
3. **Importar anclas**: leer `blockout_dimensions.json` (generar con `python3 scripts/export-hub-blockout.py`).
4. Por cada anchor, Empty 1 m con nombre del JSON en posición `pos` (X,Y,Z); caja wireframe con `size` si existe.
5. **Colecciones** (ver README): Shell, Furniture, INT_Hero, MECH, ENV_Dressing.
6. **Puerta**: Empty `door_hinge` en (4, 0, -0.45); hoja 0.09 × 2.2 × 1.64 m, origen en bisagra.
7. **Cajón**: Empty `drawer_root` en (7.3, 1.1, -2.25); frente desliza +Z ~0.55 m al abrir.
8. **Validar**: cámara a 1.62 m en spawn mirando pizarra; pasillo a puerta ≥ 0.9 m ancho.
9. Export greybox FBX por colección o usar menú Unity **Bake procedural greybox** hasta tener modelos finales.

Cuando el blockout coincida con el JSON, sustituir meshes en prefab `Office_ProceduralGreybox` o reemplazar hijos bajo `Office_Art` manteniendo `HubSceneRefs`.
