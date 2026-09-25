using UnityEngine;
using EthicalLab.Shared;

namespace EthicalLab.Presentation
{
    public sealed class InteractableView : MonoBehaviour
    {
        public string id;
        public string prompt;
        public bool grabbable;
        public TextMesh sign;
        Vector3 home;
        Quaternion homeRot;
        Transform homeParent;
        bool held;
        bool focusOn;
        Renderer surface;
        Color baseColor;
        Collider[] colliders;
        Color flashColor;
        float flashUntil;
        float flashDuration;

        void Awake()
        {
            home = transform.localPosition;
            homeRot = transform.localRotation;
            homeParent = transform.parent;
            surface = GetComponent<Renderer>();
            if (surface != null) baseColor = surface.material.color;
            colliders = GetComponentsInChildren<Collider>();
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
            if (baseColor == color) return;
            baseColor = color;
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
            if (surface == null) return;
            if (Time.unscaledTime < flashUntil)
            {
                Paint();
                return;
            }
            if (!focusOn) return;
            // Pulso suave para que el cliente vea qué objeto recibe E/G.
            float pulse = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 7f);
            surface.material.color = Color.Lerp(baseColor, Color.white, 0.22f + 0.38f * pulse);
        }

        void Paint()
        {
            if (surface == null) return;
            if (Time.unscaledTime < flashUntil && flashDuration > 0f)
            {
                float intensity = Mathf.Clamp01((flashUntil - Time.unscaledTime) / flashDuration);
                surface.material.color = Color.Lerp(baseColor, flashColor, 0.35f + 0.65f * intensity);
                return;
            }
            if (focusOn)
                surface.material.color = Color.Lerp(baseColor, Color.white, 0.35f);
            else
                surface.material.color = baseColor;
        }

        public void Label(string text)
        {
            if (sign != null) sign.text = text ?? "";
        }

        public void Grab(Transform hand)
        {
            if (!grabbable || held || hand == null) return;
            for (int i = 0; i < colliders.Length; i++) colliders[i].enabled = false;
            Focus(false);
            transform.SetParent(hand, true);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            held = true;
        }

        public void Release()
        {
            if (!held) return;
            transform.SetParent(homeParent, true);
            transform.localPosition = home;
            transform.localRotation = homeRot;
            for (int i = 0; i < colliders.Length; i++) if (colliders[i] != null) colliders[i].enabled = true;
            held = false;
        }
    }
}
