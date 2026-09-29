using UnityEngine;
using EthicalLab.Shared;

namespace EthicalLab.Presentation
{
    // Modelado modular en metros. Los detalles no interceptan el raycast de interacción.
    public static partial class HubOffice
    {
        static Transform Group(Transform parent, string name, Vector3 position = default)
        {
            var group = new GameObject(name).transform;
            group.SetParent(parent, false);
            group.localPosition = position;
            return group;
        }

        static GameObject Detail(Transform parent, string name, Vector3 position, Vector3 scale,
            Material material, PrimitiveType shape = PrimitiveType.Cube)
        {
            var go = GameObject.CreatePrimitive(shape);
            go.name = name;
            go.layer = 2; // Ignore Raycast; física desactivada en decoración.
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = material;
            if (shape == PrimitiveType.Cube && material.shader.name == "EthicalLab/Office Surface")
                OfficeBevel.Apply(go, scale);
            var collider = go.GetComponent<Collider>();
            collider.enabled = false;
            if (UnityEngine.Application.isPlaying) Object.Destroy(collider);
            else Object.DestroyImmediate(collider);
            return go;
        }

        static Material Glow(string name, Color color, float strength)
        {
            var material = Mat(name, color);
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", color * strength);
            return material;
        }

        static void DressOffice(HubScene scene)
        {
            var root = scene.Root;
            var trim = Mat("Powder coated steel", new Color(0.16f, 0.16f, 0.15f));
            var timber = Mat("Oak joinery", new Color(0.58f, 0.39f, 0.22f));
            var fabric = Mat("Woven upholstery", new Color(0.22f, 0.24f, 0.27f));
            var pale = Mat("Warm plaster", new Color(0.84f, 0.72f, 0.54f));
            var paper = Mat("Stationery", Paper);
            var foliage = Mat("Plant foliage", new Color(0.28f, 0.42f, 0.24f));
            var lamp = Glow("Warm diffuser", new Color(1f, 0.86f, 0.62f), 0.85f);
            var led = Glow("Status mint", Mint, 0.35f);
            var floorA = Mat("Carpet charcoal A", new Color(0.34f, 0.32f, 0.30f));
            var floorB = Mat("Carpet charcoal B", new Color(0.30f, 0.28f, 0.26f));
            var architecture = Group(root, "Architecture details");

            // Losetas sobre el collider continuo; las juntas nunca traban la cápsula.
            for (int x = 0; x < 18; x++)
                for (int z = 0; z < 13; z++)
                    Detail(architecture, "Carpet tile", new Vector3(-6.4f + x, 0.006f, -6.5f + z),
                        new Vector3(0.986f, 0.012f, 0.986f), (x + z) % 2 == 0 ? floorA : floorB);
            foreach (float z in new[] { -6.72f, 5.72f })
                Detail(architecture, "Skirting", new Vector3(2.35f, 0.08f, z), new Vector3(17.6f, 0.13f, 0.045f), trim);
            foreach (float x in new[] { -6.42f, 11.12f })
                Detail(architecture, "Skirting", new Vector3(x, 0.08f, -0.5f), new Vector3(0.045f, 0.13f, 12.4f), trim);

            DressWindows(architecture, trim, timber, pale);
            DressExterior(root);

            // Revestimiento bajo los tableros, sin invadir su superficie interactiva.
            for (int i = 0; i < 46; i++)
                Detail(architecture, "Oak wall slat", new Vector3(-5.5f + i * 0.22f, 0.52f, 5.68f),
                    new Vector3(0.14f, 0.92f, 0.04f), timber);
            Detail(architecture, "Board rail", new Vector3(-0.2f, 1.15f, 5.64f), new Vector3(3.9f, 0.045f, 0.12f), trim);
            Detail(architecture, "Archive door frame north", new Vector3(5.38f, 1.2f, -0.28f), new Vector3(0.23f, 2.4f, 0.09f), trim);
            Detail(architecture, "Archive door frame south", new Vector3(5.38f, 1.2f, -2.12f), new Vector3(0.23f, 2.4f, 0.09f), trim);
            Detail(architecture, "Archive door frame top", new Vector3(5.38f, 2.4f, -1.2f), new Vector3(0.23f, 0.09f, 1.95f), trim);
            Detail(scene.Door.movingPart, "Door push plate", new Vector3(-0.055f, 1.08f, -1.32f), new Vector3(0.025f, 0.28f, 0.12f), pale);
            Detail(scene.Door.movingPart, "Door handle", new Vector3(-0.095f, 1.08f, -1.28f), new Vector3(0.08f, 0.035f, 0.24f), trim);
            Detail(scene.Drawer.movingPart, "Drawer handle", new Vector3(0, 0, 0.46f), new Vector3(0.3f, 0.04f, 0.06f), trim);

            var furniture = Group(root, "Office furniture");
            Chair(furniture, new Vector3(-0.15f, 0f, -2.2f), 6f, fabric, trim);
            Chair(furniture, new Vector3(2.55f, 0f, -3.2f), -20f, fabric, trim);
            PlaceGuides(furniture);
            Plant(furniture, new Vector3(-5.7f, 0f, 4.85f), trim, foliage);
            Plant(furniture, new Vector3(4.35f, 0f, 4.85f), pale, foliage);
            Plant(furniture, new Vector3(6.55f, 0f, -5.7f), pale, foliage);
            CourseStation(root);

            // Credenza contra la pared frontal, fuera de la zona de aparición.
            var credenza = Group(furniture, "Storage credenza", new Vector3(2.9f, 0f, -6.4f));
            Box(credenza, "Cabinet body", new Vector3(0, 0.46f, 0), new Vector3(2.6f, 0.8f, 0.56f), timber);
            Detail(credenza, "Cabinet top", new Vector3(0, 0.89f, 0), new Vector3(2.68f, 0.055f, 0.61f), trim);
            for (int i = 0; i < 4; i++)
            {
                float x = -0.975f + i * 0.65f;
                Detail(credenza, "Storage door", new Vector3(x, 0.47f, 0.29f), new Vector3(0.625f, 0.74f, 0.02f), pale);
                Detail(credenza, "Storage handle", new Vector3(x + 0.21f, 0.61f, 0.32f), new Vector3(0.025f, 0.15f, 0.035f), trim);
            }
            Sign(root, "BLUE / RED", new Vector3(2.9f, 2.45f, -6.7f), Quaternion.Euler(0, 180, 0), 0.16f, Paper);
            Sign(root, "ANALYST ACADEMY  /  ESTUDIO 01", new Vector3(2.9f, 2.1f, -6.7f), Quaternion.Euler(0, 180, 0), 0.038f, Mint);

            var shelving = Group(furniture, "Archive shelving", new Vector3(8.4f, 0f, 5.2f));
            foreach (float x in new[] { -1.35f, 1.35f })
                Box(shelving, "Shelf upright", new Vector3(x, 1.05f, 0), new Vector3(0.07f, 2.1f, 0.48f), trim);
            for (int row = 0; row < 4; row++)
            {
                float y = 0.22f + row * 0.5f;
                Box(shelving, "Shelf", new Vector3(0, y, 0), new Vector3(2.7f, 0.045f, 0.48f), timber);
                for (int col = 0; col < 6; col++)
                {
                    float x = -1.05f + col * 0.41f;
                    Detail(shelving, "Archive binder", new Vector3(x, y + 0.2f, 0), new Vector3(0.28f, 0.35f, 0.35f), col % 2 == 0 ? fabric : pale);
                    Detail(shelving, "Binder label", new Vector3(x, y + 0.23f, -0.18f), new Vector3(0.16f, 0.08f, 0.01f), paper);
                }
            }

            var workstation = Group(root, "Workstation details");
            Detail(workstation, "Desk modesty panel", new Vector3(-0.4f, 0.5f, 0.48f), new Vector3(3.2f, 0.42f, 0.055f), trim);
            // Pequeños detalles sin collider: la pantalla sigue recibiendo E.
            for (int row = 0; row < 4; row++)
                for (int col = 0; col < 11; col++)
                    Detail(workstation, "Laptop key", new Vector3(-1.02f + col * 0.073f, 1.013f, -0.28f + row * 0.055f), new Vector3(0.057f, 0.008f, 0.039f), fabric);
            Detail(workstation, "Trackpad", new Vector3(-0.65f, 1.013f, -0.465f), new Vector3(0.26f, 0.007f, 0.12f), fabric);
            Detail(workstation, "Notebook pages", new Vector3(-1.55f, 1.003f, -0.46f), new Vector3(0.425f, 0.009f, 0.52f), paper);
            Detail(workstation, "Pen", new Vector3(-1.83f, 0.945f, -0.58f), new Vector3(0.018f, 0.018f, 0.24f), trim);
            Detail(workstation, "Ceramic cup", new Vector3(-1.58f, 1.015f, 0.22f), new Vector3(0.14f, 0.095f, 0.14f), pale, PrimitiveType.Cylinder);
            Detail(workstation, "Coffee", new Vector3(-1.58f, 1.111f, 0.22f), new Vector3(0.115f, 0.002f, 0.115f), timber, PrimitiveType.Cylinder);
            Detail(workstation, "Task lamp base", new Vector3(-1.89f, 0.935f, 0.21f), new Vector3(0.22f, 0.025f, 0.22f), trim, PrimitiveType.Cylinder);
            Detail(workstation, "Task lamp stem", new Vector3(-1.89f, 1.22f, 0.21f), new Vector3(0.025f, 0.55f, 0.025f), trim);
            Detail(workstation, "Task lamp head", new Vector3(-1.79f, 1.5f, 0.16f), new Vector3(0.33f, 0.06f, 0.18f), trim);
            Detail(workstation, "Task lamp diffuser", new Vector3(-1.79f, 1.466f, 0.16f), new Vector3(0.28f, 0.008f, 0.13f), lamp);

            // Frontal del rack hacia el interior (+X). El cascarón fino sigue el color del metal.
            var rackDetails = Group(root, "Atlas rack details", new Vector3(-5.6f, 0f, -2.15f));
            rackDetails.localRotation = Quaternion.Euler(0f, 90f, 0f);
            for (int i = 0; i < 7; i++)
            {
                float y = 0.17f + i * 0.17f;
                Detail(rackDetails, "Server unit", new Vector3(0, y, 0), new Vector3(0.61f, 0.145f, 0.025f), trim);
                Detail(rackDetails, "Status indicator", new Vector3(-0.24f, y, 0.018f), new Vector3(0.018f, 0.018f, 0.008f), led);
                for (int slot = 0; slot < 4; slot++)
                    Detail(rackDetails, "Vent", new Vector3(-0.12f + slot * 0.1f, y, 0.018f), new Vector3(0.055f, 0.035f, 0.008f), fabric);
            }

            var lights = Group(root, "Lighting");
            Pendant(lights, new Vector3(-0.4f, 3.45f, -0.2f), trim, lamp);
            Pendant(lights, new Vector3(8.4f, 3.45f, 0.6f), trim, lamp);
            Spot(lights, "Office warm key", new Vector3(-0.4f, 3.5f, -0.2f), 1.75f, 12f);
            Point(lights, "Board warm key", new Vector3(-0.2f, 3.3f, 3.6f), new Color(1f, 0.94f, 0.84f), 0.8f, 10f, false);
            Point(lights, "Reception fill", new Vector3(-3.8f, 3.1f, -4.6f), new Color(1f, 0.95f, 0.87f), 0.6f, 10f, false);
            Point(lights, "Window daylight fill", new Vector3(-5.2f, 2.9f, -1f), new Color(0.80f, 0.89f, 1f), 0.65f, 12f, false);
            Spot(lights, "Archive warm key", new Vector3(8.4f, 3.5f, 0.6f), 1.65f, 12f);
            var sunGo = Group(lights, "Window sun", new Vector3(-6.2f, 3.4f, -1f)).gameObject;
            sunGo.transform.rotation = Quaternion.Euler(42f, 78f, 0f);
            var sun = sunGo.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = new Color(1f, 0.93f, 0.78f);
            sun.intensity = 0.5f;
            sun.shadows = LightShadows.None;
        }

