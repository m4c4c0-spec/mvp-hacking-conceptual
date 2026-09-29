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
        public TextMesh BoardObjective;
        public TextMesh TrayHeader;
        public TextMesh ClueChecklist;
        public readonly List<InteractableView> Tickets = new List<InteractableView>();
        public readonly List<InteractableView> Folders = new List<InteractableView>();
        public readonly List<InteractableView> Trays = new List<InteractableView>();
        public InteractableView OutOfScope;
        public InteractableView ReportInbox;
        public HubMechanism Door;
        public HubMechanism Drawer;
    }

    public static partial class HubOffice
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

        static Material Mat(string name, Color color) => OfficeSurface.Create(name, color);

        static GameObject Box(Transform root, string name, Vector3 position, Vector3 size, Material material)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(root, false);
            go.transform.localPosition = position;
            go.transform.localScale = size;
            go.GetComponent<Renderer>().sharedMaterial = material;
            OfficeBevel.Apply(go, size);
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
            mesh.characterSize = size * 0.42f;
            mesh.fontSize = 64;
            mesh.font = Font;
            mesh.anchor = anchor;
            mesh.alignment = TextAlignment.Center;
            mesh.color = color;
            go.GetComponent<Renderer>().sharedMaterial = Font.material;
            var label = go.AddComponent<HubWorldLabel>();
            label.characterSize = mesh.characterSize;
            return mesh;
        }

        static void Fit(TextMesh text, float width, float height)
        {
            var label = text.GetComponent<HubWorldLabel>();
            label.width = width;
            label.height = height;
            label.FitNow();
        }

        static InteractableView Mark(GameObject go, InteractableId id, string prompt, bool grab)
        {
            var view = go.AddComponent<InteractableView>();
            view.id = id.Value;
            view.prompt = prompt;
            view.grabbable = grab;
            return view;
        }

        /// <summary>Oficina procedural (cubos). Fallback si no hay <see cref="HubSceneRefs"/> en escena.</summary>
        public static HubScene BuildProceduralLegacy() => Build();

        public static HubScene Build()
        {
            var scene = new HubScene();
            var root = new GameObject("Office_Art").transform;
            scene.Root = root;
            var navy = Mat("Midnight", Navy);
            var plaster = Mat("Ochre plaster", new Color(0.73f, 0.68f, 0.57f));
            var oak = Mat("Oak desk", new Color(0.64f, 0.43f, 0.24f));
            var wood = Mat("Walnut", Wood);
            var black = Mat("Graphite", Graphite);
            var cyan = Mat("Mint", Mint);
            var amber = Mat("Amber", Amber);
            var cream = Mat("Paper", Paper);
            var rack = Mat("Rack", Red);
            var carpet = Mat("Charcoal carpet", new Color(0.33f, 0.31f, 0.29f));

            // Sala principal (x -6.5..5.5, z -6.8..5.8) + archivo (x 5.5..11.2). Techo a 3.9 m.
            Box(root, "Floor", new Vector3(2.35f, -0.1f, -0.5f), new Vector3(18.1f, 0.2f, 13f), carpet);
            Box(root, "Ceiling", new Vector3(2.35f, 3.95f, -0.5f), new Vector3(18.1f, 0.1f, 13f), Mat("Ceiling plaster", new Color(0.86f, 0.78f, 0.66f)));
            Box(root, "Rear wall", new Vector3(2.35f, 1.95f, 5.9f), new Vector3(18.1f, 3.9f, 0.2f), plaster);
            Box(root, "Front wall", new Vector3(2.35f, 1.95f, -6.9f), new Vector3(18.1f, 3.9f, 0.2f), plaster);
            WindowWall(root, plaster);
            Box(root, "Archive outer wall", new Vector3(11.3f, 1.95f, -0.5f), new Vector3(0.2f, 3.9f, 13f), plaster);
            Box(root, "Partition north", new Vector3(5.5f, 1.95f, 2.8f), new Vector3(0.16f, 3.9f, 6.2f), plaster);
            Box(root, "Partition south", new Vector3(5.5f, 1.95f, -4.5f), new Vector3(0.16f, 3.9f, 4.8f), plaster);
            Box(root, "Door lintel", new Vector3(5.5f, 3.15f, -1.2f), new Vector3(0.16f, 1.5f, 1.8f), plaster);

            // Escritorio a escala humana, con pasillo hacia la pizarra y hacia la entrada.
            Box(root, "Desk", new Vector3(-0.4f, 0.85f, -0.2f), new Vector3(3.4f, 0.14f, 1.4f), oak);
            foreach (float x in new[] { -1.8f, 1.0f }) Box(root, "Desk leg", new Vector3(x, 0.4f, -0.2f), new Vector3(0.1f, 0.8f, 1.1f), black);

            var laptop = Box(root, "Laptop", new Vector3(-0.65f, 0.98f, -0.32f), new Vector3(0.92f, 0.06f, 0.62f), black);
            Mark(laptop, InteractableId.Laptop, "LAPTOP · E para terminal narrativa", false);
            var screen = Box(root, "Laptop display", new Vector3(-0.65f, 1.29f, -0.01f), new Vector3(0.93f, 0.57f, 0.06f), black);
            Mark(screen, InteractableId.Laptop, "LAPTOP · E para terminal narrativa", false);
            Detail(root, "Screen glow", new Vector3(-0.65f, 1.3f, -0.048f), new Vector3(0.81f, 0.45f, 0.012f), navy);
            scene.LaptopScreen = Sign(root, ">_ ACADEMY OS", new Vector3(-0.65f, 1.3f, -0.06f), Quaternion.identity, 0.035f, Mint);
            Fit(scene.LaptopScreen, 0.75f, 0.38f);

            var notebook = Box(root, "Notebook", new Vector3(-1.55f, 0.97f, -0.46f), new Vector3(0.48f, 0.06f, 0.58f), cyan);
            Mark(notebook, InteractableId.Notebook, "CUADERNO · E para glosario y notas", false);

            // Bandejas de clasificación: una por opción del paso pendiente de la carpeta en mano.
            scene.TrayHeader = Sign(root, "", new Vector3(0.6f, 1.02f, 0.32f), Quaternion.Euler(45f, 0f, 0f), 0.03f, Paper);
            for (int i = 0; i < 3; i++)
            {
                var tray = Box(root, "Tray " + (i + 1), new Vector3(0.05f + i * 0.58f, 0.95f, 0.05f), new Vector3(0.5f, 0.06f, 0.42f), amber);
                var view = Mark(tray, InteractableId.Tray(i), "BANDEJA " + (i + 1) + " · clasificación", false);
                view.sign = Sign(tray.transform, "", new Vector3(0f, 0.6f, -0.35f), Quaternion.Euler(45f, 0f, 0f), 0.05f, Graphite);
                view.sign.transform.localScale = new Vector3(1f / 0.5f, 1f / 0.06f, 1f / 0.42f);
                Fit(view.sign, 0.46f, 0.3f);
                scene.Trays.Add(view);
            }

            // Buzón de informe: paso físico tras documentar todas las pistas → expediente.
            var inbox = Box(root, "Report inbox", new Vector3(1.05f, 1.02f, -0.58f), new Vector3(0.46f, 0.22f, 0.5f), cyan);
            scene.ReportInbox = Mark(inbox, InteractableId.ReportInbox, "BUZÓN DE INFORME · E para expediente", false);
            scene.ReportInbox.sign = Sign(inbox.transform, "INFORME", new Vector3(0f, 0.55f, -0.2f), Quaternion.Euler(20f, 0f, 0f), 0.045f, Graphite);
            scene.ReportInbox.sign.transform.localScale = new Vector3(1f / 0.46f, 1f / 0.22f, 1f / 0.5f);
            Fit(scene.ReportInbox.sign, 0.4f, 0.14f);
            Sign(root, "BUZÓN →", new Vector3(1.05f, 1.22f, -0.88f), Quaternion.Euler(15f, 0f, 0f), 0.028f, Amber);

            // Checklist in-world de pistas (WorldBinder lo pinta desde ClueWorkflow).
            Box(root, "Checklist panel", new Vector3(-4.6f, 2.15f, 5.72f), new Vector3(1.55f, 1.35f, 0.06f), navy);
            scene.ClueChecklist = Sign(root, "PISTAS · sin caso", new Vector3(-4.6f, 2.55f, 5.66f), Quaternion.identity, 0.045f, Paper);
            scene.ClueChecklist.anchor = TextAnchor.UpperCenter;
            scene.ClueChecklist.alignment = TextAlignment.Left;
            Fit(scene.ClueChecklist, 1.4f, 0.95f);

            // Pizarra al fondo: cada ticket es una misión. Color = estado real del progreso.
            var board = Box(root, "Board", new Vector3(-0.2f, 2.2f, 5.72f), new Vector3(3.8f, 2f, 0.08f), navy);
            Mark(board, InteractableId.Board, "PIZARRA DE TICKETS · E para ver / aceptar", false);
            scene.BoardTitle = Sign(root, "BLUE / RED · tickets · E para aceptar", new Vector3(-0.2f, 2.95f, 5.66f), Quaternion.identity, 0.055f, Paper);
            scene.BoardObjective = Sign(root, "Acepta un ticket", new Vector3(-0.2f, 1.35f, 5.66f), Quaternion.identity, 0.04f, Mint);
            scene.BoardObjective.anchor = TextAnchor.UpperCenter;
            Fit(scene.BoardTitle, 3.4f, 0.22f);
            Fit(scene.BoardObjective, 3.4f, 0.5f);
            Fit(scene.TrayHeader, 1.7f, 0.16f);
            for (int i = 0; i < 4; i++)
            {
                var ticket = Box(root, "Ticket " + (i + 1), new Vector3(-1.35f + i * 0.8f, 2.05f, 5.655f), new Vector3(0.55f, 0.53f, 0.025f), amber);
                var view = Mark(ticket, InteractableId.Ticket(""), "TICKET", false);
                view.sign = Sign(ticket.transform, "", new Vector3(0f, 0f, -0.6f), Quaternion.identity, 0.045f, Graphite);
                view.sign.transform.localScale = new Vector3(1f / 0.55f, 1f / 0.53f, 1f / 0.025f);
                Fit(view.sign, 0.49f, 0.43f);
                scene.Tickets.Add(view);
            }

            // Servidor ajeno, contra la pared izquierda, sin tapar recepción ni la entrada.
            var atlas = Box(root, "Rack · servidor ajeno", new Vector3(-5.95f, 0.9f, -2.15f), new Vector3(0.7f, 1.8f, 0.7f), rack);
            scene.OutOfScope = Mark(atlas, InteractableId.OutOfScope, "SERVIDOR AJENO · ¿Investigar?", false);
            Sign(atlas.transform, "ATLAS\nNO ES TU CLIENTE", new Vector3(0.52f, 0.15f, 0f), Quaternion.Euler(0f, 90f, 0f), 0.05f, Paper)
                .transform.localScale = new Vector3(1f / 0.7f, 1f / 1.8f, 1f / 0.7f);

            // Puerta al archivo, en el tabique. El hueco queda libre para la cápsula.
            var hinge = new GameObject("Door hinge").transform;
            hinge.SetParent(root, false);
            hinge.localPosition = new Vector3(5.5f, 0f, -0.35f);
            var door = Box(hinge, "Archive door", new Vector3(0f, 1.1f, -0.82f), new Vector3(0.09f, 2.2f, 1.64f), wood);
            Mark(door, InteractableId.Door, "PUERTA DEL ARCHIVO · E para abrir · carpetas adentro", false);
            Sign(root, "ARCHIVO →\nE · abrir", new Vector3(5.15f, 2.65f, -1.2f), Quaternion.Euler(0f, 90f, 0f), 0.05f, Amber);
            scene.Door = door.AddComponent<HubMechanism>();
            scene.Door.kind = MechanismKind.Door;
            scene.Door.movingPart = hinge;

            // Archivo: carpetas = pistas del caso activo. Leerlas = CollectClue.
            Sign(root, "ARCHIVO DE CASOS\nOBSERVAR  /  DOCUMENTAR", new Vector3(8.4f, 2.85f, 5.66f), Quaternion.identity, 0.07f, Paper);
            Box(root, "Archive table", new Vector3(8.4f, 0.85f, 0.6f), new Vector3(2.8f, 0.14f, 1.15f), oak);
            foreach (float x in new[] { 7.3f, 9.5f }) Box(root, "Archive table leg", new Vector3(x, 0.4f, 0.6f), new Vector3(0.12f, 0.8f, 0.9f), black);
            for (int i = 0; i < 3; i++)
            {
                var folder = Box(root, "Folder " + (i + 1), new Vector3(7.6f + i * 0.8f, 0.97f, 0.55f), new Vector3(0.56f, 0.07f, 0.65f), i == 1 ? cyan : amber);
                var view = Mark(folder, InteractableId.Clue(""), "CARPETA", true);
                view.sign = Sign(folder.transform, "", new Vector3(0f, 0.55f, 0f), Quaternion.Euler(90f, 0f, 0f), 0.05f, Graphite);
                view.sign.transform.localScale = new Vector3(1f / 0.56f, 1f / 0.07f, 1f / 0.65f);
                Fit(view.sign, 0.5f, 0.5f);
                scene.Folders.Add(view);
            }

            // Cajón con la cuarta pista (USB de utilería), al fondo del archivo.
            Box(root, "Cabinet back", new Vector3(10.3f, 0.7f, -5.9f), new Vector3(1.05f, 1.4f, 0.08f), wood);
            Box(root, "Cabinet left", new Vector3(9.78f, 0.7f, -5.55f), new Vector3(0.08f, 1.4f, 0.8f), wood);
            Box(root, "Cabinet right", new Vector3(10.82f, 0.7f, -5.55f), new Vector3(0.08f, 1.4f, 0.8f), wood);
            Box(root, "Cabinet top", new Vector3(10.3f, 1.43f, -5.55f), new Vector3(1.1f, 0.08f, 0.8f), wood);
            var drawerRoot = new GameObject("Drawer").transform;
            drawerRoot.SetParent(root, false);
            drawerRoot.localPosition = new Vector3(10.3f, 1.1f, -5.45f);
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
            Fit(usbView.sign, 0.22f, 0.1f);
            scene.Folders.Add(usbView);

            // Luz y ambiente.
            DressOffice(scene);
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.64f, 0.58f, 0.48f);
            RenderSettings.fog = false;
            RenderSettings.fogColor = Navy;
            RenderSettings.fogDensity = 0.022f;

            // Jugador.
            var player = new GameObject("Player / first person");
            player.transform.SetParent(root, false);
            player.transform.position = new Vector3(0.15f, 0.08f, -5.35f);
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
            camera.fieldOfView = 70f;
            camera.nearClipPlane = 0.05f;
            camera.farClipPlane = 180f;
            camera.clearFlags = CameraClearFlags.Skybox;
            camera.backgroundColor = new Color(0.55f, 0.62f, 0.66f);
            camera.gameObject.AddComponent<AudioListener>();
            scene.Camera = camera;
            scene.Person = player.AddComponent<PcInteractor>();
            var coach = player.AddComponent<MovementCoach>();
            coach.Person = scene.Person;
            root.gameObject.AddComponent<HubSceneRefs>().PopulateFrom(scene);
            var quality = root.gameObject.AddComponent<OfficeRenderQuality>();
            quality.sky = new Material(Resources.Load<Shader>("OfficeSky")) { name = "Office afternoon sky" };
            RenderSettings.skybox = quality.sky;
            OfficeRenderQuality.Apply(camera);
            return scene;
        }
    }
}
