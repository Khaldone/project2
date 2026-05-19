using System;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public bool IsMusicPlaying => _musicPlayer?.IsPlaying ?? false;
    public string CurrentTrack => _musicPlayer?.CurrentTrack;
    public int ActiveSFXCount => _sfxPlayer?.ActiveCount ?? 0;
    public int AvailablePoolCount => _pool?.AvailableCount ?? 0;
    public IReadOnlyList<AudioBus> Buses => _allBuses;
    public IReadOnlyList<SFXLayer> SFXLayers => _sfxLayers;
    public MusicPlaylist MusicPlaylist => _musicPlaylist;
    public static AudioManager Instance { get; private set; }

    [Header("Configuration")]
    [SerializeField] private List<SFXLayer> _sfxLayers = new List<SFXLayer>();
    [SerializeField] private MusicPlaylist _musicPlaylist;

    [Header("Pool Settings")]
    [SerializeField] private int _poolSize = 10;

    private AudioSourcePool _pool;
    private BusManager _busManager;
    private SFXPlayer _sfxPlayer;
    private MusicPlayer _musicPlayer;
    
    private List<AudioBus> _allBuses = new List<AudioBus>();

    private void Awake()
    {
        if (!InitializeSingleton()) return;
        Initialize();
    }

    private void Update()
    {
        _busManager?.Update();
        _sfxPlayer?.Update();
        _musicPlayer?.Update();
    }

    private bool InitializeSingleton()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            return true;
        }

        Destroy(gameObject);
        return false;
    }

    private void Initialize()
    {
        ExtractBuses();

        _pool = new AudioSourcePool(_poolSize, transform);
        _busManager = new BusManager(_allBuses);
        _sfxPlayer = new SFXPlayer(_sfxLayers, _pool, _busManager);

        if (_musicPlaylist != null && _musicPlaylist.IsValid)
        {
            _musicPlayer = new MusicPlayer(_musicPlaylist, _pool, _busManager);
            _musicPlayer.Play();
        }
    }

    private void ExtractBuses()
    {
        _allBuses.Clear();
        var busSet = new HashSet<AudioBus>();

        for (int i = 0; i < _sfxLayers.Count; i++)
        {
            var layer = _sfxLayers[i];
            if (layer != null && layer.IsValid && layer.Bus != null)
            {
                busSet.Add(layer.Bus);
            }
        }

        if (_musicPlaylist != null && _musicPlaylist.IsValid && _musicPlaylist.Bus != null)
        {
            busSet.Add(_musicPlaylist.Bus);
        }

        foreach (var bus in busSet)
        {
            _allBuses.Add(bus);
        }

        Debug.Log($"AudioManager: Extracted {_allBuses.Count} unique buses from configuration");
    }

    // Standard SFX methods
    public long PlaySFX(string clipName, float? volume = null, float? pitchOffset = null, bool? loop = null, float? delay = null,
        bool stopIfLooping = false, Action onStart = null, Action onComplete = null)
        => _sfxPlayer?.Play(clipName, volume, pitchOffset, loop, stopIfLooping, delay, onStart, onComplete) ?? -1;
    
    /// <summary>
    /// Plays a sound from the specified layer based on intensity (0-1).
    /// Automatically selects the best matching clip based on intensity ranges and the layer's selector strategy.
    /// </summary>
    public long PlaySFXByIntensity(string layerName, float intensity, float? volume = null, float? pitchOffset = null, bool? loop = null,
        float? delay = null, Action onStart = null, Action onComplete = null)
        => _sfxPlayer?.PlayByIntensity(layerName, intensity, volume, pitchOffset, loop, delay, onStart, onComplete) ?? -1;
    
    /// <summary>
    /// Changes the clip selection strategy for a specific layer at runtime
    /// </summary>
    public void SetLayerSelector(string layerName, ClipSelectorType selectorType)
        => _sfxPlayer?.SetLayerSelector(layerName, selectorType);
    
    /// <summary>
    /// Sets a custom clip selector implementation for a specific layer
    /// </summary>
    public void SetLayerSelector(string layerName, IClipSelector selector)
        => _sfxPlayer?.SetLayerSelector(layerName, selector);
    
    public void StopSFX(long soundId) => _sfxPlayer?.Stop(soundId);
    public void StopSFX(string clipName) => _sfxPlayer?.Stop(clipName);
    public void StopAllSFX() => _sfxPlayer?.StopAll();
    public void StopSFXLayer(string layerName) => _sfxPlayer?.StopLayer(layerName);
    public bool IsSFXPlaying(string clipName) => _sfxPlayer?.IsPlaying(clipName) ?? false;
    public bool IsSFXPlaying(long soundId) => _sfxPlayer?.IsPlaying(soundId) ?? false;
    public void MuteSFX(bool mute) => _sfxPlayer?.MuteSFX(mute);
    public void SetSFXVolume(float volume) => _sfxPlayer?.SetVolume(volume);
    public SFXSoundInfo GetSFXInfo(string clipName) => _sfxPlayer?.GetSoundInfo(clipName);
    public SFXSoundInfo GetSFXInfo(long soundId) => _sfxPlayer?.GetSoundInfo(soundId);
    public List<SFXSoundInfo> GetAllActiveSounds() => _sfxPlayer?.GetAllSoundInfos() ?? new List<SFXSoundInfo>();

    // Music methods
    public void PlayMusic() => _musicPlayer?.Play();
    public void StopMusic() => _musicPlayer?.Stop();
    public void PlayNextTrack() => _musicPlayer?.PlayNext();
    public void MuteMusic(bool mute) => _musicPlayer?.MuteMusic(mute);
    public void SetMusicVolume(float volume) => _musicPlayer?.SetVolume(volume);

    // Bus methods
    public void SetBusVolume(string busName, float volume) => _busManager?.SetVolume(busName, volume);
    public void MuteBus(string busName, bool mute) => _busManager?.Mute(busName, mute);
    public void FadeBus(string busName, float targetVolume, float duration) => _busManager?.Fade(busName, targetVolume, duration);

    private void OnDestroy()
    {
        _sfxPlayer?.StopAll();
        _musicPlayer?.Stop();
    }
}