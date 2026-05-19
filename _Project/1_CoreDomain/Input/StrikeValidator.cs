// 2. Assets/_Project/CoreDomain/Input/StrikeValidator.cs
// The Firewall. This rejects malicious data before it touches the physics engine.
using UnityEngine;
public class StrikeValidator
{
    private const float MAX_ALLOWED_POWER = 1.0f;
    public StrikeIntent SanitizeInput(StrikeIntent rawInput)
    {
        return new StrikeIntent
        {
            // Lock the angle to a valid circle
            Angle = Mathf.Repeat(rawInput.Angle, 360f),

            // Clamp the power so hackers cannot launch the ball at lightspeed
            Power = Mathf.Clamp(rawInput.Power, 0f, MAX_ALLOWED_POWER)
        };
    }
}