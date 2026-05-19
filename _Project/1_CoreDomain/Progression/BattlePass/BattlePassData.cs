// Assets/_Project/CoreDomain/Progression/BattlePass/BattlePassData.cs
public struct BattlePassState
{
    public int CurrentLevel;
    public int CurrentExp;
    public int ExpToNextLevel;
    public bool IsPremiumUnlocked;
}

public struct BattlePassReward
{
    public string ItemId; // e.g., "cue_neon_strike", "coins_500"
    public int RequiredLevel;
    public bool IsPremium;
    public bool IsClaimed;
}