// Assets/_Project/CoreDomain/Audio/IAudioService.cs
public interface IAudioService
{
    void PlaySFX(string soundId);
    void PlayMusic(string trackId);
    void PlayAnnouncer(string voiceLineId);

    // Values should be passed as normalized floats (0.001f to 1.0f)
    void SetMasterVolume(float normalizedVolume);
    void SetMusicVolume(float normalizedVolume);
    void SetSFXVolume(float normalizedVolume);
}