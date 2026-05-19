// Assets/_Project/CoreDomain/MatchLogic/Modifiers/IStrikeModifier.cs
public interface IStrikeModifier
{
    // Decorator pattern
    enStrikeCommand ApplyModifier(enStrikeCommand originalCommand);
}