        static void Chair(Transform parent, Vector3 position, float yaw, Material fabric, Material metal)
        {
            var chair = Group(parent, "Task chair", position);
            chair.localRotation = Quaternion.Euler(0, yaw, 0);
            Box(chair, "Seat", new Vector3(0, 0.49f, 0), new Vector3(0.57f, 0.12f, 0.53f), fabric);
            Box(chair, "Backrest", new Vector3(0, 0.86f, -0.24f), new Vector3(0.55f, 0.63f, 0.1f), fabric);
            Detail(chair, "Chair stem", new Vector3(0, 0.25f, 0), new Vector3(0.08f, 0.2f, 0.08f), metal, PrimitiveType.Cylinder);
            for (int i = 0; i < 5; i++)
            {
                var spoke = Group(chair, "Chair spoke");
                spoke.localRotation = Quaternion.Euler(0, i * 72f, 0);
                Detail(spoke, "Foot", new Vector3(0, 0.1f, 0.14f), new Vector3(0.045f, 0.045f, 0.34f), metal);
                Detail(spoke, "Caster", new Vector3(0, 0.055f, 0.3f), new Vector3(0.075f, 0.08f, 0.075f), metal);
            }
            foreach (float x in new[] { -0.32f, 0.32f })
            {
                Detail(chair, "Armrest support", new Vector3(x, 0.58f, 0), new Vector3(0.04f, 0.24f, 0.04f), metal);
                Detail(chair, "Armrest", new Vector3(x, 0.72f, 0), new Vector3(0.08f, 0.045f, 0.32f), metal);
            }

            // Un solo volumen: el asiento no es el punto de apoyo, así la silla no se hunde.
            foreach (var child in chair.GetComponentsInChildren<Collider>())
            {
                if (UnityEngine.Application.isPlaying) Object.Destroy(child);
                else Object.DestroyImmediate(child);
            }
            var volume = chair.gameObject.AddComponent<BoxCollider>();
            volume.center = new Vector3(0f, 0.55f, -0.04f);
            volume.size = new Vector3(0.66f, 1.12f, 0.72f);
            var body = chair.gameObject.AddComponent<Rigidbody>();
            body.mass = 9f;
            body.linearDamping = 1.4f;
            body.angularDamping = 3f;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            body.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            var view = Mark(chair.gameObject, new InteractableId("chair"), "Silla de trabajo", true);
            view.dropInPlace = true;
            view.holdOffset = new Vector3(0.22f, -0.55f, 0.42f);
        }

