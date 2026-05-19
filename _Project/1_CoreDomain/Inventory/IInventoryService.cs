// Assets/_Project/1_CoreDomain/Inventory/IInventoryService.cs
using System.Collections.Generic;
using System.Threading.Tasks;


// 1. Pure C# Data Structs
public struct CueItem
{
    public string InstanceId;   // The unique ID of this specific item in the database
    public string CatalogId;    // The generic ID (e.g., "cue_dragon_01")
    public string DisplayName;
    public int PowerStat;
    public int SpinStat;
    public bool IsEquipped;
}


// 2. The Interface Contract
public interface IInventoryService
{
    Task<List<CueItem>> GetPlayerCuesAsync();
    Task<bool> EquipCueAsync(string instanceId);
}