// 3. Assets/Scripts/CoreDomain/Services/IPlayerDataService.cs
// The public face that UI scripts will access via the Service Locator
using Billiards.CoreDomain.Player;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
public interface IPlayerDataService
{
    UniTask InitializeAsync();
    PlayerProfileSave CurrentProfile { get; }
    void AddCoins(int amount);
    void UnlockCue(string cueId);
    UniTask ForceSyncAsync();
    // THE NEW METHOD: Returns the list of player achievements
    UniTask<List<AchievementData>> GetAchievementsAsync();
    void ClearAchievementsCache();
}
