// Assets/Scripts/CoreDomain/State/TableStateManager.cs
using System.Linq;
public class TableStateManager
{
    private readonly IBallStateTracker _ballTracker;
    private const float RESTING_THRESHOLD = 0.01f; // The mathematical definition of "stopped"


    public TableStateManager(IBallStateTracker ballTracker)
    {
        _ballTracker = ballTracker;
    }


    public bool AreAllBallsResting()
    {
        var speeds = _ballTracker.GetActiveBallSpeeds();

        // If there are no balls (e.g., game hasn't started), they aren't resting
        if (!speeds.Any()) return false;


        // LINQ queries the pure float values. No Unity overhead.
        return speeds.All(speed => speed <= RESTING_THRESHOLD);
    }
}
