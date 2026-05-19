// Assets/_Project/Scenes/02_Spoke_GameArena/Scripts/FusionPoolBall.cs
using Fusion;
using UnityEngine;
using VContainer;
public class FusionPoolBall_Old : NetworkBehaviour
{
    // The DI-injected pure C# physics brain
    private BallPhysicsSimulator _physicsSimulator;
    private StrikeValidator _strikeValidator;
    private ICueInputListener _cueInputListener;

    // --- FUSION NETWORKED STATE ---
    // Fusion automatically syncs these across the internet when changed by the State Authority
    [Networked] public Vector3 NetworkPosition { get; set; }
    [Networked] public Vector3 NetworkVelocity { get; set; }
    [Networked] public NetworkBool NetworkIsResting { get; set; }


    // The visual 3D sphere you actually see on screen
    [SerializeField] private Transform _visualTransform;


    [Inject]
    public void Construct(BallPhysicsSimulator physicsSimulator, StrikeValidator strikeValidator, ICueInputListener inputListener)
    {
        _physicsSimulator = physicsSimulator;
        _strikeValidator = strikeValidator;
        _cueInputListener = inputListener;

        inputListener.OnStrikeExecuted += (command) =>
        {
            // The PlayerController hears the command and feeds it into the Network Funnel!
            ApplyValidatedStrike(new StrikeIntent { Angle = command.Angle, Power = command.Power });
        };

    }

    // This runs on every client, in lockstep, 60 times a second
    public override void FixedUpdateNetwork()
    {
        // ANTI-CHEAT FIREWALL: Only the authoritative Host is allowed to calculate physics!
        if (HasStateAuthority)
        {
            // 1. Read the current networked state into our pure C# struct
            BallState currentState = new BallState
            {
                Position = NetworkPosition,
                Velocity = NetworkVelocity,
                IsResting = NetworkIsResting
            };


            // 2. Ask the CoreDomain to do the complex math
            // Runner.DeltaTime ensures the physics tick rate perfectly matches the network tick rate
            BallState nextState = _physicsSimulator.CalculateNextFrame(currentState, Runner.DeltaTime);


            // 3. Write the results back to the Fusion properties
            NetworkPosition = nextState.Position;
            NetworkVelocity = nextState.Velocity;
            NetworkIsResting = nextState.IsResting;
        }

        // 1. READ THE INPUT (This only returns true for the player who owns the ball)
        if (GetInput(out NetworkStrikeInput networkInput))
        {
            // 2. ONLY THE HOST ACTUALLY PROCESSES IT
            if (HasStateAuthority && networkInput.IsStriking)
            {
                // Translate Fusion struct to our Pure C# struct
                var rawIntent = new StrikeIntent
                {
                    Angle = networkInput.Angle,
                    Power = networkInput.Power
                };


                // PASS THROUGH THE FIREWALL
                StrikeIntent safeIntent = _strikeValidator.SanitizeInput(rawIntent);


                // Convert the safe angle and power into a 3D velocity vector
                Vector3 strikeForce = CalculateVelocityFromIntent(safeIntent);


                // Apply to the networked state (Fusion syncs this to everyone instantly)
                NetworkVelocity += strikeForce;
            }

            if (HasStateAuthority && networkInput.IsStriking)
            {
                var humanIntent = new StrikeIntent { Angle = networkInput.Angle, Power = networkInput.Power };

                // Route to the Universal Funnel
                ApplyValidatedStrike(humanIntent);
            }
        }


        // 3. APPLY PHYSICS (Host calculates the movement over time)
        if (HasStateAuthority)
        {
            var currentState = new BallState { Position = NetworkPosition, Velocity = NetworkVelocity };
            var nextState = _physicsSimulator.CalculateNextFrame(currentState, Runner.DeltaTime);

            NetworkPosition = nextState.Position;
            NetworkVelocity = nextState.Velocity;
        }


        // ... Apply physics simulator to move the ball ...

    }


    // This runs every graphical frame (e.g., 144hz on a gaming phone)
    public override void Render()
    {
        // Smoothly move the visual 3D model to wherever the network says it should be
        // Fusion's interpolation handles the smoothing between network ticks
        _visualTransform.position = Vector3.Lerp(_visualTransform.position, NetworkPosition, Time.deltaTime * 15f);
    }

    private Vector3 CalculateVelocityFromIntent(StrikeIntent intent)
    {
        // Simple trigonometry to convert Angle to X/Z directions
        float rad = intent.Angle * Mathf.Deg2Rad;
        float maxSpeed = 20f; // Max velocity magnitude

        return new Vector3(Mathf.Cos(rad), 0, Mathf.Sin(rad)) * (intent.Power * maxSpeed);
    }

    // 2. THE UNIVERSAL FUNNEL
    // The BotController will call this directly!
    public void ApplyValidatedStrike(StrikeIntent rawIntent)
    {
        // Only the Host is allowed to accept strikes
        if (!HasStateAuthority) return;


        // Both human and bot inputs must pass through the CoreDomain firewall
        StrikeIntent safeIntent = _strikeValidator.SanitizeInput(rawIntent);


        // Convert intent to physical force and apply to NetworkVelocity
        float rad = safeIntent.Angle * Mathf.Deg2Rad;
        Vector3 force = new Vector3(Mathf.Cos(rad), 0, Mathf.Sin(rad)) * (safeIntent.Power * 20f);

        NetworkVelocity += force;
    }

    // Helper for the Bot to see where the ball is right now
    public BallState GetCurrentState() => new BallState { Position = NetworkPosition, Velocity = NetworkVelocity };

}
