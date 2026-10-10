extends RefCounted

## Marca los objetos de las tres salas para tomarlos y soltarlos.
## Puertas, suelo, muros y las personas grabadas se quedan fijos.

const Interactable = preload("res://Game/Shared/interactable.gd")

const FIXED_ACTIONS := ["exit", "labdoor", "archive_door", "safety", "guide", "restore", "board"]
const STRUCTURAL := ["Floor", "Wall", "Ceiling", "Lintel", "Partition", "Skirting", "Rail", "Window", "Corridor", "Bridge", "Sill", "Deck", "Jamb", "Hinge", "Door"]


static func arm(office: Node) -> void:
	if office == null:
		return
	for node in office.find_children("*", "StaticBody3D", true, false):
		var body := node as StaticBody3D
		if body == null or _blocked(body) or _structural(str(body.name)) or _too_big(body):
			continue
		if body is OfficeInteractable:
			var item := body as OfficeInteractable
			if FIXED_ACTIONS.has(item.action):
				continue
			item.grabbable = true
			_hint(item)
			continue
		if body.get_script() != null:
			continue
		body.set_script(Interactable)
		var carried := body as OfficeInteractable
		if carried == null:
			continue
		carried.action = "prop"
		carried.grabbable = true
		carried.prompt = "G tomar · G soltar"
		carried.set_physics_process(true)


static func _blocked(body: Node) -> bool:
	if body is CollisionObject3D and (body as CollisionObject3D).collision_layer == 2:
		return true
	var current := body
	while current:
		if current is OfficeGuide:
			return true
		current = current.get_parent()
	return false


static func _structural(node_name: String) -> bool:
	for token in STRUCTURAL:
		if node_name.contains(token):
			return true
	return false


static func _too_big(body: StaticBody3D) -> bool:
	for child in body.get_children():
		if not child is CollisionShape3D:
			continue
		var box := (child as CollisionShape3D).shape as BoxShape3D
		if box == null:
			continue
		if box.size.x > 3.0 or box.size.y > 3.0 or box.size.z > 3.0:
			return true
	return false


static func _hint(item: OfficeInteractable) -> void:
	if item.prompt.contains("tomar"):
		return
	item.prompt = item.prompt + " · G tomar · G soltar"
