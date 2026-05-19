// 2. Assets/Scripts/CoreDomain/Progression/MatchRewardCalculator.cs
// A pure C# utility for displaying projected rewards to the UI before the server confirms them.
public static class MatchRewardCalculator
{
    public static int CalculateProjectedXP(bool isWinner, int shotsTaken)
    {
        int baseXP = isWinner ? 100 : 25;
        int efficiencyBonus = (isWinner && shotsTaken < 10) ? 50 : 0;
        return baseXP + efficiencyBonus;
    }
}
