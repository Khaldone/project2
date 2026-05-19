// Assets/_Project/CoreDomain/Input/ICueInputListener.cs
using System;

public interface ICueInputListener
{
    // Fired while the player is dragging (useful for updating a 3D aiming line)
    event Action<enStrikeCommand> OnAimingUpdated;

    // Fired the moment the player lifts their finger
    event Action<enStrikeCommand> OnStrikeExecuted;

    // Fired if the player drags their finger into a "Cancel Area" on the screen
    event Action OnStrikeCanceled;
}
