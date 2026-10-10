class_name QualityProfile
extends Node

## Perfiles de render para un techo de 8 GB de RAM de sistema.
## Desktop8Gb es el predeterminado. Quest3S solo si OpenXR quedó inicializado.
## No referencia EscapeSession.

const PROFILE_DESKTOP := "Desktop8Gb"
const PROFILE_QUEST := "Quest3S"

const PHYSICS_TICKS := 72
const DESKTOP_SCALE := 1.0
const QUEST_SCALE := 0.8
const DESKTOP_SHADOW_DISTANCE := 18.0
const QUEST_SHADOW_DISTANCE := 10.0
const QUEST_FOVEATION := 2
const SMALL_PROP_METERS := 0.6

static var active_profile := PROFILE_DESKTOP
static var _watching := false
static var _shadow_owner: Light3D


func _ready() -> void:
	apply(get_tree())


static func apply(tree: SceneTree) -> String:
	if tree == null:
		return active_profile
	Engine.physics_ticks_per_second = PHYSICS_TICKS
	var quest := _xr_active()
	active_profile = PROFILE_QUEST if quest else PROFILE_DESKTOP
	_shadow_owner = null
	if tree.root:
		_tune_viewport(tree.root.get_viewport(), quest)
		_scan(tree.root, quest)
	if not _watching:
		tree.node_added.connect(_on_node_added)
		_watching = true
	return active_profile


static func _xr_active() -> bool:
	var iface := XRServer.find_interface("OpenXR")
	if iface == null:
		return false
	if iface.is_initialized():
		_tune_openxr(iface)
		return true
	# OpenXR requiere inicialización del motor al arrancar (--xr-mode on / preset Quest).
	# No puede habilitarse de forma fiable durante la configuración del render.
	return false


static func _tune_openxr(iface: XRInterface) -> void:
	var openxr := iface as OpenXRInterface
	if openxr == null or not openxr.is_foveation_supported():
		return
	openxr.foveation_level = QUEST_FOVEATION
	openxr.foveation_dynamic = false


static func _on_node_added(node: Node) -> void:
	_tune_node(node, active_profile == PROFILE_QUEST)


static func _scan(root: Node, quest: bool) -> void:
	_tune_node(root, quest)
	for node in root.find_children("*", "", true, false):
		_tune_node(node, quest)


static func _tune_node(node: Node, quest: bool) -> void:
	if node is Light3D:
		_tune_light(node as Light3D, quest)
	elif node is WorldEnvironment:
		_tune_environment(node as WorldEnvironment)
	elif node is MeshInstance3D:
		_tune_geometry(node as MeshInstance3D)


static func _tune_viewport(viewport: Viewport, quest: bool) -> void:
	if viewport == null:
		return
	viewport.msaa_3d = Viewport.MSAA_DISABLED if quest else Viewport.MSAA_2X
	viewport.screen_space_aa = Viewport.SCREEN_SPACE_AA_DISABLED
	viewport.use_taa = false
	viewport.use_debanding = false
	viewport.scaling_3d_mode = Viewport.SCALING_3D_MODE_BILINEAR
	viewport.scaling_3d_scale = QUEST_SCALE if quest else DESKTOP_SCALE
	viewport.use_occlusion_culling = true


static func _tune_light(light: Light3D, quest: bool) -> void:
	if light is DirectionalLight3D:
		var sun := light as DirectionalLight3D
		sun.directional_shadow_mode = DirectionalLight3D.SHADOW_ORTHOGONAL
		sun.directional_shadow_max_distance = QUEST_SHADOW_DISTANCE if quest else DESKTOP_SHADOW_DISTANCE
		sun.shadow_blur = 0.0
		if not sun.shadow_enabled:
			return
		if _shadow_owner == null or not is_instance_valid(_shadow_owner):
			_shadow_owner = sun
		elif _shadow_owner != sun:
			sun.shadow_enabled = false
		return
	light.shadow_enabled = false


static func _tune_environment(world: WorldEnvironment) -> void:
	var env := world.environment
	if env == null:
		return
	env.glow_enabled = false
	env.volumetric_fog_enabled = false
	env.fog_enabled = false
	env.ssao_enabled = false
	env.ssil_enabled = false
	env.sdfgi_enabled = false
	env.ssr_enabled = false


static func _tune_geometry(geom: MeshInstance3D) -> void:
	if geom.mesh == null:
		return
	var size := geom.get_aabb().size
	if size.x < SMALL_PROP_METERS and size.y < SMALL_PROP_METERS and size.z < SMALL_PROP_METERS:
		geom.cast_shadow = GeometryInstance3D.SHADOW_CASTING_SETTING_OFF
