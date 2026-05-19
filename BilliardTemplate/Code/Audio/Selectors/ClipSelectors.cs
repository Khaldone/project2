using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Selects the first matching clip based on intensity range.
/// Simple and deterministic.
/// </summary>
public class FirstMatchSelector : IClipSelector
{
    public AudioClipData? SelectClip(SFXLayer layer, float intensity)
    {
        var matchingClips = layer.GetClipsForIntensity(intensity);
        return matchingClips.Count > 0 ? matchingClips[0] : (AudioClipData?)null;
    }

    public void OnClipPlayed(AudioClipData clip, float intensity) { }
    public void Reset() { }
}

/// <summary>
/// Randomly selects from all clips that match the intensity range.
/// Provides variation when multiple clips are suitable.
/// </summary>
public class RandomMatchSelector : IClipSelector
{
    public AudioClipData? SelectClip(SFXLayer layer, float intensity)
    {
        var matchingClips = layer.GetClipsForIntensity(intensity);
        
        if (matchingClips.Count == 0)
            return null;

        int randomIndex = Random.Range(0, matchingClips.Count);
        return matchingClips[randomIndex];
    }

    public void OnClipPlayed(AudioClipData clip, float intensity) { }
    public void Reset() { }
}

/// <summary>
/// Weighted random selection based on how well each range matches the intensity.
/// Ranges that match better have higher chance of being selected.
/// </summary>
public class WeightedRandomSelector : IClipSelector
{
    private readonly List<AudioClipData> _allClips = new List<AudioClipData>(16);
    private readonly List<float> _weights = new List<float>(16);
    
    public AudioClipData? SelectClip(SFXLayer layer, float intensity)
    {
        _allClips.Clear();
        _weights.Clear();
        
        float totalWeight = 0f;
        
        // Build weighted list from all ranges
        for (int i = 0; i < layer.IntensityRanges.Count; i++)
        {
            var range = layer.IntensityRanges[i];
            float match = range.GetMatch(intensity);
            
            if (match > 0f)
            {
                for (int j = 0; j < range.ClipIndices.Length; j++)
                {
                    int clipIndex = range.ClipIndices[j];
                    if (clipIndex >= 0 && clipIndex < layer.Clips.Count)
                    {
                        _allClips.Add(layer.Clips[clipIndex]);
                        _weights.Add(match);
                        totalWeight += match;
                    }
                }
            }
        }

        if (_allClips.Count == 0)
            return null;

        // Weighted random selection
        float randomValue = Random.Range(0f, totalWeight);
        float cumulative = 0f;
        
        for (int i = 0; i < _allClips.Count; i++)
        {
            cumulative += _weights[i];
            if (randomValue <= cumulative)
            {
                return _allClips[i];
            }
        }

        return _allClips[_allClips.Count - 1];
    }

    public void OnClipPlayed(AudioClipData clip, float intensity) { }
    public void Reset() { }
}

/// <summary>
/// Prevents the same clip from playing consecutively.
/// Uses random selection but excludes the last played clip.
/// </summary>
public class NoRepeatRandomSelector : IClipSelector
{
    private string _lastPlayedClipName;
    
    public AudioClipData? SelectClip(SFXLayer layer, float intensity)
    {
        var matchingClips = layer.GetClipsForIntensity(intensity);
        
        if (matchingClips.Count == 0)
            return null;

        // Remove last played clip if there are alternatives
        if (matchingClips.Count > 1)
        {
            for (int i = matchingClips.Count - 1; i >= 0; i--)
            {
                if (matchingClips[i].Name == _lastPlayedClipName)
                {
                    matchingClips.RemoveAt(i);
                    break;
                }
            }
        }

        if (matchingClips.Count == 0)
            return null;

        int randomIndex = Random.Range(0, matchingClips.Count);
        return matchingClips[randomIndex];
    }

    public void OnClipPlayed(AudioClipData clip, float intensity)
    {
        _lastPlayedClipName = clip.Name;
    }

