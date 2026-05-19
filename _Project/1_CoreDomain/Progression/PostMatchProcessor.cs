// 3. Assets/Scripts/CoreDomain/Progression/PostMatchProcessor.cs
// The brain that orchestrates the end-of-game flow.
using System.Threading.Tasks;


public class PostMatchProcessor
{
    private readonly IMatchResultSubmitter _resultSubmitter;
    private readonly IMatchUIView _uiView; // Reusing the UI interface we built earlier


    public PostMatchProcessor(IMatchResultSubmitter resultSubmitter, IMatchUIView uiView)
    {
        _resultSubmitter = resultSubmitter;
        _uiView = uiView;
    }


    public async Task ProcessMatchEndAsync(string opponentId, bool didWin, int shotsTaken)
    {
        _uiView.ShowNotification("Calculating Results...");

        // Show the player what they *should* get immediately to keep the UI responsive
        int projectedXp = MatchRewardCalculator.CalculateProjectedXP(didWin, shotsTaken);
        _uiView.ShowNotification($"Projected XP: +{projectedXp}");


        // Attempt to submit the secure payload to the backend
        bool submitSuccess = await _resultSubmitter.SubmitMatchDataAsync(opponentId, didWin, shotsTaken);


        if (submitSuccess)
        {
            _uiView.ShowNotification("Results Verified by Server.");
            _uiView.SetEndTurnButtonInteractable(true); // E.g., a "Return to Lobby" button
        }
        else
        {
            //_uiView.ShowError("Failed to sync match results. Retrying...");
            // In a production system, you would implement a retry queue here.
        }
    }
}
