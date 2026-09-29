# Ejecutar dentro de Blender (Scripting o --python).
# Exporta cada colección cuyo nombre empieza por INT_, MECH_, ENV_ a Models/

import bpy
import os

OUT = bpy.path.abspath("//../Models/")  # ajustar si el .blend vive en Art/Office/
PREFIXES = ("INT_", "MECH_", "ENV_", "COL_")


def export_collection(name):
    coll = bpy.data.collections.get(name)
    if coll is None:
        return
    path = os.path.join(OUT, name + ".fbx")
    os.makedirs(OUT, exist_ok=True)
    objs = [o for o in coll.objects if o.type == "MESH"]
    if not objs:
        return
    bpy.ops.object.select_all(action="DESELECT")
    for o in objs:
        o.select_set(True)
    bpy.ops.export_scene.fbx(
        filepath=path,
        use_selection=True,
        apply_scale_options="FBX_SCALE_ALL",
        axis_forward="-Z",
        axis_up="Y",
    )
    print("exported", path)


for coll in bpy.data.collections:
    if any(coll.name.startswith(p) for p in PREFIXES):
        export_collection(coll.name)
