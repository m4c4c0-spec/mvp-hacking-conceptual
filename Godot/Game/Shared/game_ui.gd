class_name EscapeUI
extends CanvasLayer

signal modal_changed(open: bool)

const SPATIAL_VIEWPORT := Vector2i(1280, 800)
# 640 px/m. A 1,5 m el cuerpo de 24 px subtende unos 1,4°; a los 1,7 m de main.gd, unos 1,3°.
const SPATIAL_QUAD := Vector2(2.0, 1.25)
const TITLE_PX := 32
const BODY_PX := 24
const BUTTON_PX := 22
const BUTTON_MIN_HEIGHT := 64.0

var root: Control
var hint: Label
var focus_label: Label
var reading: Label
var toast_label: Label
var subtitle: Label
var toast_seconds := 0.0
var modal: PanelContainer
var content: VBoxContainer
var modal_open := false
var spatial_viewport: SubViewport
var hint_plate: PanelContainer
var subtitle_plate: PanelContainer
var reading_plate: Panel
var controls_label: Label
var crosshair: Label
var scroll: ScrollContainer


func _ready() -> void:
	layer = 5
	root = Control.new()
	root.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	root.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(root)
	var margin := MarginContainer.new()
	margin.set_anchors_and_offsets_preset(Control.PRESET_TOP_WIDE)
	margin.add_theme_constant_override("margin_left", 24)
	margin.add_theme_constant_override("margin_top", 18)
	margin.add_theme_constant_override("margin_right", 24)
	margin.mouse_filter = Control.MOUSE_FILTER_IGNORE
	root.add_child(margin)
	var top := VBoxContainer.new()
	top.mouse_filter = Control.MOUSE_FILTER_IGNORE
	margin.add_child(top)
	hint_plate = _dark_plate()
	var objective := VBoxContainer.new()
	objective.mouse_filter = Control.MOUSE_FILTER_IGNORE
	objective.add_theme_constant_override("separation", 6)
	hint_plate.add_child(objective)
	hint = _label("", 26)
	hint.add_theme_color_override("font_color", Color("ffffff"))
	objective.add_child(hint)
	controls_label = _label("WASD caminar · mouse mirar · E usar · G tomar/soltar · Shift correr · Esc menú", 20)
	controls_label.add_theme_color_override("font_color", Color("ffffff"))
	objective.add_child(controls_label)
	focus_label = _label("", 24)
	focus_label.add_theme_color_override("font_color", Color("ffffff"))
	objective.add_child(focus_label)
	top.add_child(hint_plate)
	reading_plate = Panel.new()
	reading_plate.position = Vector2(28, 176)
	reading_plate.size = Vector2(720, 360)
	reading_plate.mouse_filter = Control.MOUSE_FILTER_IGNORE
	reading_plate.clip_contents = true
	reading_plate.add_theme_stylebox_override("panel", _dark_style())
	reading_plate.visible = false
	root.add_child(reading_plate)
	reading = _label("", BODY_PX)
	reading.add_theme_color_override("font_color", Color("ffffff"))
	reading.position = Vector2(16, 12)
	reading.size = Vector2(688, 336)
	reading_plate.add_child(reading)
	toast_label = _label("", BODY_PX)
	toast_label.set_anchors_and_offsets_preset(Control.PRESET_BOTTOM_WIDE)
	toast_label.offset_left = 30
	toast_label.offset_top = -160
	toast_label.offset_right = -30
	# El aviso en pantalla es el subtítulo. Este label conserva el mismo texto.
	toast_label.visible = false
	root.add_child(toast_label)
	subtitle_plate = _dark_plate()
	subtitle_plate.name = "SubtitlePlate"
	subtitle_plate.set_anchors_and_offsets_preset(Control.PRESET_BOTTOM_WIDE)
	subtitle_plate.offset_left = 48
	subtitle_plate.offset_right = -48
	subtitle_plate.offset_top = -148
	subtitle_plate.offset_bottom = -24
	subtitle = _label("", 28)
	subtitle.name = "Subtitle"
	subtitle.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	subtitle.vertical_alignment = VERTICAL_ALIGNMENT_CENTER
	subtitle.add_theme_color_override("font_color", Color("f7fff8"))
	subtitle.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	subtitle.size_flags_vertical = Control.SIZE_EXPAND_FILL
	subtitle_plate.add_child(subtitle)
	subtitle_plate.hide()
	root.add_child(subtitle_plate)
	crosshair = _label("+", 26)
	_float_contrast(crosshair)
	crosshair.set_anchors_and_offsets_preset(Control.PRESET_CENTER)
	crosshair.position -= Vector2(7, 18)
	root.add_child(crosshair)
	modal = PanelContainer.new()
	modal.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	modal.offset_left = 80
	modal.offset_top = 176
	modal.offset_right = -80
	modal.offset_bottom = -168
	var style := StyleBoxFlat.new()
	style.bg_color = Color("152832")
	style.border_color = Color("55d4be")
	style.set_border_width_all(2)
	style.set_corner_radius_all(12)
	style.content_margin_left = 28
	style.content_margin_right = 28
	style.content_margin_top = 24
	style.content_margin_bottom = 24
	modal.add_theme_stylebox_override("panel", style)
	root.add_child(modal)
	scroll = ScrollContainer.new()
	scroll.horizontal_scroll_mode = ScrollContainer.SCROLL_MODE_DISABLED
	modal.add_child(scroll)
	content = VBoxContainer.new()
	content.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	content.add_theme_constant_override("separation", 14)
	scroll.add_child(content)
	modal.hide()


