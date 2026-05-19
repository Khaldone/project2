// 2. The Backend Contract (What we need the server to do)
using System.Collections.Generic;
using System.Threading.Tasks;

public interface ILeagueBackend
{
    Task<List<LeaguePlayer>> FetchTopPlayersAsync(int count);
    Task<bool> SubmitTrophiesAsync(int newTotal);
}