extends SceneTree

const Finish = preload("res://Game/Shared/surface_finish.gd")
const Budget = preload("res://Game/Shared/render_budget.gd")

func _initialize() -> void:
	# Edita los materiales guardados sin volver a construir la oficina.
	var office := (load("res://Game/Levels/office.tscn") as PackedScene).instantiate()
	var before := Budget.measure(office)
	Finish.refresh_scene(office)
	Finish.tune_lighting(office)
	var after := Budget.measure(office)
	if not after.ok or before.triangles != after.triangles:
		push_error("Actualizar acabados no debe alterar geometría ni presupuesto.")
		office.free()
		quit(1)
		return
	var packed := PackedScene.new()
	var result := packed.pack(office)
	if result == OK:
		result = ResourceSaver.save(packed, "res://Game/Levels/office.tscn")
	office.free()
	print("MATERIALS: ", error_string(result), " · ", after.triangles, " triángulos")
	quit(0 if result == OK else 1)