        static void PlaceGuides(Transform parent)
        {
            CartoonGuide(parent, "Movement coach", new Vector3(4.05f, 0f, -4.2f), 200f,
                "Coach", Mint, new Color(0.12f, 0.13f, 0.16f), "IRIARTE", "coach");
            CartoonGuide(parent, "Reception guide", new Vector3(-4.55f, 0f, -5f), 70f,
                "Reception", Paper, new Color(0.36f, 0.22f, 0.14f), "VERA", "reception");
            CartoonGuide(parent, "Tickets guide", new Vector3(-3.5f, 0f, 4.05f), 165f,
                "Tickets", Paper, new Color(0.40f, 0.26f, 0.16f), "HUGO", "tickets");
            CartoonGuide(parent, "Report guide", new Vector3(2.6f, 0f, -1.2f), -80f,
                "Report", new Color(0.40f, 0.36f, 0.30f), new Color(0.14f, 0.11f, 0.1f), "NURIA", "report");
        }

        static void CartoonGuide(Transform parent, string objectName, Vector3 position, float yaw,
            string materialPrefix, Color jacketColor, Color hairColor, string label, string role)
        {
            var guide = Group(parent, objectName, position);
            guide.localRotation = Quaternion.Euler(0f, yaw, 0f);
            const float line = 0.0007f;
            var jacket = CelShading.Create(materialPrefix + " jacket", jacketColor, line, 0.14f);
            var shirt = CelShading.Create(materialPrefix + " shirt", Paper, line, 0.1f);
            var skin = CelShading.Create(materialPrefix + " skin", new Color(0.90f, 0.72f, 0.58f), line, 0.12f);
            var hair = CelShading.Create(materialPrefix + " hair", hairColor, line, 0.12f);
            var legs = CelShading.Create(materialPrefix + " trousers",
                role == "report" ? new Color(0.16f, 0.16f, 0.17f) : new Color(0.27f, 0.31f, 0.36f), line, 0.12f);
            var shoes = CelShading.Create(materialPrefix + " shoes",
                role == "tickets" ? new Color(0.36f, 0.24f, 0.16f) : Graphite, line, 0.1f);
            var accent = CelShading.Create(materialPrefix + " accent",
                role == "tickets" ? new Color(0.16f, 0.24f, 0.42f) : Mint, line, 0.1f);

            foreach (float x in new[] { -0.09f, 0.09f })
            {
                Detail(guide, "Shoe", new Vector3(x, 0.045f, 0.03f), new Vector3(0.11f, 0.07f, 0.24f), shoes);
                Detail(guide, "Leg", new Vector3(x, 0.48f, 0f), new Vector3(0.11f, 0.38f, 0.11f), legs, PrimitiveType.Capsule);
            }
            Detail(guide, "Hips", new Vector3(0f, 0.86f, 0f), new Vector3(0.34f, 0.14f, 0.18f), legs);
            Detail(guide, "Torso", new Vector3(0f, 1.18f, 0f), new Vector3(0.42f, 0.48f, 0.20f), jacket);
            Detail(guide, "Shirt", new Vector3(0f, 1.16f, 0.09f), new Vector3(0.24f, 0.34f, 0.03f), shirt);
            Detail(guide, "Collar", new Vector3(0f, 1.40f, 0.04f), new Vector3(0.16f, 0.06f, 0.12f), shirt);
            if (role == "tickets")
                Detail(guide, "Tie", new Vector3(0f, 1.12f, 0.11f), new Vector3(0.06f, 0.22f, 0.02f), accent);
            else if (role == "coach")
                Detail(guide, "Folder", new Vector3(-0.24f, 0.98f, 0.12f), new Vector3(0.04f, 0.22f, 0.16f), shoes);
            else if (role == "reception")
                Detail(guide, "Belt", new Vector3(0f, 0.96f, 0.1f), new Vector3(0.36f, 0.045f, 0.04f), shoes);
            else if (role == "report")
                Detail(guide, "Tray", new Vector3(0.24f, 1.02f, 0.14f), new Vector3(0.22f, 0.08f, 0.16f), shoes);

            foreach (float x in new[] { -0.28f, 0.28f })
            {
                Detail(guide, "Arm", new Vector3(x, 1.08f, 0f), new Vector3(0.09f, 0.26f, 0.09f), jacket, PrimitiveType.Capsule);
                Detail(guide, "Hand", new Vector3(x, 0.82f, 0.02f), new Vector3(0.07f, 0.07f, 0.08f), skin, PrimitiveType.Sphere);
            }
            Detail(guide, "Neck", new Vector3(0f, 1.46f, 0f), new Vector3(0.08f, 0.08f, 0.08f), skin, PrimitiveType.Capsule);
            Detail(guide, "Head", new Vector3(0f, 1.62f, 0f), new Vector3(0.24f, 0.26f, 0.24f), skin, PrimitiveType.Sphere);
            Detail(guide, "Hair", new Vector3(0f, 1.72f, -0.02f), new Vector3(0.26f, 0.12f, 0.26f), hair, PrimitiveType.Sphere);
            if (role == "reception" || role == "report")
                Detail(guide, "Bun", new Vector3(0f, 1.78f, -0.06f), new Vector3(0.12f, 0.10f, 0.12f), hair, PrimitiveType.Sphere);
            Detail(guide, "Eye", new Vector3(-0.045f, 1.64f, 0.11f), new Vector3(0.035f, 0.04f, 0.02f), shoes, PrimitiveType.Sphere);
            Detail(guide, "Eye", new Vector3(0.045f, 1.64f, 0.11f), new Vector3(0.035f, 0.04f, 0.02f), shoes, PrimitiveType.Sphere);
            Sign(guide, label, new Vector3(0f, 1.98f, 0f), Quaternion.Euler(0f, 180f, 0f), 0.04f, Paper);

            var volume = guide.gameObject.AddComponent<BoxCollider>();
            volume.center = new Vector3(0f, 0.95f, 0f);
            volume.size = new Vector3(0.5f, 1.85f, 0.36f);
            Mark(guide.gameObject, InteractableId.Guide(role), label + " · E", false);
        }

