using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using EthicalLab.Shared;

namespace EthicalLab.Presentation
{
    /// <summary>
    /// Referencias a los objetos del mundo que cambian con el progreso.
    /// El geometría es fija; los textos y colores los pinta WorldBinder desde los casos de uso.
    /// </summary>
    public sealed class HubScene
    {
        public Camera Camera;
        public PcInteractor Person;
        public Transform Root;
        public TextMesh LaptopScreen;
        public TextMesh BoardTitle;
        public TextMesh TrayHeader;
        public readonly List<InteractableView> Tickets = new List<InteractableView>();
        public readonly List<InteractableView> Folders = new List<InteractableView>();
        public readonly List<InteractableView> Trays = new List<InteractableView>();
        public InteractableView OutOfScope;
        public HubMechanism Door;
        public HubMechanism Drawer;
    }

    public static class HubOffice
    {
        public static readonly Color Navy = new Color(0.045f, 0.08f, 0.12f);
        public static readonly Color Slate = new Color(0.12f, 0.19f, 0.23f);
        public static readonly Color Wood = new Color(0.38f, 0.23f, 0.15f);
        public static readonly Color Graphite = new Color(0.025f, 0.033f, 0.04f);
        public static readonly Color Mint = new Color(0.22f, 0.83f, 0.73f);
        public static readonly Color Amber = new Color(0.92f, 0.68f, 0.32f);
        public static readonly Color Paper = new Color(0.82f, 0.84f, 0.78f);
        public static readonly Color Muted = new Color(0.3f, 0.36f, 0.4f);
        public static readonly Color Green = new Color(0.36f, 0.78f, 0.42f);
        public static readonly Color Red = new Color(0.78f, 0.32f, 0.3f);

        static Font font;
        public static Font Font => font != null ? font : (font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"));

        static Material Mat(string name, Color color)
        {
            var shader = Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Universal Render Pipeline/Lit");
            return new Material(shader) { name = name, color = color };
        }

        static GameObject Box(Transform root, string name, Vector3 position, Vector3 size, Material material)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(root, false);
            go.transform.localPosition = position;
            go.transform.localScale = size;
            go.GetComponent<Renderer>().sharedMaterial = material;
            return go;
        }

        static TextMesh Sign(Transform parent, string text, Vector3 localPosition, Quaternion rotation, float size, Color color, TextAnchor anchor = TextAnchor.MiddleCenter)
        {
            var go = new GameObject("Sign");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            go.transform.localRotation = rotation;
            var mesh = go.AddComponent<TextMesh>();
            mesh.text = text;
            mesh.characterSize = size;
            mesh.fontSize = 64;
            mesh.font = Font;
            mesh.anchor = anchor;
            mesh.alignment = TextAlignment.Center;
            mesh.color = color;
            go.GetComponent<Renderer>().sharedMaterial = Font.material;
            return mesh;
        }

        static InteractableView Mark(GameObject go, InteractableId id, string prompt, bool grab)
        {
            var view = go.AddComponent<InteractableView>();
            view.id = id.Value;
            view.prompt = prompt;
            view.grabbable = grab;
            return view;
        }

