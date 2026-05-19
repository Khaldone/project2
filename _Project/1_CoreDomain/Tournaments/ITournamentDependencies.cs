// Assets/_Project/1_CoreDomain/Tournaments/ITournamentDependencies.cs
using System.Collections.Generic;

public enum BracketRound { QuarterFinal, SemiFinal, Final, Champion }
public enum TournamentState_New { NotJoined, Active, Eliminated, Completed }

public struct TournamentData
{
    public string TournamentId;
    public TournamentState_New State;
    public BracketRound CurrentRound;
    public List<string> ActiveBotIds; // Bots still alive in the bracket
}

public interface ITournamentCloudService
{
    TournamentData GetLocalBracketState();
    void SaveBracketState(TournamentData data);
}

// Injected so we can force specific outcomes in our unit tests!
public interface IRngProvider
{
    float GetRandomFloat01();
    float GetRandomFloatMinus1To1();
}

public struct BracketUpdatedMessage
{
    public BracketRound NewRound;
    public int BotsRemaining;
}