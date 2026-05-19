// Assets/_Project/CoreDomain/Navigation/DeepLinkOrchestrator.cs
public class DeepLinkOrchestrator : IDeepLinkOrchestrator
{
    private string _pendingActionId;

    public void SetPendingDeepLink(string actionId)
    {
        _pendingActionId = actionId;
    }

    public bool TryConsumePendingLink(out string actionId)
    {
        if (!string.IsNullOrEmpty(_pendingActionId))
        {
            actionId = _pendingActionId;
            _pendingActionId = null; // Clear it immediately!
            return true;
        }


        actionId = null;
        return false;
    }
}