        public static HubScene Build()
        {
            var scene = new HubScene();
            var root = new GameObject("Office · first person").transform;
            scene.Root = root;
            var navy = Mat("Midnight", Navy);
            var wall = Mat("Slate", Slate);
            var wood = Mat("Walnut", Wood);
            var black = Mat("Graphite", Graphite);
            var cyan = Mat("Mint", Mint);
            var amber = Mat("Amber", Amber);
            var cream = Mat("Paper", Paper);
            var rack = Mat("Rack", Red);

            // Sala principal (x -4..4) + archivo (x 4..8), separadas por tabique con puerta.
            Box(root, "Floor", new Vector3(2f, -0.1f, -0.5f), new Vector3(12.2f, 0.2f, 7.8f), navy);
            Box(root, "Ceiling", new Vector3(2f, 4.6f, -0.5f), new Vector3(12.2f, 0.1f, 7.8f), navy);
            Box(root, "Rear wall", new Vector3(2f, 2.3f, 3.3f), new Vector3(12.2f, 4.6f, 0.2f), wall);
            Box(root, "Front wall", new Vector3(2f, 2.3f, -4.3f), new Vector3(12.2f, 4.6f, 0.2f), wall);
            Box(root, "Left wall", new Vector3(-4f, 2.3f, -0.5f), new Vector3(0.2f, 4.6f, 7.8f), wall);
            Box(root, "Archive outer wall", new Vector3(8f, 2.3f, -0.5f), new Vector3(0.2f, 4.6f, 7.8f), wall);
            Box(root, "Partition north", new Vector3(4f, 2.3f, 1.45f), new Vector3(0.15f, 4.6f, 3.7f), wall);
            Box(root, "Partition south", new Vector3(4f, 2.3f, -3.25f), new Vector3(0.15f, 4.6f, 2.1f), wall);
            Box(root, "Door lintel", new Vector3(4f, 3.45f, -1.3f), new Vector3(0.15f, 2.3f, 1.8f), wall);

            // Escritorio y objetos de trabajo.
            Box(root, "Desk", new Vector3(0f, 0.85f, 0.5f), new Vector3(3.4f, 0.14f, 1.4f), wood);
            foreach (float x in new[] { -1.4f, 1.4f }) Box(root, "Desk leg", new Vector3(x, 0.4f, 0.5f), new Vector3(0.1f, 0.8f, 1.1f), black);

            var laptop = Box(root, "Laptop", new Vector3(-0.25f, 0.98f, 0.38f), new Vector3(0.92f, 0.06f, 0.62f), black);
            Mark(laptop, InteractableId.Laptop, "LAPTOP · Abrir terminal narrativa", false);
            var screen = Box(root, "Laptop display", new Vector3(-0.25f, 1.29f, 0.69f), new Vector3(0.93f, 0.57f, 0.06f), black);
            Mark(screen, InteractableId.Laptop, "LAPTOP · Abrir terminal narrativa", false);
            Box(root, "Screen glow", new Vector3(-0.25f, 1.3f, 0.652f), new Vector3(0.81f, 0.45f, 0.012f), navy);
            scene.LaptopScreen = Sign(root, ">_ ACADEMY OS", new Vector3(-0.25f, 1.3f, 0.64f), Quaternion.identity, 0.035f, Mint);

            var notebook = Box(root, "Notebook", new Vector3(-1.15f, 0.97f, 0.24f), new Vector3(0.48f, 0.06f, 0.58f), cyan);
            Mark(notebook, InteractableId.Notebook, "CUADERNO · Glosario y notas", false);

            // Bandejas de clasificación: una por opción del paso pendiente de la carpeta en mano.
            scene.TrayHeader = Sign(root, "", new Vector3(1.0f, 1.02f, 1.02f), Quaternion.Euler(45f, 0f, 0f), 0.03f, Paper);
            for (int i = 0; i < 3; i++)
            {
                var tray = Box(root, "Tray " + (i + 1), new Vector3(0.45f + i * 0.58f, 0.95f, 0.75f), new Vector3(0.5f, 0.06f, 0.42f), amber);
                var view = Mark(tray, InteractableId.Tray(i), "BANDEJA " + (i + 1), false);
                view.sign = Sign(tray.transform, "", new Vector3(0f, 0.6f, -0.35f), Quaternion.Euler(45f, 0f, 0f), 0.05f, Graphite);
                view.sign.transform.localScale = new Vector3(1f / 0.5f, 1f / 0.06f, 1f / 0.42f);
                scene.Trays.Add(view);
            }

            // Pizarra: cada ticket es una misión. Color = estado real del progreso.
            var board = Box(root, "Board", new Vector3(0.9f, 2.2f, 3.13f), new Vector3(3.3f, 1.8f, 0.08f), navy);
            Mark(board, InteractableId.Board, "PIZARRA · Ver misiones", false);
            scene.BoardTitle = Sign(root, "BLUE / RED · ANALYST ACADEMY", new Vector3(0.9f, 2.85f, 3.07f), Quaternion.identity, 0.06f, Paper);
            for (int i = 0; i < 4; i++)
            {
                var ticket = Box(root, "Ticket " + (i + 1), new Vector3(-0.2f + i * 0.73f, 1.97f, 3.045f), new Vector3(0.55f, 0.53f, 0.025f), amber);
                var view = Mark(ticket, InteractableId.Ticket(""), "TICKET", false);
                view.sign = Sign(ticket.transform, "", new Vector3(0f, 0f, -0.6f), Quaternion.identity, 0.045f, Graphite);
                view.sign.transform.localScale = new Vector3(1f / 0.55f, 1f / 0.53f, 1f / 0.025f);
                scene.Tickets.Add(view);
            }

            // Servidor ajeno: tentación fuera de alcance. Enseña autorización sin castigo.
            var atlas = Box(root, "Rack · servidor ajeno", new Vector3(-3.4f, 0.9f, -3.4f), new Vector3(0.7f, 1.8f, 0.7f), rack);
            scene.OutOfScope = Mark(atlas, InteractableId.OutOfScope, "SERVIDOR AJENO · ¿Investigar?", false);
            Sign(atlas.transform, "ATLAS\nNO ES TU CLIENTE", new Vector3(0f, 0.15f, -0.52f), Quaternion.identity, 0.05f, Paper)
                .transform.localScale = new Vector3(1f / 0.7f, 1f / 1.8f, 1f / 0.7f);

            // Puerta al archivo.
            var hinge = new GameObject("Door hinge").transform;
            hinge.SetParent(root, false);
            hinge.localPosition = new Vector3(4f, 0f, -0.45f);
            var door = Box(hinge, "Archive door", new Vector3(0f, 1.1f, -0.82f), new Vector3(0.09f, 2.2f, 1.64f), wood);
            Mark(door, InteractableId.Door, "PUERTA DEL ARCHIVO · Abrir / cerrar", false);
            scene.Door = door.AddComponent<HubMechanism>();
            scene.Door.kind = MechanismKind.Door;
            scene.Door.movingPart = hinge;

            // Archivo: carpetas = pistas del caso activo. Leerlas = CollectClue.
            Sign(root, "ARCHIVO DE CASOS\nOBSERVAR  /  DOCUMENTAR", new Vector3(6f, 2.65f, 3.16f), Quaternion.identity, 0.07f, Paper);
            Box(root, "Archive table", new Vector3(6f, 0.85f, 1.1f), new Vector3(2.6f, 0.14f, 1.1f), wood);
            foreach (float x in new[] { 4.9f, 7.1f }) Box(root, "Archive table leg", new Vector3(x, 0.4f, 1.1f), new Vector3(0.12f, 0.8f, 0.85f), black);
            for (int i = 0; i < 3; i++)
            {
                var folder = Box(root, "Folder " + (i + 1), new Vector3(5.2f + i * 0.78f, 0.97f, 0.98f), new Vector3(0.56f, 0.07f, 0.65f), i == 1 ? cyan : amber);
                var view = Mark(folder, InteractableId.Clue(""), "CARPETA", true);
                view.sign = Sign(folder.transform, "", new Vector3(0f, 0.55f, 0f), Quaternion.Euler(90f, 0f, 0f), 0.05f, Graphite);
                view.sign.transform.localScale = new Vector3(1f / 0.56f, 1f / 0.07f, 1f / 0.65f);
                scene.Folders.Add(view);
            }

            // Cajón con la cuarta pista (USB de utilería).
            Box(root, "Cabinet back", new Vector3(7.3f, 0.7f, -2.7f), new Vector3(1.05f, 1.4f, 0.08f), wood);
            Box(root, "Cabinet left", new Vector3(6.78f, 0.7f, -2.35f), new Vector3(0.08f, 1.4f, 0.8f), wood);
            Box(root, "Cabinet right", new Vector3(7.82f, 0.7f, -2.35f), new Vector3(0.08f, 1.4f, 0.8f), wood);
            Box(root, "Cabinet top", new Vector3(7.3f, 1.43f, -2.35f), new Vector3(1.1f, 0.08f, 0.8f), wood);
            var drawerRoot = new GameObject("Drawer").transform;
            drawerRoot.SetParent(root, false);
            drawerRoot.localPosition = new Vector3(7.3f, 1.1f, -2.25f);
            var drawerFront = Box(drawerRoot, "Drawer front", new Vector3(0f, 0f, 0.38f), new Vector3(0.93f, 0.38f, 0.09f), amber);
            Box(drawerRoot, "Drawer bottom", new Vector3(0f, -0.18f, 0f), new Vector3(0.93f, 0.05f, 0.76f), wood);
            Mark(drawerFront, InteractableId.Drawer, "CAJÓN · Abrir / cerrar", false);
            scene.Drawer = drawerFront.AddComponent<HubMechanism>();
            scene.Drawer.kind = MechanismKind.Drawer;
            scene.Drawer.movingPart = drawerRoot;
            var usb = Box(drawerRoot, "USB de utilería", new Vector3(0f, -0.105f, 0.05f), new Vector3(0.25f, 0.07f, 0.11f), cyan);
            var usbView = Mark(usb, InteractableId.Clue(""), "USB", true);
            usbView.sign = Sign(usb.transform, "", new Vector3(0f, 0.6f, 0f), Quaternion.Euler(90f, 0f, 0f), 0.05f, Graphite);
            usbView.sign.transform.localScale = new Vector3(1f / 0.25f, 1f / 0.07f, 1f / 0.11f);
            scene.Folders.Add(usbView);

            // Luz y ambiente.
            var sun = new GameObject("Key light").AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.transform.rotation = Quaternion.Euler(45f, -35f, 0f);
            sun.color = new Color(1f, 0.84f, 0.65f);
            sun.intensity = 1.25f;
            sun.shadows = LightShadows.Soft;
            var archiveLight = new GameObject("Archive light").AddComponent<Light>();
            archiveLight.type = LightType.Point;
            archiveLight.transform.position = new Vector3(6f, 3.4f, 0f);
            archiveLight.color = new Color(1f, 0.78f, 0.52f);
            archiveLight.intensity = 2.2f;
            archiveLight.range = 7f;
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.34f, 0.4f, 0.47f);
            RenderSettings.fog = true;
            RenderSettings.fogColor = Navy;
            RenderSettings.fogDensity = 0.022f;

            // Jugador.
            var player = new GameObject("Player / first person");
            player.transform.SetParent(root, false);
            player.transform.position = new Vector3(0f, 0.08f, -2.85f);
            var controller = player.AddComponent<CharacterController>();
            controller.height = 1.8f;
            controller.radius = 0.28f;
            controller.center = new Vector3(0f, 0.9f, 0f);
            controller.slopeLimit = 40f;
            controller.stepOffset = 0.3f;
            controller.skinWidth = 0.035f;
            var camera = new GameObject("First-person camera").AddComponent<Camera>();
            camera.tag = "MainCamera";
            camera.transform.SetParent(player.transform, false);
            camera.transform.localPosition = new Vector3(0f, 1.62f, 0f);
            camera.fieldOfView = 62f;
            camera.nearClipPlane = 0.05f;
            camera.farClipPlane = 40f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Navy;
            camera.gameObject.AddComponent<AudioListener>();
            scene.Camera = camera;
            scene.Person = player.AddComponent<PcInteractor>();
            return scene;
        }
    }
}
