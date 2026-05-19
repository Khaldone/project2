// Assets/_Project/2_Infrastructure/Network/FusionMatchRunner.cs
using Fusion;
using UnityEngine;


public class FusionMatchRunner_New : NetworkBehaviour, IPlayerLeft, IPlayerJoined
{
    [Networked] public bool IsMatchPausedForDisconnect { get; set; }

    private const float FORFEIT_TIME_SECONDS = 30.0f;
    private float _disconnectTimer;
    private PlayerRef _disconnectedPlayerRef;


    // Called automatically by Photon when a player loses connection
    public void PlayerLeft(PlayerRef player)
    {
        if (Object.HasStateAuthority) // Only the Server/Host executes this
        {
            IsMatchPausedForDisconnect = true;
            _disconnectedPlayerRef = player;
            _disconnectTimer = FORFEIT_TIME_SECONDS;

            // Tell the active UI to show the "Waiting for Opponent..." overlay
            //MessageBroker.Instance.Publish(new OpponentDisconnectedMessage());
        }
    }


    public override void FixedUpdateNetwork()
    {
        if (IsMatchPausedForDisconnect && Object.HasStateAuthority)
        {
            _disconnectTimer -= Runner.DeltaTime;

            if (_disconnectTimer <= 0)
            {
                // Time's up. The connected player wins by forfeit.
                //ForceForfeitWin();
            }
            return; // Halt the physics engine completely while paused
        }


        // Normal physics execution happens here...
    }


    // Called automatically when the player manages to reconnect to the session
    public void PlayerJoined(PlayerRef player)
    {
        if (Object.HasStateAuthority && IsMatchPausedForDisconnect && player == _disconnectedPlayerRef)
        {
            // They made it back!
            IsMatchPausedForDisconnect = false;

            // 1. Generate the Snapshot from the Server's Core Domain
            //MatchSnapshot currentSnapshot = _matchCoordinator.GenerateCurrentSnapshot();

            // 2. Send the Snapshot explicitly to the player who just joined
            //RPC_SendHydrationData(player, currentSnapshot);
        }
    }


    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_SendHydrationData([RpcTarget] PlayerRef targetPlayer, MatchSnapshot snapshot)
    {
        // This method only executes on the specific client that reconnected.
        // We pass the data into our Core Domain to rebuild the local world.
        //MessageBroker.Instance.Publish(new HydrateStateCommand(snapshot));
    }
}
