// Assets/_Project/CoreDomain/MatchLogic/Rules/EightBallStrategy.cs
public class EightBallStrategy : IGameRuleset
{
    public string ModeName => "8-Ball Classic";

    //public MatchConclusion CheckWinCondition(TableState tableState, string activePlayerId)
    //{
    //    bool isEightBallSunk = tableState.IsBallPocketed(8);

    //    if (isEightBallSunk)
    //    {
    //        // Did they sink it prematurely, or were they on the black?
    //        if (PlayerHasClearedTheirSuit(activePlayerId, tableState))
    //            return new MatchConclusion { IsGameOver = true, WinnerId = activePlayerId };
    //        else
    //            return new MatchConclusion { IsGameOver = true, WinnerId = "Opponent" }; // Scratch on 8-ball
    //    }

    //    return new MatchConclusion { IsGameOver = false };
    //}

    // ... Implement other interface methods ...
}