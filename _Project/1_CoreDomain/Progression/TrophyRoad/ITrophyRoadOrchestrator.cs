// Assets/_Project/1_CoreDomain/Progression/TrophyRoad/ITrophyRoadOrchestrator.cs
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Billiards.Core.Progression
{
    public interface ITrophyRoadOrchestrator
    {
        event Action<int> OnCupBalanceChanged;
        event Action<IReadOnlyList<TrophyMilestone>> OnMilestonesUpdated;

        int CurrentCups { get; }
        IReadOnlyList<TrophyMilestone> CachedMilestones { get; }

        UniTask FetchTrophyRoadDataAsync(string poolId);
        UniTask<bool> ClaimMilestoneRewardAsync(string taskId);
        void DebugSetCupBalance(int targetCups);
    }
}