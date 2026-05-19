using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SFXPlayer
{
    private readonly List<SFXLayer> _layers;
    private readonly AudioSourcePool _pool;
    private readonly BusManager _busManager;
    private readonly Dictionary<string, AudioClipData> _clipLookup;
    private readonly Dictionary<string, List<AudioClipData>> _layerClipsLookup;
    private readonly Dictionary<string, IClipSelector> _layerSelectors;
    private readonly List<ActiveSound> _activeSounds;
    
    private readonly Dictionary<string, List<ActiveSound>> _soundsByLayer = new Dictionary<string, List<ActiveSound>>();
    private readonly HashSet<string> _activeBusNames = new HashSet<string>();
    private readonly List<ActiveSound> _sortBuffer = new List<ActiveSound>(32);
    private Comparison<ActiveSound> _comparison;
    
    private long _nextSoundId = 1;

    private class ActiveSound
    {
        public long Id;
        public AudioSource Source;
        public string LayerName;
        public string ClipName;
        public float EndTime;
        public bool IsLooping;
        public float BaseVolume;
        public float PlayTime;
        public int Priority;
        public bool BypassFade;
        public float CurrentFadeMultiplier = 1f;
        public float TargetFadeMultiplier = 1f;
        public float StartDelay;
        public float ActualStartTime;
        public Action OnStart;
        public Action OnComplete;
        public bool HasStarted;

        public bool IsFinished => !IsLooping && Time.time >= EndTime;
    }

    public int ActiveCount => _activeSounds.Count;

    public SFXPlayer(List<SFXLayer> layers, AudioSourcePool pool, BusManager busManager)
    {
        _layers = new List<SFXLayer>(layers.Count);
        for (int i = 0; i < layers.Count; i++)
        {
            if (layers[i] != null && layers[i].IsValid)
                _layers.Add(layers[i]);
        }

        _comparison = ActiveSoundPriorityComparison;
        _pool = pool;
        _busManager = busManager;
        _clipLookup = new Dictionary<string, AudioClipData>();
        _layerClipsLookup = new Dictionary<string, List<AudioClipData>>();
        _layerSelectors = new Dictionary<string, IClipSelector>();
        _activeSounds = new List<ActiveSound>();

        BuildClipLookup();
        InitializeSelectors();
    }

    private void BuildClipLookup()
    {
        for (int i = 0; i < _layers.Count; i++)
        {
            var layer = _layers[i];
            
            for (int j = 0; j < layer.Clips.Count; j++)
            {
                var clip = layer.Clips[j];
                if (clip.IsValid && !_clipLookup.ContainsKey(clip.Name))
                    _clipLookup[clip.Name] = clip;
            }
        }
    }

    private void InitializeSelectors()
    {
        for (int i = 0; i < _layers.Count; i++)
        {
            var layer = _layers[i];
            _layerSelectors[layer.LayerName] = ClipSelectorFactory.Create(layer.SelectorType);
        }
    }

    public void Update()
    {
        ProcessDelayedStarts();
        RemoveFinishedSounds();
        UpdatePriorityFades();
        UpdateBusStates();
    }

    private void ProcessDelayedStarts()
    {
        for (int i = 0; i < _activeSounds.Count; i++)
        {
            var sound = _activeSounds[i];
            
            if (!sound.HasStarted && Time.time >= sound.ActualStartTime)
            {
                if (sound.Source != null)
                {
                    sound.Source.Play();
                    sound.HasStarted = true;
                    sound.OnStart?.Invoke();
                }
            }
        }
    }

    private void RemoveFinishedSounds()
    {
        for (int i = _activeSounds.Count - 1; i >= 0; i--)
        {
            var sound = _activeSounds[i];

            if (sound.Source == null || !sound.Source.gameObject.activeInHierarchy || sound.IsFinished)
            {
                if (sound.Source != null)
                {
                    sound.OnComplete?.Invoke();
                    _pool.Return(sound.Source);
                }

                _activeSounds.RemoveAt(i);
            }
        }
    }

    private void UpdatePriorityFades()
    {
        Dictionary<string, List<ActiveSound>>.ValueCollection.Enumerator activeSoundListEnumerator =
            _soundsByLayer.Values.GetEnumerator();
        while (activeSoundListEnumerator.MoveNext())
        {
            List<ActiveSound> currentValue = activeSoundListEnumerator.Current;
            if (currentValue != null)
                currentValue.Clear();
        }

        for (int i = 0; i < _activeSounds.Count; i++)
        {
            var sound = _activeSounds[i];
            if (!_soundsByLayer.TryGetValue(sound.LayerName, out var list))
            {
                list = new List<ActiveSound>(8);
                _soundsByLayer[sound.LayerName] = list;
            }

            list.Add(sound);
        }

        Dictionary<string, List<ActiveSound>>.Enumerator enumerator = _soundsByLayer.GetEnumerator();
        while (enumerator.MoveNext())
        {
            var kvp = enumerator.Current;

            if (kvp.Value.Count == 0) continue;

            var layer = GetLayer(kvp.Key);
            if (layer == null || layer.PriorityFadeSteps.Count == 0) continue;

            _sortBuffer.Clear();
            _sortBuffer.AddRange(kvp.Value);
            _sortBuffer.Sort(_comparison);

            for (int i = 0; i < _sortBuffer.Count; i++)
            {
                var sound = _sortBuffer[i];

                if (sound.BypassFade)
                {
                    sound.TargetFadeMultiplier = 1f;
                }
                else if (i == 0)
                {
                    sound.TargetFadeMultiplier = 1f;
                }
                else
                {
                    int stepIndex = Mathf.Min(i - 1, layer.PriorityFadeSteps.Count - 1);
                    sound.TargetFadeMultiplier = layer.PriorityFadeSteps[stepIndex];
                }

                bool isNewSound = Time.time - sound.PlayTime < 0.1f;

                if (isNewSound)
                {
                    sound.CurrentFadeMultiplier = sound.TargetFadeMultiplier;
                }
                else
                {
                    sound.CurrentFadeMultiplier = Mathf.MoveTowards(
                        sound.CurrentFadeMultiplier,
                        sound.TargetFadeMultiplier,
                        layer.FadeSpeed * Time.deltaTime
                    );
                }

                if (sound.Source != null)
                {
                    sound.Source.volume = sound.BaseVolume * sound.CurrentFadeMultiplier;
                }
            }
        }
    }

    private int ActiveSoundPriorityComparison(ActiveSound a, ActiveSound b)
    {
        int priorityCompare = b.Priority.CompareTo(a.Priority);
        if (priorityCompare != 0) return priorityCompare;
        return b.PlayTime.CompareTo(a.PlayTime);
    }

    private void UpdateBusStates()
    {
        _activeBusNames.Clear();

        foreach (var kvp in _soundsByLayer)
        {
            if (kvp.Value.Count == 0) continue;

            ActiveSound highestPriority = kvp.Value[0];
            for (int i = 1; i < kvp.Value.Count; i++)
            {
                if (kvp.Value[i].Priority > highestPriority.Priority)
                    highestPriority = kvp.Value[i];
            }

            var layer = GetLayer(kvp.Key);
            if (layer != null)
            {
                _activeBusNames.Add(layer.Bus.BusName);
                _busManager.UpdateActiveState(layer.Bus.BusName, true, highestPriority.ClipName);
            }
        }

        for (int i = 0; i < _layers.Count; i++)
        {
            if (!_activeBusNames.Contains(_layers[i].Bus.BusName))
            {
                _busManager.UpdateActiveState(_layers[i].Bus.BusName, false);
            }
        }
    }

    public long Play(string clipName, float? volume = null, float? pitchOffset = null, bool? loop = null, bool stopIfLooping = false, 
        float? delay = null, Action onStart = null, Action onComplete = null)
    {
        if (!_clipLookup.TryGetValue(clipName, out var clipData)) return -1;

        var layer = GetLayerForClip(clipName);
        if (layer == null)
        {
            Debug.LogError($"Could not find layer for clip: {clipName}");
            return -1;
        }

        if (stopIfLooping && clipData.Loop && IsPlaying(clipName))
        {
            Stop(clipName);
        }

        if (volume.HasValue)
            clipData.Volume = volume.Value;
        if (loop.HasValue)
            clipData.Loop = loop.Value;
        if (delay.HasValue)
            clipData.Delay = delay.Value;
        if (pitchOffset.HasValue)
            clipData.PitchOffsetMinMax = Vector2.one * pitchOffset.Value;

        return PlaySound(clipData, layer, onStart, onComplete);
    }

    /// <summary>
    /// Plays a sound from the specified layer based on intensity (0-1).
    /// Uses the layer's configured clip selector strategy.
    /// </summary>
    public long PlayByIntensity(string layerName, float intensity, float? volume = null, float? pitchOffset = null, bool? loop = null,
        float? delay = null, Action onStart = null, Action onComplete = null)
    {
        var layer = GetLayer(layerName);
        if (layer == null)
        {
            Debug.LogWarning($"Layer '{layerName}' not found");
            return -1;
        }

        // Use the layer's selector strategy
        if (!_layerSelectors.TryGetValue(layerName, out var selector))
        {
            Debug.LogWarning($"No selector found for layer '{layerName}'");
            return -1;
        }

        AudioClipData? selectedClip = selector.SelectClip(layer, intensity);

        if (!selectedClip.HasValue)
        {
            Debug.LogWarning($"No suitable clip found for intensity {intensity:F2} in layer '{layerName}'");
            return -1;
        }

        var clipData = selectedClip.Value;

        // Apply overrides
        if (volume.HasValue)
            clipData.Volume = volume.Value;
        if (loop.HasValue)
            clipData.Loop = loop.Value;
        if (delay.HasValue)
            clipData.Delay = delay.Value;
        if (pitchOffset.HasValue)
            clipData.PitchOffsetMinMax = pitchOffset.Value * Vector2.one;

        long soundId = PlaySound(clipData, layer, onStart, onComplete);
        
        // Notify selector that clip was played
        if (soundId != -1)
        {
            selector.OnClipPlayed(clipData, intensity);
        }

        return soundId;
    }

    /// <summary>
    /// Sets the clip selector strategy for a specific layer at runtime
    /// </summary>
    public void SetLayerSelector(string layerName, ClipSelectorType selectorType)
    {
        if (_layerSelectors.ContainsKey(layerName))
        {
            _layerSelectors[layerName] = ClipSelectorFactory.Create(selectorType);
        }
    }

    /// <summary>
    /// Sets a custom clip selector for a specific layer
    /// </summary>
    public void SetLayerSelector(string layerName, IClipSelector selector)
    {
        if (_layerSelectors.ContainsKey(layerName))
        {
            _layerSelectors[layerName] = selector;
        }
    }

    private long PlaySound(AudioClipData clipData, SFXLayer layer, Action onStart, Action onComplete)
    {
        var source = _pool.Get();
        if (source == null) return -1;

        source.clip = clipData.Clip;
        source.loop = clipData.Loop;
        source.volume = clipData.Volume;
        source.outputAudioMixerGroup = layer.Bus.MixerGroup;

        float pitch = 1f + UnityEngine.Random.Range(clipData.PitchOffsetMinMax.x, clipData.PitchOffsetMinMax.y);
        source.pitch = pitch;

        if (_nextSoundId >= long.MaxValue - 2)
            _nextSoundId = 0;
        
        long soundId = _nextSoundId++;
        float playTime = Time.time;
        float actualStartTime = playTime + clipData.Delay;
        
        if (clipData.Delay <= 0f)
        {
            source.Play();
            onStart?.Invoke();
        }

        var activeSound = new ActiveSound
        {
            Id = soundId,
            Source = source,
            LayerName = layer.LayerName,
            ClipName = clipData.Name,
            EndTime = actualStartTime + clipData.Clip.length,
            IsLooping = clipData.Loop,
            BaseVolume = clipData.Volume,
            PlayTime = playTime,
            Priority = clipData.Priority,
            BypassFade = clipData.BypassPriorityFade,
            CurrentFadeMultiplier = 1f,
            TargetFadeMultiplier = 1f,
            StartDelay = clipData.Delay,
            ActualStartTime = actualStartTime,
            OnStart = onStart,
            OnComplete = onComplete,
            HasStarted = clipData.Delay <= 0f
        };

        _activeSounds.Add(activeSound);
        return soundId;
    }

    public void Stop(long soundId)
    {
        for (int i = _activeSounds.Count - 1; i >= 0; i--)
        {
            if (_activeSounds[i].Id == soundId)
            {
                var sound = _activeSounds[i];
                sound.OnComplete?.Invoke();
                _pool.Return(sound.Source);
                _activeSounds.RemoveAt(i);
                return;
            }
        }
    }

    public void Stop(string clipName)
    {
        for (int i = _activeSounds.Count - 1; i >= 0; i--)
        {
            if (_activeSounds[i].ClipName == clipName)
            {
                var sound = _activeSounds[i];
                sound.OnComplete?.Invoke();
                _pool.Return(sound.Source);
                _activeSounds.RemoveAt(i);
            }
        }
    }

    public void StopAll()
    {
        for (int i = _activeSounds.Count - 1; i >= 0; i--)
        {
            var sound = _activeSounds[i];
            sound.OnComplete?.Invoke();
            _pool.Return(sound.Source);
        }

        _activeSounds.Clear();
        
        // Reset all selectors
        foreach (var selector in _layerSelectors.Values)
        {
            selector.Reset();
        }
    }

    public void StopLayer(string layerName)
    {
        for (int i = _activeSounds.Count - 1; i >= 0; i--)
        {
            if (_activeSounds[i].LayerName == layerName)
            {
                var sound = _activeSounds[i];
                sound.OnComplete?.Invoke();
                _pool.Return(sound.Source);
                _activeSounds.RemoveAt(i);
            }
        }
        
        // Reset selector for this layer
        if (_layerSelectors.TryGetValue(layerName, out var selector))
        {
            selector.Reset();
        }
    }

    public bool IsPlaying(string clipName)
    {
        for (int i = 0; i < _activeSounds.Count; i++)
        {
            if (_activeSounds[i].ClipName == clipName && _activeSounds[i].Source != null)
                return true;
        }
        return false;
    }

    public bool IsPlaying(long soundId)
    {
        for (int i = 0; i < _activeSounds.Count; i++)
        {
            if (_activeSounds[i].Id == soundId && _activeSounds[i].Source != null)
                return true;
        }
        return false;
    }

    public SFXSoundInfo GetSoundInfo(string clipName)
    {
        for (int i = 0; i < _activeSounds.Count; i++)
        {
            var sound = _activeSounds[i];
            if (sound.ClipName == clipName && sound.Source != null)
            {
                return CreateSoundInfo(sound);
            }
        }
        return null;
    }

    public SFXSoundInfo GetSoundInfo(long soundId)
    {
        for (int i = 0; i < _activeSounds.Count; i++)
        {
            var sound = _activeSounds[i];
            if (sound.Id == soundId && sound.Source != null)
            {
                return CreateSoundInfo(sound);
            }
        }
        return null;
    }

    public List<SFXSoundInfo> GetAllSoundInfos()
    {
        var infos = new List<SFXSoundInfo>(_activeSounds.Count);
        
        for (int i = 0; i < _activeSounds.Count; i++)
        {
            var sound = _activeSounds[i];
            if (sound.Source == null) continue;

            infos.Add(CreateSoundInfo(sound));
        }
        
        return infos;
    }

    private SFXSoundInfo CreateSoundInfo(ActiveSound sound)
    {
        var layer = GetLayer(sound.LayerName);
        float duckMultiplier = layer?.Bus != null ? layer.Bus.CurrentDuckVolume : 1f;

        return new SFXSoundInfo
        {
            Id = sound.Id,
            ClipName = sound.ClipName,
            LayerName = sound.LayerName,
            Priority = sound.Priority,
            CurrentVolume = sound.Source.volume,
            FadeMultiplier = sound.CurrentFadeMultiplier,
            DuckMultiplier = duckMultiplier,
            PlayTime = sound.PlayTime,
            IsLooping = sound.IsLooping,
            Duration = sound.IsLooping ? 0f : (sound.EndTime - sound.PlayTime)
        };
    }

    private SFXLayer GetLayerForClip(string clipName)
    {
        for (int i = 0; i < _layers.Count; i++)
        {
            for (int j = 0; j < _layers[i].Clips.Count; j++)
            {
                if (_layers[i].Clips[j].Name == clipName)
                    return _layers[i];
            }
        }
        return null;
    }

    private SFXLayer GetLayer(string layerName)
    {
        for (int i = 0; i < _layers.Count; i++)
        {
            if (_layers[i].LayerName == layerName)
                return _layers[i];
        }
        return null;
    }

    public void MuteSFX(bool mute)
    {
        foreach (var sfxLayer in _layers)
        {
            _busManager.Mute(sfxLayer.Bus.BusName, mute);
        }
    }

    public void SetVolume(float volume)
    {
        foreach (var sfxLayer in _layers)
        {
            _busManager.SetVolume(sfxLayer.Bus.BusName, volume);
        }
    }
}