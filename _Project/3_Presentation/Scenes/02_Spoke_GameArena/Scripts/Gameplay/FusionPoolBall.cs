// Assets/_Project/Scenes/02_Spoke_GameArena/Scripts/Gameplay/FusionPoolBall.cs
using Fusion;
using UnityEngine;
using VContainer;


public class FusionPoolBall : NetworkBehaviour
{
    [Networked] public Vector3 NetworkPosition { get; set; }
    [Networked] public Vector3 NetworkVelocity { get; set; }


    private BilliardsPhysicsEngine _physicsEngine;
    private float _ballRadius = 0.0285f; // Standard 2.25 inch pool ball
    [SerializeField] private LayerMask _railLayer;

    [Inject]
    public void Construct(BilliardsPhysicsEngine physicsEngine)
    {
        _physicsEngine = physicsEngine;
    }

    public override void FixedUpdateNetwork()
    {
        // Only the Server/Host calculates physics to prevent desyncs
        if (!HasStateAuthority) return;


        // 1. Map networked state to our pure struct
        BallState currentState = new BallState
        {
            Position = NetworkPosition,
            Velocity = NetworkVelocity,
            Radius = _ballRadius
        };


        // 2. Execute the Continuous Collision step
        _physicsEngine.SimulateBallStep(ref currentState, Runner.DeltaTime, _railLayer);


        // 3. Write the results back to the network state
        NetworkPosition = currentState.Position;
        NetworkVelocity = currentState.Velocity;
    }


    // 2. THE VISUAL LOOP (Runs every frame the screen draws)
    public override void Render()
    {
        // Smoothly interpolate the visual 3D sphere to match the network math
        // This ensures the game looks like buttery 60fps even if the network tick rate is 30hz
        transform.position = Vector3.Lerp(transform.position, NetworkPosition, Time.deltaTime * 15f);

        // Note: The physical Unity Transform is NEVER used for collision math. It is just a visual ghost.

    }
}