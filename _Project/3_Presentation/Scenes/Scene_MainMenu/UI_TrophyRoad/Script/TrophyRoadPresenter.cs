// Assets/_Project/3_Presentation/TrophyRoad/TrophyRoadPresenter.cs
using Billiards.Core.Progression;
using Billiards.CoreDomain.Services;
using Billiards.CoreDomain.Telemetry;
using Cysharp.Threading.Tasks;
using Sentry.Unity;
using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

namespace Billiards.Presentation.TrophyRoad
{
    public sealed class TrophyRoadPresenter : VContainer.Unity.IStartable, IDisposable
    {
        private readonly ITrophyRoadOrchestrator _orchestrator;
        private readonly ITrophyRoadView _view;
        private readonly IAssetDeliveryService _assetService;
        private readonly ITelemetryService _telemetryService;

        [Inject]
        public TrophyRoadPresenter(
            ITrophyRoadOrchestrator orchestrator,
            ITrophyRoadView view,
            IAssetDeliveryService assetService,
            ITelemetryService telemetryService)
        {
            _orchestrator = orchestrator;
            _view = view;
            _assetService = assetService;
            _telemetryService = telemetryService;
        }

        public void Start()
        {
            _orchestrator.OnMilestonesUpdated += HandleMilestonesUpdated;
            _view.OnClaimNodeClicked += HandleClaimClicked;

            ExecuteInitializationSequenceAsync().Forget();
        }

        /// <summary>
        /// Asynchronous event handler that captures server profile pushes 
        /// and locks interactions until the visual slider animation concludes.
        /// </summary>
        private async void HandleMilestonesUpdated(IReadOnlyList<TrophyMilestone> m)
        {
            try
            {
                _view.SetInteractionLock(true);

                // Await the combined node generation pass and DOTween slider animation loop
                await _view.RenderTrack(m, _orchestrator.CurrentCups, LoadSpriteIconAsync);
                int milestoneIndex = 1;
                var contextData = new Dictionary<string, string> { { "milestone_node", milestoneIndex.ToString() } };
                _telemetryService.AddBreadcrumb("Player successfully triggered claimed reward milestone validation node click.", "ui_interaction", "click", contextData);
                SentrySdk.CaptureMessage("Test event");
                UnityEngine.Debug.Log("Captured");
                _view.SetInteractionLock(false);
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[Presenter] Track rendering loop encountered an error: {ex.Message}");
                _view.SetInteractionLock(false);
            }
        }

        private async UniTask<Sprite> LoadSpriteIconAsync(string assetKey)
        {
            if (string.IsNullOrEmpty(assetKey)) return null;
            return await _assetService.LoadAssetAsync<Sprite>(assetKey);
        }

        private void HandleClaimClicked(string taskId)
        {
            ExecuteClaimSequenceAsync(taskId).Forget();
        }

        private async UniTaskVoid ExecuteClaimSequenceAsync(string taskId)
        {
            try
            {
                _view.SetInteractionLock(true);

                // Execute transactional server update validation sequences
                bool success = await _orchestrator.ClaimMilestoneRewardAsync(taskId);

                // NOTE: The interaction lock is maintained by HandleMilestonesUpdated which triggers automatically 
                // when ClaimMilestoneRewardAsync updates the data cache, ensuring safety until the glide completes.
                if (success)
                {
                    _view.PlayBurstEffect(taskId);
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[Presenter] Claim sequence failed: {ex.Message}");
                _view.SetInteractionLock(false);
            }
        }

        private async UniTaskVoid ExecuteInitializationSequenceAsync()
        {
            try
            {
                _view.SetInteractionLock(true);
                await _orchestrator.FetchTrophyRoadDataAsync("trophyrd");
                // Handled securely: lock lifted automatically inside HandleMilestonesUpdated callback completion
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[Presenter] Trophy Road failed loading data: {ex.Message}");
                _view.SetInteractionLock(false);
            }
        }

        public void Dispose()
        {
            _orchestrator.OnMilestonesUpdated -= HandleMilestonesUpdated;
            _view.OnClaimNodeClicked -= HandleClaimClicked;
        }
    }
}