        static void CourseStation(Transform root)
        {
            var station = Group(root, "Course station", new Vector3(-4.55f, 1.9f, -6.68f));
            station.localRotation = Quaternion.Euler(0f, 180f, 0f);
            Detail(station, "Course board", Vector3.zero, new Vector3(1.85f, 1.28f, 0.04f), Mat("Course board", Navy));
            Sign(station, "CURSO · 4 CASOS", new Vector3(0f, 0.46f, -0.03f), Quaternion.identity, 0.048f, Paper);
            Sign(station, "01  Reconocimiento\n02  Personas\n03  Identidad\n04  Sistemas", new Vector3(0f, 0.02f, -0.03f), Quaternion.identity, 0.036f, Mint);
            Sign(station, "Los tickets están en la pizarra", new Vector3(0f, -0.46f, -0.03f), Quaternion.identity, 0.028f, Amber);
        }

        static void Plant(Transform parent, Vector3 position, Material pot, Material leaves)
        {
            var plant = Group(parent, "Indoor plant", position);
            var planter = Detail(plant, "Planter", new Vector3(0, 0.23f, 0), new Vector3(0.42f, 0.23f, 0.42f), pot, PrimitiveType.Cylinder);
            var collision = planter.AddComponent<BoxCollider>();
            collision.size = new Vector3(1, 2, 1);
            planter.layer = 0;
            for (int i = 0; i < 7; i++)
            {
                var leaf = Detail(plant, "Leaf", new Vector3(Mathf.Sin(i * 2.4f) * 0.17f, 0.65f + (i % 3) * 0.17f, Mathf.Cos(i * 2.4f) * 0.17f),
                    new Vector3(0.2f, 0.62f, 0.065f), leaves, PrimitiveType.Sphere);
                leaf.transform.localRotation = Quaternion.Euler(18f + i * 4, i * 137f, 22);
            }
        }

