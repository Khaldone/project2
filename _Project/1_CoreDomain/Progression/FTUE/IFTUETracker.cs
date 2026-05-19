// Assets/_Project/CoreDomain/Progression/FTUE/IFTUETracker.cs
using System.Threading.Tasks;

// First-time-user-experience
public interface IFTUETracker
{
    bool HasCompletedTutorial { get; }
    Task FetchFTUEStateAsync();
    Task MarkTutorialCompleteAsync();
}
