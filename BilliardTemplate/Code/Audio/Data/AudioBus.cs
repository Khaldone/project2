using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public enum DuckingReductionMode
{
    Relative,
    Absolute
}

[CreateAssetMenu(menuName = "Audio/Bus")]
public class AudioBus : ScriptableObject
{
    public string BusName => _busName;
    public AudioMixerGroup MixerGroup => _mixerGroup;
    public float DuckingSpeed => _duckingSpeed;
    public List<DuckingTarget> DuckingTargets => _duckingTargets;
    public int Priority => _priority;

    public float BaseVolume
    {
        get => _baseVolume;
        set => _baseVolume = Mathf.Clamp01(value);
    }

    public float FinalVolume => _baseVolume * FadeVolume * CurrentDuckVolume;

    public bool IsValid => !string.IsNullOrEmpty(_busName) && _mixerGroup != null;

    public string VolumeParameterName
    {
        get
        {
            if (_busName != _lastBusName)
            {
                _cachedVolumeParameterName = $"{_busName} Volume";
                _lastBusName = _busName;
            }
            return _cachedVolumeParameterName;
        }
    }
    [SerializeField] private string _busName;
    [SerializeField] private AudioMixerGroup _mixerGroup;
    [SerializeField] [Range(0f, 1f)] private float _baseVolume = 1f;
    [SerializeField] private float _duckingSpeed = 2f;
    [SerializeField] private int _priority = 0;
    [SerializeField] private List<DuckingTarget> _duckingTargets = new List<DuckingTarget>();

    [NonSerialized] public float FadeVolume = 1f;
    [NonSerialized] public float CurrentDuckVolume = 1f;
    [NonSerialized] public float TargetDuckVolume = 1f;
    [NonSerialized] public bool HasActiveAudio = false;
    [NonSerialized] public bool Muted = false;
    [NonSerialized] public string CurrentPlayingSound = "";

    [NonSerialized] private string _cachedVolumeParameterName;
    [NonSerialized] private string _lastBusName;

    public void Reset()
    {
        FadeVolume = 1f;
        CurrentDuckVolume = 1f;
        TargetDuckVolume = 1f;
        HasActiveAudio = false;
        Muted = false;
        CurrentPlayingSound = "";
    }
}