#!/usr/bin/env python3
"""Compacta la oficina Unity de su revisión de origen en un GLB por material.

No abre Unity. Lee el prefab versionado, fusiona los objetos por acabado y
omite personajes (en Godot ya son guías animadas) y mapas de normales.
"""

from __future__ import annotations

import json
import math
import struct
import subprocess
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
# La limpieza de Unity no debe cambiar el origen de la conversión.
SOURCE_REV = "9d0176d65efc4c14596f8fa4914025f186456f12"
PREFAB = "Assets/Art/Office/Prefabs/Office_AnalystAcademy.prefab"
OUT = ROOT / "Godot/Game/Levels/office_detail.glb"
TRIANGLE_BUDGET = 110_000
DETAIL_WORDS = (
    "blind", "key", "binder", "mullion", "carpet", "neighbour", "street", "tree",
    "courtyard", "shutter", "frame", "slat", "crown", "leaf", "plant", "rack",
    "window", "lintel", "skirting", "sign", "marking", "building", "asphalt",
    "sky", "facade", "planter", "city",
)
SHELL_WORDS = ("wall", "floor", "ceiling", "partition", "desk", "door")
BUILTIN = "0000000000000000e000000000000000"
SKIP_MATERIAL = ("skin", "hair", "jacket", "shirt", "trouser", "shoe")


def git_text(path: str) -> str:
    return subprocess.check_output(["git", "show", f"{SOURCE_REV}:{path}"], cwd=ROOT, text=True, errors="replace")


def git_lines(args: list[str]) -> list[str]:
    text = subprocess.check_output(["git", *args], cwd=ROOT, text=True, errors="replace")
    return [line for line in text.splitlines() if line]


def parse_documents(text: str) -> dict[str, dict]:
    docs: dict[str, dict] = {}
    chunks = text.split("\n--- ")
    for chunk in chunks:
        lines = chunk.splitlines()
        if not lines or "&" not in lines[0]:
            continue
        header = lines[0]
        file_id = header.rsplit("&", 1)[-1].strip()
        body = "\n".join(lines[1:])
        kind = header.split("&", 1)[0].replace("!u!", "").strip().split()[-1]
        docs[file_id] = {"kind": kind, "body": body}
    return docs


def field(body: str, key: str) -> str | None:
    needle = f"{key}:"
    for line in body.splitlines():
        stripped = line.strip()
        if stripped.startswith(needle):
            return stripped.split(":", 1)[1].strip()
    return None


def vec(text: str | None, names: str) -> tuple[float, ...]:
    if not text:
        return tuple(0.0 for _ in names)
    values = []
    for name in names:
        piece = text.split(f"{name}:", 1)[1]
        number = piece.split(",", 1)[0].split("}", 1)[0].strip()
        values.append(float(number))
    return tuple(values)


def guid_of(text: str | None) -> tuple[str, str]:
    if not text or "fileID:" not in text:
        return "0", ""
    file_id = text.split("fileID:", 1)[1].split(",", 1)[0].split("}", 1)[0].strip()
    guid = text.split("guid:", 1)[1].split(",", 1)[0].split("}", 1)[0].strip() if "guid:" in text else ""
    return file_id, guid


def mat3_mul(a, b):
    return tuple(tuple(sum(a[i][k] * b[k][j] for k in range(3)) for j in range(3)) for i in range(3))


def mat3_vec(m, v):
    return tuple(sum(m[i][k] * v[k] for k in range(3)) for i in range(3))


def quat_matrix(x: float, y: float, z: float, w: float):
    xx, yy, zz = x * x, y * y, z * z
    xy, xz, yz = x * y, x * z, y * z
    wx, wy, wz = w * x, w * y, w * z
    return (
        (1 - 2 * (yy + zz), 2 * (xy - wz), 2 * (xz + wy)),
        (2 * (xy + wz), 1 - 2 * (xx + zz), 2 * (yz - wx)),
        (2 * (xz - wy), 2 * (yz + wx), 1 - 2 * (xx + yy)),
    )


