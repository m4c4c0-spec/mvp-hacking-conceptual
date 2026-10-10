class_name RenderBudget
extends RefCounted

## Presupuesto de la escena para que el proceso quepa en 8 GB de RAM.
const MAX_TRIANGLES := 150000
const MAX_TEXTURE_EDGE := 1024
const PROCESS_TARGET_BYTES := 2684354560
const CAMERA_FAR := 40.0

static func measure(root: Node) -> Dictionary:
	var report := {
		"triangles": 0,
		"normal_maps": 0,
		"oversized_textures": 0,
		"ok": false,
	}
	if root:
		_walk(root, report)
	report.ok = report.triangles < MAX_TRIANGLES and report.normal_maps == 0 and report.oversized_textures == 0
	return report

static func enforce(root: Node) -> Dictionary:
	var report := measure(root)
	if not report.ok:
		push_error("Presupuesto 8 GB superado: %s" % str(report))
	else:
		print("Presupuesto 8 GB: %d triángulos, sin normales, texturas <= %d." % [report.triangles, MAX_TEXTURE_EDGE])
	return report

static func warn_if_heavy() -> void:
	var used := OS.get_static_memory_usage()
	if used > PROCESS_TARGET_BYTES:
		push_error("Memoria estática %d MB; el objetivo del proceso en juego es mantenerse bajo 2560 MB." % int(used / (1024 * 1024)))

static func _walk(node: Node, report: Dictionary) -> void:
	if node is MeshInstance3D:
		var mesh_instance := node as MeshInstance3D
		report.triangles += _triangles(mesh_instance.mesh)
		_check_material(mesh_instance.material_override, report)
		if mesh_instance.mesh:
			for surface in mesh_instance.mesh.get_surface_count():
				_check_material(mesh_instance.mesh.surface_get_material(surface), report)
	for child in node.get_children():
		_walk(child, report)

static func _triangles(mesh: Mesh) -> int:
	if mesh == null:
		return 0
	if mesh is PrimitiveMesh:
		return _count_mesh_arrays((mesh as PrimitiveMesh).get_mesh_arrays(), Mesh.PRIMITIVE_TRIANGLES)
	if mesh is ArrayMesh:
		var array_mesh := mesh as ArrayMesh
		var total := 0
		for surface in array_mesh.get_surface_count():
			total += _count_mesh_arrays(array_mesh.surface_get_arrays(surface), array_mesh.surface_get_primitive_type(surface))
		return total
	return int(mesh.get_faces().size() / 3)


static func _count_mesh_arrays(arrays: Array, primitive: int) -> int:
	if arrays.is_empty():
		return 0
	var count := 0
	if arrays.size() > Mesh.ARRAY_INDEX:
		var indices = arrays[Mesh.ARRAY_INDEX]
		if indices is PackedInt32Array and not (indices as PackedInt32Array).is_empty():
			count = (indices as PackedInt32Array).size()
	if count == 0 and arrays.size() > Mesh.ARRAY_VERTEX:
		var vertices = arrays[Mesh.ARRAY_VERTEX]
		if vertices is PackedVector3Array:
			count = (vertices as PackedVector3Array).size()
	if primitive == Mesh.PRIMITIVE_TRIANGLE_STRIP:
		return maxi(count - 2, 0)
	if primitive == Mesh.PRIMITIVE_TRIANGLES:
		return int(count / 3)
	return 0

static func _check_material(material: Material, report: Dictionary) -> void:
	var standard := material as StandardMaterial3D
	if standard == null:
		return
	if standard.normal_enabled or standard.normal_texture != null:
		report.normal_maps += 1
	for texture in [standard.albedo_texture, standard.orm_texture, standard.roughness_texture, standard.metallic_texture]:
		_check_texture(texture, report)

static func _check_texture(texture: Texture2D, report: Dictionary) -> void:
	if texture == null or texture is ViewportTexture:
		return
	if texture.get_width() > MAX_TEXTURE_EDGE or texture.get_height() > MAX_TEXTURE_EDGE:
		report.oversized_textures += 1
