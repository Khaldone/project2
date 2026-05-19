// Inside Assets/Scripts/CoreDomain/Player/TurnController.cs
using UnityEngine; // Only for Vector3/Mathf structs, not MonoBehaviours


public class TurnController
{
    private readonly IPlayerInput _input;
    private readonly StrikeCalculator _strikeCalculator;
    private readonly IPhysicsBody _cueBall;
    public Vector3 CurrentAimDirection { get; private set; }
    private float _currentAimAngle = 0f;


    // Dependency Injection
    public TurnController(IPlayerInput input, StrikeCalculator strikeCalculator, IPhysicsBody cueBall)
    {
        _input = input;
        _strikeCalculator = strikeCalculator;
        _cueBall = cueBall;
    }



    // Called by a central game loop (or a lightweight MonoBehaviour Update)
    public void Tick(float deltaTime, float ballMass)
    {
        // 1. Handle Aiming (Drag to Aim)
        float aimDelta = _input.GetAimDelta();
        if (Mathf.Abs(aimDelta) > 0.01f)
        {
            // Adjust sensitivity as needed
            _currentAimAngle += aimDelta * deltaTime * 50f;
            UpdateAimDirection();
        }


        // 2. Handle Shooting (Pull back and release)
        if (_input.IsStrikeReleased())
        {
            float power = _input.GetStrikePower();
            if (power > 0.05f) // Deadzone to prevent accidental taps
            {
                ExecuteStrike(1f);
            }
        }
    }


    private void UpdateAimDirection()
    {
        // Convert the angle into a forward vector
        Quaternion rotation = Quaternion.Euler(0, _currentAimAngle, 0);
        CurrentAimDirection = rotation * Vector3.forward;
    }

    private void ExecuteStrike(float normalizedPower)
    {
        float actualForce = normalizedPower * 20f;

        // 1. Calculate the perfect math
        float finalSpeed = _strikeCalculator.CalculateFinalVelocity(actualForce, _cueBall.GetMass());

        // 2. Determine the vector
        Vector3 finalVelocity = CurrentAimDirection * finalSpeed;


        // 3. Command the physics body to move
        _cueBall.ApplyVelocity(finalVelocity);
    }
}