    public void Reset()
    {
        _lastPlayedClipName = null;
    }
}

/// <summary>
/// Round-robin selection through matching clips.
/// Ensures even distribution of clip usage.
/// </summary>
public class RoundRobinSelector : IClipSelector
{
    private readonly Dictionary<float, int> _intensityIndexMap = new Dictionary<float, int>();
    private const float INTENSITY_BUCKET_SIZE = 0.1f;
    
    public AudioClipData? SelectClip(SFXLayer layer, float intensity)
    {
        var matchingClips = layer.GetClipsForIntensity(intensity);
        
        if (matchingClips.Count == 0)
            return null;

        // Get bucket for this intensity range
        float bucket = Mathf.Floor(intensity / INTENSITY_BUCKET_SIZE) * INTENSITY_BUCKET_SIZE;
        
        if (!_intensityIndexMap.TryGetValue(bucket, out int currentIndex))
        {
            currentIndex = 0;
        }

        currentIndex = currentIndex % matchingClips.Count;
        AudioClipData selectedClip = matchingClips[currentIndex];
        
        // Advance to next clip for this bucket
        _intensityIndexMap[bucket] = (currentIndex + 1) % matchingClips.Count;
        
        return selectedClip;
    }

    public void OnClipPlayed(AudioClipData clip, float intensity) { }
    
    public void Reset()
    {
        _intensityIndexMap.Clear();
    }
}

/// <summary>
/// Selects clips based on a shuffled sequence.
/// Plays through all matching clips before repeating.
/// </summary>
public class ShuffleSelector : IClipSelector
{
    private readonly List<AudioClipData> _shuffledSequence = new List<AudioClipData>(16);
    private readonly Dictionary<float, int> _intensityIndexMap = new Dictionary<float, int>();
    private readonly Dictionary<float, int> _intensityHashMap = new Dictionary<float, int>();
    private const float INTENSITY_BUCKET_SIZE = 0.1f;
    
    public AudioClipData? SelectClip(SFXLayer layer, float intensity)
    {
        var matchingClips = layer.GetClipsForIntensity(intensity);
        
        if (matchingClips.Count == 0)
            return null;

        float bucket = Mathf.Floor(intensity / INTENSITY_BUCKET_SIZE) * INTENSITY_BUCKET_SIZE;
        
        // Calculate hash of current matching clips to detect changes
        int currentHash = GetClipsHash(matchingClips);
        
        bool needsReshuffle = false;
        if (!_intensityHashMap.TryGetValue(bucket, out int storedHash) || storedHash != currentHash)
        {
            needsReshuffle = true;
            _intensityHashMap[bucket] = currentHash;
        }
        
        if (!_intensityIndexMap.TryGetValue(bucket, out int currentIndex) || needsReshuffle)
        {
            _shuffledSequence.Clear();
            _shuffledSequence.AddRange(matchingClips);
            ShuffleList(_shuffledSequence);
            currentIndex = 0;
        }

        if (currentIndex >= _shuffledSequence.Count)
        {
            // Reshuffle for next cycle
            ShuffleList(_shuffledSequence);
            currentIndex = 0;
        }

        AudioClipData selectedClip = _shuffledSequence[currentIndex];
        
        currentIndex++;
        _intensityIndexMap[bucket] = currentIndex;
        
        return selectedClip;
    }

    private int GetClipsHash(List<AudioClipData> clips)
    {
        int hash = clips.Count;
        for (int i = 0; i < clips.Count; i++)
        {
            hash = hash * 31 + clips[i].Name.GetHashCode();
        }
        return hash;
    }

    private void ShuffleList(List<AudioClipData> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            var temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }

    public void OnClipPlayed(AudioClipData clip, float intensity) { }
    
    public void Reset()
    {
        _intensityIndexMap.Clear();
        _intensityHashMap.Clear();
        _shuffledSequence.Clear();
    }
}