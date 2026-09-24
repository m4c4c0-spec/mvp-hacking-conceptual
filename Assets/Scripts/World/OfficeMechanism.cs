using UnityEngine;

namespace Academy
{
    public enum MechanismKind { Door, Drawer, Light }
    [RequireComponent(typeof(AcademyInteractable))]
    public sealed class OfficeMechanism : MonoBehaviour
    {
        public MechanismKind kind;
        public Transform movingPart;
        public Vector3 openOffset = new Vector3(0, 0, -.35f);
        public Light controlledLight;
        private bool open;
        private Vector3 closedPosition;
        private Quaternion closedRotation;
        private void Start()
        {
            if (movingPart == null) movingPart = transform;
            closedPosition = movingPart.localPosition; closedRotation = movingPart.localRotation;
            GetComponent<AcademyInteractable>().Activated += Toggle;
        }
        private void Toggle(InteractionAction action)
        {
            open = !open;
            if (kind == MechanismKind.Light && controlledLight != null) controlledLight.enabled = !controlledLight.enabled;
        }
        private void Update()
        {
            if (movingPart == null) return;
            float t = 1 - Mathf.Exp(-8 * Time.deltaTime);
            if (kind == MechanismKind.Drawer) movingPart.localPosition = Vector3.Lerp(movingPart.localPosition, closedPosition + (open ? openOffset : Vector3.zero), t);
            if (kind == MechanismKind.Door) movingPart.localRotation = Quaternion.Slerp(movingPart.localRotation, closedRotation * Quaternion.Euler(0, open ? -100 : 0, 0), t);
        }
    }
}
