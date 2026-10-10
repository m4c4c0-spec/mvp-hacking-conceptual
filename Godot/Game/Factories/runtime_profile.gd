class_name RuntimeProfile
extends RefCounted

## Fábrica abstracta. Cada subclase arma una familia que funciona junta:
## el jugador y si la interfaz va en el espacio.
var notice := ""


func prepare(_host: Node) -> void:
	pass


func create_player() -> Node3D:
	return null


func uses_spatial_ui() -> bool:
	return false


func mount(_host: Node, _player: Node3D, _ui: EscapeUI, _on_suspend: Callable, _on_focus: Callable) -> Node3D:
	return null
