using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Audio/SFX Layer")]
public class SFXLayer : ScriptableObject
{
    [Header("Layer Configuration")]
    public string LayerName;
    public AudioBus Bus;
    
    [Header("Clip Selection")]
    [Tooltip("Strategy for selecting which clip to play when multiple clips match")]
    public ClipSelectorType SelectorType = ClipSelectorType.Random;
    
    [Header("Audio Clips")]
    [Tooltip("All audio clips available in this layer")]
    public List<AudioClipData> Clips = new List<AudioClipData>();
    
    [Header("Intensity Mapping")]
    [Tooltip("Maps intensity ranges to clip indices. Each range can reference multiple clips for variation.")]
    public List<IntensityRange> IntensityRanges = new List<IntensityRange>();
    
    [Header("Priority Fading")]
    [Tooltip("Volume multipliers for each priority level (0 = highest priority)")]
    public List<float> PriorityFadeSteps = new List<float> { 0.7f, 0.4f, 0f };
    
    [Tooltip("Speed at which priority fading occurs")]
    public float FadeSpeed = 3f;
    
    public bool IsValid => !string.IsNullOrEmpty(LayerName) && Bus != null && Bus.IsValid;

    /// <summary>
    /// Gets all clips that match the given intensity value
    /// </summary>
    public List<AudioClipData> GetClipsForIntensity(float intensity)
    {
        var matchingClips = new List<AudioClipData>();
        
        for (int i = 0; i < IntensityRanges.Count; i++)
        {
            var range = IntensityRanges[i];
            if (range.Contains(intensity))
            {
                for (int j = 0; j < range.ClipIndices.Length; j++)
                {
                    int clipIndex = range.ClipIndices[j];
                    if (clipIndex >= 0 && clipIndex < Clips.Count)
                    {
                        matchingClips.Add(Clips[clipIndex]);
                    }
                }
            }
        }
        
        return matchingClips;
    }

#if UNITY_EDITOR
    [ContextMenu("Auto-Create Equal Intensity Ranges")]
    private void AutoCreateEqualRanges()
    {
        if (Clips.Count == 0)
        {
            Debug.LogWarning("No clips to create ranges for!");
            return;
        }

        IntensityRanges.Clear();
        float rangeSize = 1f / Clips.Count;
        
        for (int i = 0; i < Clips.Count; i++)
        {
            IntensityRanges.Add(new IntensityRange
            {
                Min = i * rangeSize,
                Max = (i + 1) * rangeSize,
                ClipIndices = new int[] { i }
            });
        }
        
        UnityEditor.EditorUtility.SetDirty(this);
        Debug.Log($"Created {IntensityRanges.Count} equal intensity ranges");
    }

    [ContextMenu("Group Clips by Priority")]
    private void GroupClipsByPriority()
    {
        if (Clips.Count == 0)
        {
            Debug.LogWarning("No clips to group!");
            return;
        }

        // Group clips by priority
        var priorityGroups = new Dictionary<int, List<int>>();
        
        for (int i = 0; i < Clips.Count; i++)
        {
            int priority = Clips[i].Priority;
            if (!priorityGroups.ContainsKey(priority))
                priorityGroups[priority] = new List<int>();
            
            priorityGroups[priority].Add(i);
        }

        // Sort priorities
        var sortedPriorities = new List<int>(priorityGroups.Keys);
        sortedPriorities.Sort();

        // Create intensity ranges for each priority group
        IntensityRanges.Clear();
        float rangeSize = 1f / sortedPriorities.Count;
        
        for (int i = 0; i < sortedPriorities.Count; i++)
        {
            int priority = sortedPriorities[i];
            IntensityRanges.Add(new IntensityRange
            {
                Min = i * rangeSize,
                Max = (i + 1) * rangeSize,
                ClipIndices = priorityGroups[priority].ToArray()
            });
        }
        
        UnityEditor.EditorUtility.SetDirty(this);
        Debug.Log($"Created {IntensityRanges.Count} intensity ranges from {sortedPriorities.Count} priority groups");
    }

    [ContextMenu("Create 3-Tier Setup (Soft/Medium/Hard)")]
    private void CreateThreeTierSetup()
    {
        IntensityRanges.Clear();
        
        var softClips = new List<int>();
        var mediumClips = new List<int>();
        var hardClips = new List<int>();
        
        // Group clips by priority tiers
        for (int i = 0; i < Clips.Count; i++)
        {
            int priority = Clips[i].Priority;
            
            if (priority <= 10)
                softClips.Add(i);
            else if (priority <= 20)
                mediumClips.Add(i);
            else
                hardClips.Add(i);
        }
        
        // Create ranges
        if (softClips.Count > 0)
        {
            IntensityRanges.Add(new IntensityRange
            {
                Min = 0f,
                Max = 0.35f,
                ClipIndices = softClips.ToArray()
            });
        }
        
        if (mediumClips.Count > 0)
        {
            IntensityRanges.Add(new IntensityRange
            {
                Min = 0.3f,
                Max = 0.75f,
                ClipIndices = mediumClips.ToArray()
            });
        }
        
        if (hardClips.Count > 0)
        {
            IntensityRanges.Add(new IntensityRange
            {
                Min = 0.7f,
                Max = 1.0f,
                ClipIndices = hardClips.ToArray()
            });
        }
        
        UnityEditor.EditorUtility.SetDirty(this);
        Debug.Log($"Created 3-tier setup: {softClips.Count} soft, {mediumClips.Count} medium, {hardClips.Count} hard clips");
    }

    [ContextMenu("Validate Configuration")]
    private void ValidateConfiguration()
    {
        bool hasErrors = false;
        
        // Check for invalid clip indices
        for (int i = 0; i < IntensityRanges.Count; i++)
        {
            var range = IntensityRanges[i];
            for (int j = 0; j < range.ClipIndices.Length; j++)
            {
                int clipIndex = range.ClipIndices[j];
                if (clipIndex < 0 || clipIndex >= Clips.Count)
                {
                    Debug.LogError($"Range {i} references invalid clip index {clipIndex}");
                    hasErrors = true;
                }
            }
        }
        
        // Check intensity coverage
        const int samples = 100;
        int uncoveredCount = 0;
        
        for (int i = 0; i <= samples; i++)
        {
            float intensity = i / (float)samples;
            bool hasCoverage = false;
            
            foreach (var range in IntensityRanges)
            {
                if (range.Contains(intensity))
                {
                    hasCoverage = true;
                    break;
                }
            }
            
            if (!hasCoverage)
                uncoveredCount++;
        }
        
        if (uncoveredCount > 0)
        {
            float uncoveredPercent = (uncoveredCount / (float)(samples + 1)) * 100f;
            Debug.LogWarning($"⚠ {uncoveredPercent:F1}% of intensity range (0.0-1.0) is uncovered");
            hasErrors = true;
        }
        
        if (!hasErrors)
        {
            Debug.Log($"✓ Layer '{LayerName}' configuration is valid!");
        }
    }

    [ContextMenu("Print Layer Info")]
    private void PrintLayerInfo()
    {
        Debug.Log($"=== SFX Layer: {LayerName} ===");
        Debug.Log($"Selector Type: {SelectorType}");
        Debug.Log($"Total Clips: {Clips.Count}");
        Debug.Log($"Intensity Ranges: {IntensityRanges.Count}");
        
        for (int i = 0; i < IntensityRanges.Count; i++)
        {
            var range = IntensityRanges[i];
            Debug.Log($"  Range {i}: [{range.Min:F2}-{range.Max:F2}] → {range.ClipIndices.Length} clips");
        }
    }
#endif
}