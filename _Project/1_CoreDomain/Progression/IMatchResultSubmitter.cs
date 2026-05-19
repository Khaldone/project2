// 1. Assets/Scripts/CoreDomain/Progression/IMatchResultSubmitter.cs
using System.Threading.Tasks;

public interface IMatchResultSubmitter
{
    // The client never tells the server "give me 100 XP."
    // It tells the server the raw facts: "I played Player B, and I won."
    Task<bool> SubmitMatchDataAsync(string opponentId, bool didWin, int shotsTaken);
}