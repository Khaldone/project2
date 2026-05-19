using UnityEngine;
using System.Collections.Generic;

public class AudioManagerTestGUI : MonoBehaviour
{
    [SerializeField] private AudioManager _audioManager;
    
    [Header("UI Settings")]
    [SerializeField] private int _fontSize = 14;
    [SerializeField] private int _headerFontSize = 20;
    [SerializeField] private int _subHeaderFontSize = 16;
    [SerializeField] private bool _useMobileLayout = true;
    [SerializeField] private float _mobileButtonHeight = 50f;
    [SerializeField] private float _mobilePadding = 15f;
    [SerializeField] private float _sliderHeight = 40f;
    [SerializeField] private float _progressBarHeight = 30f;
    
    private Vector2 _scrollPosition;
    private Dictionary<string, bool> _sfxFoldouts = new Dictionary<string, bool>();
    private Dictionary<string, bool> _busFoldouts = new Dictionary<string, bool>();
    private Dictionary<string, float> _busVolumeSliders = new Dictionary<string, float>();
    private Dictionary<string, bool> _busMuteToggles = new Dictionary<string, bool>();
    
    private bool _showStats = true;
    private bool _showMusic = true;
    private bool _showSFX = false;
    private bool _showBuses = false;
    private bool _showQuickActions = false;
    private bool _showActiveSounds = false;
    private bool _forceLoopAllSounds = false;
    
    private GUIStyle _headerStyle;
    private GUIStyle _subHeaderStyle;
    private GUIStyle _boxStyle;
    private GUIStyle _buttonStyle;
    private GUIStyle _labelStyle;
    private GUIStyle _highlightStyle;
    private GUIStyle _toggleStyle;
    private bool _stylesInitialized = false;

    private bool IsLandscape => Screen.width > Screen.height;
    private float ButtonWidth => _useMobileLayout ? (IsLandscape ? 120f : 100f) : 100f;
    private float ButtonHeight => _useMobileLayout ? _mobileButtonHeight : 30f;

    private void Start()
    {
        if (_audioManager == null)
            _audioManager = AudioManager.Instance;
            
        if (_audioManager == null)
        {
            Debug.LogError("AudioManager not found!");
            enabled = false;
            return;
        }

        InitializeBusControls();
    }

    private void InitializeStyles()
    {
        if (_stylesInitialized) return;

        int padding = _useMobileLayout ? (int)_mobilePadding : 10;

        GUI.skin.horizontalSlider.fixedHeight = _sliderHeight * 0.9f;
        GUI.skin.horizontalSliderThumb.fixedHeight = _sliderHeight;
        GUI.skin.horizontalSliderThumb.fixedWidth = _sliderHeight;

        
        _headerStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = _headerFontSize,
            fontStyle = FontStyle.Bold,
            normal = { textColor = Color.yellow },
            wordWrap = true
        };

