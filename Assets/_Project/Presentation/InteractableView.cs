using UnityEngine;
using EthicalLab.Shared;

namespace EthicalLab.Presentation
{
    public sealed class InteractableView : MonoBehaviour
    {
        public string id;
        public string prompt;
        public bool grabbable;
        /// <summary>Al soltar, se queda en el mundo con Rigidbody. Las pistas vuelven a su sitio.</summary>
        public bool dropInPlace;
        public Vector3 holdOffset = new Vector3(0.2f, -0.5f, 0.4f);
        public TextMesh sign;
        Vector3 home;
        Quaternion homeRot;
        Transform homeParent;
        bool held;
        bool focusOn;
        Renderer[] surfaces = System.Array.Empty<Renderer>();
        Color[] baseColors = System.Array.Empty<Color>();
        Collider[] colliders;
        Color flashColor;
        float flashUntil;
        float flashDuration;

        void Awake()
        {
            home = transform.localPosition;
            homeRot = transform.localRotation;
            homeParent = transform.parent;
            CacheSurfaces();
            colliders = GetComponentsInChildren<Collider>();
        }

        /// <summary>
        /// Pistas y bandejas tienen malla en el propio objeto. La silla es un grupo vacío:
        /// el resaltado tiñe los renderers de los hijos, no el cartel TextMesh.
        /// </summary>
        void CacheSurfaces()
        {
            var own = GetComponent<Renderer>();
            if (own != null)
                surfaces = new[] { own };
            else
            {
                var found = GetComponentsInChildren<Renderer>(true);
                int count = 0;
                for (int i = 0; i < found.Length; i++)
                    if (IsHighlightSurface(found[i])) count++;
                surfaces = new Renderer[count];
                int n = 0;
                for (int i = 0; i < found.Length; i++)
                    if (IsHighlightSurface(found[i])) surfaces[n++] = found[i];
            }
            baseColors = new Color[surfaces.Length];
            for (int i = 0; i < surfaces.Length; i++)
                baseColors[i] = surfaces[i].material.color;
        }

        static bool IsHighlightSurface(Renderer renderer)
        {
            return renderer != null && renderer.GetComponent<TextMesh>() == null;
        }

        public InteractableId Id => new InteractableId(id);
        public bool IsHeld => held;

        public void Focus(bool on)
        {
            focusOn = on;
            Paint();
        }

        public void Tint(Color color)
        {
            if (baseColors.Length == 0) return;
            bool same = true;
            for (int i = 0; i < baseColors.Length; i++)
            {
                if (baseColors[i] != color) { same = false; break; }
            }
            if (same) return;
            for (int i = 0; i < baseColors.Length; i++) baseColors[i] = color;
            Paint();
        }

        /// <summary>Destello breve (bandeja correcta/incorrecta). No cambia el tint base.</summary>
        public void Flash(Color color, float duration = 0.4f)
        {
            flashColor = color;
            flashDuration = Mathf.Max(0.05f, duration);
            flashUntil = Time.unscaledTime + flashDuration;
            Paint();
        }

        void Update()
        {
            if (surfaces.Length == 0) return;
            if (Time.unscaledTime < flashUntil)
            {
                Paint();
                return;
            }
            if (!focusOn) return;
            // Pulso suave para que el cliente vea qué objeto recibe E/G.
            float pulse = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 7f);
            float amount = 0.22f + 0.38f * pulse;
            for (int i = 0; i < surfaces.Length; i++)
            {
                if (surfaces[i] == null) continue;
                surfaces[i].material.color = Color.Lerp(baseColors[i], Color.white, amount);
            }
        }

        void Paint()
        {
            if (surfaces.Length == 0) return;
            bool flashing = Time.unscaledTime < flashUntil && flashDuration > 0f;
            float intensity = flashing ? Mathf.Clamp01((flashUntil - Time.unscaledTime) / flashDuration) : 0f;
            for (int i = 0; i < surfaces.Length; i++)
            {
                var surface = surfaces[i];
                if (surface == null) continue;
                if (flashing)
                    surface.material.color = Color.Lerp(baseColors[i], flashColor, 0.35f + 0.65f * intensity);
                else if (focusOn)
                    surface.material.color = Color.Lerp(baseColors[i], Color.white, 0.35f);
                else
                    surface.material.color = baseColors[i];
            }
        }

        public void Label(string text)
        {
            if (sign != null) sign.text = text ?? "";
        }

        public void Grab(Transform hand)
        {
            if (!grabbable || held || hand == null) return;
            for (int i = 0; i < colliders.Length; i++) if (colliders[i] != null) colliders[i].enabled = false;
            var body = GetComponent<Rigidbody>();
            if (body != null)
            {
                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
                body.isKinematic = true;
            }
            Focus(false);
            transform.SetParent(hand, true);
            transform.localPosition = dropInPlace ? holdOffset : Vector3.zero;
            transform.localRotation = Quaternion.identity;
            held = true;
        }

        public void Release()
        {
            if (!held) return;
            if (dropInPlace)
            {
                transform.SetParent(null, true);
                EnableColliders();
                var body = GetComponent<Rigidbody>();
                if (body != null)
                {
                    body.isKinematic = false;
                    body.WakeUp();
                }
                held = false;
                return;
            }
            transform.SetParent(homeParent, true);
            transform.localPosition = home;
            transform.localRotation = homeRot;
            EnableColliders();
            held = false;
        }

        void EnableColliders()
        {
            for (int i = 0; i < colliders.Length; i++) if (colliders[i] != null) colliders[i].enabled = true;
        }
    }
}
