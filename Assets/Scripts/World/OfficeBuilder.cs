using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Academy
{
    public static class OfficeBuilder
    {
        private static Material Material(string name, Color color)
        {
            var shader = Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Universal Render Pipeline/Lit");
            var template = Resources.Load<Material>("AcademySurface");
            return template != null ? new Material(template) { name = name, color = color } : new Material(shader) { name = name, color = color };
        }
        private static GameObject Box(Transform root, string name, Vector3 position, Vector3 size, Material material)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name; go.transform.SetParent(root);
            go.transform.localPosition = position; go.transform.localScale = size;
            go.GetComponent<Renderer>().sharedMaterial = material;
            return go;
        }
        private static GameObject Sign(Transform root, string text, Vector3 position, float size, Color color)
        {
            var go = new GameObject("Sign · " + text); go.transform.SetParent(root); go.transform.localPosition = position;
            go.transform.localRotation = Quaternion.identity;
            var mesh = go.AddComponent<TextMesh>(); mesh.text = text; mesh.characterSize = size; mesh.fontSize = 64;
            mesh.font = UIFactory.Font; go.GetComponent<Renderer>().sharedMaterial = UIFactory.Font.material;
            mesh.anchor = TextAnchor.MiddleCenter; mesh.alignment = TextAlignment.Center; mesh.color = color;
            return go;
        }
        private static void Interactive(GameObject go, InteractionAction action, string label, Action<AcademyInteractable> callback, bool grab = false)
        {
            var target = go.AddComponent<AcademyInteractable>(); target.action = action; target.label = label;
            target.grabbable = grab; target.Interacted += callback;
            go.AddComponent<XRInteractionBridge>().target = target;
        }
        public static Camera Build(Action<AcademyInteractable> callback)
        {
            var root = new GameObject("Office · desktop and future XR").transform;
            var navy = Material("Midnight", new Color(.045f, .08f, .12f));
            var wall = Material("Blue slate", new Color(.12f, .19f, .23f));
            var wood = Material("Warm walnut", new Color(.38f, .23f, .15f));
            var black = Material("Graphite", new Color(.025f, .033f, .04f));
            var cyan = Material("Mint", new Color(.22f, .83f, .73f));
            var amber = Material("Paper amber", new Color(.92f, .68f, .32f));
            var cream = Material("Paper", new Color(.82f, .84f, .78f));
            var glass = Material("Window glow", new Color(.15f, .42f, .5f));
            glass.EnableKeyword("_EMISSION"); glass.SetColor("_EmissionColor", new Color(.08f, .22f, .28f));
            Box(root, "Floor", new Vector3(2, -.1f, -.5f), new Vector3(12.2f, .2f, 7.8f), navy);
            Box(root, "Rear wall", new Vector3(2, 2.3f, 3.3f), new Vector3(12.2f, 4.6f, .2f), wall);
            Box(root, "Front wall", new Vector3(2, 2.3f, -4.3f), new Vector3(12.2f, 4.6f, .2f), wall);
            Box(root, "Left wall", new Vector3(-4, 2.3f, -.5f), new Vector3(.2f, 4.6f, 7.8f), wall);
            Box(root, "Archive outer wall", new Vector3(8, 2.3f, -.5f), new Vector3(.2f, 4.6f, 7.8f), wall);
            Box(root, "Archive partition north", new Vector3(4, 2.3f, 1.45f), new Vector3(.15f, 4.6f, 3.7f), wall);
            Box(root, "Archive partition south", new Vector3(4, 2.3f, -3.25f), new Vector3(.15f, 4.6f, 2.1f), wall);
            Box(root, "Door lintel", new Vector3(4, 3.45f, -1.3f), new Vector3(.15f, 2.3f, 1.8f), wall);
            Box(root, "Ceiling", new Vector3(2, 4.6f, -.5f), new Vector3(12.2f, .1f, 7.8f), navy);
            Box(root, "Window", new Vector3(-3.87f, 2.45f, .2f), new Vector3(.04f, 2.6f, 3.8f), glass);
            for (int i = 0; i < 6; i++) Box(root, "Window slat", new Vector3(-3.8f, 1.4f + i * .42f, .2f), new Vector3(.07f, .045f, 3.85f), black);
            Box(root, "Desk top", new Vector3(0, .85f, .5f), new Vector3(3.4f, .14f, 1.4f), wood);
            foreach (float x in new[] { -1.4f, 1.4f }) Box(root, "Desk leg", new Vector3(x, .4f, .5f), new Vector3(.1f, .8f, 1.1f), black);
            Box(root, "Desk mat", new Vector3(0, .93f, .25f), new Vector3(1.6f, .018f, .8f), navy);
            var laptop = Box(root, "Laptop base", new Vector3(-.25f, .98f, .38f), new Vector3(.92f, .06f, .62f), black);
            var laptopGroup = new GameObject("Portable laptop assembly").transform; laptopGroup.SetParent(root, false); laptopGroup.position = laptop.transform.position;
            laptop.transform.SetParent(laptopGroup, true);
            var screen = Box(root, "Laptop display", new Vector3(-.25f, 1.29f, .69f), new Vector3(.93f, .57f, .06f), black);
            screen.transform.SetParent(laptopGroup, true);
            Box(root, "Display pixels", new Vector3(-.25f, 1.3f, .652f), new Vector3(.81f, .45f, .012f), navy).transform.SetParent(laptopGroup, true);
            Sign(root, ">_  B / R\nACADEMY OS", new Vector3(-.25f, 1.3f, .638f), .046f, cyan.color).transform.SetParent(laptopGroup, true);
            for (int i = 0; i < 4; i++) Box(root, "Keyboard row", new Vector3(-.25f, 1.015f, .24f + i * .09f), new Vector3(.72f, .007f, .018f), wall).transform.SetParent(laptopGroup, true);
            Interactive(laptopGroup.gameObject, InteractionAction.Laptop, "LAPTOP · Abrir terminal", callback, true);
            var tickets = Box(root, "Ticket inbox", new Vector3(1.12f, 1.02f, .55f), new Vector3(.56f, .18f, .48f), amber);
            Interactive(tickets, InteractionAction.Tickets, "BUZÓN · Elegir misión", callback, true);
            var notebook = Box(root, "Analyst notebook", new Vector3(-1.15f, .97f, .24f), new Vector3(.48f, .06f, .58f), cyan);
            Interactive(notebook, InteractionAction.Notebook, "CUADERNO · Notas y hallazgos", callback, true);
            var board = Box(root, "Mission whiteboard", new Vector3(.9f, 2.2f, 3.13f), new Vector3(3.3f, 1.8f, .08f), navy);
            Interactive(board, InteractionAction.Board, "PIZARRA · Misiones y progreso", callback);
            Sign(root, "BLUE / RED\nANALYST ACADEMY", new Vector3(.9f, 2.65f, 3.065f), .105f, cream.color);
            for (int i = 0; i < 4; i++)
            {
                var ticket = Box(root, "Pinned ticket " + (i + 1), new Vector3(-.2f + i * .73f, 1.97f, 3.045f), new Vector3(.55f, .53f, .025f), i == 0 ? cyan : amber);
                Interactive(ticket, InteractionAction.Tickets, "TICKET " + (i + 1) + " · Revisar misiones", callback, true);
                ticket.GetComponent<AcademyInteractable>().missionId = new[] { "recon", "social", "identity", "web" }[i];
                Sign(root, "0" + (i + 1), new Vector3(-.2f + i * .73f, 1.98f, 3.025f), .1f, navy.color).transform.SetParent(ticket.transform, true);
            }
            Box(root, "Bookshelf", new Vector3(3.2f, 1.1f, 2.8f), new Vector3(1.2f, 2.2f, .55f), wood);
            for (int i = 0; i < 9; i++) Box(root, "Reference book", new Vector3(2.76f + i * .105f, 1.4f, 2.47f), new Vector3(.075f, .55f, .25f), i % 3 == 0 ? amber : navy);
            Box(root, "Plant pot", new Vector3(-2.7f, .32f, 2.1f), new Vector3(.55f, .65f, .55f), cream);
            for (int i = 0; i < 5; i++)
            {
                var leaf = Box(root, "Stylized leaf", new Vector3(-2.7f + (i - 2) * .12f, .95f, 2.1f), new Vector3(.17f, .9f, .14f), cyan);
                leaf.transform.localRotation = Quaternion.Euler(0, i * 35, (i - 2) * 14);
            }
            var sun = new GameObject("Warm key light").AddComponent<Light>(); sun.type = LightType.Directional;
            sun.transform.rotation = Quaternion.Euler(45, -35, 0); sun.color = new Color(1, .84f, .65f); sun.intensity = 1.25f;
            sun.shadows = LightShadows.Soft;
            var fill = new GameObject("Cool office light").AddComponent<Light>(); fill.type = LightType.Point;
            fill.transform.position = new Vector3(-2, 3, -1); fill.color = new Color(.45f, .79f, 1); fill.intensity = 2; fill.range = 9;
            // The archive is a second walkable room reached through a working hinged door.
            var hinge = new GameObject("Archive door hinge").transform; hinge.SetParent(root, false); hinge.localPosition = new Vector3(4, 0, -.45f);
            var door = Box(hinge, "Archive door", new Vector3(0, 1.1f, -.82f), new Vector3(.09f, 2.2f, 1.64f), wood);
            Interactive(door, InteractionAction.Environment, "PUERTA DEL ARCHIVO · Abrir / cerrar", callback);
            var doorMechanism = door.AddComponent<OfficeMechanism>(); doorMechanism.kind = MechanismKind.Door; doorMechanism.movingPart = hinge;
            door.GetComponent<AcademyInteractable>().description = "El archivo conserva las fichas del caso activo. Abre la puerta y explora las evidencias impresas.";
            var switchObject = Box(root, "Light switch", new Vector3(3.88f, 1.35f, -.04f), new Vector3(.06f, .19f, .12f), cream);
            Interactive(switchObject, InteractionAction.Environment, "INTERRUPTOR · Encender / apagar", callback);
            var lightMechanism = switchObject.AddComponent<OfficeMechanism>(); lightMechanism.kind = MechanismKind.Light; lightMechanism.controlledLight = fill;
            var archiveLight = new GameObject("Archive light").AddComponent<Light>(); archiveLight.type = LightType.Point;
            archiveLight.transform.position = new Vector3(6, 3.4f, 0); archiveLight.color = new Color(1, .78f, .52f); archiveLight.intensity = 2.2f; archiveLight.range = 7;
            Sign(root, "ARCHIVO DE CASOS\nOBSERVAR  /  DOCUMENTAR", new Vector3(6, 2.65f, 3.16f), .085f, cream.color);
            Box(root, "Archive table", new Vector3(6, .85f, 1.1f), new Vector3(2.6f, .14f, 1.1f), wood);
            foreach (float x in new[] { 4.9f, 7.1f }) Box(root, "Archive table leg", new Vector3(x, .4f, 1.1f), new Vector3(.12f, .8f, .85f), black);
            for (int i = 0; i < 3; i++)
            {
                var folder = Box(root, "Case evidence folio " + (i + 1), new Vector3(5.2f + i * .78f, .97f, .98f), new Vector3(.56f, .07f, .65f), i == 1 ? cyan : amber);
                Interactive(folder, InteractionAction.Evidence, "FICHA " + (i + 1) + " · Evidencia del caso activo", callback, true);
                var interactable = folder.GetComponent<AcademyInteractable>(); interactable.evidenceSlot = i;
                interactable.description = "Carpeta de trabajo: contiene una evidencia del ticket activo. Puedes leerla, llevarla contigo y documentar el hallazgo.";
            }
            Box(root, "Cabinet back", new Vector3(7.3f, .7f, -2.7f), new Vector3(1.05f, 1.4f, .08f), wood);
            Box(root, "Cabinet left", new Vector3(6.78f, .7f, -2.35f), new Vector3(.08f, 1.4f, .8f), wood);
            Box(root, "Cabinet right", new Vector3(7.82f, .7f, -2.35f), new Vector3(.08f, 1.4f, .8f), wood);
            Box(root, "Cabinet top", new Vector3(7.3f, 1.43f, -2.35f), new Vector3(1.1f, .08f, .8f), wood);
            var drawerRoot = new GameObject("Drawer assembly").transform; drawerRoot.SetParent(root, false); drawerRoot.localPosition = new Vector3(7.3f, 1.1f, -2.25f);
            var drawer = Box(drawerRoot, "Sliding drawer front", new Vector3(0, 0, .38f), new Vector3(.93f, .38f, .09f), amber);
            Box(drawerRoot, "Sliding drawer bottom", new Vector3(0, -.18f, 0), new Vector3(.93f, .05f, .76f), wood);
            Interactive(drawer, InteractionAction.Environment, "CAJÓN · Abrir / cerrar", callback);
            var drawerMechanism = drawer.AddComponent<OfficeMechanism>(); drawerMechanism.kind = MechanismKind.Drawer; drawerMechanism.movingPart = drawerRoot; drawerMechanism.openOffset = new Vector3(0, 0, .55f);
            var usb = Box(drawerRoot, "Fictional USB evidence", new Vector3(0, -.105f, .05f), new Vector3(.25f, .07f, .11f), cyan);
            Interactive(usb, InteractionAction.Evidence, "USB DE UTILERÍA · Ficha adicional", callback, true);
            usb.GetComponent<AcademyInteractable>().evidenceSlot = 3;
            usb.GetComponent<AcademyInteractable>().description = "Objeto narrativo. No ejecuta archivos: enlaza con la cuarta ficha del caso activo, o con la última ficha disponible en el tutorial.";
            var mug = GameObject.CreatePrimitive(PrimitiveType.Cylinder); mug.name = "Analyst coffee cup"; mug.transform.SetParent(root);
            mug.transform.localPosition = new Vector3(.86f, 1.08f, .1f); mug.transform.localScale = new Vector3(.15f, .13f, .15f); mug.GetComponent<Renderer>().sharedMaterial = cream;
            Interactive(mug, InteractionAction.Environment, "TAZA · Una pausa para pensar", callback, true);
            mug.GetComponent<AcademyInteractable>().description = "No todo se resuelve escribiendo más rápido. Una pausa también forma parte del trabajo de análisis.";
            var badge = Box(root, "Junior analyst badge", new Vector3(-.9f, .98f, .65f), new Vector3(.19f, .025f, .28f), amber);
            Interactive(badge, InteractionAction.Environment, "CREDENCIAL · Analista junior", callback, true);
            badge.GetComponent<AcademyInteractable>().description = "Tu credencial de Blue / Red. La autorización de cada misión tiene límites: que algo esté a tu alcance no te da permiso para investigarlo.";
            RenderSettings.ambientMode = AmbientMode.Flat; RenderSettings.ambientLight = new Color(.34f, .4f, .47f);
            RenderSettings.fog = true; RenderSettings.fogColor = navy.color; RenderSettings.fogDensity = .022f;
            var camera = new GameObject("Analyst camera").AddComponent<Camera>(); camera.tag = "MainCamera";
            camera.transform.position = new Vector3(0, 1.72f, -2.7f); camera.transform.rotation = Quaternion.Euler(10, 0, 0);
            camera.fieldOfView = 62; camera.nearClipPlane = .05f; camera.farClipPlane = 40;
            camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = navy.color;
            camera.gameObject.AddComponent<AudioListener>();
            return camera;
        }
    }
}
