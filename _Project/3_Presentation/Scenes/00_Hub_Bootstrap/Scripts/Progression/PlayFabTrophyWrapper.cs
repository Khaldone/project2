//// Assets/_Project/Scenes/00_Hub_Bootstrap/Scripts/Backend/PlayFabTrophyWrapper.cs
//using System;
//using System.Collections.Generic;
//using System.Threading.Tasks;
//using UnityEngine;
//using Billiards.Core.Progression;
//using Cysharp.Threading.Tasks;

//public class PlayFabTrophyWrapper : MonoBehaviour, ITrophyRoadOrchestrator
//{
//    public event Action<int> OnTrophyBalanceChanged;
//    public event Action<IReadOnlyList<TrophyMilestone>> OnMilestonesUpdated;


//    public int CurrentTrophies { get; private set; }

//    public IReadOnlyList<Billiards.Core.Progression.TrophyMilestone> CachedMilestones => throw new NotImplementedException();

//    private List<TrophyMilestone> _cachedMilestones;

//    event Action<IReadOnlyList<Billiards.Core.Progression.TrophyMilestone>> ITrophyRoadOrchestrator.OnMilestonesUpdated
//    {
//        add
//        {
//            throw new NotImplementedException();
//        }

//        remove
//        {
//            throw new NotImplementedException();
//        }
//    }

//    public async Task FetchTrophyDataAsync()
//    {
//        // 1. Call PlayFab API to get Player Statistic "Trophies"
//        // 2. Call PlayFab Title Data to get the master list of Trophy Milestones
//        // 3. Call PlayFab User Data to get the JSON array of "ClaimedTrophyIndexes"

//        // Assemble the pure C# structs
//        CurrentTrophies = 450; // Mock response
//        _cachedMilestones = new List<TrophyMilestone>
//        {
//            new TrophyMilestone { RequiredTrophies = 100, RewardId = "coins_500", IsClaimed = true },
//            new TrophyMilestone { RequiredTrophies = 500, RewardId = "cue_sapphire", IsClaimed = false }
//        };


//        // Inject the current trophies into the structs so they know if they are unlocked
//        for (int i = 0; i < _cachedMilestones.Count; i++)
//        {
//            var m = _cachedMilestones[i];
//            m.CurrentPlayerTrophies = CurrentTrophies;
//            _cachedMilestones[i] = m;
//        }


//        OnTrophyBalanceChanged?.Invoke(CurrentTrophies);
//        OnMilestonesUpdated?.Invoke(_cachedMilestones);
//    }


//    public async Task<bool> ClaimMilestoneAsync(int milestoneIndex)
//    {
//        var targetMilestone = _cachedMilestones[milestoneIndex];


//        // Client-side validation (fast fail)
//        if (targetMilestone.IsClaimed || !targetMilestone.IsUnlocked) return false;


//        // 1. Call secure PlayFab CloudScript: "ClaimTrophyReward"
//        // 2. CloudScript verifies the player actually has the required trophies.
//        // 3. CloudScript grants the item to their inventory and marks the index as claimed.

//        bool serverSuccess = true; // Assume CloudScript returned success


//        if (serverSuccess)
//        {
//            // Update local cache and fire event so the UI refreshes
//            targetMilestone.IsClaimed = true;
//            _cachedMilestones[milestoneIndex] = targetMilestone;
//            OnMilestonesUpdated?.Invoke(_cachedMilestones);
//            return true;
//        }


//        return false;
//    }

//    UniTask ITrophyRoadOrchestrator.FetchTrophyDataAsync()
//    {
//        throw new NotImplementedException();
//    }

//    UniTask<bool> ITrophyRoadOrchestrator.ClaimMilestoneAsync(int milestoneIndex)
//    {
//        throw new NotImplementedException();
//    }
//}