        static void Pendant(Transform parent, Vector3 position, Material metal, Material diffuser)
        {
            var fixture = Group(parent, "Suspended linear light", position);
            Detail(fixture, "Housing", Vector3.zero, new Vector3(2.6f, 0.09f, 0.2f), metal);
            Detail(fixture, "Diffuser", new Vector3(0, -0.05f, 0), new Vector3(2.5f, 0.01f, 0.14f), diffuser);
            foreach (float x in new[] { -1f, 1f })
                Detail(fixture, "Suspension", new Vector3(x, 0.18f, 0), new Vector3(0.012f, 0.35f, 0.012f), metal);
        }

        static void Point(Transform parent, string name, Vector3 position, Color color, float intensity, float range, bool shadows)
        {
            var light = Group(parent, name, position).gameObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = color;
            light.intensity = intensity;
            light.range = range;
            light.shadows = shadows ? LightShadows.Soft : LightShadows.None;
        }

        static void Spot(Transform parent, string name, Vector3 position, float intensity, float range)
        {
            var light = Group(parent, name, position).gameObject.AddComponent<Light>();
            light.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            light.type = LightType.Spot;
            light.spotAngle = 125f;
            light.innerSpotAngle = 75f;
            light.color = new Color(1f, 0.94f, 0.85f);
            light.intensity = intensity;
            light.range = range;
            light.shadows = LightShadows.Soft;
            light.shadowStrength = 0.72f;
            light.shadowBias = 0.025f;
            light.shadowNormalBias = 0.15f;
            light.renderMode = LightRenderMode.ForcePixel;
        }
    }
}
