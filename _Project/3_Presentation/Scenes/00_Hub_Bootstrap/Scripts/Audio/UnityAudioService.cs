// Assets/_Project/Scenes/00_Hub_Bootstrap/Scripts/Audio/UnityAudioService.cs
using System;
using UnityEngine;
using UnityEngine.Audio;
using VContainer;
using VContainer.Unity;
public class UnityAudioService : MonoBehaviour, IAudioService, IStartable, IDisposable
{
    [SerializeField] private AudioMixer _mainMixer;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource _musicSource;
    [SerializeField] private AudioSource _sfxSource;
    [SerializeField] private AudioSource _announcerSource;


    private readonly float _minVolumeDb = -80f;
    [SerializeField] private IMessageBroker _messageBroker;

    // A dictionary linking string IDs to actual AudioClips
    // (In a full AAA setup, Addressables would handle this, but this is the concept)
    [SerializeField] private AudioClip[] _sfxLibrary;

    [Inject]
    public void Construct(IMessageBroker broker) => _messageBroker = broker;
    public void PlaySFX(string soundId)
    {
        // Lookup the clip and play it
        AudioClip clip = FindClip(soundId);
        if (clip != null)
        {
            _sfxSource.PlayOneShot(clip);
        }
    }
    private void OnMatchEnded(MatchEndedMessage msg)
    {
        if (msg.WinnerId == "Me") PlaySFX("victory_fanfare");
        else PlaySFX("defeat_trombone");
    }

    public void PlayMusic(string trackId) { /* ... */ }
    public void PlayAnnouncer(string voiceLineId) { /* ... */ }
    public void SetVolume(float sfxVolume, float musicVolume) { /* ... */ }

    private AudioClip FindClip(string id) { /* ... */ return null; }

    public void Start()
    {
        // In a full implementation, you would load these values from the
        // ILocalSaveService we built earlier, rather than defaulting to 1.0f
        SetMasterVolume(1.0f);
        SetMusicVolume(0.8f);
        SetSFXVolume(1.0f);
        _messageBroker.Subscribe<MatchEndedMessage>(OnMatchEnded);
    }

    public void SetMasterVolume(float normalizedVolume) => ApplyLogarithmicVolume("MasterVol", normalizedVolume);
    public void SetMusicVolume(float normalizedVolume) => ApplyLogarithmicVolume("MusicVol", normalizedVolume);
    public void SetSFXVolume(float normalizedVolume) => ApplyLogarithmicVolume("SFXVol", normalizedVolume);


    private void ApplyLogarithmicVolume(string exposedParameterName, float normalizedVolume)
    {
        // Clamp to prevent log(0) which equals negative infinity (crashes audio engine)
        float clampedVol = Mathf.Clamp(normalizedVolume, 0.0001f, 1f);

        // THE AAA MATH: Convert linear (0-1) to Decibels (-80dB to 0dB)
        float dbVolume = Mathf.Log10(clampedVol) * 20f;


        if (clampedVol <= 0.001f) dbVolume = _minVolumeDb; // Absolute mute


        _mainMixer.SetFloat(exposedParameterName, dbVolume);
    }

    // ... PlaySFX, PlayMusic implementations ...

    public void Dispose() => _messageBroker.Unsubscribe<MatchEndedMessage>(OnMatchEnded);

}