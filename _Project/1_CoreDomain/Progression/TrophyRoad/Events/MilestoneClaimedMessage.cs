// Assets/_Project/1_CoreDomain/Progression/TrophyRoad/Events/MilestoneClaimedMessage.cs
namespace Billiards.Core.Progression.Events
{
    /// <summary>
    /// Pure, allocation-free message envelope dispatched when a reward is successfully redeemed.
    /// </summary>
    public struct MilestoneClaimedMessage
    {
        public string TaskId;
        public string RewardId;

        public MilestoneClaimedMessage(string taskId, string rewardId)
        {
            TaskId = taskId;
            RewardId = rewardId;
        }
    }
}