func _process(delta: float) -> void:
	toast_seconds -= delta
	if toast_seconds <= 0.0:
		toast_label.text = ""
		subtitle.text = ""
	subtitle_plate.visible = not subtitle.text.is_empty()
	hint.visible = not hint.text.is_empty()
	focus_label.visible = not focus_label.text.is_empty()
	reading_plate.visible = not reading.text.is_empty()


func show_card(title: String, body: String) -> void:
	for child in content.get_children():
		content.remove_child(child)
		child.queue_free()
	content.add_child(_label(title, TITLE_PX))
	content.add_child(_label(body, BODY_PX))
	scroll.scroll_vertical = 0
	modal.show()
	modal_open = true
	modal_changed.emit(true)


func add_button(text: String, callback: Callable, disabled := false) -> Button:
	var button := Button.new()
	button.text = text
	button.disabled = disabled
	button.alignment = HORIZONTAL_ALIGNMENT_LEFT
	button.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
	button.custom_minimum_size.y = BUTTON_MIN_HEIGHT
	button.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	button.add_theme_font_size_override("font_size", BUTTON_PX)
	button.add_theme_color_override("font_color", Color("102028"))
	button.add_theme_color_override("font_hover_color", Color("102028"))
	button.add_theme_color_override("font_pressed_color", Color("102028"))
	button.add_theme_color_override("font_disabled_color", Color("1c2a30"))
	button.add_theme_color_override("font_focus_color", Color("102028"))
	button.add_theme_stylebox_override("normal", _button_box(Color("e7f2ec")))
	button.add_theme_stylebox_override("hover", _button_box(Color("f4fffb")))
	button.add_theme_stylebox_override("pressed", _button_box(Color("b7ddd2")))
	button.add_theme_stylebox_override("disabled", _button_box(Color("c5d0cb")))
	button.add_theme_stylebox_override("focus", _button_box(Color("f4fffb")))
	button.pressed.connect(callback)
	content.add_child(button)
	return button


func add_text(text: String) -> void:
	content.add_child(_label(text, BODY_PX))


func close() -> void:
	modal.hide()
	modal_open = false
	modal_changed.emit(false)


func toast(text: String) -> void:
	toast_label.text = text
	subtitle.text = text
	subtitle_plate.visible = not text.is_empty()
	toast_seconds = 7.0


func _label(text: String, size: int) -> Label:
	var label := Label.new()
	label.text = text
	label.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
	label.add_theme_font_size_override("font_size", size)
	label.add_theme_color_override("font_color", Color("e6eade"))
	label.add_theme_color_override("font_shadow_color", Color("111b21"))
	label.add_theme_constant_override("shadow_offset_x", 1)
	label.add_theme_constant_override("shadow_offset_y", 2)
	label.mouse_filter = Control.MOUSE_FILTER_IGNORE
	return label


