using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BusManager
{
    private readonly Dictionary<string, AudioBus> _buses;
    private readonly Dictionary<string, FadeInfo> _activeFades;
    
    private readonly List<string> _completedFades = new List<string>(8);
    private List<AudioBus> _activeBuses = new List<AudioBus>(8);

    private Comparison<AudioBus> _comparison;
    
    private const float MUTE_VOLUME_DB = -80f;
    private const float MIN_VOLUME_DB = -80f;

    private struct FadeInfo
    {
        public float StartVolume;
        public float TargetVolume;
        public float Duration;
        public float StartTime;

        public FadeInfo(float start, float target, float duration)
        {
            StartVolume = start;
            TargetVolume = target;
            Duration = duration;
            StartTime = Time.time;
        }

        public bool IsComplete => Time.time >= StartTime + Duration;

        public float GetCurrentVolume()
        {
            if (Duration <= 0f) return TargetVolume;
            float t = Mathf.Clamp01((Time.time - StartTime) / Duration);
            return Mathf.Lerp(StartVolume, TargetVolume, t);
        }
    }

    public BusManager(List<AudioBus> buses)
    {
        _buses = new Dictionary<string, AudioBus>(buses.Count);
        _comparison = AudioBusPriorityComparison; 
        
        for (int i = 0; i < buses.Count; i++)
        {
            var bus = buses[i];
            if (bus != null && bus.IsValid)
                _buses[bus.BusName] = bus;
        }
        
        _activeFades = new Dictionary<string, FadeInfo>();

        foreach (var bus in _buses.Values)
            bus.Reset();
    }

    public AudioBus GetBus(string busName)
    {
        _buses.TryGetValue(busName, out var bus);
        return bus;
    }

    public void UpdateActiveState(string busName, bool hasActive, string soundName = "")
    {
        var bus = GetBus(busName);
        if (bus != null)
        {
            bus.HasActiveAudio = hasActive;
            bus.CurrentPlayingSound = hasActive ? soundName : "";
        }
    }

    public void Update()
    {
        UpdateDucking();
        UpdateFades();
        UpdateVolumes();
    }

    private void UpdateDucking()
    {
        // Use struct enumerator for garbage-free iteration
        Dictionary<string, AudioBus>.ValueCollection.Enumerator valueEnumerator = _buses.Values.GetEnumerator();
        while (valueEnumerator.MoveNext())
        {
            if (valueEnumerator.Current != null) 
                valueEnumerator.Current.TargetDuckVolume = 1f;
        }

        _activeBuses.Clear(); 
        valueEnumerator = _buses.Values.GetEnumerator();
        while (valueEnumerator.MoveNext())
        {
            var bus = valueEnumerator.Current;
            if (bus != null && bus.HasActiveAudio)
                _activeBuses.Add(bus);
        }

        _activeBuses.Sort(_comparison);

        for (int i = 0; i < _activeBuses.Count; i++)
        {
            var bus = _activeBuses[i];
            if (bus.DuckingTargets.Count == 0) continue;

            for (int j = 0; j < bus.DuckingTargets.Count; j++)
            {
                var target = bus.DuckingTargets[j];
                var targetBus = GetBus(target.BusName);
                if (targetBus == null) continue;

                if (target.ShouldDuck(bus.CurrentPlayingSound))
                {
                    float duckAmount = Mathf.Clamp01(target.DuckAmount);
                    float reduction = target.ReductionMode == DuckingReductionMode.Absolute
                        ? duckAmount
                        : targetBus.TargetDuckVolume * duckAmount;

                    targetBus.TargetDuckVolume -= reduction;
                    targetBus.TargetDuckVolume = Mathf.Max(0f, targetBus.TargetDuckVolume);
                }
            }
        }

        // Apply ducking volume
        valueEnumerator = _buses.Values.GetEnumerator();
        while (valueEnumerator.MoveNext())
        {
            var bus = valueEnumerator.Current;
            if (bus == null) continue;
            bus.CurrentDuckVolume = Mathf.MoveTowards(
                bus.CurrentDuckVolume,
                bus.TargetDuckVolume,
                bus.DuckingSpeed * Time.deltaTime
            );
        }
    }

    private int AudioBusPriorityComparison(AudioBus a, AudioBus b)
    {
        return b.Priority.CompareTo(a.Priority);
    }

    private void UpdateFades()
    {
        _completedFades.Clear();

        Dictionary<string, FadeInfo>.Enumerator fadeEnumerator = _activeFades.GetEnumerator();
        while (fadeEnumerator.MoveNext())
        {
            var kvp = fadeEnumerator.Current;
            var bus = GetBus(kvp.Key);
            if (bus == null) continue;

            if (kvp.Value.IsComplete)
            {
                bus.FadeVolume = kvp.Value.TargetVolume;
                _completedFades.Add(kvp.Key);
            }
            else
            {
                bus.FadeVolume = kvp.Value.GetCurrentVolume();
            }
        }

        for (int i = 0; i < _completedFades.Count; i++)
            _activeFades.Remove(_completedFades[i]);
    }

    private void UpdateVolumes()
    {
        // Garbage-free iteration
        Dictionary<string, AudioBus>.ValueCollection.Enumerator valueEnumerator = _buses.Values.GetEnumerator();
        while (valueEnumerator.MoveNext())
        {
            var bus = valueEnumerator.Current;
            if (bus == null) continue;
            float finalVolume = bus.FinalVolume;

            float volumeDb = bus.Muted || finalVolume <= 0.0001f
                ? MUTE_VOLUME_DB
                : Mathf.Max(Mathf.Log10(finalVolume) * 20f, MIN_VOLUME_DB);

            bus.MixerGroup.audioMixer.SetFloat(bus.VolumeParameterName, volumeDb);
        }
    }

    public void SetVolume(string busName, float volume)
    {
        var bus = GetBus(busName);
        if (bus != null)
            bus.BaseVolume = Mathf.Clamp01(volume);
    }

    public void Mute(string busName, bool mute)
    {
        var bus = GetBus(busName);
        if (bus != null)
            bus.Muted = mute;
    }

    public void Fade(string busName, float targetVolume, float duration)
    {
        var bus = GetBus(busName);
        if (bus != null)
            _activeFades[busName] = new FadeInfo(bus.FadeVolume, Mathf.Clamp01(targetVolume), Mathf.Max(0f, duration));
    }
}