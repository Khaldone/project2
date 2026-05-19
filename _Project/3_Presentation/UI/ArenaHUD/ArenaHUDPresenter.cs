//// 3_Presentation/UI_Features/ArenaHUD/ArenaHUDPresenter.cs
//using PlayFab.EconomyModels;
//using System;
//using VContainer.Unity;


//public class ArenaHUDPresenter : IStartable, IDisposable
//{
//    //private readonly IArenaHUDView _view;
//    private readonly IMessageBroker _messageBroker;
//    private IDisposable _pocketSubscription;

//    //public ArenaHUDPresenter(IArenaHUDView view, IMessageBroker messageBroker)
//    //{
//    //    _view = view;
//    //    _messageBroker = messageBroker;
//    //}

//    public ArenaHUDPresenter(IMessageBroker messageBroker)
//    {
//        _messageBroker = messageBroker;
//    }


//    public void Start()
//    {
//        //_pocketSubscription = _messageBroker.Subscribe<BallPocketedMessage>(OnBallPocketed);
//    }


//    //private void OnBallPocketed(BallPocketedMessage msg)
//    //{
//    //    if (msg.WasFoul)
//    //    {
//    //        _view.ShowFoulBanner();
//    //    }
//    //    else
//    //    {
//    //        // Flash a little UI icon showing the ball was sunk
//    //        _view.UpdateSunkBallsList(msg.BallId);
//    //    }
//    //}


//    public void Dispose()
//    {
//        // CRITICAL: If the player leaves the match, this stops the UI from
//        // trying to update a Canvas that no longer exists!
//        _pocketSubscription?.Dispose();
//    }
//}
