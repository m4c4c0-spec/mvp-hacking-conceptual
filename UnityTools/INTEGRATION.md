# Integración de la entrega Unity Academy

Durante esta implementación aparecieron en paralelo el cliente web y las capas `Assets/_Project/EthicalLab`. Se conservaron. La entrada de esta entrega es **Assets/Scenes/Academy.unity**; el proyecto alternativo usa Boot/Hub. No mezclar sus sesiones ni sus guardados.

Correcciones mínimas de integración realizadas al validar todos los C# de Assets:

- Se añadió la llave de cierre que faltaba en el switch de `Assets/_Project/Domain/MissionCatalogRules.cs`; impedía analizar/compilar el archivo.
- Se movieron únicamente los tests Academy a `Assets/Tests/EditMode/Academy/`, para que no convivan dos asmdefs en el mismo directorio con `EthicalLab.Tests`.
- Se calificó `UnityEngine.Application` en LabBootstrap/JsonProgressStore para evitar colisión con el namespace `EthicalLab.Application`.
- El autoinicio de LabBootstrap se limita a Boot/Hub para no generar una segunda oficina al correr tests en una escena vacía.

Estas correcciones no convierten la comprobación estática en una compilación Unity. Ver `UNITY_README.md` para las instrucciones de importación, tests y build de Academy.
