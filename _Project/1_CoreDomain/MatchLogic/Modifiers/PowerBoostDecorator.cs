// Assets/_Project/CoreDomain/MatchLogic/Modifiers/PowerBoostDecorator.cs
public class PowerBoostDecorator : IStrikeProvider
{
    private readonly IStrikeProvider _wrappedProvider;
    private readonly float _powerMultiplier;


    public PowerBoostDecorator(IStrikeProvider wrappedProvider, float multiplier)
    {
        _wrappedProvider = wrappedProvider;
        _powerMultiplier = multiplier;
    }


    public enStrikeCommand GetStrike()
    {
        // 1. Get the strike from whatever is underneath this wrapper
        enStrikeCommand command = _wrappedProvider.GetStrike();


        // 2. Modify it
        command.Power = UnityEngine.Mathf.Clamp01(command.Power * _powerMultiplier);


        // 3. Pass it up the chain
        return command;
    }
}