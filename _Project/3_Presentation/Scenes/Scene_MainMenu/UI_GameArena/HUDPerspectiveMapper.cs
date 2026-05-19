// Assets/_Project/3_Presentation/UI_GameArena/HUDPerspectiveMapper.cs
using System;


// The translated messages that the UI actually listens to
public struct MappedTurnStateMessage
{
    public bool IsMyTurn;
}


public struct MappedHUDUpdateMessage
{
    public bool IsLocalHUD;
    public BallSuit AssignedSuit;
    public int BallsRemaining;
}


public class HUDPerspectiveMapper : IDisposable
{
    private readonly string _localPlayerId;


    public HUDPerspectiveMapper(string localPlayerId)
    {
        _localPlayerId = localPlayerId;


        // Subscribe to the Absolute Core Domain events
        //MessageBroker.Instance.Subscribe<TurnChangedMessage>(OnTurnChanged);
        //MessageBroker.Instance.Subscribe<PlayerStateUpdatedMessage>(OnPlayerStateUpdated);
    }


    private void OnTurnChanged(TurnChangedMessage msg)
    {
        // Translate the absolute ID into a relative boolean
        bool isMyTurn = msg.ActivePlayerId == _localPlayerId;


        // Shout the relative state to the UI components
        //MessageBroker.Instance.Publish(new MappedTurnStateMessage { IsMyTurn = isMyTurn });
    }


    private void OnPlayerStateUpdated(PlayerStateUpdatedMessage msg)
    {
        // Route the data to either the Left (Local) or Right (Remote) HUD element
        bool isLocal = msg.PlayerId == _localPlayerId;


        //MessageBroker.Instance.Publish(new MappedHUDUpdateMessage
        //{
        //    IsLocalHUD = isLocal,
        //    AssignedSuit = msg.AssignedSuit,
        //    BallsRemaining = msg.BallsRemaining
        //});
    }


    public void Dispose()
    {
        // Always clean up subscriptions!
        //MessageBroker.Instance.Unsubscribe<TurnChangedMessage>(OnTurnChanged);
        //MessageBroker.Instance.Unsubscribe<PlayerStateUpdatedMessage>(OnPlayerStateUpdated);
    }
}
