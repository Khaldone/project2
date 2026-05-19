// Assets/Scripts/Presentation/Network/FusionMatchBroadcaster.cs
using Fusion;
public class FusionMatchBroadcaster : NetworkBehaviour, IMatchBroadcaster
{
    // We use a networked variable for the active player so late-joiners instantly know whose turn it is
    [Networked, OnChangedRender(nameof(OnActivePlayerChanged))]
    public int NetworkedActivePlayerId { get; set; }


    public void BroadcastActivePlayer(int playerId)
    {
        // Only the State Authority (Host/Server) is allowed to change this
        if (HasStateAuthority)
        {
            NetworkedActivePlayerId = playerId;
        }
    }


    // We use an RPC for events because they are fleeting moments (like triggering a sound)
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void Rpc_BroadcastEvent(MatchEventType eventType, int playerId)
    {
        // Route this to your UI Controller or Audio Manager
        //MatchUIController.Instance.DisplayEvent(eventType, playerId);
    }


    public void BroadcastMatchEvent(MatchEventType eventType, int offendingPlayerId)
    {
        if (HasStateAuthority)
        {
            Rpc_BroadcastEvent(eventType, offendingPlayerId);
        }
    }


    // Called automatically on all clients when the NetworkedActivePlayerId changes
    private void OnActivePlayerChanged()
    {
        //MatchUIController.Instance.UpdateTurnIndicator(NetworkedActivePlayerId);
    }
}
