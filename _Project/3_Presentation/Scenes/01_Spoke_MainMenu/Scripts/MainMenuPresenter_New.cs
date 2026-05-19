using PlayFab.EconomyModels;
using System;

public class MainMenuPresenter_New : IDisposable
{
    //private readonly IMainMenuView _view;
    private readonly IMessageBroker_New _broker;
    private readonly IDisposable _currencySubscription; // Holds the token!


    //public MainMenuPresenter_New(IMainMenuView view, IMessageBroker_New broker)
    //{
    //    _view = view;
    //    _broker = broker;


    //    // 1. Subscribe and save the token
    //    _currencySubscription = _broker.Subscribe<CurrencyUpdatedMessage>(OnCurrencyUpdated);
    //}

    public MainMenuPresenter_New(IMessageBroker_New broker)
    {
        _broker = broker;


        // 1. Subscribe and save the token
        _currencySubscription = _broker.Subscribe<CurrencyUpdatedMessage>(OnCurrencyUpdated);
    }


    // 2. The reaction
    private void OnCurrencyUpdated(CurrencyUpdatedMessage msg)
    {
        //_view.UpdateCoins(msg.NewCoinBalance);
    }


    // 3. THE CLEANUP (Called by VContainer when the scene unloads)
    public void Dispose()
    {
        _currencySubscription?.Dispose(); // Prevents the memory leak!
    }
}