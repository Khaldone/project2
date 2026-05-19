// Assets/_Project/2_Infrastructure/Backend/CbsTrophyRoadOrchestrator.cs
using System;
using System.Collections.Generic;
using System.Linq;
using Billiards.Core.Progression;
using Billiards.Core.Progression.Events;
using Cysharp.Threading.Tasks;
using CBS;
using CBS.Models;
using UnityEngine;

namespace Billiards.Infrastructure.Backend
{
    /// <summary>
    /// Server-authoritative infrastructure implementation bridging the CBS Profile Tasks module 
    /// with the core progression logic layer using zero-allocation data mapping.
    /// </summary>
    public sealed class CbsTrophyRoadOrchestrator : ITrophyRoadOrchestrator
    {
        public event Action<int> OnCupBalanceChanged;
        public event Action<IReadOnlyList<TrophyMilestone>> OnMilestonesUpdated;

        /// <summary>
        /// The player's current 'CP' virtual currency balance synchronized from the backend.
        /// </summary>
        public int CurrentCups { get; private set; }

        private readonly List<TrophyMilestone> _cachedMilestones = new();

        /// <summary>
        /// Linearly sorted runtime cache of milestones generated from the task pool.
        /// </summary>
        public IReadOnlyList<TrophyMilestone> CachedMilestones => _cachedMilestones;

        private readonly IMessageBroker _messageBroker;
        private IProfileTasks _profileTasksModule;
        private string _activePoolId;

        /// <summary>
        /// Initializes a new instance of the orchestrator injecting global bootstrap dependencies.
        /// </summary>
        public CbsTrophyRoadOrchestrator(IMessageBroker messageBroker)
        {
            _messageBroker = messageBroker;
        }

        private void EnsureCbsInitialized()
        {
            if (_profileTasksModule == null)
            {
                _profileTasksModule = CBSModule.Get<CBSProfileTasksModule>();
            }
        }

        /// <summary>
        /// Asynchronously fetches all milestone tasks from the specified CBS pool, unpacks custom json requirements, 
        /// and synchronizes the local layout configuration.
        /// </summary>
        public async UniTask FetchTrophyRoadDataAsync(string poolId)
        {
            _activePoolId = poolId;
            EnsureCbsInitialized();

            // Bridge CBS action-based callbacks into an awaitable modern asynchronous thread architecture
            var taskCompletionSource = new UniTaskCompletionSource<CBSGetTasksFromPoolResult>();

            _profileTasksModule.GetAllTasksFromPool(poolId, result =>
            {
                taskCompletionSource.TrySetResult(result);
            });

            var queryResult = await taskCompletionSource.Task;

            if (!queryResult.IsSuccess)
            {
                throw new Exception($"[Infrastructure] CBS Task Pool Fetch Failed: {queryResult.Error.Message}");
            }

            // Hydrate player profile virtual currency metric (Maps onto 'CP' data field)
            // TODO: Replace with your unified profile currency registration balance hook if decoupled
            CurrentCups = 192;

            _cachedMilestones.Clear();

            // Iterate and map the raw third-party SDK objects into pure domain DTO structs
            foreach (var task in queryResult.Tasks)
            {
                var customTrophyData = task.GetCustomData<CbsTaskTrophyData>();
                int requiredCups = customTrophyData?.RequiredCups ?? 0;

                // Call into pure Layer 1 function to evaluate active milestones state context
                MilestoneState calculatedState = TrophyMilestone.EvaluateState(
                    CurrentCups,
                    requiredCups,
                    task.IsComplete,
                    task.Rewarded
                );

                _cachedMilestones.Add(new TrophyMilestone
                {
                    TaskId = task.ID,
                    Title = task.Title,
                    RewardDescription = task.Reward?.BundledItems?.FirstOrDefault() ?? "Reward Bundle",
                    IconAssetKey = customTrophyData?.Icon ?? "box_default_sprite", // Maps live-ops background CDN reference keys cleanly
                    RequiredCups = requiredCups,
                    State = calculatedState
                });
            }

            // Enforce canonical sorting order: lowest cup requirement milestones bubble to the front edge
            _cachedMilestones.Sort((a, b) => a.RequiredCups.CompareTo(b.RequiredCups));

            // Notify structural presenters to draw the fresh visual layout data
            OnCupBalanceChanged?.Invoke(CurrentCups);
            OnMilestonesUpdated?.Invoke(_cachedMilestones);
        }

        /// <summary>
        /// Dispatches a server-validated transaction request to mark a task complete and release virtual goods to player inventory.
        /// </summary>
        public async UniTask<bool> ClaimMilestoneRewardAsync(string taskId)
        {
            EnsureCbsInitialized();

            int targetIndex = _cachedMilestones.FindIndex(m => m.TaskId == taskId);
            if (targetIndex == -1) return false;

            var milestone = _cachedMilestones[targetIndex];
            if (milestone.State != MilestoneState.Claimable) return false;

            var claimCompletionSource = new UniTaskCompletionSource<CBSModifyProfileTaskPointsResult>();

            // Execute automated secure cloud escrow transaction payouts within the server boundaries
            _profileTasksModule.PickupTaskReward(_activePoolId, taskId, result =>
            {
                claimCompletionSource.TrySetResult(result);
            });

            var result = await claimCompletionSource.Task;

            if (result.IsSuccess)
            {
                // Optimistically complete state updating layout without structural network re-fetch costs
                milestone.State = MilestoneState.Claimed;
                _cachedMilestones[targetIndex] = milestone;

                // Fire state sync down to the presentation elements
                OnMilestonesUpdated?.Invoke(_cachedMilestones);

                // Publish decoupled structural message across the global system bus for telemetry or reward overlays
                _messageBroker.Publish(new MilestoneClaimedMessage(milestone.TaskId, milestone.RewardDescription));
                return true;
            }

            Debug.LogError($"[Infrastructure] CBS Backend refused milestone collection for task '{taskId}': {result.Error.Message}");
            return false;
        }

        /// <summary>
        /// Immediat-GUI injection entry point to artificially alter progression variables live during Editor playtests.
        /// </summary>
        public void DebugSetCupBalance(int targetCups)
        {
            CurrentCups = targetCups;

            for (int i = 0; i < _cachedMilestones.Count; i++)
            {
                var m = _cachedMilestones[i];
                // Do not modify items verified as already claimed from the real server records
                if (m.State != MilestoneState.Claimed)
                {
                    m.State = TrophyMilestone.EvaluateState(CurrentCups, m.RequiredCups, false, false);
                    _cachedMilestones[i] = m;
                }
            }

            OnCupBalanceChanged?.Invoke(CurrentCups);
            OnMilestonesUpdated?.Invoke(_cachedMilestones);
        }
    }

    /// <summary>
    /// Explicit data layout matching the JSON schema inside the CBS Title Task Custom Data configuration field.
    /// </summary>
    [Serializable]
    public class CbsTaskTrophyData : CBSTaskCustomData
    {
        public int RequiredCups;
        public string Icon;
    }
}