        _subHeaderStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = _subHeaderFontSize,
            fontStyle = FontStyle.Bold,
            normal = { textColor = Color.cyan },
            wordWrap = true
        };

        _boxStyle = new GUIStyle(GUI.skin.box)
        {
            padding = new RectOffset(padding, padding, padding, padding),
            margin = new RectOffset(5, 5, 5, 5)
        };

        _buttonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = _fontSize,
            padding = new RectOffset(padding, padding, 5, 5),
            wordWrap = false,
            alignment = TextAnchor.MiddleCenter
        };

        _labelStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = _fontSize,
            normal = { textColor = Color.white },
            wordWrap = true
        };

        _highlightStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = _fontSize,
            fontStyle = FontStyle.Bold,
            normal = { textColor = Color.green },
            wordWrap = true
        };

        _toggleStyle = new GUIStyle(GUI.skin.toggle)
        {
            fontSize = _fontSize,
            fixedHeight = _sliderHeight,
        };

        _stylesInitialized = true;
    }

    private void InitializeBusControls()
    {
        foreach (var bus in _audioManager.Buses)
        {
            if (bus == null || !bus.IsValid) continue;
            
            _busVolumeSliders[bus.BusName] = bus.BaseVolume;
            _busMuteToggles[bus.BusName] = false;
            _busFoldouts[bus.BusName] = false;
        }
    }

    private void OnGUI()
    {
        InitializeStyles();

        float margin = _useMobileLayout ? _mobilePadding : 10f;
        GUILayout.BeginArea(new Rect(margin, margin, Screen.width - margin * 2, Screen.height - margin * 2));
        _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);

        DrawHeader();
        
        if (_showStats) DrawStatsSection();
        if (_showActiveSounds) DrawActiveSoundsSection();
        if (_showMusic) DrawMusicSection();
        if (_showSFX) DrawSFXSection();
        if (_showBuses) DrawBusSection();
        if (_showQuickActions) DrawQuickActionsSection();

        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }

    private void DrawHeader()
    {
        GUILayout.BeginVertical(_boxStyle);
        GUILayout.Label("Audio Manager", _headerStyle);
        GUILayout.Space(5);
        
        if (_useMobileLayout)
        {
            DrawMobileHeader();
        }
        else
        {
            DrawDesktopHeader();
        }
        
        GUILayout.EndVertical();
        GUILayout.Space(10);
    }

    private void DrawMobileHeader()
    {
        GUILayout.Space(5);
        
        GUILayout.BeginVertical();
        if (GUILayout.Button(_showStats ? "Stats +" : "Stats", _buttonStyle, GUILayout.Height(ButtonHeight)))
            _showStats = !_showStats;
        if (GUILayout.Button(_showActiveSounds ? "Active Sounds +" : "Active Sounds", _buttonStyle, GUILayout.Height(ButtonHeight)))
            _showActiveSounds = !_showActiveSounds;
        if (GUILayout.Button(_showMusic ? "Music +" : "Music", _buttonStyle, GUILayout.Height(ButtonHeight)))
            _showMusic = !_showMusic;
        if (GUILayout.Button(_showSFX ? "SFX +" : "SFX", _buttonStyle, GUILayout.Height(ButtonHeight)))
            _showSFX = !_showSFX;
        if (GUILayout.Button(_showBuses ? "Buses +" : "Buses", _buttonStyle, GUILayout.Height(ButtonHeight)))
            _showBuses = !_showBuses;
        if (GUILayout.Button(_showQuickActions ? "Actions +" : "Actions", _buttonStyle, GUILayout.Height(ButtonHeight)))
            _showQuickActions = !_showQuickActions;
        GUILayout.EndVertical();
        
        GUILayout.Space(10);
        
        Color originalColor = GUI.backgroundColor;
        GUI.backgroundColor = _forceLoopAllSounds ? Color.yellow : originalColor;
        
        if (GUILayout.Button(_forceLoopAllSounds ? "Force Loop: ON" : "Force Loop: OFF", _buttonStyle, GUILayout.Height(ButtonHeight)))
            _forceLoopAllSounds = !_forceLoopAllSounds;
        
        GUI.backgroundColor = originalColor;
    }

    private void DrawDesktopHeader()
    {
        GUILayout.BeginHorizontal();
        if (GUILayout.Button(_showStats ? "Hide Stats" : "Stats", _buttonStyle, GUILayout.Width(ButtonWidth)))
            _showStats = !_showStats;
        if (GUILayout.Button(_showActiveSounds ? "Hide Active" : "Active", _buttonStyle, GUILayout.Width(ButtonWidth)))
            _showActiveSounds = !_showActiveSounds;
        if (GUILayout.Button(_showMusic ? "Hide Music" : "Music", _buttonStyle, GUILayout.Width(ButtonWidth)))
            _showMusic = !_showMusic;
        if (GUILayout.Button(_showSFX ? "Hide SFX" : "SFX", _buttonStyle, GUILayout.Width(ButtonWidth)))
            _showSFX = !_showSFX;
        if (GUILayout.Button(_showBuses ? "Hide Buses" : "Buses", _buttonStyle, GUILayout.Width(ButtonWidth)))
            _showBuses = !_showBuses;
        if (GUILayout.Button(_showQuickActions ? "Hide Actions" : "Actions", _buttonStyle, GUILayout.Width(ButtonWidth)))
            _showQuickActions = !_showQuickActions;
        GUILayout.EndHorizontal();
        
        GUILayout.Space(5);
        
        Color originalColor = GUI.backgroundColor;
        GUI.backgroundColor = _forceLoopAllSounds ? Color.yellow : originalColor;
        
        _forceLoopAllSounds = GUILayout.Toggle(_forceLoopAllSounds, "Force Loop All Sounds", _toggleStyle);
        
        GUI.backgroundColor = originalColor;
    }
    
    private void DrawStatsSection()
    {
        GUILayout.BeginVertical(_boxStyle);
        GUILayout.Label("Statistics", _subHeaderStyle);
        GUILayout.Space(5);

        GUILayout.Label($"Active SFX: {_audioManager.ActiveSFXCount}", _labelStyle);
        GUILayout.Label($"Pool Available: {_audioManager.AvailablePoolCount}", _labelStyle);
        GUILayout.Label($"Music: {(_audioManager.IsMusicPlaying ? "Playing" : "Stopped")}", _labelStyle);
        
        if (_audioManager.IsMusicPlaying)
        {
            GUILayout.Label($"Track: {_audioManager.CurrentTrack}", _labelStyle);
        }

        GUILayout.EndVertical();
        GUILayout.Space(10);
    }

    private void DrawActiveSoundsSection()
    {
        GUILayout.BeginVertical(_boxStyle);
        GUILayout.Label("Active Sounds", _subHeaderStyle);
        GUILayout.Space(5);

        var allSounds = _audioManager.GetAllActiveSounds();
        
        if (allSounds.Count == 0)
        {
            GUILayout.Label("No sounds playing", _labelStyle);
        }
        else
        {
            allSounds.Sort((a, b) =>
            {
                int priorityCompare = b.Priority.CompareTo(a.Priority);
                if (priorityCompare != 0) return priorityCompare;
                return b.PlayTime.CompareTo(a.PlayTime);
            });

            GUILayout.Label($"Total: {allSounds.Count}", _labelStyle);
            GUILayout.Space(5);

            for (int i = 0; i < allSounds.Count; i++)
            {
                DrawActiveSoundEntry(allSounds[i], i);
            }
        }

        GUILayout.EndVertical();
        GUILayout.Space(10);
    }

    private void DrawActiveSoundEntry(SFXSoundInfo sound, int index)
    {
        GUILayout.BeginVertical(_boxStyle);
        
        GUILayout.BeginHorizontal();
        string header = $"#{index + 1} {sound.ClipName}";
        if (sound.IsLooping) header += " [LOOP]";
        
        GUILayout.Label(header, _highlightStyle);
        GUILayout.FlexibleSpace();
        
        if (GUILayout.Button("Stop", _buttonStyle, GUILayout.Width(ButtonWidth * 0.6f), GUILayout.Height(ButtonHeight * 0.8f)))
            _audioManager.StopSFX(sound.Id);
        
        GUILayout.EndHorizontal();

        GUILayout.Label($"ID: {sound.Id} | Layer: {sound.LayerName}", _labelStyle);
        GUILayout.Label($"Priority: {sound.Priority} | Vol: {sound.CurrentVolume:F2}", _labelStyle);
        GUILayout.Label($"Fade: {sound.FadeMultiplier * 100f:F0}% | Duck: {sound.DuckMultiplier * 100f:F0}%", _labelStyle);

        if (!sound.IsLooping)
        {
            float elapsed = Time.time - sound.PlayTime;
            float remaining = sound.Duration - elapsed;
            
            GUILayout.Label($"{elapsed:F1}s / {sound.Duration:F1}s ({remaining:F1}s left)", _labelStyle);

            float progress = Mathf.Clamp01(elapsed / sound.Duration);
            Rect barRect = GUILayoutUtility.GetRect(0, _progressBarHeight, GUILayout.ExpandWidth(true));
            GUI.Box(barRect, "");
            Rect fillRect = new Rect(barRect.x, barRect.y, barRect.width * progress, barRect.height);
            GUI.Box(fillRect, "", GUI.skin.button);
        }
        else
        {
            float elapsed = Time.time - sound.PlayTime;
            GUILayout.Label($"Playing: {elapsed:F1}s", _labelStyle);
        }

        GUILayout.EndVertical();
    }

    private void DrawMusicSection()
    {
        GUILayout.BeginVertical(_boxStyle);
        GUILayout.Label("Music", _subHeaderStyle);
        GUILayout.Space(5);

        if (_audioManager.MusicPlaylist != null && _audioManager.MusicPlaylist.IsValid)
        {
            GUILayout.Label($"Playlist: {_audioManager.MusicPlaylist.PlaylistName}", _labelStyle);
            GUILayout.Label($"Tracks: {_audioManager.MusicPlaylist.Tracks.Count}", _labelStyle);
            GUILayout.Label($"Shuffle: {_audioManager.MusicPlaylist.Shuffle} | Repeat: {_audioManager.MusicPlaylist.Repeat}", _labelStyle);
            GUILayout.Space(5);

            if (_useMobileLayout)
            {
                if (GUILayout.Button("Play Music", _buttonStyle, GUILayout.Height(ButtonHeight)))
                    _audioManager.PlayMusic();
                if (GUILayout.Button("Stop Music", _buttonStyle, GUILayout.Height(ButtonHeight)))
                    _audioManager.StopMusic();
                if (GUILayout.Button("Next Track", _buttonStyle, GUILayout.Height(ButtonHeight)))
                    _audioManager.PlayNextTrack();
            }
            else
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Play", _buttonStyle, GUILayout.Height(ButtonHeight)))
                    _audioManager.PlayMusic();
                if (GUILayout.Button("Stop", _buttonStyle, GUILayout.Height(ButtonHeight)))
                    _audioManager.StopMusic();
                if (GUILayout.Button("Next", _buttonStyle, GUILayout.Height(ButtonHeight)))
                    _audioManager.PlayNextTrack();
                GUILayout.EndHorizontal();
            }
        }
        else
        {
            GUILayout.Label("No playlist configured", _labelStyle);
        }

        GUILayout.EndVertical();
        GUILayout.Space(10);
    }

    private void DrawSFXSection()
    {
        GUILayout.BeginVertical(_boxStyle);
        GUILayout.Label("SFX", _subHeaderStyle);
        GUILayout.Space(5);

        for (int i = 0; i < _audioManager.SFXLayers.Count; i++)
        {
            var layer = _audioManager.SFXLayers[i];
            if (layer == null || !layer.IsValid) continue;

            if (!_sfxFoldouts.ContainsKey(layer.LayerName))
                _sfxFoldouts[layer.LayerName] = false;

            GUILayout.BeginVertical(_boxStyle);
            
            GUILayout.BeginHorizontal();
            
            if (GUILayout.Button(_sfxFoldouts[layer.LayerName] ? "▼" : "▶", _buttonStyle, 
                GUILayout.Width(_useMobileLayout ? 50 : 30), GUILayout.Height(ButtonHeight * 0.8f)))
            {
                _sfxFoldouts[layer.LayerName] = !_sfxFoldouts[layer.LayerName];
            }
            
            GUILayout.Label($"{layer.LayerName}", _subHeaderStyle);
            GUILayout.FlexibleSpace();
            
            if (GUILayout.Button("Stop All", _buttonStyle, GUILayout.Width(ButtonWidth * 0.8f), GUILayout.Height(ButtonHeight * 0.8f)))
                _audioManager.StopSFXLayer(layer.LayerName);
            GUILayout.EndHorizontal();

            if (_sfxFoldouts[layer.LayerName])
            {
                GUILayout.Space(5);
                
                for (int j = 0; j < layer.Clips.Count; j++)
                {
                    var clip = layer.Clips[j];
                    if (!clip.IsValid) continue;
                    DrawSFXButton(clip);
                }
            }

            GUILayout.EndVertical();
            GUILayout.Space(5);
        }

        GUILayout.EndVertical();
        GUILayout.Space(10);
    }

    private void DrawSFXButton(AudioClipData clip)
    {
        GUILayout.BeginVertical(_boxStyle);
        
        GUILayout.BeginHorizontal();

        bool isPlaying = _audioManager.IsSFXPlaying(clip.Name);
        Color originalColor = GUI.backgroundColor;
        GUI.backgroundColor = isPlaying ? Color.green : originalColor;

        if (GUILayout.Button("▶", _buttonStyle, GUILayout.Width(_useMobileLayout ? 60 : 40), GUILayout.Height(ButtonHeight)))
            _audioManager.PlaySFX(clip.Name, null, loop:_forceLoopAllSounds ? true : (bool?)null);

        GUI.backgroundColor = originalColor;

        string label = clip.Name;
        if (clip.Loop || _forceLoopAllSounds) label += " [L]";
        if (clip.Priority != 0) label += $" P{clip.Priority}";
        
        GUILayout.Label(label, _labelStyle);
        GUILayout.FlexibleSpace();

        if (isPlaying && GUILayout.Button("■", _buttonStyle, GUILayout.Width(_useMobileLayout ? 60 : 40), GUILayout.Height(ButtonHeight)))
            _audioManager.StopSFX(clip.Name);

        GUILayout.EndHorizontal();
        
        if (_useMobileLayout)
        {
            string details = $"Vol: {clip.Volume:F2}";
            if (clip.Clip != null) details += $" | {clip.Clip.length:F1}s";
            if (clip.PitchOffsetMinMax != Vector2.zero) details += $" | Pitch ±{clip.PitchOffsetMinMax.y:F2}";
            GUILayout.Label(details, _labelStyle);
        }
        
        GUILayout.EndVertical();
    }

    private void DrawBusSection()
    {
        GUILayout.BeginVertical(_boxStyle);
        GUILayout.Label("Buses", _subHeaderStyle);
        GUILayout.Space(5);

        for (int i = 0; i < _audioManager.Buses.Count; i++)
        {
            var bus = _audioManager.Buses[i];
            if (bus == null || !bus.IsValid) continue;
            DrawBusControl(bus);
        }

        GUILayout.EndVertical();
        GUILayout.Space(10);
    }

    private void DrawBusControl(AudioBus bus)
    {
        if (!_busFoldouts.ContainsKey(bus.BusName))
            _busFoldouts[bus.BusName] = false;

        GUILayout.BeginVertical(_boxStyle);

        GUILayout.BeginHorizontal();
        
        if (GUILayout.Button(_busFoldouts[bus.BusName] ? "▼" : "▶", _buttonStyle, 
            GUILayout.Width(_useMobileLayout ? 50 : 30), GUILayout.Height(ButtonHeight * 0.8f)))
        {
            _busFoldouts[bus.BusName] = !_busFoldouts[bus.BusName];
        }
        
        Color originalColor = GUI.contentColor;
        GUI.contentColor = bus.HasActiveAudio ? Color.green : Color.gray;
        GUILayout.Label($"{bus.BusName} {(bus.HasActiveAudio ? "[ON]" : "[OFF]")}", _subHeaderStyle);
        GUI.contentColor = originalColor;
        
        GUILayout.EndHorizontal();

        if (_busFoldouts[bus.BusName])
        {
            GUILayout.Space(5);

            GUILayout.Label($"Volume: {bus.FinalVolume:F2} | Duck: {bus.CurrentDuckVolume:P0}", _labelStyle);
            GUILayout.Label($"Muted: {bus.Muted} | Priority: {bus.Priority}", _labelStyle);
            
            if (bus.HasActiveAudio && !string.IsNullOrEmpty(bus.CurrentPlayingSound))
            {
                GUILayout.Label($"Playing: {bus.CurrentPlayingSound}", _labelStyle);
            }
            
            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Vol:", _labelStyle, GUILayout.Width(70));
            
            float newVolume = GUILayout.HorizontalSlider(
                _busVolumeSliders[bus.BusName], 
                0f, 
                1f,
                GUILayout.Height(_sliderHeight)
            );
            
            if (newVolume != _busVolumeSliders[bus.BusName])
            {
                _busVolumeSliders[bus.BusName] = newVolume;
                _audioManager.SetBusVolume(bus.BusName, newVolume);
            }
            
            GUILayout.Label($"{_busVolumeSliders[bus.BusName]:P0}", _labelStyle, GUILayout.Width(60));
            GUILayout.EndHorizontal();

            if (GUILayout.Button(_busMuteToggles[bus.BusName] ? "Mute" : "Unmute", _buttonStyle,
                    GUILayout.Height(ButtonHeight)))
            {
                _busMuteToggles[bus.BusName] = !_busMuteToggles[bus.BusName];
                _audioManager.MuteBus(bus.BusName, _busMuteToggles[bus.BusName]);
            }

            GUILayout.Space(5);

            if (_useMobileLayout)
            {
                if (GUILayout.Button("Fade to 0%", _buttonStyle, GUILayout.Height(ButtonHeight)))
                    _audioManager.FadeBus(bus.BusName, 0f, 2f);
                if (GUILayout.Button("Fade to 50%", _buttonStyle, GUILayout.Height(ButtonHeight)))
                    _audioManager.FadeBus(bus.BusName, 0.5f, 2f);
                if (GUILayout.Button("Fade to 100%", _buttonStyle, GUILayout.Height(ButtonHeight)))
                    _audioManager.FadeBus(bus.BusName, 1f, 2f);
            }
            else
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("0%", _buttonStyle))
                    _audioManager.FadeBus(bus.BusName, 0f, 2f);
                if (GUILayout.Button("50%", _buttonStyle))
                    _audioManager.FadeBus(bus.BusName, 0.5f, 2f);
                if (GUILayout.Button("100%", _buttonStyle))
                    _audioManager.FadeBus(bus.BusName, 1f, 2f);
                GUILayout.EndHorizontal();
            }
        }

        GUILayout.EndVertical();
        GUILayout.Space(5);
    }

    private void DrawQuickActionsSection()
    {
        GUILayout.BeginVertical(_boxStyle);
        GUILayout.Label("Quick Actions", _subHeaderStyle);
        GUILayout.Space(5);

        if (_useMobileLayout)
        {
            if (GUILayout.Button("Stop All SFX", _buttonStyle, GUILayout.Height(ButtonHeight)))
                _audioManager.StopAllSFX();
            if (GUILayout.Button("Mute All Buses", _buttonStyle, GUILayout.Height(ButtonHeight)))
                MuteAllBuses(true);
            if (GUILayout.Button("Unmute All Buses", _buttonStyle, GUILayout.Height(ButtonHeight)))
                MuteAllBuses(false);
        }
        else
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Stop All SFX", _buttonStyle, GUILayout.Height(ButtonHeight)))
                _audioManager.StopAllSFX();
            if (GUILayout.Button("Mute All", _buttonStyle, GUILayout.Height(ButtonHeight)))
                MuteAllBuses(true);
            if (GUILayout.Button("Unmute All", _buttonStyle, GUILayout.Height(ButtonHeight)))
                MuteAllBuses(false);
            GUILayout.EndHorizontal();
        }

        GUILayout.EndVertical();
    }

    private void MuteAllBuses(bool mute)
    {
        for (int i = 0; i < _audioManager.Buses.Count; i++)
        {
            var bus = _audioManager.Buses[i];
            if (bus != null && bus.IsValid)
            {
                _audioManager.MuteBus(bus.BusName, mute);
                _busMuteToggles[bus.BusName] = mute;
            }
        }
    }
}