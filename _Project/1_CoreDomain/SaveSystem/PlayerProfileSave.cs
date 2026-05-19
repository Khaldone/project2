// 1. Assets/Scripts/CoreDomain/SaveSystem/PlayerProfile.cs
using System.Collections.Generic;

// A simple, pure data container. Easily serializable to JSON.
public class PlayerProfileSave
{
    public int Coins { get; set; } = 0;
    public string EquippedCueId { get; set; } = "cue_default";
    public List<string> UnlockedCues { get; set; } = new List<string> { "cue_default" };
}