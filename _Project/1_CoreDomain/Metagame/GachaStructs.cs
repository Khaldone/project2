// Assets/_Project/1_CoreDomain/Metagame/GachaStructs.cs
using System;


// The tangible impact a Cue has on the physics and rules engine
public struct CueStats
{
    public float ForceMultiplier; // Increases max velocity of the physics strike
    public float AimLineLength;   // Passed into the AimPredictor we wrote earlier
    public float SpinEfficiency;  // Multiplies the English/Draw effect on the cue ball
    public float TimeBonusSeconds; // Added to the Turn Timer in MatchCoordinator
}


// Represents a box sitting in one of the 4 lobby slots
public class LootboxData
{
    public string InstanceId;
    public string CatalogId; // e.g., "Box_Gold", "Box_Pro"
    public bool IsUnlocking;
    public DateTime? UnlockCompletesAt; // Sourced from the SERVER's clock, not the phone's clock
}


public struct CuePieceInventory
{
    public string CueId;
    public int CurrentPieces;
    public int RequiredPieces;
}
