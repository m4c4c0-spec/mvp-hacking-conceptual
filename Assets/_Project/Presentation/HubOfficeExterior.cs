using UnityEngine;
using UnityEngine.Rendering;

namespace EthicalLab.Presentation
{
    public static partial class HubOffice
    {
        // Huecos reales: los marcos no ocultan un muro sólido detrás del vidrio.
        static void WindowWall(Transform root, Material plaster)
        {
            Box(root, "Window wall parapet", new Vector3(-6.6f, 0.54f, -0.5f), new Vector3(0.2f, 1.08f, 13f), plaster);
            Box(root, "Window wall lintel", new Vector3(-6.6f, 3.465f, -0.5f), new Vector3(0.2f, 0.87f, 13f), plaster);
            float start = -7f;
            for (int i = 0; i < 5; i++)
            {
                float end = i < 4 ? -4.6f + i * 2.6f - 0.73f : 6f;
                Box(root, "Window wall pier " + i, new Vector3(-6.6f, 2.055f, (start + end) * 0.5f),
                    new Vector3(0.2f, 1.95f, end - start), plaster);
                start = end + 1.46f;
            }
        }

        static void DressWindows(Transform architecture, Material trim, Material timber, Material pale)
        {
            var glass = new Material(Shader.Find("Standard")) { name = "Clear window glass", color = new Color(0.72f, 0.85f, 0.91f, 0.075f) };
            glass.SetFloat("_Mode", 3);
            glass.SetInt("_SrcBlend", (int)BlendMode.One);
            glass.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
            glass.SetInt("_ZWrite", 0);
            glass.EnableKeyword("_ALPHAPREMULTIPLY_ON");
            glass.SetFloat("_Glossiness", 0.85f);
            glass.renderQueue = 3000;
            for (int i = 0; i < 4; i++)
            {
                var window = Group(architecture, "Window " + (i + 1), new Vector3(-6.52f, 2.05f, -4.6f + i * 2.6f));
                foreach (float y in new[] { -0.94f, 0.94f })
                    Detail(window, "Frame horizontal", new Vector3(0, y, 0), new Vector3(0.18f, 0.075f, 1.55f), trim);
                foreach (float z in new[] { -0.735f, 0.735f })
                    Detail(window, "Frame vertical", new Vector3(0, 0, z), new Vector3(0.18f, 1.88f, 0.075f), trim);
                var pane = Detail(window, "Transparent glazing", Vector3.zero, new Vector3(0.016f, 1.83f, 1.4f), glass);
                pane.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;
                pane.layer = 0;
                pane.AddComponent<BoxCollider>(); // Contención de jugador y sillas, independiente de transparencia.
                Detail(window, "Mullion", new Vector3(0.025f, 0, 0), new Vector3(0.06f, 1.86f, 0.035f), trim);
                Detail(window, "Sill", new Vector3(0.08f, -0.98f, 0), new Vector3(0.35f, 0.065f, 1.66f), timber);
                Detail(window, "Window handle", new Vector3(0.07f, -0.15f, 0.06f), new Vector3(0.055f, 0.15f, 0.025f), trim);
                Detail(window, "Blind housing", new Vector3(0.12f, 0.95f, 0), new Vector3(0.16f, 0.08f, 1.58f), pale);
                for (int j = 0; j < 4; j++)
                {
                    var slat = Detail(window, "Blind slat", new Vector3(0.12f, 0.81f - j * 0.11f, 0), new Vector3(0.12f, 0.018f, 1.43f), pale);
                    slat.transform.localRotation = Quaternion.Euler(0, 0, -18);
                }
            }
        }

        static void DressExterior(Transform parent)
        {
            var exterior = Group(parent, "Exterior courtyard and city");
            var stone = Mat("Courtyard stone", new Color(0.52f, 0.54f, 0.52f));
            var road = Mat("Street asphalt", new Color(0.22f, 0.25f, 0.28f));
            var facade = Mat("Exterior limestone plaster", new Color(0.68f, 0.65f, 0.58f));
            var blue = Mat("Exterior blue grey plaster", new Color(0.46f, 0.55f, 0.61f));
            var distant = Mat("Distant skyline plaster", new Color(0.57f, 0.64f, 0.69f));
            var glazing = Mat("Exterior glazing", new Color(0.26f, 0.40f, 0.48f));
            var foliage = Mat("Courtyard foliage", new Color(0.28f, 0.39f, 0.26f));
            var bark = Mat("Courtyard walnut bark", new Color(0.29f, 0.23f, 0.18f));
            Detail(exterior, "Ground", new Vector3(-47, -4.4f, 0), new Vector3(80, 0.3f, 140), stone);
            Detail(exterior, "Street", new Vector3(-20, -4.23f, 0), new Vector3(9, 0.02f, 130), road);
            Detail(exterior, "Sidewalk", new Vector3(-12, -4.15f, 0), new Vector3(6, 0.15f, 85), stone);
            for (int i = 0; i < 12; i++)
                Detail(exterior, "Street marking", new Vector3(-20, -4.20f, -45 + i * 8), new Vector3(0.12f, 0.015f, 3.2f), stone);
            for (int i = 0; i < 7; i++)
            {
                float z = -40 + i * 13;
                float height = 7.5f + (i * 7 % 5) * 1.35f;
                var building = Group(exterior, "Neighbour building " + i, new Vector3(-33 - i % 2 * 4, -4.1f, z));
                Detail(building, "Facade", new Vector3(0, height / 2, 0), new Vector3(10, height, 10), i % 2 == 0 ? facade : blue);
                Detail(building, "Roof coping", new Vector3(0, height + 0.07f, 0), new Vector3(10.25f, 0.15f, 10.25f), stone);
                Detail(building, "Rooftop equipment", new Vector3(-1, height + 0.55f, 1), new Vector3(2.5f, 1, 1.5f), blue);
                for (int row = 0; row < (int)(height / 2.5f); row++)
                    for (int col = 0; col < 4; col++)
                        Detail(building, "Facade window", new Vector3(5.025f, 1.6f + row * 2.5f, -3.5f + col * 2.3f), new Vector3(0.025f, 1.4f, 1.3f), glazing);
                Detail(exterior, "Distant city " + i, new Vector3(-64 - i % 3 * 5, height * 0.65f - 4, z + 4), new Vector3(12, height * 1.3f, 9), distant);
            }
            for (int i = 0; i < 7; i++)
            {
                var tree = Group(exterior, "Courtyard tree " + i, new Vector3(-12.3f, -4.0f, -23 + i * 9));
                Detail(tree, "Trunk", new Vector3(0, 1.7f, 0), new Vector3(0.23f, 1.7f, 0.23f), bark, PrimitiveType.Cylinder);
                Detail(tree, "Crown", new Vector3(0, 3.8f, 0), new Vector3(3.2f, 3.3f, 3.1f), foliage, PrimitiveType.Sphere);
                Detail(tree, "Crown offset", new Vector3(0.8f, 3.3f, 0.7f), new Vector3(2.4f, 2.4f, 2.4f), foliage, PrimitiveType.Sphere);
                Detail(tree, "Planter", new Vector3(0, 0.12f, 0), new Vector3(2.1f, 0.24f, 2.1f), stone);
            }
        }
    }
}
