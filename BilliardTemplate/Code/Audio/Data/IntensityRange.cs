using System;
using UnityEngine;

/// <summary>
/// Defines an intensity range that maps to a set of audio clips.
/// Multiple clips can share the same intensity range for variation.
/// </summary>
[Serializable]
public struct IntensityRange
{
    [Tooltip("Minimum intensity value (0-1)")]
    [Range(0f, 1f)]
    public float Min;
    
    [Tooltip("Maximum intensity value (0-1)")]
    [Range(0f, 1f)]
    public float Max;
    
    [Tooltip("Indices of clips (from the layer's Clips list) that belong to this intensity range")]
    public int[] ClipIndices;

    public bool IsValid => Min >= 0f && Max <= 1f && Min <= Max && ClipIndices != null && ClipIndices.Length > 0;
    
    /// <summary>
    /// Returns true if this range contains the given intensity
    /// </summary>
    public bool Contains(float intensity)
    {
        intensity = Mathf.Clamp01(intensity);
        return intensity >= Min && intensity <= Max;
    }
    
    /// <summary>
    /// Returns how well this range matches the given intensity (0-1).
    /// Returns 1.0 for perfect match, decreasing value for nearby ranges.
    /// </summary>
    public float GetMatch(float intensity)
    {
        intensity = Mathf.Clamp01(intensity);
        
        if (Contains(intensity))
            return 1.0f;
        
        // Calculate distance to nearest edge
        if (intensity < Min)
        {
            float distance = Min - intensity;
            return Mathf.Max(0f, 1f - distance * 2f);
        }
        else
        {
            float distance = intensity - Max;
            return Mathf.Max(0f, 1f - distance * 2f);
        }
    }
}