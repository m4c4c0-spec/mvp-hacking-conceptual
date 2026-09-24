using System;
using EthicalLab.Shared;

namespace EthicalLab.Presentation
{
    /// <summary>
    /// Puerta única de interacción. PC hoy, XR después: mismos eventos, mismos ids.
    /// </summary>
    public interface IInteractor
    {
        event Action<InteractableId> Used;
        event Action<InteractableId> Grabbed;
        event Action<InteractableId> Released;
        event Action<InteractableId, bool> Hovered;
    }
}
