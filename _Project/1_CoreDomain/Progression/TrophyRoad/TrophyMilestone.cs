// Assets/_Project/1_CoreDomain/Progression/TrophyRoad/TrophyMilestone.cs
namespace Billiards.Core.Progression
{
    public enum MilestoneState
    {
        Locked,
        Claimable,
        Claimed
    }

    /// <summary>
    /// Pure, immutable C# domain model representing a single progression step.
    /// </summary>
    public struct TrophyMilestone
    {
        public string TaskId;
        public string Title;
        public string RewardDescription;
        public string IconAssetKey; // <-- NEW: Clean domain key for asset mapping
        public int RequiredCups;
        public MilestoneState State;

        public static MilestoneState EvaluateState(int currentCups, int requiredCups, bool isComplete, bool rewarded)
        {
            if (isComplete && rewarded) return MilestoneState.Claimed;
            return currentCups >= requiredCups ? MilestoneState.Claimable : MilestoneState.Locked;
        }
    }
}