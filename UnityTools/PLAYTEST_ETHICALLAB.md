# Prueba manual — EthicalLab Hub / Hub_Art

Estado: checklist manual pendiente para Unity 6000.0.62f1. El 27 de septiembre de 2026 pasaron 17 pruebas automatizadas en Unity 6000.6.3f1, incluyendo las cuatro misiones, persistencia y siete pruebas de teclado/mouse virtuales a través del controlador. También se generó y abrió el ejecutable Linux. No sustituye esta pasada humana; alcance y evidencias en `docs/previews/VALIDACION.md`. Automatizado sin editor: `scripts/test-dotnet.sh`, `python3 UnityTools/validate_project.py`.

## Apertura

1. Abrir proyecto; Console sin errores de compilación.
2. Play en `Assets/Scenes/Boot.unity`: carga Hub con el prefab de oficina. `Hub_Art.unity` comparte ese modelo para edición artística.
3. Pantalla de entrada: Entrar a la oficina o Continuar. Primera visita: panel de introducción; comprobar que no permite caminar hasta cerrarlo.

## Movimiento e input

1. WASD, Shift, mouse para mirar, E/click usar, G tomar/soltar, ESC menú, H/F1 ayuda.
2. No atravesar paredes; recuperación si caes bajo Y=-3.
3. Puerta archivo: E abre/cierra; cruzar con CharacterController.
4. Cajón: E abre; tomar USB con G; devolver con G.
5. ESC: no caminar ni mirar detrás del menú. Alt-tab pausa; volver a la ventana no debe capturar el cursor hasta reanudar.
6. ESC → Inicio: sensibilidad, FOV e inversión Y se aplican y persisten. Restaurar cámara recupera 1× / 70° / no invertido.
7. «Volver a la entrada» conserva la misión, devuelve cualquier carpeta y recupera la posición inicial.

## Bucle pedagógico 3D (misión 01)

1. Pizarra: E en ticket 01 → toast con alcance.
2. Archivo: leer carpeta (E), tomar (G), bandejas observación + defensa.
3. Panel PISTAS en pared refleja estado.
4. Servidor Atlas (rojo): E → mensaje fuera de alcance, sin castigo.
5. Buzón informe: solo listo cuando todas las pistas documentadas → expediente.
6. Completar informe + quiz → toast MISIÓN CERRADA, ticket verde en pizarra.

## Menú uGUI (ESC)

1. Pestañas Inicio, Misiones, Terminal, Expediente, Glosario, Oficina.
2. Reiniciar demo (dos pasos) borra progreso y vuelve a oficina.
3. Terminal: escribir `help`, enviar con Enter o Ejecutar; escribir una E no debe ejecutar. Rechaza `nmap`, `curl`, etc.; no ejecuta comandos del sistema.

## Progresión

1. Ticket 04 bloqueado al inicio; 02 tras cerrar 01.
2. Salir/reentrar Play: progreso persiste (JsonProgressStore).
3. Completar 4 misiones: 8 conceptos, puntuación en pizarra.

## Presentación 18–32

1. Resoluciones 1280×720, 1920×1080: textos legibles, sin recortes graves.
2. Encuesta informal 3 testers 20–30 años: “simulador de analyst” ≥ 2/3; Atlas comprendido ≥ 2/3.

## Build y pitch

1. **EthicalLab → Build → Linux first person** (o Windows con su módulo): incluye Boot → Hub. En Linux, `bash scripts/play-linux.sh` abre la aplicación.
2. Ejecutable en máquina limpia: movimiento + una misión.
3. Capturas y clip 30 s: ver `docs/PITCH_CAPTURE.md`.

## Regresión rápida CI local

```bash
scripts/test-dotnet.sh
python3 UnityTools/validate_project.py
node scripts/test-session.mjs
```
