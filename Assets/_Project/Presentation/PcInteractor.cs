using System;
using UnityEngine;
using EthicalLab.Shared;

namespace EthicalLab.Presentation
{
    /// <summary>
    /// Primera persona de escritorio. El visor XR implementará la misma IInteractor.
    /// Solo traduce input a eventos; no conoce misiones ni reglas.
    /// Física: aceleración / frenado separados, control aéreo reducido,
    /// pegado al suelo, empuje proporcional a la masa y caída con recuperación.
    /// Expone Velocity / LookMotion / IsGrounded para las manos procedurales.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public sealed class PcInteractor : MonoBehaviour, IInteractor
    {
        public event Action<InteractableId> Used;
        public event Action<InteractableId> Grabbed;
        public event Action<InteractableId, bool> Hovered;
        public event Action<InteractableId> Released;
        public event Action ControlInterrupted;

        [Header("Movimiento de escritorio (metros / segundo)")]
        [SerializeField, Min(0.5f)] float walkSpeed = 2.6f;
        [SerializeField, Min(0.5f)] float runSpeed = 4.4f;
        [SerializeField, Min(1f)] float acceleration = 18f;
        [SerializeField, Min(1f)] float deceleration = 26f;
        [SerializeField, Range(0.05f, 1f)] float airControl = 0.35f;
        [SerializeField, Min(0f)] float pushStrength = 34f;
        [SerializeField, Min(0f)] float dropToss = 0.45f;
        [SerializeField, Range(45f, 85f)] float pitchLimit = 80f;

        public float Sensitivity { get; private set; } = 1f;
        public bool InvertY { get; private set; }
        public float FieldOfView => view != null ? view.fieldOfView : 70f;
        public bool HasFocus => hasFocus;
        public const string SensitivityKey = "EthicalLab.MouseSensitivity";
        public const string InvertYKey = "EthicalLab.InvertY";
        public const string FieldOfViewKey = "EthicalLab.FieldOfView";

        public bool MenuOpen = true;
        public string Prompt { get; private set; }
        public InteractableView Held { get; private set; }

        // Lectura para animación en primera persona (manos) y tutoriales.
        public Vector3 Velocity { get; private set; }
        public Vector2 LookMotion { get; private set; }
        public bool IsGrounded { get; private set; } = true;
        public float SprintBlend { get; private set; }
        public float LandDip { get; private set; }
        public float WalkSpeed => walkSpeed;
        public float RunSpeed => runSpeed;
        public Transform HoldAnchor => hand;

        CharacterController body;
        Camera view;
        Transform hand;
        FirstPersonHands hands;
        float yaw;
        float pitch = 8f;
        float fallSpeed;
        InteractableView focused;
        Vector3 spawnPosition;
        float spawnYaw;
        Vector3 planarVelocity;
        bool wasBlocked = true;
        bool hasFocus = true;
        bool wasGrounded = true;

        void Awake()
        {
            body = GetComponent<CharacterController>();
            view = GetComponentInChildren<Camera>();
            if (view == null)
            {
                view = new GameObject("First-person camera").AddComponent<Camera>();
                view.transform.SetParent(transform, false);
                view.transform.localPosition = new Vector3(0f, 1.62f, 0f);
                view.nearClipPlane = 0.05f;
            }
            yaw = transform.eulerAngles.y;
            spawnYaw = yaw;
            spawnPosition = transform.position;
            ApplyViewSettings(PlayerPrefs.GetFloat(SensitivityKey, 1f),
                PlayerPrefs.GetInt(InvertYKey, 0) == 1, PlayerPrefs.GetFloat(FieldOfViewKey, 70f), false);
            view.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
            hand = new GameObject("Hand anchor / future XR hand").transform;
            hand.SetParent(view.transform, false);
            hand.localPosition = new Vector3(0.28f, -0.22f, 0.72f);
            hand.localRotation = Quaternion.Euler(20f, -15f, 0f);
            // Manos ficticias: decoración procedural hija de la cámara.
            hands = view.gameObject.AddComponent<FirstPersonHands>();
            hands.Initialize(this, view);
        }

        void Update()
        {
            float dt = Mathf.Min(Time.deltaTime, 0.05f);
            LandDip = Mathf.MoveTowards(LandDip, 0f, dt * 3.5f);

            if (MenuOpen || !hasFocus)
            {
                wasBlocked = true;
                planarVelocity = Vector3.zero;
                Velocity = Vector3.zero;
                LookMotion = Vector2.zero;
                SprintBlend = Mathf.MoveTowards(SprintBlend, 0f, dt * 6f);
                fallSpeed = 0f;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                ClearFocus();
                Prompt = "ESC · volver a caminar";
                return;
            }

            // Consume el click que cerró la UI y descarta el delta acumulado del mouse.
            if (wasBlocked)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                wasBlocked = false;
                LookMotion = Vector2.zero;
                return;
            }
            // El SO/editor puede liberar el cursor. Pedir reanudar, no recapturarlo en bucle.
            // Batchmode no tiene ventana de juego donde el SO pueda capturar el mouse.
            if (!UnityEngine.Application.isBatchMode && Cursor.lockState != CursorLockMode.Locked)
            {
                InterruptControls();
                return;
            }

            Vector2 delta = PcButtons.LookDelta * Sensitivity;
            LookMotion = delta;
            yaw = Mathf.Repeat(yaw + delta.x, 360f);
            pitch = Mathf.Clamp(pitch + delta.y * (InvertY ? 1f : -1f), -pitchLimit, pitchLimit);

            transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            view.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);

