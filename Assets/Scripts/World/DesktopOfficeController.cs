using UnityEngine;

namespace Academy
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class DesktopOfficeController : MonoBehaviour
    {
        public AcademyUI ui;
        public Camera view;
        public float walkSpeed = 2.5f, sprintSpeed = 4.2f, sensitivity = 1f;
        private CharacterController controller;
        private AcademyInteractable focused, held;
        private Transform hand;
        private float yaw, pitch, verticalSpeed, bobPhase;
        private bool freeCursor, wasOffice;
        private const float StandingHeight = 1.8f;
        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            controller.height = StandingHeight; controller.center = Vector3.up * .9f;
            controller.radius = .25f; controller.stepOffset = .25f; controller.slopeLimit = 45;
            controller.skinWidth = .035f;
        }
        private void Start()
        {
            if (view == null) view = GetComponentInChildren<Camera>();
            hand = new GameObject("Inspection anchor / future XR hand").transform;
            hand.SetParent(view.transform, false); hand.localPosition = new Vector3(.28f, -.23f, .8f);
        }
        private void Update()
        {
            bool office = ui != null && ui.IsOffice;
            if (!office)
            {
                UnlockCursor(); ClearFocus(); wasOffice = false;
                return;
            }
            if (!wasOffice) { freeCursor = false; wasOffice = true; }
            if (DesktopInput.ToggleCursor) freeCursor = !freeCursor;
            Cursor.lockState = freeCursor ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = freeCursor;
            ui.SetPointerMode(freeCursor);
            if (freeCursor)
            {
                ClearFocus(); ui.SetOfficePrompt("TAB · Volver a mirar     Usa las pestañas del escritorio con el cursor."); return;
            }
            Vector2 delta = DesktopInput.LookDelta * sensitivity;
            if (held != null && DesktopInput.RotateHeld) held.transform.Rotate(view.transform.up, -delta.x * 2, Space.World);
            else
            {
                yaw += delta.x; pitch = Mathf.Clamp(pitch - delta.y, -80, 80);
                transform.rotation = Quaternion.Euler(0, yaw, 0);
                view.transform.localRotation = Quaternion.Euler(pitch, 0, 0);
            }
            bool crouch = DesktopInput.Crouch;
            if (!crouch && controller.height < StandingHeight - .05f)
            {
                foreach (var hit in Physics.OverlapCapsule(transform.position + Vector3.up * .3f, transform.position + Vector3.up * 1.55f, .24f, ~0, QueryTriggerInteraction.Ignore))
                    if (hit != controller) { crouch = true; break; }
            }
            controller.height = Mathf.MoveTowards(controller.height, crouch ? 1.1f : StandingHeight, Time.deltaTime * 5);
            controller.center = Vector3.up * (controller.height * .5f);
            Vector2 input = Vector2.ClampMagnitude(DesktopInput.Move, 1);
            float speed = crouch ? 1.25f : DesktopInput.Sprint ? sprintSpeed : walkSpeed;
            Vector3 movement = (transform.forward * input.y + transform.right * input.x) * speed;
            if (controller.isGrounded && verticalSpeed < 0) verticalSpeed = -2;
            if (controller.isGrounded && !crouch && DesktopInput.Jump) verticalSpeed = 3.5f;
            verticalSpeed += Physics.gravity.y * Time.deltaTime;
            controller.Move((movement + Vector3.up * verticalSpeed) * Time.deltaTime);
            bobPhase += input.magnitude * speed * Time.deltaTime * 3.5f;
            float bob = ui.ReducedMotion ? 0 : Mathf.Sin(bobPhase) * .018f * input.magnitude;
            view.transform.localPosition = new Vector3(0, controller.height - .13f + bob, 0);
            if (transform.position.y < -3) { controller.enabled = false; transform.position = new Vector3(0, .05f, -3); controller.enabled = true; verticalSpeed = 0; }
            AcademyInteractable target = null;
            Ray ray = view.ViewportPointToRay(new Vector3(.5f, .5f, 0));
            if (Physics.Raycast(ray, out RaycastHit hitInfo, 2.7f, ~0, QueryTriggerInteraction.Ignore)) target = hitInfo.collider.GetComponentInParent<AcademyInteractable>();
            if (target != focused)
            {
                ClearFocus(); focused = target;
                if (focused != null) focused.Focus(true);
            }
            string prompt = focused != null ? "E / CLIC · " + focused.Prompt + (focused.grabbable ? "     G · Tomar" : "") + "     F · Inspeccionar" : "WASD · Moverte   SHIFT · Correr   CTRL · Agacharte   ESPACIO · Saltar   TAB · Cursor";
            if (held != null) prompt += "\nEN MANO: " + held.Prompt + "     G · Devolver   R + mouse · Girar   F · Inspeccionar";
            ui.SetOfficePrompt(prompt);
            if (DesktopInput.Grab)
            {
                if (held != null) { held.Release(); held = null; }
                else if (focused != null && focused.grabbable) { held = focused; held.Grab(hand); ClearFocus(); }
            }
            if (DesktopInput.Inspect && (held != null || focused != null)) ui.InspectObject(held != null ? held : focused);
            if (focused != null && (DesktopInput.Use || DesktopInput.Click))
            {
                focused.Activate();
            }
        }
        private void ClearFocus() { if (focused != null) focused.Focus(false); focused = null; }
        private static void UnlockCursor() { Cursor.lockState = CursorLockMode.None; Cursor.visible = true; }
        private void OnApplicationFocus(bool focus) { if (!focus) { freeCursor = true; UnlockCursor(); } }
        private void OnDisable() { UnlockCursor(); ClearFocus(); if (held != null) held.Release(); }
    }
}
