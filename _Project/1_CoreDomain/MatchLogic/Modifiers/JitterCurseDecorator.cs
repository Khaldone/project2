// Assets/_Project/CoreDomain/MatchLogic/Modifiers/JitterCurseDecorator.cs
public class JitterCurseDecorator : IStrikeProvider
{
    private readonly IStrikeProvider _wrappedProvider;


    public JitterCurseDecorator(IStrikeProvider wrappedProvider)
    {
        _wrappedProvider = wrappedProvider;
    }


    public enStrikeCommand GetStrike()
    {
        enStrikeCommand command = _wrappedProvider.GetStrike();

        // Add a random slight inaccuracy to the angle (e.g., if they are playing a "Hardcore" mode)
        command.Angle += UnityEngine.Random.Range(-2f, 2f);

        return command;
    }
}