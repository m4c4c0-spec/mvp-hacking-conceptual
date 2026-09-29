# Precio ilustrativo del MVP

Fecha: **2026-09-27**. Esto no es una cotización contractual ni una oferta. Es un rango de esfuerzo para el producto que el repo entrega hoy, más el hueco que falta para mostrarlo a alguien que no lo construyó.

El producto cotizado es **EthicalLab en PC**: escenas `Boot` y `Hub`, cuatro misiones en `Assets/Resources/Content/missions.json`, oficina en primera persona, UI uGUI/TextMesh Pro y build Linux. `Assets/Scenes/Academy.unity` es un prototipo congelado y no entra en las horas. Tampoco entran `Builds/` ni `Library/`.

## Alcance que ya está en el repo

| Bloque | Qué hay | Horas hechas |
| --- | ---: | ---: |
| Arquitectura | Capas `EthicalLab.Shared`, `Domain`, `Application`, `Infrastructure`, `Presentation` y `Editor`. Domain y Application con `noEngineReferences`. Casos de uso, persistencia JSON, terminal narrativa y 5 pruebas EditMode en `UseCaseTests.cs`. | 64 |
| Hub 3D | Oficina generada en `HubOffice` + `HubOfficeDressing`, prefab `Assets/Art/Office/Prefabs/Office_AnalystAcademy.prefab`, escenas `Hub` y `Hub_Art`. `HubSceneLoader` usa el prefab si hay `HubSceneRefs` válidos; si no, `HubOffice.BuildProceduralLegacy()`. | 72 |
| Interacción | Primera persona (`PcInteractor`, `CharacterController`), puerta y cajón, carpetas, sillas con `Rigidbody`, coach (`MovementCoach`) y manos (`FirstPersonHands`). | 56 |
| Contenido | 4 misiones, 15 pistas, 8 conceptos, 12 preguntas de quiz. Una sola fuente: `missions.json`. Validadores en `scripts/` y `UnityTools/validate_project.py`. | 40 |
| UI | Menú ESC, bienvenida, expediente, glosario y terminal en uGUI/TMP (`HubHud`, `HubMenuUi`, `HubOnboardingOverlay`, `HubStartScreen`). | 44 |
| Build y pruebas automáticas | Menú cuyo método es `EthicalLab.Editor.HubDesktopBuild.Linux` (Boot + Hub). `scripts/play-linux.sh`. El 2026-09-27: 17 pruebas PlayMode y build Linux en `docs/previews/VALIDACION.md`. | 36 |
| Arte blockout | Materiales en `Assets/Art/Office/Materials/Generated/`, medidas en `blockout_dimensions.json`, guías en `docs/ART_BIBLE.md` y `Assets/Art/Office/`. Geometría de Unity, no mallas PBR finales. | 28 |
| **Hecho** | | **340** |

La bitácora y el historial git comprimen ese trabajo en pocos días (21–27 de septiembre de 2026). Las horas son esfuerzo de estudio para producir el mismo alcance, no el tiempo de reloj de los commits.

## Hueco para un MVP mostrable a extraños

`docs/ROADMAP.md` y `UnityTools/PLAYTEST_ETHICALLAB.md` dejan fuera del “ya está” tres cosas:

| Hueco | Horas | Supuesto |
| --- | ---: | --- |
| Playtest humano y ajustes chicos | 32 | Checklist de `PLAYTEST_ETHICALLAB.md`: mouse real, Alt-tab, máquina limpia, una misión completa en el ejecutable. Sin rehacer el loop. |
| 2 misiones más | 20 | Solo datos en `missions.json`, mismos tipos de paso. El roadmap las ubica en el mes 2–3. |
| Pulido de presentación | 48 | Legibilidad, audio, primeros minutos y correcciones del playtest. Sigue siendo blockout. |
| **Falta** | **100** | |
| **Mostrable** | **440** | 340 hechas + 100 |

## Tarifas

Dos tarifas blended, explícitas e ilustrativas. No salen de una encuesta de mercado.

| Caso | USD / hora | Qué representa |
| --- | ---: | --- |
| Estudio indie LatAm | 35 | Equipo chico de la región: programación, diseño de contenido y QA ligero. |
| Estudio senior | 110 | Estudio con dirección senior, producción y revisión. Banda alta ilustrativa frente a la indie. |

## Tipo de cambio

Consultado el **2026-09-27** en [dolarapi.com](https://dolarapi.com/v1/dolares):

- **Oficial venta: 1.545 ARS por USD.** `fechaActualizacion`: 2026-09-25T18:55:00.000Z. El 27 es domingo; esta es la última venta oficial publicada ahí.
- Blue venta el mismo día: **1.560** (actualizado 2026-09-27T20:57:00.000Z).

La conversión a pesos usa el **oficial venta 1.545**. El blue mueve el total cerca de un 1 %.

## Tabla

Horas del paquete mostrable (hecho + hueco). USD bajo = horas × 35. USD alto = horas × 110.

| Bloque | Horas | USD bajo | USD alto |
| --- | ---: | ---: | ---: |
| Arquitectura | 64 | 2.240 | 7.040 |
| Hub 3D | 72 | 2.520 | 7.920 |
| Interacción | 56 | 1.960 | 6.160 |
| Contenido y misiones | 60 | 2.100 | 6.600 |
| UI | 44 | 1.540 | 4.840 |
| Build y pruebas automáticas | 36 | 1.260 | 3.960 |
| Playtest | 32 | 1.120 | 3.520 |
| Arte blockout | 28 | 980 | 3.080 |
| Pulido para extraños | 48 | 1.680 | 5.280 |
| **Total mostrable** | **440** | **15.400** | **48.400** |

En pesos, al oficial venta 1.545: **ARS 23.793.000 – 74.778.000**.

Solo lo ya hecho (340 h), sin playtest ni dos misiones ni pulido: **USD 11.900 – 37.400** (ARS 18.385.500 – 57.783.000 al mismo tipo).

## Fuera de este rango

- VR y `XrInteractor` más allá del stub.
- Localización (el roadmap la deja para el mes 2–3, en otro JSON).
- Multijugador o netcode.
- Arte final Blender PBR, texturas y el reemplazo de módulos descrito en `Assets/Art/Office/PBR_CHECKLIST.md` y `EXPORT_FBX.md`.
- Exploits, payloads o herramientas reales. La terminal rechaza ese vocabulario a propósito.
- Tienda (Steam, itch), trailer y página de venta. `docs/STEAM.md` queda fuera.
- Mantener `Academy.unity`, `Assets/Scripts/` de Academy y el prototipo `web/` como segundo producto. Existen en el repo; no están en las 440 h.
- Build de Windows. El menú existe (`HubDesktopBuild.Windows`); en la validación del 27 de septiembre el módulo no estaba instalado.

## Qué mueve el número

1. **La tarifa.** 35 frente a 110 USD/h multiplica todo por 3,1. Es la palanca más grande del rango en dólares.
2. **Las 100 h del hueco, y que sigan siendo hueco chico.** Esas horas asumen playtest con ajustes menores, dos misiones que caben en el esquema actual y pulido de blockout. Si el playtest obliga a rehacer la física o la primera persona, o si el pulido se lee como arte PBR, este techo no cubre el trabajo.
3. **Que no hay timesheet.** Las 340 h hechas son juicio sobre el alcance del código, no horas registradas. Un ±25 % sobre lo hecho mueve el paquete mostrable a unas 355–525 h, es decir unos USD 12.400–57.800 con las mismas tarifas.
