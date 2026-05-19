// Assets/_Project/2_Infrastructure/Network/FusionMatchRunner.cs
using Fusion;
using UnityEngine;
using VContainer;

public class FusionMatchRunner : NetworkBehaviour
{
    // The pure C# engine injected by VContainer. The remote team cannot see this code.
    private BilliardsPhysicsEngine _physicsEngine;

    // Fusion's networked state array. The Server's version of this array will ALWAYS overwrite the Client's version.
    //Networked, Capacity(16)]
    //private NetworkArray<NetworkBallState> _networkedBalls { get; }


    [Inject]
    public void Construct(BilliardsPhysicsEngine physicsEngine)
    {
        _physicsEngine = physicsEngine;
    }


    public override void FixedUpdateNetwork()
    {
        // 1. Fetch the input for this specific Fusion Tick
        if (GetInput(out StrikeInput input))
        {
            if (input.IsStriking)
            {
                // 2. Translate the Network Input into your pure C# CoreDomain command
                //var strikeCommand = new CoreDomain.StrikeCommand(
                //    input.AimDirection,
                //    input.PowerModifier,
                //    input.SpinEnglish
                //);


                // 3. Execute the proprietary physics simulation!
                // -> On the Client: This happens instantly on tick 100 (Client Prediction)
                // -> On the Server: This happens slightly later when the packet arrives (Server Authority)
                //var simulatedResult = _physicsEngine.SimulateShot(strikeCommand, ExtractCurrentState());


                // 4. Update the visual state
                //ApplyResultToNetworkState(simulatedResult);
            }
        }
    }

    public void OnShotReleased(ShotPowerResult result, Vector3 aimDir)
    {
        // RPC ensures the opponent sees the cue stick "snap" forward
        // and the balls react at the exact same time.
        RPC_ExecuteNetworkShot(result.FinalImpulse, aimDir);
    }


    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    public void RPC_ExecuteNetworkShot(float force, Vector3 direction)
    {
        // Both players now run the identical CoreDomain Physics Engine
        //_physicsEngine.ApplyImpulseToCueBall(direction * force);
    }


    //private void ApplyResultToNetworkState(CoreDomain.SimulationResult result)
    //{
    //    // We only allow the Server, or a Client predicting their own shot, to update the state.
    //    if (HasStateAuthority || HasInputAuthority)
    //    {
    //        for (int i = 0; i < result.FinalBallPositions.Length; i++)
    //        {
    //            // Update the Fusion NetworkArray
    //            var netBall = _networkedBalls[i];
    //            netBall.Position = result.FinalBallPositions[i];
    //            netBall.IsPocketed = result.PocketedStates[i];
    //            _networkedBalls.Set(i, netBall);
    //        }
    //    }
    //}
}