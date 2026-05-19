// Assets/_Project/CoreDomain/MatchLogic/ITurnStateMachine.cs
using System;


public interface ITurnStateMachine
{
    // The Event: Anyone can listen, nobody but the StateMachine can fire it.
    event Action<string> OnTurnStarted;

    string CurrentPlayerId { get; }
    void EndCurrentTurn();
}