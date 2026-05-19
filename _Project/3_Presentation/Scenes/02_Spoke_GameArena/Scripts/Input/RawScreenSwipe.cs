// Assets/_Project/Scenes/02_Spoke_GameArena/Scripts/Input/RawScreenSwipe.cs

public class RawScreenSwipe : IStrikeProvider
{
    private float _rawAngle;
    private float _rawPower; // 0.0 to 1.0

    // (Assume these are updated by Unity's EventSystems)


    public enStrikeCommand GetStrike()
    {
        return new enStrikeCommand { Angle = _rawAngle, Power = _rawPower };
    }
}