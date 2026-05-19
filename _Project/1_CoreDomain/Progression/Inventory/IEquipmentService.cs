// Assets/_Project/CoreDomain/Progression/Inventory/IEquipmentService.cs
using System;
public interface IEquipmentService
{
    string EquippedCueId { get; }
    event Action<string> OnCueEquipped;

    void EquipCue(string cueId);
}