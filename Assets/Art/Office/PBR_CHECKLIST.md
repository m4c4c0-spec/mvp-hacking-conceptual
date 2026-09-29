# PBR checklist — hero props

## Materiales base (valores iniciales Standard / URP Lit)

| Material | Albedo | Metallic | Smoothness |
| --- | --- | --- | --- |
| M_Walnut | #61402A | 0 | 0.35 |
| M_SlateWall | #1E303A | 0 | 0.25 |
| M_Graphite | #080A0A | 0.4 | 0.55 |
| M_MintAccent | #38D4BA | 0.1 | 0.65 |
| M_Paper | #D1D6C7 | 0 | 0.2 |
| M_RackRed | #C8524D | 0.3 | 0.4 |

## Hero props (2K albedo opcional)

- `INT_laptop`: pantalla emissive navy + texto mint en Unity (`LaptopScreen` TextMesh).
- `INT_board` + tickets: superficie mate; contraste alto para números de misión.
- `INT_clue_folder`: dos variantes amber/cyan; bordes redondeados leves.
- `INT_atlas_rack`: rojo apagado; panel legible “NO ES TU CLIENTE”.

## Trim sheet entorno

- Una fila: borde madera, yeso, metal oscuro, goma suelo.
- UV shell compartido en `Office_Shell` para reducir draw calls.

## Antes de commit

- [ ] Normales hacia fuera (no flipped en puerta).
- [ ] Pivotes en bisagra / cajón probados en Unity `HubMechanism`.
- [ ] Tri count hub &lt; 150k (Stats en Scene view).