def invert3(m):
    a, b, c = m
    det = (
        a[0] * (b[1] * c[2] - b[2] * c[1])
        - a[1] * (b[0] * c[2] - b[2] * c[0])
        + a[2] * (b[0] * c[1] - b[1] * c[0])
    )
    if abs(det) < 1e-12:
        return ((1, 0, 0), (0, 1, 0), (0, 0, 1))
    inv = 1 / det
    return (
        ((b[1] * c[2] - b[2] * c[1]) * inv, (a[2] * c[1] - a[1] * c[2]) * inv, (a[1] * b[2] - a[2] * b[1]) * inv),
        ((b[2] * c[0] - b[0] * c[2]) * inv, (a[0] * c[2] - a[2] * c[0]) * inv, (a[2] * b[0] - a[0] * b[2]) * inv),
        ((b[0] * c[1] - b[1] * c[0]) * inv, (a[1] * c[0] - a[0] * c[1]) * inv, (a[0] * b[1] - a[1] * b[0]) * inv),
    )


def transpose3(m):
    return tuple(tuple(m[row][col] for row in range(3)) for col in range(3))


def normalize(v):
    length = math.sqrt(sum(n * n for n in v))
    if length < 1e-8:
        return (0.0, 1.0, 0.0)
    return tuple(n / length for n in v)


def transform_of(docs: dict, file_id: str, cache: dict) -> tuple:
    if file_id in cache:
        return cache[file_id]
    body = docs[file_id]["body"]
    px, py, pz = vec(field(body, "m_LocalPosition"), "xyz")
    qx, qy, qz, qw = vec(field(body, "m_LocalRotation"), "xyzw")
    sx, sy, sz = vec(field(body, "m_LocalScale"), "xyz")
    rotation = quat_matrix(qx, qy, qz, qw)
    linear = tuple(tuple(rotation[row][col] * (sx, sy, sz)[col] for col in range(3)) for row in range(3))
    father, _ = guid_of(field(body, "m_Father"))
    if father not in ("", "0") and father in docs:
        parent_linear, parent_pos = transform_of(docs, father, cache)
        linear = mat3_mul(parent_linear, linear)
        offset = mat3_vec(parent_linear, (px, py, pz))
        position = tuple(parent_pos[i] + offset[i] for i in range(3))
    else:
        position = (px, py, pz)
    cache[file_id] = (linear, position)
    return cache[file_id]


def active_chain(docs, transform_id: str, gameobjects: dict, cache: dict) -> bool:
    if transform_id in cache:
        return cache[transform_id]
    body = docs[transform_id]["body"]
    go_id, _ = guid_of(field(body, "m_GameObject"))
    own = gameobjects.get(go_id, {}).get("active", True)
    father, _ = guid_of(field(body, "m_Father"))
    parent_ok = True
    if father not in ("", "0") and father in docs:
        parent_ok = active_chain(docs, father, gameobjects, cache)
    cache[transform_id] = own and parent_ok
    return cache[transform_id]


def cube():
    faces = []
    for axis, normal in ((0, 1), (0, -1), (1, 1), (1, -1), (2, 1), (2, -1)):
        verts = []
        for corner in ((-1, -1), (1, -1), (1, 1), (-1, 1)):
            position = [0.0, 0.0, 0.0]
            position[axis] = normal * 0.5
            position[(axis + 1) % 3] = corner[0] * 0.5
            position[(axis + 2) % 3] = corner[1] * 0.5
            uv = (corner[0] * 0.5 + 0.5, corner[1] * 0.5 + 0.5)
            n = [0.0, 0.0, 0.0]
            n[axis] = float(normal)
            verts.append((tuple(position), tuple(n), uv))
        faces.append(verts)
    return _faces_to_mesh(faces)


def _faces_to_mesh(faces):
    positions, normals, uvs, indices = [], [], [], []
    for verts in faces:
        start = len(positions)
        for position, normal, uv in verts:
            positions.append(position)
            normals.append(normal)
            uvs.append(uv)
        indices.extend((start, start + 1, start + 2, start, start + 2, start + 3))
    return positions, normals, uvs, indices


