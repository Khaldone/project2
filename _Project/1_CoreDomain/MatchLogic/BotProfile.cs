// Assets/_Project/1_CoreDomain/MatchLogic/BotProfile.cs

public enum AIDifficulty { Beginner, Intermediate, Advanced, Shark }

public struct BotProfile
{
    public string Id { get; }
    public string DisplayName { get; }
    public string AvatarAddressableKey { get; }
    public int SimulatedTrophies { get; }
    public AIDifficulty Difficulty { get; }

    public BotProfile(string id, string name, string avatarKey, int trophies, AIDifficulty difficulty)
    {
        Id = id;
        DisplayName = name;
        AvatarAddressableKey = avatarKey;
        SimulatedTrophies = trophies;
        Difficulty = difficulty;
    }
}