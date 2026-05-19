//// Assets/_Project/1_CoreDomain/MatchLogic/MatchCoordinator.Lifecycle.cs


//public partial class MatchCoordinator
//{
//    public void OnAppResumed(AppResumedMessage msg)
//    {
//        // We know we are out of sync. We must ask the Server/Host for the absolute truth.
//        // This utilizes the exact same Hydration struct we built in the previous step.
//        _broker.Publish(new RequestHydrationDataCommand());
//    }


//    // When the server responds with the MatchSnapshot...
//    public void OnHydrateStateReceived(HydrateStateCommand cmd)
//    {
//        MatchSnapshot snapshot = cmd.Snapshot;


//        // Force our local physics to snap to the server's exact current coordinates
//        for (int i = 0; i < 16; i++)
//        {
//            _physicsEngine.SetBallState(i, snapshot.BallStates[i]);
//        }


//        // CRITICAL: Overwrite our local timer with the Server's timer.
//        // If it was our turn, and we had 15 seconds left before we backgrounded,
//        // the snapshot will tell us we now only have 11 seconds left.
//        _currentTurnTime = snapshot.TurnTimeRemaining;


//        // Tell the UI to drop the curtain!
//        _broker.Publish(new StateSuccessfullyHydratedMessage());
//    }
//}
