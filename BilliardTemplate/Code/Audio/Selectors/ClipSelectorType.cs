using System;

/// <summary>
/// Defines available clip selection strategies
/// </summary>
[Serializable]
public enum ClipSelectorType
{
    /// <summary>First matching clip (deterministic)</summary>
    FirstMatch,
    
    /// <summary>Random from all matching clips</summary>
    Random,
    
    /// <summary>Weighted random based on intensity match quality</summary>
    WeightedRandom,
    
    /// <summary>Random but prevents consecutive repeats</summary>
    NoRepeatRandom,
    
    /// <summary>Cycles through clips in order</summary>
    RoundRobin,
    
    /// <summary>Shuffled sequence, plays all before repeating</summary>
    Shuffle
}