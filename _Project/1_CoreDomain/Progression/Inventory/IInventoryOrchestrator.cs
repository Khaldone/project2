// Assets/_Project/CoreDomain/Progression/Inventory/IInventoryOrchestrator.cs
using System.Collections.Generic;
using System.Threading.Tasks;


public interface IInventoryOrchestrator
{
    IReadOnlyList<string> OwnedCueIds { get; }
    Task RefreshInventoryAsync();
    bool OwnsItem(string itemId);
}