using System.Collections.Generic;
using UnityEngine;

namespace EthicalLab.Presentation
{
    public static class HubSceneLoader
    {
        /// <summary>Escena con <see cref="HubSceneRefs"/> válido, o oficina procedural legacy.</summary>
        public static HubScene Resolve()
        {
            var refs = Object.FindFirstObjectByType<HubSceneRefs>();
            if (refs != null && TryCreate(refs, out HubScene fromRefs))
                return fromRefs;
            return HubOffice.BuildProceduralLegacy();
        }

        public static bool TryCreate(HubSceneRefs refs, out HubScene scene)
        {
            scene = null;
            if (refs == null || !refs.IsValid) return false;

            scene = new HubScene
            {
                Root = refs.root != null ? refs.root : refs.transform,
                Camera = refs.mainCamera,
                Person = refs.person,
                LaptopScreen = refs.laptopScreen,
                BoardTitle = refs.boardTitle,
                BoardObjective = refs.boardObjective,
                TrayHeader = refs.trayHeader,
                ClueChecklist = refs.clueChecklist,
                OutOfScope = refs.outOfScope,
                ReportInbox = refs.reportInbox,
                Door = refs.door,
                Drawer = refs.drawer
            };

            scene.Tickets.AddRange(NonNull(refs.tickets));
            scene.Folders.AddRange(NonNull(refs.folders));
            scene.Trays.AddRange(NonNull(refs.trays));
            return true;
        }

        static IEnumerable<InteractableView> NonNull(InteractableView[] items)
        {
            if (items == null) yield break;
            for (int i = 0; i < items.Length; i++)
                if (items[i] != null) yield return items[i];
        }
    }
}
