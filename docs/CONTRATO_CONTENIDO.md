# Contrato de contenido

El prototipo **no duplica** misiones. **`Assets/Resources/Content/missions.json` es la única fuente de verdad** (Unity `Resources.Load("Content/missions")`). Si el esquema cambia, avisar antes de romper el cliente.

Tres superficies leen el mismo archivo: web (`web/js`), EthicalLab (`JsonMissionRepository`) y Academy (`Academy.ContentLoader`). Academy prefiere el catálogo de ScriptableObjects si existe; por eso esos assets (`Assets/Content/Missions`, `Assets/Content/Concepts`, `Assets/Resources/MissionCatalog.asset`) son **una vista generada, no se editan a mano**:

- Al guardar `missions.json` en el editor, `EthicalLab.Editor.ContentSync` regenera los assets (sobrescribe `data`, reordena el catálogo, reporta huérfanos sin borrarlos).
- Menú `EthicalLab/Contenido/Regenerar ScriptableObjects desde JSON` fuerza la regeneración; `Verificar coherencia` marca error si algún asset difiere.
- Al abrir el editor, cualquier divergencia sale como warning con la lista de campos.
- El menú `Academy/Contenido/Importar…` (conserva existentes) sigue funcionando pero ya no es el camino: solo sirve para la primera importación.

Quien quiera cambiar una misión edita el JSON. Cambiar un `.asset` es un error que el editor va a señalar.

El guardado Unity (`SaveData`) incluye `muted` y `reducedMotion` (pausa de oficina). El web los persiste; no afectan el score.

## Raíz

| Campo | Tipo | Uso |
| --- | --- | --- |
| `title` | string | Nombre del producto |
| `version` | number | Incrementar si cambia el esquema |
| `concepts[]` | Concept | Glosario global |
| `missions[]` | Mission | Orden = progreso del hub |

## Concept

`id`, `name`, `category`, `definition`, `importance`, `defense`

Los `id` actuales: `recon`, `scope`, `phishing`, `verification`, `passwords`, `mfa`, `surface`, `transport`.

## Mission

`id`, `number`, `type`, `title`, `subtitle`, `client`, `duration` (`N min`), `accent` (`cyan` \| `amber` \| `violet` \| `green`), `concepts[]`, `brief`, `mentor`, `scope`, `objective`, `commandHint`, `evidence[]`, `report`, `quiz[]` (3), `learned`

## Evidence

`id`, `label`, `kind`, `icon` (`map` \| `shield` \| `note` \| `mail` \| `key` \| `globe` \| `archive`), `source`, `body`, `question`, `options[]`, `answer` (índice), `reasonQuestion`, `reasons[]`, `reasonAnswer`, `feedback`, `concept`

Al resolver una pista se desbloquea la ficha `concept`. No hay campos para payloads ni comandos reales.

## Report / Quiz

- `report.prompt`, `report.options`, `report.answer`
- `quiz[].question`, `options`, `answer`, `explanation`

## Límites éticos del JSON

- Dominios de ficción en `.invalid`.
- Enlaces inertes; el juego no envía correo ni hace peticiones a los hosts del lore.
- Alcance y autorización van como concepto, no como “truco para saltárselo”.
