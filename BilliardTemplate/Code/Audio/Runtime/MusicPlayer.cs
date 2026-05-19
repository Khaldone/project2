using UnityEngine;

public class MusicPlayer
{
    private readonly MusicPlaylist _playlist;
    private readonly AudioSourcePool _pool;
    private readonly BusManager _busManager;

    private AudioSource _currentSource;
    private int _currentIndex;
    private float _trackEndTime;
    private bool _isPlaying;

    public bool IsPlaying => _isPlaying;
    public string CurrentTrack { get; private set; }

    public MusicPlayer(MusicPlaylist playlist, AudioSourcePool pool, BusManager busManager)
    {
        _playlist = playlist;
        _pool = pool;
        _busManager = busManager;
    }

    public void Update()
    {
        if (!_isPlaying || _playlist.Tracks.Count == 0) return;

        if (Time.time >= _trackEndTime && !string.IsNullOrEmpty(CurrentTrack))
        {
            PlayNext();
        }
    }

    public void Play()
    {
        if (_playlist.Tracks.Count == 0) return;

        _isPlaying = true;
        _currentIndex = 0;
        PlayCurrent();
    }

    public void Stop()
    {
        _isPlaying = false;
        ReturnCurrentSource();
        CurrentTrack = null;
    }

    public void PlayNext()
    {
        if (_playlist.Tracks.Count == 0) return;

        if (_playlist.Shuffle)
        {
            _currentIndex = Random.Range(0, _playlist.Tracks.Count);
        }
        else
        {
            _currentIndex++;
            if (_currentIndex >= _playlist.Tracks.Count)
            {
                if (_playlist.Repeat)
                    _currentIndex = 0;
                else
                {
                    Stop();
                    return;
                }
            }
        }

        PlayCurrent();
    }

    private void PlayCurrent()
    {
        if (_currentIndex >= _playlist.Tracks.Count) return;

        var track = _playlist.Tracks[_currentIndex];
        if (!track.IsValid) return;

        ReturnCurrentSource();

        _currentSource = _pool.Get();
        if (_currentSource == null) return;

        _currentSource.clip = track.Clip;
        _currentSource.loop = false;
        _currentSource.volume = track.Volume;
        _currentSource.outputAudioMixerGroup = _playlist.Bus.MixerGroup;
        _currentSource.pitch = 1f;

        _currentSource.Play();
        _trackEndTime = Time.time + track.Clip.length;

        CurrentTrack = track.Name;
        _busManager.UpdateActiveState(_playlist.Bus.BusName, true);
    }

    private void ReturnCurrentSource()
    {
        if (_currentSource != null)
        {
            _pool.Return(_currentSource);
            _currentSource = null;
            _busManager.UpdateActiveState(_playlist.Bus.BusName, false);
        }
    }

    public void MuteMusic(bool mute)
    {
        _busManager.Mute(_playlist.Bus.BusName, mute);
    }

    public void SetVolume(float volume)
    {
        _busManager.SetVolume(_playlist.Bus.BusName, volume);
    }
}