func _float_contrast(label: Label) -> void:
	label.add_theme_color_override("font_color", Color("ffffff"))
	label.add_theme_color_override("font_outline_color", Color("071014"))
	label.add_theme_constant_override("outline_size", 6)


func _dark_style() -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = Color(0.03, 0.05, 0.06, 0.94)
	style.border_color = Color("f4fff8")
	style.set_border_width_all(2)
	style.set_corner_radius_all(8)
	style.content_margin_left = 16
	style.content_margin_right = 16
	style.content_margin_top = 10
	style.content_margin_bottom = 10
	return style


func _dark_plate() -> PanelContainer:
	var plate := PanelContainer.new()
	plate.mouse_filter = Control.MOUSE_FILTER_IGNORE
	plate.add_theme_stylebox_override("panel", _dark_style())
	return plate


func _button_box(fill: Color) -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = fill
	style.set_corner_radius_all(8)
	style.content_margin_left = 16
	style.content_margin_right = 16
	style.content_margin_top = 8
	style.content_margin_bottom = 8
	return style


func make_spatial(parent: Node3D) -> Node3D:
	# Una sola interfaz: el mismo árbol Control vive en un viewport espacial en VR.
	set_vr_controls()
	spatial_viewport = SubViewport.new()
	spatial_viewport.name = "SpatialUIViewport"
	spatial_viewport.size = SPATIAL_VIEWPORT
	spatial_viewport.disable_3d = true
	spatial_viewport.render_target_update_mode = SubViewport.UPDATE_ALWAYS
	spatial_viewport.transparent_bg = true
	parent.add_child(spatial_viewport)
	root.reparent(spatial_viewport)
	root.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	var screen := Node3D.new()
	screen.name = "SpatialInterface"
	parent.add_child(screen)
	var mesh := MeshInstance3D.new()
	var quad := QuadMesh.new()
	quad.size = SPATIAL_QUAD
	mesh.mesh = quad
	mesh.cast_shadow = GeometryInstance3D.SHADOW_CASTING_SETTING_OFF
	var material := StandardMaterial3D.new()
	material.shading_mode = BaseMaterial3D.SHADING_MODE_UNSHADED
	material.transparency = BaseMaterial3D.TRANSPARENCY_ALPHA
	material.cull_mode = BaseMaterial3D.CULL_DISABLED
	material.texture_filter = BaseMaterial3D.TEXTURE_FILTER_LINEAR
	material.albedo_texture = spatial_viewport.get_texture()
	mesh.material_override = material
	screen.add_child(mesh)
	var body := StaticBody3D.new()
	body.name = "PointerTarget"
	body.collision_layer = 8
	body.collision_mask = 0
	body.set_meta("spatial_ui", true)
	screen.add_child(body)
	var shape := CollisionShape3D.new()
	var box := BoxShape3D.new()
	box.size = Vector3(SPATIAL_QUAD.x, SPATIAL_QUAD.y, 0.02)
	shape.shape = box
	body.add_child(shape)
	return screen


func set_vr_controls() -> void:
	controls_label.text = "Trigger derecho usar · grip tomar/soltar · mantener click stick izquierdo y soltar para teletransportar · stick derecho girar/desplazar menú · B menú"
	crosshair.hide()


func scroll_spatial(amount: float) -> void:
	if modal_open:
		scroll.scroll_vertical += roundi(amount)


func pointer(world_position: Vector3, screen: Node3D, click: bool) -> void:
	var local := screen.to_local(world_position)
	var point := Vector2(
		(local.x / SPATIAL_QUAD.x + 0.5) * SPATIAL_VIEWPORT.x,
		(0.5 - local.y / SPATIAL_QUAD.y) * SPATIAL_VIEWPORT.y
	)
	var motion := InputEventMouseMotion.new()
	motion.position = point
	motion.global_position = point
	spatial_viewport.push_input(motion, true)
	if click:
		for pressed in [true, false]:
			var event := InputEventMouseButton.new()
			event.button_index = MOUSE_BUTTON_LEFT
			event.pressed = pressed
			event.position = point
			event.global_position = point
			spatial_viewport.push_input(event, true)
