public class PowerBoostModifier : IStrikeModifier
{
    private readonly float _powerMultiplier;
    public PowerBoostModifier(float multiplier) { _powerMultiplier = multiplier; }

    public enStrikeCommand ApplyModifier(enStrikeCommand cmd)
    {
        cmd.Power = UnityEngine.Mathf.Clamp01(cmd.Power * _powerMultiplier);
        return cmd;
    }
}