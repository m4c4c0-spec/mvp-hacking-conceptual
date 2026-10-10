class_name InteractableFactory
extends RefCounted

## Un interactuable nace con identidad de uso. La malla y la colisión las pone quien lo coloca.


static func create(node_name: String, action: String, argument: int, prompt: String, grabbable := false) -> OfficeInteractable:
	var body := OfficeInteractable.new()
	body.name = node_name
	body.action = action
	body.argument = argument
	body.prompt = prompt
	body.grabbable = grabbable
	return body
