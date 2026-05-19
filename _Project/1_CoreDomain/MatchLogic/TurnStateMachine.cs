// Assets/_Project/CoreDomain/MatchLogic/TurnStateMachine.cs
using System;
public class TurnStateMachine : ITurnStateMachine
{
    public event Action<string> OnTurnStarted;
    public string CurrentPlayerId { get; private set; }

    public void EndCurrentTurn()
    {
        // ... logic to figure out whose turn is next ...
        CurrentPlayerId = "Player_2";


        // FIRE THE EVENT:
        OnTurnStarted?.Invoke(CurrentPlayerId);
    }
}