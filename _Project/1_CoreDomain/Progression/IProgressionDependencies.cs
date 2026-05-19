// Assets/_Project/1_CoreDomain/Progression/IProgressionDependencies.cs
using System.Threading.Tasks;


public struct PlayerProfileStats
{
    public int Level;
    public int CurrentXP;
    public int Trophies;
}


public interface IProfileDataService
{
    // Fetches the cached local data (hydrated from PlayFab on boot)
    PlayerProfileStats GetCurrentStats();

    // Commands the infrastructure layer to securely send the new stats to the server
    void SyncNewStatsToServer(PlayerProfileStats updatedStats);
}


// The message broadcasted to the UI so it can play the "+30 Trophies" animation
public struct ProgressionUpdatedMessage
{
    public int TrophyDelta;
    public int XpDelta;
    public bool DidLevelUp;
    public PlayerProfileStats NewTotalStats;
}