def revolution(radius: float, y0: float, y1: float, sides: int, caps: str):
    positions, normals, uvs, indices = [], [], [], []

    def add_ring(y, normal_y, scale):
        start = len(positions)
        for i in range(sides):
            angle = i / sides * math.tau
            x, z = math.cos(angle) * radius * scale, math.sin(angle) * radius * scale
            normal = normalize((x, normal_y, z))
            positions.append((x, y, z))
            normals.append(normal)
            uvs.append((i / sides, 0.0 if y == y0 else 1.0))
        return start

    bottom = add_ring(y0, 0.0, 1.0)
    top = add_ring(y1, 0.0, 1.0)
    for i in range(sides):
        j = (i + 1) % sides
        indices.extend((bottom + i, top + i, top + j, bottom + i, top + j, bottom + j))
    if caps == "flat":
        for y, ring, flip in ((y0, bottom, True), (y1, top, False)):
            center = len(positions)
            normal = (0.0, -1.0 if flip else 1.0, 0.0)
            positions.append((0.0, y, 0.0))
            normals.append(normal)
            uvs.append((0.5, 0.5))
            for i in range(sides):
                j = (i + 1) % sides
                tri = (center, ring + j, ring + i) if flip else (center, ring + i, ring + j)
                indices.extend(tri)
    return positions, normals, uvs, indices


def sphere(segments: int = 12, rings: int = 8):
    positions, normals, uvs, indices = [], [], [], []
    for ring in range(rings + 1):
        v = ring / rings
        phi = v * math.pi
        for seg in range(segments + 1):
            u = seg / segments
            theta = u * math.tau
            normal = (math.sin(phi) * math.cos(theta), math.cos(phi), math.sin(phi) * math.sin(theta))
            positions.append(tuple(0.5 * n for n in normal))
            normals.append(normal)
            uvs.append((u, v))
    stride = segments + 1
    for ring in range(rings):
        for seg in range(segments):
            a = ring * stride + seg
            b = a + stride
            indices.extend((a, b, a + 1, a + 1, b, b + 1))
    return positions, normals, uvs, indices


def capsule():
    body = revolution(0.5, -0.5, 0.5, 12, "none")
    positions, normals, uvs, indices = list(body[0]), list(body[1]), list(body[2]), list(body[3])
    for cap, y_center in ((1.0, 0.5), (-1.0, -0.5)):
        start = len(positions)
        rings = 4
        segments = 12
        for ring in range(rings + 1):
            phi = ring / rings * math.pi * 0.5
            radius = math.cos(phi) * 0.5
            y = y_center + math.sin(phi) * 0.5 * cap
            for seg in range(segments + 1):
                theta = seg / segments * math.tau
                normal = normalize((math.cos(theta) * radius, math.sin(phi) * cap, math.sin(theta) * radius))
                positions.append((normal[0] * 0.5, y, normal[2] * 0.5))
                normals.append(normal)
                uvs.append((seg / segments, ring / rings))
        stride = segments + 1
        for ring in range(rings):
            for seg in range(segments):
                a = start + ring * stride + seg
                b = a + stride
                if cap > 0:
                    indices.extend((a, a + 1, b, a + 1, b + 1, b))
                else:
                    indices.extend((a, b, a + 1, a + 1, b, b + 1))
    return positions, normals, uvs, indices


PRIMITIVES = {
    "10202": cube,
    "10206": lambda: revolution(0.5, -1.0, 1.0, 12, "flat"),
    "10207": sphere,
    "10208": capsule,
}


def load_mesh_assets() -> tuple[dict, dict]:
    meshes: dict[str, str] = {}
    materials: dict[str, dict] = {}
    for path in git_lines(["ls-tree", "-r", "--name-only", SOURCE_REV, "--", "Assets/Art/Office/Meshes", "Assets/Art/Office/Materials"]):
        if path.endswith(".meta"):
            guid = ""
            for line in git_text(path).splitlines():
                if line.startswith("guid:"):
                    guid = line.split(":", 1)[1].strip()
            asset = path[:-5]
            if asset.endswith(".asset"):
                meshes[guid] = asset
            elif asset.endswith(".mat"):
                body = git_text(asset)
                name = field(body, "m_Name") or "flat"
                color = vec(field(body, "- _Color"), "rgba")
                if len(color) != 4:
                    color = (0.7, 0.7, 0.7, 1.0)
                materials[guid] = {"name": name, "color": color}
    return meshes, materials


