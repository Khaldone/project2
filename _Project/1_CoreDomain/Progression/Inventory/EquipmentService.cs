// Assets/_Project/CoreDomain/Progression/Inventory/EquipmentService.cs
using System;
using System.Threading.Tasks;

public class EquipmentService : IEquipmentService
{
    private readonly ILocalSaveService _saveService;
    private readonly IInventoryOrchestrator _inventory;

    public string EquippedCueId { get; private set; }
    public event Action<string> OnCueEquipped;

    public EquipmentService(ILocalSaveService saveService, IInventoryOrchestrator inventory)
    {
        _saveService = saveService;
        _inventory = inventory;
    }

    // Called on boot to load their last choice
    public async Task InitializeAsync()
    {
        EquippedCueId = await _saveService.LoadDataAsync<string>("equipped_cue", "cue_basic_wood");
    }

    public void EquipCue(string cueId)
    {
        // Security Check: Don't let hackers send an API call to equip a cue they don't own!
        if (!_inventory.OwnsItem(cueId)) return;


        EquippedCueId = cueId;

        // Save the choice securely to the phone's local disk
        _saveService.SaveDataAsync("equipped_cue", EquippedCueId);

        // Notify the UI or the 3D Avatar to update visually
        OnCueEquipped?.Invoke(EquippedCueId);
    }
}