            Vector2 move = Vector2.ClampMagnitude(PcButtons.Move, 1f);
            bool wantsMove = move.sqrMagnitude > 0.0001f;
            bool sprinting = PcButtons.Sprint && wantsMove && move.y > 0.1f;
            SprintBlend = Mathf.MoveTowards(SprintBlend, sprinting ? 1f : 0f, dt * 4f);
            float speed = sprinting ? runSpeed : walkSpeed;
            Vector3 planar = transform.forward * move.y + transform.right * move.x;

            bool grounded = body.isGrounded;
            Vector3 desired = planar * speed;
            if (grounded)
            {
                float rate = wantsMove ? acceleration : deceleration;
                planarVelocity = Vector3.MoveTowards(planarVelocity, desired, rate * dt);
                // Anti-deriva: sin input y casi quieto, clavar a cero para que el test
                // de frenado y el tacto sean exactos.
                if (!wantsMove && planarVelocity.sqrMagnitude < 0.0004f)
                    planarVelocity = Vector3.zero;
            }
            else
            {
                // En el aire se conserva inercia: corrección limitada, sin frenado brusco.
                planarVelocity = Vector3.MoveTowards(planarVelocity, desired, acceleration * airControl * dt);
            }

            float impactSpeed = fallSpeed;
            if (grounded && fallSpeed < 0f) fallSpeed = -2f; // Pegado al suelo.
            else fallSpeed = Mathf.Max(fallSpeed + Physics.gravity.y * dt, -30f);
            body.Move((planarVelocity + Vector3.up * fallSpeed) * dt);
            IsGrounded = body.isGrounded;
            if (!wasGrounded && IsGrounded && impactSpeed < -3.5f)
                LandDip = Mathf.Clamp01(-impactSpeed / 14f);
            wasGrounded = IsGrounded;
            Velocity = planarVelocity + Vector3.up * fallSpeed;
            if (transform.position.y < -3f) ReturnToEntrance();

