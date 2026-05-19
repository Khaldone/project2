public class PlayFabStoreWrapper
{
    private readonly IMessageBroker_New _broker;


    // ... code that talks to Apple/PlayFab ...


    private void OnPurchaseSuccessful(int newCoinTotal, int newGemTotal)
    {
        // You just tell the broker what happened.
        // You don't care if the Main Menu is open, or the Settings menu, or the Arena.
        _broker.Publish(new CurrencyUpdatedMessage(newCoinTotal, newGemTotal));
    }
}