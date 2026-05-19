using System;
using UnityEngine;

[Serializable]
public struct AudioTrackData
{
    public string Name;
    public AudioClip Clip;
    public float Volume;

    public bool IsValid => !string.IsNullOrEmpty(Name) && Clip != null && Volume > 0;
}