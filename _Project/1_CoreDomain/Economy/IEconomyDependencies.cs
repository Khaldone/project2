// Assets/_Project/1_CoreDomain/Economy/IEconomyDependencies.cs
using System.Threading.Tasks;

public enum CurrencyType { Coins, Gems }

public struct PlayerWallet
{
    public int Coins;
    public int Gems;
    public bool HasPendingCloudSync; // The "Dirty Bit"
}


public interface ILocalWalletCache
{
    PlayerWallet GetWallet();
    void SaveWallet(PlayerWallet wallet);
}


public interface IEconomyCloudService
{
    // Talks to PlayFab to actually validate and store the new balances
    Task SyncWalletToServerAsync(PlayerWallet wallet);
}


// The message broadcasted to the UI
public struct CurrencyUpdatedMessage_New
{
    public int NewCoins;
    public int NewGems;
}