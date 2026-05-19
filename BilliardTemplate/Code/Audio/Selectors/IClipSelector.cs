using System.Collections.Generic;

/// <summary>
/// Interface for clip selection strategies.
/// Allows different algorithms for choosing which clip to play.
/// </summary>
public interface IClipSelector
{
    /// <summary>
    /// Selects a clip from the available options based on intensity.
    /// </summary>
    /// <param name="layer">The SFX layer containing clips and intensity ranges</param>
    /// <param name="intensity">Normalized intensity value (0-1)</param>
    /// <returns>Selected clip, or null if no suitable clip found</returns>
    AudioClipData? SelectClip(SFXLayer layer, float intensity);
    
    /// <summary>
    /// Called when a clip is played. Useful for strategies that need to track history.
    /// </summary>
    void OnClipPlayed(AudioClipData clip, float intensity);
    
    /// <summary>
    /// Resets any internal state. Called when layer is stopped or reset.
    /// </summary>
    void Reset();
}