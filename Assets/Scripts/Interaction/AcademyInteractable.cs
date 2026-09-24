using System;
using UnityEngine;

namespace Academy
{
    public enum InteractionAction { Laptop, Tickets, Board, Notebook, Evidence, Environment }
    public interface IAcademyInteractable
    {
        string Prompt { get; }
        void Focus(bool focused);
        void Activate();
        void Grab(Transform anchor);
        void Release();
    }

    public sealed class AcademyInteractable : MonoBehaviour, IAcademyInteractable
    {
        public InteractionAction action;
        public string label;
        public bool grabbable;
        public string missionId;
        public string evidenceId;
        public int evidenceSlot = -1;
        [TextArea] public string description;
        public bool IsHeld => grabbed;
        public event Action<InteractionAction> Activated;
        public event Action<AcademyInteractable> Interacted;
        public string Prompt => label;
        private Transform originalParent;
        private Vector3 originalPosition;
        private Quaternion originalRotation;
        private bool grabbed;
        private Renderer surface;
        private Color baseColor;
        private Collider[] colliders;
        private bool[] colliderStates;
        private void Awake()
        {
            surface = GetComponentInChildren<Renderer>();
            if (surface != null) baseColor = surface.material.color;
        }
        public void Focus(bool focused)
        {
            if (surface != null) surface.material.color = focused ? Color.Lerp(baseColor, Color.white, .25f) : baseColor;
        }
        public void Activate() { Activated?.Invoke(action); Interacted?.Invoke(this); }
        public void Grab(Transform anchor)
        {
            if (!grabbable || grabbed || anchor == null) return;
            originalParent = transform.parent; originalPosition = transform.localPosition; originalRotation = transform.localRotation;
            colliders = GetComponentsInChildren<Collider>(); colliderStates = new bool[colliders.Length];
            for (int i = 0; i < colliders.Length; i++) { colliderStates[i] = colliders[i].enabled; colliders[i].enabled = false; }
            Focus(false);
            transform.SetParent(anchor, true); transform.localPosition = Vector3.zero; transform.localRotation = Quaternion.identity; grabbed = true;
        }
        public void Release()
        {
            if (!grabbed) return;
            transform.SetParent(originalParent, true);
            transform.localPosition = originalPosition; transform.localRotation = originalRotation;
            for (int i = 0; i < colliders.Length; i++) if (colliders[i] != null) colliders[i].enabled = colliderStates[i];
            grabbed = false;
        }
    }
}
