class_name RoomFactory
extends RefCounted

const ROOM_PATHS: PackedStringArray = [
	"res://Game/Levels/sala1.gd",
	"res://Game/Levels/sala2.gd",
	"res://Game/Levels/sala3.gd",
]
const PHYSICS_PATH := "res://Game/Levels/condor_physics.gd"
const PROPS_PATH := "res://Game/Levels/prop_physics.gd"
const VISUALS_PATH := "res://Game/Levels/visual_pop.gd"


static func build(office: Node3D) -> Array:
	var rooms: Array = []
	if office == null:
		return rooms
	for path in ROOM_PATHS:
		var script := _optional(path)
		if script == null:
			continue
		rooms.append(script)
		script.call("build", office)
	return rooms


static func tune(office: Node, player: Node) -> void:
	var script := _optional(PHYSICS_PATH)
	if script:
		script.call("tune", office, player)


static func arm(office: Node) -> void:
	var script := _optional(PROPS_PATH)
	if script:
		script.call("arm", office)


static func dress(office: Node3D) -> void:
	var script := _optional(VISUALS_PATH)
	if script:
		script.call("attach", office)


static func handle(rooms: Array, main: Node, target: OfficeInteractable) -> bool:
	for script in rooms:
		if script == null:
			continue
		var handled = script.call("handle", main, target)
		if typeof(handled) == TYPE_BOOL and handled:
			return true
	return false


static func _optional(path: String) -> GDScript:
	if not ResourceLoader.exists(path):
		return null
	var loaded = load(path)
	return loaded if loaded is GDScript else null
