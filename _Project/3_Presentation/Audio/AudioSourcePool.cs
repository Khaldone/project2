// Assets/_Project/3_Presentation/Audio/AudioSourcePool.cs
using System.Collections.Generic;
using UnityEngine;

public class AudioSourcePool
{
    private readonly AudioSource _prefab;
    private readonly Transform _parent;
    private readonly Queue<AudioSource> _pool = new Queue<AudioSource>();


    public AudioSourcePool(AudioSource prefab, Transform parent, int initialSize)
    {
        _prefab = prefab;
        _parent = parent;


        // "Pre-warm" the pool. We take the performance hit NOW, during the loading screen.
        for (int i = 0; i < initialSize; i++)
        {
            CreateNewSource();
        }
    }


    private void CreateNewSource()
    {
        AudioSource newSource = Object.Instantiate(_prefab, _parent);
        newSource.gameObject.SetActive(false); // Hide it immediately
        _pool.Enqueue(newSource);
    }


    public AudioSource GetSource()
    {
        // Safety Net: If the break shot is so massive that we run out of our initial 10 sounds,
        // we dynamically expand the pool rather than failing to play the sound.
        if (_pool.Count == 0)
        {
            CreateNewSource();
            Debug.LogWarning("AudioPool exhausted! Expanding pool size automatically.");
        }


        // Pull the oldest available source from the front of the queue
        AudioSource source = _pool.Dequeue();
        source.gameObject.SetActive(true); // Wake it up
        return source;
    }


    public void ReturnSource(AudioSource source)
    {
        // Reset the source and put it back at the end of the line
        source.Stop();
        source.clip = null;
        source.gameObject.SetActive(false);
        _pool.Enqueue(source);
    }
}