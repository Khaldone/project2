// Assets/_Project/CoreDomain/Progression/BattlePass/IBattlePassOrchestrator.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
public interface IBattlePassOrchestrator
{
    // The UI listens to this to update progress bars
    event Action<BattlePassState> OnStateUpdated;

    BattlePassState CurrentState { get; }
    IReadOnlyList<BattlePassReward> AvailableRewards { get; }


    Task FetchLatestDataAsync();
    Task<bool> ClaimRewardAsync(string itemId);
    Task PurchasePremiumPassAsync();

    // Internal use: Called when a match ends
    Task AddExperienceAsync(int amount);
}