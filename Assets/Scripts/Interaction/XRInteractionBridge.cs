using UnityEngine;

namespace Academy
{
    // Stub only. Later wire XRI hover/select/activate events to these same actions.
    // No headset or XR package is required to build the desktop demo.
    public sealed class XRInteractionBridge : MonoBehaviour
    {
        public AcademyInteractable target;
        public Transform handAnchor;
        public void HoverEnter() { if (target != null) target.Focus(true); }
        public void HoverExit() { if (target != null) target.Focus(false); }
        public void Activate() { if (target != null) target.Activate(); }
        public void Select() { if (target != null) target.Grab(handAnchor); }
        public void Deselect() { if (target != null) target.Release(); }
        private void OnDisable() { HoverExit(); Deselect(); }
    }
}
