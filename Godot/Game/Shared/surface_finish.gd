extends RefCounted

## Albedos 1K compartidos por la oficina. Sin mapa de normales.
const OAK := "res://Game/Shared/Textures/oak.png"
const PLASTER := "res://Game/Shared/Textures/plaster.png"
const FABRIC := "res://Game/Shared/Textures/fabric.png"
const PAPER := Color("d5d4c1")
const _PAPER_SCALE := Vector3(2.0, 2.0, 2.0)

const _WOOD := ["966746", "987b59", "806448", "9a7853", "a47e57", "cda873", "d4ba86", "b27d59", "bf9b69"]
const _PLASTER := ["aab3ad", "bbc2b6", "9daaa3", "c5c7bd"]
const _FABRIC := ["41494b", "487b86", "627dab", "b28452", "6b8b70", "9570a4"]
const _METAL := ["778b91", "75878b"]

static var _materials: Dictionary = {}
static var _textures: Dictionary = {}

static func for_bucket(bucket_name: String) -> StandardMaterial3D:
	var bucket := bucket_name.trim_prefix("interior_").trim_prefix("exterior_")
	if _materials.has(bucket):
		return _materials[bucket]
	var material := _base(Color.WHITE)
	material.vertex_color_use_as_albedo = true
	if bucket == "oak":
		_texture_on(material, OAK, _scale(OAK))
	elif bucket == "plaster":
		_texture_on(material, PLASTER, _scale(PLASTER))
	elif bucket == "fabric":
		_texture_on(material, FABRIC, _scale(FABRIC))
	elif bucket == "metal":
		_metal_on(material)
	elif bucket == "glass":
		material.transparency = BaseMaterial3D.TRANSPARENCY_ALPHA
		material.albedo_color = Color(0.75, 0.86, 0.9, 0.28)
		material.cull_mode = BaseMaterial3D.CULL_DISABLED
	material.set_meta("finish_bucket", bucket)
	_materials[bucket] = material
	return material

static func make(color: Color, surface_name := "") -> StandardMaterial3D:
	var family := _family(surface_name)
	var key := "name:paper" if family == "paper" else "name:" + family + "|" + color.to_html(false)
	if _materials.has(key):
		return _materials[key]
	var material := _base(color)
	_apply(material, family, color)
	material.normal_enabled = false
	material.set_meta("finish_color", color)
	material.set_meta("finish_family", family)
	_materials[key] = material
	return material

static func _apply(material: StandardMaterial3D, family: String, color: Color) -> void:
	if family == "cable":
		_polished_metal(material)
		return
	if family == "keys":
		material.roughness = 0.55
		material.metallic = 0.08
		material.metallic_specular = 0.2
		return
	if family == "flat":
		material.roughness = 0.86
		material.metallic = 0.0
		return
	if family == "paper":
		material.albedo_color = PAPER
		_texture_on(material, PLASTER, _PAPER_SCALE)
		material.roughness = 0.96
		material.metallic = 0.0
		return
	if family == "rack":
		_metal_on(material)
		return
	if family == "floor":
		material.albedo_color = color.lightened(0.12)
		_texture_on(material, FABRIC, _scale(FABRIC))
		material.texture_filter = BaseMaterial3D.TEXTURE_FILTER_LINEAR_WITH_MIPMAPS_ANISOTROPIC
		return
	if family == "wall":
		material.albedo_color = color.lightened(0.12)
		_texture_on(material, PLASTER, _scale(PLASTER))
		return
	if family == "upholstery":
		material.albedo_color = color.lightened(0.12)
		_texture_on(material, FABRIC, _scale(FABRIC))
		return
	var color_key := color.to_html(false)
	var finish := _finish(color_key)
	if finish != "":
		# El albedo ya contiene color: un tinte oscuro adicional ocultaba la veta.
		material.albedo_color = color.lightened(0.22 if finish == OAK else 0.12)
		_texture_on(material, finish, _scale(finish))
	elif color_key in _METAL:
		_metal_on(material)

static func _family(surface_name: String) -> String:
	# El suelo gana a ServerFloor; el muro, a ServerPartition.
	if _segment(surface_name, "Floor"):
		return "floor"
	if not surface_name.contains("Slat") and (_segment(surface_name, "Wall") or _segment(surface_name, "Partition") or _segment(surface_name, "Lintel")):
		return "wall"
	if surface_name.begins_with("Cable"):
		return "cable"
	if surface_name.begins_with("Key") or surface_name.begins_with("DemoKey") or surface_name.contains("Keyboard"):
		return "keys"
	if surface_name.begins_with("Token") or surface_name.begins_with("Lamp") or surface_name.begins_with("Login") or surface_name.begins_with("Kill") or surface_name.begins_with("Screen") or surface_name.ends_with("Screen") or surface_name.begins_with("Led"):
		return "flat"
	if _segment(surface_name, "Email") or _segment(surface_name, "Manual") or _segment(surface_name, "Badge") or _segment(surface_name, "Hash") or _segment(surface_name, "Config") or _segment(surface_name, "Traffic") or _segment(surface_name, "Cork") or _segment(surface_name, "Log"):
		return "paper"
	if surface_name.begins_with("Rack") or surface_name.contains("RACK") or _segment(surface_name, "Server"):
		return "rack"
	if surface_name.begins_with("ChairSeat") or surface_name.begins_with("ChairBack"):
		return "upholstery"
	return ""