def decode_asset(text: str):
    vertex_count = int(field(text, "m_VertexCount") or "0")
    index_format = int(field(text, "m_IndexFormat") or "0")
    channels = []
    for line in text.splitlines():
        stripped = line.strip()
        if stripped.startswith("- stream:"):
            channels.append({})
        elif channels and stripped.startswith(("stream:", "offset:", "format:", "dimension:")):
            key, value = stripped.split(":", 1)
            channels[-1][key] = int(value.strip())
    stride = 0
    layout = {}
    names = ("position", "normal", "tangent", "color", "uv")
    for index, channel in enumerate(channels[:5]):
        dimension = channel.get("dimension", 0)
        if dimension <= 0 or channel.get("format", 0) != 0:
            continue
        offset = channel.get("offset", 0)
        layout[names[index]] = (offset, dimension)
        stride = max(stride, offset + dimension * 4)
    hex_data = "".join(ch for ch in (field(text, "_typelessdata") or "") if ch in "0123456789abcdefABCDEF")
    raw = bytes.fromhex(hex_data)
    positions, normals, uvs = [], [], []
    for index in range(vertex_count):
        base = index * stride
        def read(slot, default):
            if slot not in layout:
                return default
            offset, dimension = layout[slot]
            values = struct.unpack_from(f"<{dimension}f", raw, base + offset)
            return values
        position = read("position", (0.0, 0.0, 0.0))
        normal = read("normal", (0.0, 1.0, 0.0))
        uv = read("uv", (0.0, 0.0))
        positions.append(position[:3])
        normals.append(normalize(normal[:3]))
        uvs.append(uv[:2] if len(uv) >= 2 else (0.0, 0.0))
    index_hex = "".join(ch for ch in (field(text, "m_IndexBuffer") or "") if ch in "0123456789abcdefABCDEF")
    index_raw = bytes.fromhex(index_hex)
    width = 4 if index_format == 1 else 2
    kind = "<I" if width == 4 else "<H"
    submeshes = []
    first = None
    count = None
    for line in text.splitlines():
        stripped = line.strip()
        if stripped.startswith("- serializedVersion:") and "m_SubMeshes" in text:
            if first is not None and count is not None:
                submeshes.append((first, count))
            first, count = None, None
        elif stripped.startswith("firstByte:"):
            first = int(stripped.split(":", 1)[1])
        elif stripped.startswith("indexCount:"):
            count = int(stripped.split(":", 1)[1])
        elif stripped.startswith("topology:") and int(stripped.split(":", 1)[1]) != 0:
            first, count = None, None
    if first is not None and count is not None:
        submeshes.append((first, count))
    decoded = []
    for start, index_count in submeshes:
        indices = [struct.unpack_from(kind, index_raw, start + i * width)[0] for i in range(index_count)]
        decoded.append(indices)
    return positions, normals, uvs, decoded


def bucket(name: str) -> str | None:
    lowered = name.lower()
    if any(word in lowered for word in SKIP_MATERIAL):
        return None
    if any(word in lowered for word in ("glass", "glazing")):
        return "glass"
    if any(word in lowered for word in ("oak", "walnut", "joinery", "bark")):
        return "oak"
    if "plaster" in lowered or "limestone" in lowered or "skyline" in lowered:
        return "plaster"
    if any(word in lowered for word in ("carpet", "upholstery", "woven")):
        return "fabric"
    if any(word in lowered for word in ("steel", "graphite", "rack")):
        return "metal"
    if "foliage" in lowered:
        return "foliage"
    return "flat"


def bake_vertices(mesh, linear, position):
    normal_matrix = transpose3(invert3(linear))
    positions, normals, uvs, indices = mesh
    baked_p, baked_n = [], []
    for point, normal in zip(positions, normals):
        world = mat3_vec(linear, point)
        baked_p.append(tuple(world[i] + position[i] for i in range(3)))
        baked_n.append(normalize(mat3_vec(normal_matrix, normal)))
    return baked_p, baked_n, uvs, indices


def flip_z(positions, normals, indices):
    positions = [(x, y, -z) for x, y, z in positions]
    normals = [(x, y, -z) for x, y, z in normals]
    flipped = []
    for i in range(0, len(indices) - 2, 3):
        flipped.extend((indices[i], indices[i + 2], indices[i + 1]))
    return positions, normals, flipped


