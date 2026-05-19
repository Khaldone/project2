// Assets/_Project/1_CoreDomain/Progression/TrophyRoad/ITrophyRoadView.cs
using System;
using System.Collections.Generic;
using Billiards.Core.Progression;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Billiards.Presentation.TrophyRoad
{
    public interface ITrophyRoadView
    {
        event Action<string> OnClaimNodeClicked;

        UniTask RenderTrack(IReadOnlyList<TrophyMilestone> milestones, int activeCups, Func<string, UniTask<Sprite>> iconProvider);

        // FIXED: Signature updated to match the async DOTween implementation perfectly
        UniTask UpdateProgressFill(int activeCups, bool animate);

        void UpdateNodeState(string taskId, MilestoneState newState);
        void SetInteractionLock(bool isLocked);
        void PlayBurstEffect(string taskId);
    }
}