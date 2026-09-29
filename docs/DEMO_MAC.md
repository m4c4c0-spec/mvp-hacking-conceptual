# Presentación en Mac

## Preparación

El destino de compilación es `Builds/Mac/AnalystAcademy.app`: Universal (Intel x86_64 y Apple Silicon arm64), Mono, Metal, ventana inicial 1600 × 900 redimensionable. Las únicas escenas del juego son Boot y Hub; Academy es un prototipo aparte y no se incluye.

La compilación se realiza en Linux con Unity 6000.6.3f1 y su módulo oficial Mac Build Support (Mono). Generar una aplicación macOS no equivale a probarla en un Mac. Hacer la comprobación de arranque en el equipo de presentación antes de clase.

1. Transferir la aplicación completa, preferiblemente dentro del ZIP suministrado. No copiar solo el archivo de `Contents/MacOS`.
2. Extraer el ZIP en el Mac y abrir `AnalystAcademy.app`.
3. Entrar a la oficina y cerrar la introducción si aparece. Usar WASD y ratón; Shift para correr, E para usar, G para tomar/soltar y Esc para liberar el cursor y abrir el menú.
4. En Esc → Inicio, ajustar sensibilidad y campo de visión. Para recuperar la posición, usar «Volver a la entrada (conservar progreso)».

## Aviso de seguridad de macOS

Esta demo no está firmada con una identidad Apple Developer ID ni notarizada por Apple. Si macOS bloquea su apertura por desarrollador no identificado, y has comprobado que procede de esta entrega, Apple documenta la excepción por aplicación en **Ajustes del Sistema → Privacidad y seguridad → Abrir igualmente** después de intentar abrirla. No desactivar Gatekeeper globalmente. Una advertencia de malware o archivo dañado requiere revisar el paquete, no ignorarla.

Referencia: [Abrir aplicaciones de forma segura, Apple](https://support.apple.com/en-gb/102445).

## Recorrido sugerido de dos minutos

- Entrar y caminar: mostrar vista en primera persona, personajes y mobiliario.
- Acercarse a un ventanal: mostrar vidrio, marcos, calle, árboles y edificios con profundidad real.
- Pizarra: aceptar el primer ticket con E.
- Puerta del archivo: abrir con E, entrar y tomar una carpeta.
- Volver al escritorio: clasificar la pista en las bandejas; mostrar la terminal narrativa y el cuaderno.

El menú «Reiniciar demo» elimina el progreso del juego solo después de confirmación. No usarlo si se desea conservar una partida.

## Recompilar

Abrir el proyecto con su versión fijada y elegir **EthicalLab → Build → Mac universal first person**. Si se cambia el modelo procedural, ejecutar antes **EthicalLab → Office → Prepare modeled scene** y volver a comprobar las escenas y pruebas. La configuración de Higgsfield es de producción y no debe empaquetar secretos ni requerir conexión a internet en el aula.
