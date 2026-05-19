// 2. Assets/Scripts/CoreDomain/Rules/TurnContext.cs
// This acts as a pure C# ledger for everything that happens between the strike and the resting state.
using System.Collections.Generic;


public class TurnContext
{
    public List<enBallType> PocketedBalls { get; private set; } = new List<enBallType>();


    public void RegisterPocketedBall(enBallType type)
    {
        PocketedBalls.Add(type);
    }


    public void Reset()
    {
        PocketedBalls.Clear();
    }
}


// 3. Assets/Scripts/CoreDomain/Rules/RulesEngine.cs
// The target of our test.
public class RulesEngine
{
    public enum TurnResult { ContinueTurn, EndTurn, Foul }


    public TurnResult EvaluateTurn(TurnContext context)
    {
        // Rule 1: If the cue ball goes in, it is immediately a foul (scratch).
        if (context.PocketedBalls.Contains(enBallType.Cue))
        {
            return TurnResult.Foul;
        }


        // Rule 2: If a valid ball was pocketed (simplified for this example)
        if (context.PocketedBalls.Count > 0)
        {
            return TurnResult.ContinueTurn;
        }


        // Default: Nothing happened, turn ends.
        return TurnResult.EndTurn;
    }
}
