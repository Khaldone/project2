// Assets/_Project/CoreDomain/Player/AchievementData.cs
namespace Billiards.CoreDomain.Player
{
    public struct AchievementData
    {
        public string Id;               // e.g., "win_10_matches"
        public string Title;            // e.g., "Hustler In Training"
        public string Description;      // e.g., "Win 10 matches in the Game Arena."
        public int CurrentProgress;     // e.g., 7
        public int MaxProgress;         // e.g., 10
        public bool IsClaimed;          // True if the player already collected the reward

        // Helper property for the UI progress bar (returns 0.0 to 1.0)
        public float ProgressNormalized => MaxProgress > 0 ? (float)CurrentProgress / MaxProgress : 0f;
    }
}