static func _segment(surface_name: String, token: String) -> bool:
	if surface_name.begins_with(token):
		return true
	var at := surface_name.find(token)
	while at > 0:
		var previous := surface_name.unicode_at(at - 1)
		var head := token.unicode_at(0)
		var hump := head >= 65 and head <= 90 and ((previous >= 97 and previous <= 122) or (previous >= 48 and previous <= 57))
		if hump:
			return true
		at = surface_name.find(token, at + 1)
	return false

static func _base(color: Color) -> StandardMaterial3D:
	var material := StandardMaterial3D.new()
	material.albedo_color = color
	material.roughness = 0.82
	material.metallic = 0.0
	material.diffuse_mode = BaseMaterial3D.DIFFUSE_LAMBERT
	material.specular_mode = BaseMaterial3D.SPECULAR_SCHLICK_GGX
	material.metallic_specular = 0.25
	material.normal_enabled = false
	material.texture_filter = BaseMaterial3D.TEXTURE_FILTER_LINEAR_WITH_MIPMAPS_ANISOTROPIC
	return material

static func _texture_on(material: StandardMaterial3D, path: String, scale: Vector3) -> void:
	material.albedo_texture = _texture(path)
	material.uv1_triplanar = true
	material.uv1_triplanar_sharpness = 8.0
	material.uv1_scale = scale
	material.roughness = 0.62 if path == OAK else 0.94

static func _metal_on(material: StandardMaterial3D) -> void:
	material.roughness = 0.38
	material.metallic = 0.45
	material.metallic_specular = 0.5
	material.normal_enabled = false

static func _polished_metal(material: StandardMaterial3D) -> void:
	material.roughness = 0.18
	material.metallic = 0.72
	material.metallic_specular = 0.62
	material.normal_enabled = false

static func _finish(key: String) -> String:
	if key in _WOOD:
		return OAK
	if key in _PLASTER:
		return PLASTER
	if key in _FABRIC:
		return FABRIC
	return ""

static func _scale(path: String) -> Vector3:
	if path == OAK:
		return Vector3(0.65, 0.65, 0.65)
	if path == FABRIC:
		return Vector3(1.5, 1.5, 1.5)
	return Vector3(0.75, 0.75, 0.75)

## Actualiza acabados de la oficina horneada y de los nodos que las salas cuelgan en runtime.
static func refresh_scene(root: Node) -> void:
	if root == null:
		return
	_refresh_tree(root)

static func _refresh_tree(node: Node) -> void:
	for child in node.get_children():
		if child is MeshInstance3D:
			_refresh_mesh(child as MeshInstance3D)
		_refresh_tree(child)

static func _refresh_mesh(instance: MeshInstance3D) -> void:
	var previous := instance.material_override as StandardMaterial3D
	var surface_name := _surface_name(instance)
	if previous == null:
		instance.material_override = make(_mesh_color(instance), surface_name)
		return
	if previous.has_meta("finish_bucket"):
		instance.material_override = for_bucket(str(previous.get_meta("finish_bucket")))
		return
	if previous.vertex_color_use_as_albedo:
		instance.material_override = for_bucket(str(instance.name))
		return
	var source_color: Color = previous.get_meta("finish_color", previous.albedo_color)
	instance.material_override = make(source_color, surface_name)

static func _surface_name(instance: MeshInstance3D) -> String:
	if str(instance.name) == "Mesh" and instance.get_parent():
		return str(instance.get_parent().name)
	return str(instance.name)

static func _mesh_color(instance: MeshInstance3D) -> Color:
	if instance.mesh and instance.mesh.get_surface_count() > 0:
		var surface_material := instance.mesh.surface_get_material(0) as StandardMaterial3D
		if surface_material:
			return surface_material.albedo_color
	return Color.WHITE

static func tune_lighting(root: Node) -> void:
	for node in root.find_children("*", "", true, false):
		if node is WorldEnvironment and node.environment:
			node.environment.ambient_light_color = Color("e4eaf2")
			node.environment.ambient_light_energy = 0.55
		elif node is DirectionalLight3D:
			node.light_energy = 1.05
			node.light_specular = 0.6
		elif node is OmniLight3D:
			node.light_color = Color("fff0db")
			node.light_energy = 0.32
			node.light_specular = 0.35

static func _texture(path: String) -> Texture2D:
	if not _textures.has(path):
		_textures[path] = load(path)
	return _textures[path]
