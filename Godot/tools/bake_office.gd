extends SceneTree

const Builder = preload("res://Game/Levels/office_builder.gd")
const Budget = preload("res://Game/Shared/render_budget.gd")

func _initialize() -> void:
	var office := Builder.build()
	var report: Dictionary = Budget.measure(office)
	print("OFFICE BUDGET: ", report)
	if not report.ok:
		office.free()
		quit(1)
		return
	_assign_owner(office, office)
	var packed := PackedScene.new()
	var result := packed.pack(office)
	if result == OK:
		var restored := packed.instantiate()
		var restored_report: Dictionary = Budget.measure(restored)
		restored.free()
		if restored_report != report:
			push_error("El horneado cambia la geometría: %s -> %s" % [report, restored_report])
			result = ERR_INVALID_DATA
		else:
			result = ResourceSaver.save(packed, "res://Game/Levels/office.tscn")
	office.free()
	print("OFFICE BAKE: ", error_string(result))
	quit(0 if result == OK else 1)

func _assign_owner(node: Node, owner_root: Node) -> void:
	for child in node.get_children():
		# Se hornea una copia editable. Conservar la instancia GLB y guardar
		# también sus hijos como nodos propios los duplica al reimportar.
		if not child.scene_file_path.is_empty():
			child.scene_file_path = ""
		child.owner = owner_root
		_assign_owner(child, owner_root)