def collect():
    docs = parse_documents(git_text(PREFAB))
    mesh_paths, materials = load_mesh_assets()
    decoded_cache = {}
    gameobjects = {}
    filters = {}
    renderers = {}
    for file_id, doc in docs.items():
        body = doc["body"]
        if body.startswith("GameObject:"):
            go = field(body, "m_GameObject")
            gameobjects[file_id] = {
                "name": field(body, "m_Name") or "",
                "active": field(body, "m_IsActive") != "0",
                "transform": None,
            }
        elif body.startswith("MeshFilter:"):
            go_id, _ = guid_of(field(body, "m_GameObject"))
            filters[go_id] = guid_of(field(body, "m_Mesh"))
        elif body.startswith("MeshRenderer:"):
            go_id, _ = guid_of(field(body, "m_GameObject"))
            enabled = field(body, "m_Enabled") != "0"
            material_guids = []
            for line in body.splitlines():
                if "guid:" in line and "m_Materials" not in line and line.strip().startswith("-"):
                    _, guid = guid_of(line.strip()[1:].strip())
                    material_guids.append(guid)
            renderers[go_id] = {"enabled": enabled, "materials": material_guids}
    for file_id, doc in docs.items():
        if not doc["body"].startswith("Transform:"):
            continue
        go_id, _ = guid_of(field(doc["body"], "m_GameObject"))
        if go_id in gameobjects:
            gameobjects[go_id]["transform"] = file_id
    worlds = {}
    actives = {}
    parts = []
    for go_id, info in gameobjects.items():
        transform_id = info["transform"]
        if not transform_id or go_id not in filters or go_id not in renderers:
            continue
        if not renderers[go_id]["enabled"] or not active_chain(docs, transform_id, gameobjects, actives):
            continue
        mesh_id, mesh_guid = filters[go_id]
        if mesh_id in ("", "0"):
            continue
        material_ids = renderers[go_id]["materials"] or [""]
        material = materials.get(material_ids[0], {"name": "flat", "color": (0.75, 0.75, 0.75, 1.0)})
        group = bucket(material["name"])
        if group is None:
            continue
        if mesh_guid == BUILTIN:
            if mesh_id not in PRIMITIVES:
                continue
            source = PRIMITIVES[mesh_id]()
        else:
            path = mesh_paths.get(mesh_guid)
            if not path:
                continue
            if path not in decoded_cache:
                decoded_cache[path] = decode_asset(git_text(path))
            positions, normals, uvs, submeshes = decoded_cache[path]
            source = (positions, normals, uvs, submeshes[0] if submeshes else [])
        linear, position = transform_of(docs, transform_id, worlds)
        baked = bake_vertices(source, linear, position)
        positions, normals, indices = flip_z(baked[0], baked[1], baked[3])
        if not indices:
            continue
        center = (
            sum(point[0] for point in positions) / len(positions),
            sum(point[2] for point in positions) / len(positions),
        )
        distance = math.sqrt(center[0] * center[0] + center[1] * center[1])
        lowered = info["name"].lower()
        exterior = distance > 14 or any(word in lowered for word in ("neighbour", "street", "skyline", "distant", "city", "facade"))
        if any(word in lowered for word in SHELL_WORDS) and not any(word in lowered for word in ("frame", "lintel", "skirting")):
            continue
        if not exterior and not any(word in lowered for word in DETAIL_WORDS):
            continue
        parts.append({
            "name": info["name"],
            "group": group,
            "color": material["color"],
            "positions": positions,
            "normals": normals,
            "uvs": baked[2],
            "indices": indices,
            "triangles": len(indices) // 3,
            "distance": distance,
            "exterior": exterior,
        })
    return parts


def trim(parts: list[dict]) -> list[dict]:
    total = sum(part["triangles"] for part in parts)
    if total <= TRIANGLE_BUDGET:
        return parts
    ranked = sorted((part for part in parts if part["exterior"]), key=lambda part: part["triangles"], reverse=True)
    drop = set()
    for part in ranked:
        if total <= TRIANGLE_BUDGET:
            break
        drop.add(id(part))
        total -= part["triangles"]
    return [part for part in parts if id(part) not in drop]


