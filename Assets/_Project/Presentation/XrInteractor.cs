using System;
using EthicalLab.Shared;

namespace EthicalLab.Presentation
{
    /// <summary>
    /// Stub VR. Más adelante XRI hover/select/activate disparan los mismos eventos.
    /// </summary>
    public sealed class XrInteractor : IInteractor
    {
        public event Action<InteractableId> Used;
        public event Action<InteractableId> Grabbed;
        public event Action<InteractableId> Released;
        public event Action<InteractableId, bool> Hovered;

        public void Hover(InteractableId id, bool on) => Hovered?.Invoke(id, on);
        public void Use(InteractableId id) => Used?.Invoke(id);
        public void Grab(InteractableId id) => Grabbed?.Invoke(id);
        public void Release(InteractableId id) => Released?.Invoke(id);
    }
}
