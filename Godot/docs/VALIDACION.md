# Validación · Texturas y aplicación de conceptos · 2026-10-07

Editor utilizado: Godot 4.6.3 Standard oficial para Linux. Renderer Compatibility con el perfil Desktop8Gb.

- Importación de scripts y escena principal: correcta.
- `tests/test_session.gd`: **339 comprobaciones**, sin fallos. Cuatro fases, bloqueos, error/reintento, observación/defensa, desbloqueo de conceptos, expediente/quiz, controles ordenados, victoria, guardado/carga y reinicio. Incluye guardados truncados, vacíos y JSON `null`: se rechazan, se conservan y deshabilitan autosave.
- `tests/test_world.gd`: **102 comprobaciones**, sin fallos. UI real y objetos físicos: aceptar fases, tomar/devolver carpeta, bandejas, expediente y preguntas mediante botones, activar seguros y abrir puerta. Incluye los cinco guías, movimiento de manos, ayuda contextual sin cambiar progreso, retorno a primera persona, alcance por raycast y presupuesto geométrico.
- Aplicación de conceptos: ocho situaciones con decisión y justificación; el quiz solo no activa seguros. Se verifican errores, reintentos, orden de etapas, ayuda sin avance automático, guardado entre acción y justificación, rechazo de seguros sin aplicación y migración desde v1 sin perder pistas ni expedientes.
- Materiales: madera, yeso, suelo y sillas tienen textura con filtrado anisotrópico y proyección triplanar; se comprueban texturas de hasta 1024 px.
- Escena de oficina con assets mejorados, cinco guías y detalle GLB: **49.572 triángulos**. El horneado comprueba que empaquetar e instanciar conserve la geometría, para impedir duplicación de escenas importadas. Sin mapas normales y sin texturas grandes. Ese conteo no mide rendimiento ni memoria total en Quest.
- Capturas de render: [oficina PC](previews/office-pc.png) y [pantalla de inicio](previews/welcome-pc.png), obtenidas ejecutando Godot con GPU local. También se revisaron [materiales de cerca](previews/materials-pc.png) y [panel de aplicación](previews/application-pc.png).
- [Capturas de cada guía y documentación de assets](ASSETS_Y_GUIAS.md).
- Export anterior, no actualizado en esta entrega: paquete PCK exportado en `Builds/Godot/Linux/ProtocoloSalida.pck`: arranque comprobado desde `/tmp`, sin depender de los archivos fuente. Es un paquete de desarrollo que requiere Godot; no sustituye un ejecutable independiente.
- Catálogo único en `Game/Features/Quests/missions.json`, comprobado con los validadores de contenido y flujo.
- Rutas, clases duplicadas y filtros de exportación: comprobados por `scripts/check-godot-structure.py`, integrado en arranque, tests y export. El paquete incluye catálogo y ambos avisos de licencia de `ThirdParty/godot/`.
- Shell scripts de inicio, pruebas y exportación: sintaxis comprobada.

Las pruebas automáticas recorren las acciones; no acreditan un playtest manual completo con teclado/ratón. Mac, APK Android y visor Quest 3S todavía requieren exportar con las templates/SDK correspondientes y probar hardware. Se ha importado una primera capa compactada del detalle geométrico original en `Game/Levels/office_detail.glb`.

Para repetir:

```bash
bash scripts/test-godot.sh
```

El script detecta la copia local de Godot en `.tools/` o el editor instalado, y admite `GODOT_BIN`. Las partidas de prueba usan una ruta separada y no reemplazan el guardado del jugador.

Para vigilar cambios y repetir automáticamente la validación:

```bash
python3 scripts/colab_watch.py --watch --validate
```

Se inicia desde la raíz del repositorio, requiere Python 3, Node.js y Godot, y se detiene con Ctrl+C. Ignora cachés, builds y capturas; espera que los archivos dejen de cambiar antes de validar. Resultados en `docs/COLABORACION.md` y estado local en `docs/.colab-state.json`. [Revisión de la reorganización](../../docs/REVISION_EN_VIVO.md).
