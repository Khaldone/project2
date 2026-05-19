// Assets/Scripts/CoreDomain/State/IBallStateTracker.cs
using System.Collections.Generic;
public interface IBallStateTracker
{
    // Returns the current speed (magnitude of velocity) of all active balls on the table.
    IEnumerable<float> GetActiveBallSpeeds();
}
