// Assets/_Project/1_CoreDomain/MatchLogic/MatchCoordinator.Hydration.cs


public partial class MatchCoordinator
{
    // The UI handles the visual loading screen, we just handle the math
    //public void OnHydrateStateReceived(HydrateStateCommand cmd)
    //{
    //    MatchSnapshot snapshot = cmd.Snapshot;


    //    // 1. Overwrite Physics State
    //    for (int i = 0; i < 16; i++)
    //    {
    //        //_physicsEngine.SetBallState(i, snapshot.BallStates[i]);
    //    }


    //    // 2. Overwrite Match State
    //    _state.ActivePlayerId = snapshot.ActivePlayerId;
    //    _state.IsOpenTable = snapshot.IsOpenTable;
    //    _state.IsBallInHand = snapshot.IsBallInHand;
    //    _state.PlayerSuits = snapshot.PlayerSuits;
    //    _state.PlayerBallsRemaining = snapshot.PlayerBallsRemaining;


    //    // 3. Overwrite Timers
    //    _currentTurnTime = snapshot.TurnTimeRemaining;


    //    // 4. Shout to the Presentation Layer that the world is rebuilt!
    //    MessageBroker.Instance.Publish(new StateSuccessfullyHydratedMessage());

    //    // 5. Re-evaluate the perspective so the UI knows whose turn it is
    //    ForcePerspectiveUpdate();
    //}
}
