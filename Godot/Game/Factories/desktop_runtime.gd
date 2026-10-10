class_name DesktopRuntime
extends RuntimeProfile

## Familia de escritorio: primera persona con teclado y ratón.


func create_player() -> Node3D:
	return DesktopPlayer.new()
