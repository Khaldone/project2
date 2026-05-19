// Assets/_Project/CoreDomain/Navigation/IDeepLinkOrchestrator.cs
public interface IDeepLinkOrchestrator
{
    // Saves the action (e.g., "open_shop", "claim_daily")
    void SetPendingDeepLink(string actionId);

    // Checks if we have a link, returns it, and immediately clears it so it doesn't fire twice
    bool TryConsumePendingLink(out string actionId);
}