def merge(parts: list[dict]) -> dict[str, dict]:
    groups: dict[str, dict] = {}
    for part in parts:
        key = ("exterior_" if part["exterior"] else "interior_") + part["group"]
        group = groups.setdefault(key, {"positions": [], "normals": [], "colors": [], "uvs": [], "indices": []})
        start = len(group["positions"])
        group["positions"].extend(part["positions"])
        group["normals"].extend(part["normals"])
        group["uvs"].extend(part["uvs"])
        group["colors"].extend((part["color"],) * len(part["positions"]))
        group["indices"].extend(index + start for index in part["indices"])
    return groups


def write_glb(groups: dict[str, dict], path: Path) -> None:
    buffer = bytearray()
    views = []
    accessors = []
    meshes = []
    nodes = []

    def align():
        while len(buffer) % 4:
            buffer.append(0)

    def push(values, fmt: str) -> int:
        align()
        start = len(buffer)
        for value in values:
            buffer.extend(struct.pack(fmt, *value) if isinstance(value, tuple) else struct.pack(fmt, value))
        views.append({"buffer": 0, "byteOffset": start, "byteLength": len(buffer) - start})
        return len(views) - 1

    for name, group in groups.items():
        positions = group["positions"]
        mins = [min(p[axis] for p in positions) for axis in range(3)]
        maxs = [max(p[axis] for p in positions) for axis in range(3)]
        pos_view = push(positions, "<3f")
        accessors.append({"bufferView": pos_view, "componentType": 5126, "count": len(positions), "type": "VEC3", "min": mins, "max": maxs})
        normal_view = push(group["normals"], "<3f")
        accessors.append({"bufferView": normal_view, "componentType": 5126, "count": len(positions), "type": "VEC3"})
        color_view = push(group["colors"], "<4f")
        accessors.append({"bufferView": color_view, "componentType": 5126, "count": len(positions), "type": "VEC4"})
        uv_view = push(group["uvs"], "<2f")
        accessors.append({"bufferView": uv_view, "componentType": 5126, "count": len(positions), "type": "VEC2"})
        index_view = push(group["indices"], "<I")
        accessors.append({"bufferView": index_view, "componentType": 5125, "count": len(group["indices"]), "type": "SCALAR"})
        base = len(accessors) - 5
        meshes.append({
            "name": name,
            "primitives": [{
                "attributes": {"POSITION": base, "NORMAL": base + 1, "COLOR_0": base + 2, "TEXCOORD_0": base + 3},
                "indices": base + 4,
                "material": len(meshes),
            }],
        })
        nodes.append({"name": name, "mesh": len(meshes) - 1})
    document = {
        "asset": {"version": "2.0", "generator": "export-office-glb.py"},
        "scene": 0,
        "scenes": [{"nodes": list(range(len(nodes)))}],
        "nodes": nodes,
        "meshes": meshes,
        "materials": [{"name": name, "doubleSided": False, "pbrMetallicRoughness": {"metallicFactor": 0, "roughnessFactor": 0.82}} for name in groups],
        "buffers": [{"byteLength": len(buffer)}],
        "bufferViews": views,
        "accessors": accessors,
    }
    payload = json.dumps(document, separators=(",", ":")).encode()
    while len(payload) % 4:
        payload += b" "
    while len(buffer) % 4:
        buffer.append(0)
    glb = bytearray()
    glb.extend(struct.pack("<4sII", b"glTF", 2, 12 + 8 + len(payload) + 8 + len(buffer)))
    glb.extend(struct.pack("<I4s", len(payload), b"JSON"))
    glb.extend(payload)
    glb.extend(struct.pack("<I4s", len(buffer), b"BIN\0"))
    glb.extend(buffer)
    temporary = path.with_suffix(".glb.tmp")
    temporary.write_bytes(glb)
    temporary.replace(path)


def main() -> None:
    parts = trim(collect())
    groups = merge(parts)
    write_glb(groups, OUT)
    triangles = sum(len(group["indices"]) // 3 for group in groups.values())
    print(f"OFFICE DETAIL: {triangles} triangulos en {len(groups)} mallas -> {OUT.relative_to(ROOT)}")
    for name, group in groups.items():
        print(f"  {name}: {len(group['indices']) // 3}")


if __name__ == "__main__":
    main()
