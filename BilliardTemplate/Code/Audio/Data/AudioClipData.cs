using System;
using UnityEngine;

[Serializable]
public struct AudioClipData
{
    public string Name;
    public AudioClip Clip;
    public float Volume;
    public float Delay;
    public bool Loop;
    public Vector2 PitchOffsetMinMax;
    public bool BypassPriorityFade;
    public int Priority;

    public bool IsValid => !string.IsNullOrEmpty(Name) && Clip != null && Volume > 0;
}