            Ray ray = view.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            InteractableView target = null;
            if (Physics.Raycast(ray, out RaycastHit hit, 3f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
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
                : "WASD · caminar     Shift · correr     Mouse · mirar     E · usar     ESC · menú";
            if (Held != null) Prompt += "\nEN MANO: " + Held.prompt + "     G · devolver";

            if (PcButtons.Drop)
            {
                if (Held != null) Drop();
                else if (focused != null && focused.grabbable) Take(focused);
            }

            if (focused != null && (PcButtons.Use || PcButtons.Click))
                Used?.Invoke(focused.Id);
        }

        public void ApplyViewSettings(float sensitivity, bool invertY, float fieldOfView, bool persist = true)
        {
            Sensitivity = float.IsNaN(sensitivity) ? 1f : Mathf.Clamp(sensitivity, 0.25f, 2.5f);
            InvertY = invertY;
            if (view != null)
                view.fieldOfView = float.IsNaN(fieldOfView) ? 70f : Mathf.Clamp(fieldOfView, 60f, 90f);
            if (!persist) return;
            PlayerPrefs.SetFloat(SensitivityKey, Sensitivity);
            PlayerPrefs.SetInt(InvertYKey, InvertY ? 1 : 0);
            PlayerPrefs.SetFloat(FieldOfViewKey, view != null ? view.fieldOfView : 70f);
            PlayerPrefs.Save();
        }

        public void ReturnToEntrance()
        {
            Drop();
            ClearFocus();
            body.enabled = false;
            transform.SetPositionAndRotation(spawnPosition, Quaternion.Euler(0f, spawnYaw, 0f));
            body.enabled = true;
            yaw = spawnYaw;
            pitch = 8f;
            view.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
            planarVelocity = Vector3.zero;
            Velocity = Vector3.zero;
            LookMotion = Vector2.zero;
            SprintBlend = 0f;
            LandDip = 0f;
            fallSpeed = 0f;
            wasGrounded = true;
            IsGrounded = true;
            wasBlocked = true;
        }

        void InterruptControls()
        {
            MenuOpen = true;
            wasBlocked = true;
            planarVelocity = Vector3.zero;
            Velocity = Vector3.zero;
            LookMotion = Vector2.zero;
            SprintBlend = 0f;
            fallSpeed = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            ClearFocus();
            ControlInterrupted?.Invoke();
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
            bool heavy = Held.dropInPlace;
            if (heavy)
            {
                Vector3 flat = transform.forward;
                flat.y = 0f;
                flat = flat.sqrMagnitude > 0.0001f ? flat.normalized : Vector3.forward;
                Held.transform.position = transform.position + flat * 1.05f + Vector3.up * 0.02f;
            }
            Held.Release();
            // Los objetos pesados (silla) heredan un poco del impulso para que
            // soltar en marcha se sienta físico; las pistas vuelven a casa exactas.
            if (heavy)
            {
                var droppedBody = Held.GetComponent<Rigidbody>();
                if (droppedBody != null && !droppedBody.isKinematic)
                {
                    Vector3 flat = transform.forward;
                    flat.y = 0f;
                    flat = flat.sqrMagnitude > 0.0001f ? flat.normalized : Vector3.forward;
                    Vector3 toss = planarVelocity * dropToss + flat * 0.5f;
                    if (toss.sqrMagnitude > 9f) toss = toss.normalized * 3f;
                    toss.y = 0.35f;
                    droppedBody.linearVelocity = toss;
                    droppedBody.angularVelocity = Vector3.zero;
                }
            }
            Held = null;
            Released?.Invoke(id);
        }

        void OnControllerColliderHit(ControllerColliderHit hit)
        {
            var other = hit.rigidbody;
            if (other == null || other.isKinematic) return;
            // No empujar techos/suelos, solo laterales.
            if (hit.normal.y < -0.35f) return;
            Vector3 push = hit.moveDirection;
            push.y = 0f;
            if (push.sqrMagnitude < 0.01f) return;
            push.Normalize();
            float speedFactor = Mathf.Clamp01(planarVelocity.magnitude / Mathf.Max(runSpeed, 0.01f));
            float force = pushStrength * (0.35f + 0.65f * speedFactor);
            other.AddForce(push * force, ForceMode.Force);
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
            wasBlocked = true;
            planarVelocity = Vector3.zero;
            Velocity = Vector3.zero;
            LookMotion = Vector2.zero;
            SprintBlend = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            ClearFocus();
            Drop();
        }

        void OnApplicationFocus(bool focus)
        {
            hasFocus = focus;
            wasBlocked = true;
            if (!focus)
            {
                InterruptControls();
            }
        }

        void OnApplicationPause(bool paused) { if (paused) InterruptControls(); }
    }
}
