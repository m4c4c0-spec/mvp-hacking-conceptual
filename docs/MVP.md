# Plan MVP adaptado (4 semanas, dos personas)

Pitch: laboratorio narrativo. El jugador es junior en una consultora; cada misión enseña qué es el concepto, por qué importa y cómo defenderlo. En PC es escritorio + terminal estilizada; en VR será la misma oficina.

## Semana 1 — Núcleo jugable

- Hub: escritorio, laptop, pizarra de misión, buzón de tickets.
- Tutorial: “¿Qué es el reconocimiento?”
- Terminal abstracta: `scan`, `inspect`, `map`, `notes`, `report`, `scope`.
- Glosario: cada hallazgo desbloquea definición + importancia + defensa.

**Hecho en contenido:** 4 misiones y 8 fichas en `Assets/Resources/Content/missions.json`.  
**Hecho en Unity:** sesión, terminal ficticia, oficina procedimental, stub XR (`Assets/Scripts/`).  
**Hecho en web:** el mismo loop jugable en el navegador, espejando esas reglas.

## Semana 2 — Tres misiones conceptuales

1. Ingeniería social: clasificar mensajes y explicar el engaño.
2. Contraseñas y autenticación: políticas del laboratorio, no cracking.
3. Superficie web: inventario y reporte de hardening.

El JSON ya cubre las tres. El cliente no añade payloads ni herramientas reales.

## Semana 3 — Loop de negocio + polish

- Score: precisión de evidencia/informe, uso del glosario, tiempo razonable.
- Cierre: pantalla “Concepto aprendido” + quiz de 3 preguntas (viene en el JSON).
- Demo 10–15 min.
- Stub XR: mismos ids de interactable.

## Semana 4 — Empaquetado

- Steam Coming Soon / itch: ver `docs/STEAM.md`.
- Precio tentativo $9.99–$14.99 o demo gratis + full de pago.
- Roadmap visible: redes, permisos, phishing avanzado conceptual, forense light.

## Criterio de “MVP listo”

Un jugador nuevo termina tutorial + 3 misiones, entiende 6–8 conceptos con defensas, y deja wishlist o review. Hay base de datos de misiones ampliable (`Assets/Resources/Content/missions.json`) y stub XR (`XRInteractionBridge`) sin haber comprado visor.

## Stack

- **Web (sin casco, sin Editor):** HTML/CSS/JS + el JSON de Resources. Jugable para demo e itch.
- **Unity 6** (`6000.0.62f1`): Input System + uGUI. Menú `Academy/` importa JSON a ScriptableObjects y arma builds Linux/Windows. Escena `Assets/Scenes/Academy.unity`. XR se enchufa después en `XRInteractionBridge`.
