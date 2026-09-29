using UnityEngine;

namespace EthicalLab.Presentation
{
    /// <summary>
    /// Referencias serializadas del hub modelado (prefab o escena Hub_Art).
    /// Si <see cref="IsValid"/>, <see cref="HubSceneLoader"/> usa esto en lugar de geometría procedural.
    /// </summary>
    public sealed class HubSceneRefs : MonoBehaviour
    {
        public Camera mainCamera;
        public PcInteractor person;
        public Transform root;
        public TextMesh laptopScreen;
        public TextMesh boardTitle;
        public TextMesh boardObjective;
        public TextMesh trayHeader;
        public TextMesh clueChecklist;
        public InteractableView[] tickets = new InteractableView[4];
        public InteractableView[] folders = new InteractableView[4];
        public InteractableView[] trays = new InteractableView[3];
        public InteractableView outOfScope;
        public InteractableView reportInbox;
        public HubMechanism door;
        public HubMechanism drawer;

        public bool IsValid =>
            person != null &&
            mainCamera != null &&
            laptopScreen != null && boardTitle != null && boardObjective != null &&
            trayHeader != null && clueChecklist != null &&
            Complete(tickets, 4) && Complete(folders, 4) && Complete(trays, 3) &&
            outOfScope != null &&
            reportInbox != null &&
            door != null &&
            drawer != null;

        static bool Complete(InteractableView[] views, int count)
        {
            if (views == null || views.Length < count) return false;
            for (int i = 0; i < views.Length; i++)
                if (views[i] == null) return false;
            return true;
        }

        public void PopulateFrom(HubScene scene)
        {
            if (scene == null) return;
            root = scene.Root;
            mainCamera = scene.Camera;
            person = scene.Person;
            laptopScreen = scene.LaptopScreen;
            boardTitle = scene.BoardTitle;
            boardObjective = scene.BoardObjective;
            trayHeader = scene.TrayHeader;
            clueChecklist = scene.ClueChecklist;
            outOfScope = scene.OutOfScope;
            reportInbox = scene.ReportInbox;
            door = scene.Door;
            drawer = scene.Drawer;
            tickets = CopyList(scene.Tickets, 4);
            folders = CopyList(scene.Folders, 4);
            trays = CopyList(scene.Trays, 3);
        }

        static InteractableView[] CopyList(System.Collections.Generic.List<InteractableView> list, int min)
        {
            var arr = new InteractableView[min];
            for (int i = 0; i < min; i++)
                arr[i] = i < list.Count ? list[i] : null;
            return arr;
        }
    }
}
