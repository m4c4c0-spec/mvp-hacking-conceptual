extends Node3D
class_name OfficeGuide

## Grabaciones de la UCD: orientan en la oficina y no resuelven ningún acertijo.
const ROLES := [
	{"name": "VERA", "zone": "Recepción", "action": "Señala el corcho de correos", "lesson": "Grabación de la UCD. En el corcho del fondo hay cuatro correos. Compara el remitente letra a letra y fíjate si piden una clave o apuran el plazo. Leer esta grabación no marca las señales ni destapa un dígito."},
	{"name": "HUGO", "zone": "Pizarra", "action": "Recuerda el reloj de la operación", "lesson": "Grabación de la UCD. Este reloj marca el tiempo de la operación. La oficina es para detectar. Escucharme no anota un hallazgo ni abre una puerta."},
	{"name": "NURIA", "zone": "Archivo", "action": "Deja a la vista el registro del turno", "lesson": "Grabación de la UCD. La hoja del archivo tiene doce líneas: cuenta, hora y privilegio. La hora que no encaja se elige ahí. Yo no la señalo."},
	{"name": "ASISTENTE TÉCNICO", "zone": "Escritorio", "action": "Muestra las fichas de la clave", "lesson": "Grabación de la UCD. En el escritorio están el post-it, el banderín y el calendario. La sesión se arma con fichas, sin teclado libre. No te dicto la clave."},
	{"name": "IRIARTE", "zone": "Salida", "action": "Vigila el token y la puerta magnética", "lesson": "Grabación de la UCD. Junto a la puerta de servidores hay un token: la pantalla dice la hora del último ingreso y la ruleta se gira con botones. Esa puerta no abre sin el segundo factor. La salida de emergencia del corredor sigue abierta. Consultarme no la usa ni entrega el código."},
]

@export_range(0, 4) var role: int = 0
var elapsed := 0.0
@onready var left_arm: Node3D = $LeftArm
@onready var right_arm: Node3D = $RightArm
@onready var left_elbow: Node3D = $LeftArm/Elbow
@onready var right_elbow: Node3D = $RightArm/Elbow
@onready var head: Node3D = $Head
@onready var carried_prop: Node3D = get_node_or_null("LeftArm/Elbow/Hand/CarriedProp")

func _ready() -> void:
	elapsed = role * 0.7
	_animate()

func _process(delta: float) -> void:
	elapsed += delta
	_animate()

func _animate() -> void:
	var beat := sin(elapsed * 1.8)
	var slow := sin(elapsed * 0.7)
	head.rotation = Vector3(0.09, slow * 0.09, 0)
	left_arm.rotation = Vector3(-0.85, 0, -0.12)
	left_elbow.rotation.x = -0.75
	right_arm.rotation = Vector3(-0.65, 0, 0.12)
	right_elbow.rotation.x = -0.9
	match role:
		0: # Portapapeles sostenido; comprueba renglones con el lápiz.
			right_arm.rotation.x = -0.85 + beat * 0.06
			right_arm.rotation.y = -0.75
			right_elbow.rotation.x = -0.83 + beat * 0.08
			head.rotation.x = 0.12 + slow * 0.05
		1: # Brazo extendido señalando; el otro sostiene las notas.
			right_arm.rotation = Vector3(-1.55 + slow * 0.12, -1.45, 0.18)
			right_elbow.rotation.x = -0.15 + beat * 0.05
			head.rotation.y = -0.3 + slow * 0.08
		2: # La carpeta va desde el pecho hasta el casillero y vuelve.
			var reach := (sin(elapsed * 0.9) + 1.0) * 0.5
			left_arm.rotation.x = -0.8 - reach * 0.55
			left_elbow.rotation.x = -0.85 + reach * 0.65
			right_arm.rotation.x = left_arm.rotation.x
			right_elbow.rotation.x = left_elbow.rotation.x
			head.rotation.y = slow * 0.18
		3: # Teclea alternando las manos sobre un equipo físico.
			left_arm.rotation.x = -0.62 + beat * 0.04
			right_arm.rotation.x = -0.62 - beat * 0.04
			left_elbow.rotation.x = -0.3 + beat * 0.08
			right_elbow.rotation.x = -0.3 - beat * 0.08
			head.rotation.x = 0.22
		4: # Tableta fija a una mano, la otra pulsa controles de revisión.
			right_arm.rotation.x = -0.85 + beat * 0.08
			right_arm.rotation.y = -0.75
			right_elbow.rotation.x = -0.8 + beat * 0.15
			head.rotation.x = 0.16
	if carried_prop:
		# Mantiene el documento legible mientras se flexionan hombro y codo.
		carried_prop.rotation.x = -(left_arm.rotation.x + left_elbow.rotation.x)
