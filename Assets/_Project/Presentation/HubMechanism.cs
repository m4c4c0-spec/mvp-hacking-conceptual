using System;
using UnityEngine;

namespace EthicalLab.Presentation
{
    public enum MechanismKind { Door, Drawer }

    /// <summary>Puerta con bisagra o cajón deslizante. Solo física de escena; sin reglas de juego.</summary>
    public sealed class HubMechanism : MonoBehaviour
    {
        public MechanismKind kind;
        public Transform movingPart;
        public Vector3 openOffset = new Vector3(0f, 0f, 0.55f);
        public float openAngle = -100f;
        bool open;
        Vector3 closedPosition;
        Quaternion closedRotation;

        void Start()
        {
            if (movingPart == null) movingPart = transform;
            closedPosition = movingPart.localPosition;
            closedRotation = movingPart.localRotation;
        }

        public bool IsOpen => open;

        public void Toggle() => open = !open;

        void Update()
        {
            if (movingPart == null) return;
            float t = 1f - Mathf.Exp(-8f * Time.deltaTime);
            switch (kind)
            {
                case MechanismKind.Door:
                    movingPart.localRotation = Quaternion.Slerp(movingPart.localRotation, closedRotation * Quaternion.Euler(0f, open ? openAngle : 0f, 0f), t);
                    break;
                case MechanismKind.Drawer:
                    movingPart.localPosition = Vector3.Lerp(movingPart.localPosition, closedPosition + (open ? openOffset : Vector3.zero), t);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(kind), kind, null);
            }
        }
    }
}
