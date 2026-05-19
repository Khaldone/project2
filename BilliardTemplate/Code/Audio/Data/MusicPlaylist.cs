using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Audio/Music Playlist")]
public class MusicPlaylist : ScriptableObject
{
    public string PlaylistName;
    public AudioBus Bus;
    public List<AudioTrackData> Tracks = new ();
    public bool Shuffle = false;
    public bool Repeat = true;
    
    public bool IsValid => !string.IsNullOrEmpty(PlaylistName) && Bus != null && Bus.IsValid;
}