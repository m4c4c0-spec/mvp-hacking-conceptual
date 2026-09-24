using System;
using UnityEngine;
using UnityEngine.EventSystems;
using EthicalLab.Shared;

namespace EthicalLab.Presentation
{
    /// <summary>
    /// Primera persona de escritorio. El visor XR implementará la misma IInteractor.
    /// Solo traduce input a eventos; no conoce misiones ni reglas.
    /// </summary>
    public sealed class PcInteractor : MonoBehaviour, IInteractor
    {
        public event Action<InteractableId> Used;
        public event Action<InteractableId> Grabbed;
        public event Action<InteractableId, bool> Hovered;
        public event Action<InteractableId> Released;

        public bool MenuOpen;
        public string Prompt { get; private set; }
        public InteractableView Held { get; private set; }

        CharacterController body;
        Camera view;
        Transform hand;
        float yaw;
        float pitch = 8f;
        float fallSpeed;
        InteractableView focused;

        void Awake()
        {
            body = GetComponent<CharacterController>();
            view = GetComponentInChildren<Camera>();
            if (view == null) view = GetComponent<Camera>();
            if (view == null) view = gameObject.AddComponent<Camera>();
            yaw = transform.eulerAngles.y;
            hand = new GameObject("Hand anchor / future XR hand").transform;
            hand.SetParent(view.transform, false);
            hand.localPosition = new Vector3(0.28f, -0.22f, 0.72f);
            hand.localRotation = Quaternion.Euler(20f, -15f, 0f);
        }

        void Update()
        {
            if (MenuOpen)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                ClearFocus();
                Prompt = "ESC · volver a caminar";
                return;
            }

            bool look = PcButtons.Look;
            Cursor.lockState = look ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !look;
            if (look)
            {
                Vector2 delta = PcButtons.LookDelta;
                yaw += delta.x;
                pitch = Mathf.Clamp(pitch - delta.y, -70f, 70f);
            }

            transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            view.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);

            Vector2 move = Vector2.ClampMagnitude(PcButtons.Move, 1f);
            Vector3 planar = transform.forward * move.y + transform.right * move.x;
            float speed = PcButtons.Sprint ? 4.4f : 2.6f;
            if (body != null)
            {
                if (body.isGrounded && fallSpeed < 0f) fallSpeed = -2f;
                else fallSpeed += Physics.gravity.y * Time.deltaTime;
                body.Move((planar * speed + Vector3.up * fallSpeed) * Time.deltaTime);
                if (transform.position.y < -3f)
                {
                    body.enabled = false;
                    transform.position = new Vector3(0f, 0.08f, -2.85f);
                    body.enabled = true;
                    fallSpeed = 0f;
                }
            }
            else
            {
                transform.position += planar * (Time.deltaTime * speed);
            }

            bool overUi = !look && EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
            Ray ray = look
                ? view.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f))
                : view.ScreenPointToRay(PcButtons.Pointer);
            InteractableView target = null;
            if (!overUi && Physics.Raycast(ray, out RaycastHit hit, 3f, ~0, QueryTriggerInteraction.Ignore))
                target = hit.collider.GetComponentInParent<InteractableView>();
            if (target == Held) target = null;

            if (target != focused)
            {
                if (focused != null)
                {
                    focused.Focus(false);
                    Hovered?.Invoke(focused.Id, false);
                }
                focused = target;
                if (focused != null)
                {
                    focused.Focus(true);
                    Hovered?.Invoke(focused.Id, true);
                }
            }

            Prompt = focused != null
                ? "[ E ]  " + focused.prompt + (focused.grabbable && Held == null ? "     G · tomar" : "")
                : "WASD · caminar     Shift · correr     botón derecho · mirar     E · usar     ESC · menú";
            if (Held != null) Prompt += "\nEN MANO: " + Held.prompt + "     G · devolver";

            if (PcButtons.Drop)
            {
                if (Held != null) Drop();
                else if (focused != null && focused.grabbable) Take(focused);
            }

            if (focused != null && (PcButtons.Use || PcButtons.Click))
                Used?.Invoke(focused.Id);
        }

        public void Take(InteractableView view)
        {
            if (view == null || !view.grabbable || Held != null) return;
            if (focused == view) ClearFocus();
            view.Grab(hand);
            Held = view;
            Grabbed?.Invoke(view.Id);
        }

        public void Drop()
        {
            if (Held == null) return;
            var id = Held.Id;
            Held.Release();
            Held = null;
            Released?.Invoke(id);
        }

        void ClearFocus()
        {
            if (focused == null) return;
            focused.Focus(false);
            Hovered?.Invoke(focused.Id, false);
            focused = null;
        }

        void OnDisable()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            ClearFocus();
            Drop();
        }
    }
}
