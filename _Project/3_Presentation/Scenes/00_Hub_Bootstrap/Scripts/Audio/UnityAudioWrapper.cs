// Attached to: Audio_Orchestrator
using UnityEngine;


public class UnityAudioWrapper : MonoBehaviour, IAudioService
{
    [SerializeField] private AudioSource _musicSource;
    [SerializeField] private AudioSource _sfxUiSource;
    [SerializeField] private AudioSource _sfxGameSource;

    public void PlayAnnouncer(string voiceLineId)
    {
        throw new System.NotImplementedException();
    }

    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        _musicSource.clip = clip;
        _musicSource.loop = loop;
        _musicSource.Play();
    }

    public void PlayMusic(string trackId)
    {
        throw new System.NotImplementedException();
    }

    public void PlaySFX(string soundId)
    {
        throw new System.NotImplementedException();
    }

    public void PlayUIFx(AudioClip clip)
    {
        _sfxUiSource.PlayOneShot(clip);
    }

    // The SettingsOrchestrator will call this when the player moves the volume slider!
    public void SetMasterVolume(float volume)
    {
        AudioListener.volume = volume;
    }

    public void SetMusicVolume(float normalizedVolume)
    {
        throw new System.NotImplementedException();
    }

    public void SetSFXVolume(float normalizedVolume)
    {
        throw new System.NotImplementedException();
    }
}