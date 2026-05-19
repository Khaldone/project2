using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Groups clips by priority and selects randomly within the best matching priority group.
/// Clips with the same priority are treated as variations of the same sound.
/// This is useful when you have multiple variations (e.g., "Hit_01", "Hit_02", "Hit_03") 
/// that should all be considered equivalent when matching intensity.
/// </summary>
public class PriorityGroupSelector : IClipSelector
{
    private readonly Dictionary<int, List<AudioClipData>> _priorityGroups = new Dictionary<int, List<AudioClipData>>(8);
    private string _lastPlayedClipName;
    private readonly bool _preventRepeat;

    public PriorityGroupSelector(bool preventRepeat = true)
    {
        _preventRepeat = preventRepeat;
    }

    public AudioClipData? SelectClip(SFXLayer layer, float intensity)
    {
        _priorityGroups.Clear();

        // Get all matching clips and group by priority
        var matchingClips = layer.GetClipsForIntensity(intensity);
        
        if (matchingClips.Count == 0)
            return null;

        for (int i = 0; i < matchingClips.Count; i++)
        {
            var clip = matchingClips[i];
            if (!_priorityGroups.TryGetValue(clip.Priority, out var group))
            {
                group = new List<AudioClipData>();
                _priorityGroups[clip.Priority] = group;
            }
            group.Add(clip);
        }

        // Find the priority group with highest priority value
        int bestPriority = int.MinValue;
        List<AudioClipData> bestGroup = null;

        foreach (var kvp in _priorityGroups)
        {
            if (kvp.Key > bestPriority)
            {
                bestPriority = kvp.Key;
                bestGroup = kvp.Value;
            }
        }

        if (bestGroup == null || bestGroup.Count == 0)
            return null;

        // Select randomly from the best priority group, optionally avoiding repeats
        var selectableClips = new List<AudioClipData>(bestGroup);

        if (_preventRepeat && selectableClips.Count > 1)
        {
            // Remove last played clip if possible
            for (int i = selectableClips.Count - 1; i >= 0; i--)
            {
                if (selectableClips[i].Name == _lastPlayedClipName)
                {
                    selectableClips.RemoveAt(i);
                    break;
                }
            }
        }

        if (selectableClips.Count == 0)
            selectableClips.AddRange(bestGroup); // Fallback if we removed the only clip

        int randomIndex = Random.Range(0, selectableClips.Count);
        return selectableClips[randomIndex];
    }

    public void OnClipPlayed(AudioClipData clip, float intensity)
    {
        _lastPlayedClipName = clip.Name;
    }

    public void Reset()
    {
        _lastPlayedClipName = null;
        _priorityGroups.Clear();
    }
}

/// <summary>
/// Similar to PriorityGroupSelector but uses a round-robin approach within each priority group.
/// Ensures even distribution of clip usage within the same priority level.
/// </summary>
public class PriorityGroupRoundRobinSelector : IClipSelector
{
    private readonly Dictionary<int, int> _priorityGroupIndices = new Dictionary<int, int>(8);
    private readonly Dictionary<int, List<AudioClipData>> _priorityGroups = new Dictionary<int, List<AudioClipData>>(8);

    public AudioClipData? SelectClip(SFXLayer layer, float intensity)
    {
        _priorityGroups.Clear();

        // Get all matching clips and group by priority
        var matchingClips = layer.GetClipsForIntensity(intensity);
        
        if (matchingClips.Count == 0)
            return null;

        for (int i = 0; i < matchingClips.Count; i++)
        {
            var clip = matchingClips[i];
            if (!_priorityGroups.TryGetValue(clip.Priority, out var group))
            {
                group = new List<AudioClipData>();
                _priorityGroups[clip.Priority] = group;
            }
            group.Add(clip);
        }

        // Find highest priority group
        int highestPriority = int.MinValue;
        foreach (var priority in _priorityGroups.Keys)
        {
            if (priority > highestPriority)
                highestPriority = priority;
        }

        var selectedGroup = _priorityGroups[highestPriority];
        if (selectedGroup.Count == 0)
            return null;

        // Round-robin within this priority group
        if (!_priorityGroupIndices.TryGetValue(highestPriority, out int currentIndex))
        {
            currentIndex = 0;
        }

        currentIndex = currentIndex % selectedGroup.Count;
        AudioClipData selectedClip = selectedGroup[currentIndex];

        // Advance index for next time
        _priorityGroupIndices[highestPriority] = (currentIndex + 1) % selectedGroup.Count;

        return selectedClip;
    }

    public void OnClipPlayed(AudioClipData clip, float intensity) { }

    public void Reset()
    {
        _priorityGroupIndices.Clear();
        _priorityGroups.Clear();
    }
}