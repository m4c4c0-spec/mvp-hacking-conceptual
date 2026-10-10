# Arquitectura del juego

El ejecutable es Godot. La carpeta `Godot/` es el proyecto. Dentro, el código sigue esta estructura.

| Carpeta | Qué hay |
| --- | --- |
| `Game/Bootstrap/` | `main.gd`. Pide a las fábricas el jugador, las salas y la interfaz, y las conecta con la sesión. |
| `Game/Factories/` | Creación. `RuntimeFactory` elige la familia PC u OpenXR. `RoomFactory` monta las salas. `InteractableFactory` crea objetos usables. |
| `Game/Features/Player/` | Primera persona de PC y base OpenXR. |
| `Game/Features/Combat/` | Vacía. Este escape room no tiene combate. |
| `Game/Features/Inventory/` | Vacía. Las pruebas se llevan en la mano, sin ranuras. |
| `Game/Features/Enemies/` | Vacía. No hay enemigos. |
| `Game/Features/Quests/` | Reglas de fases, terminal y `missions.json`. |
| `Game/Shared/` | Interacción, interfaz, presupuesto de render y albedos 1K. |
| `Game/Infrastructure/` | Lectura del catálogo y guardado atómico de la partida. |
| `Game/Levels/` | Oficina, constructor y guías. |
| `ThirdParty/godot/` | Avisos de licencia del motor. |
| `docs/` | Diseño, historia y esta guía. El nombre ya existía en minúsculas. |

El catálogo jugable es `Godot/Game/Features/Quests/missions.json`. Una fase nueva se escribe ahí y se valida con `scripts/validate-mission-flow.sh`.
