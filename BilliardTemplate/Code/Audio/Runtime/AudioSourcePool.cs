using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

[Serializable]
public class AudioSourcePool
{
    private readonly Queue<AudioSource> _pool;
    private readonly Transform _parent;
    private readonly int _maxSize;

    public int AvailableCount => _pool.Count;

    public AudioSourcePool(int initialSize, Transform parent)
    {
        _pool = new Queue<AudioSource>(initialSize);
        _parent = parent;
        _maxSize = initialSize * 2;

        for (int i = 0; i < initialSize; i++)
        {
            _pool.Enqueue(CreateAudioSource($"Pooled_AudioSource_{i}"));
        }
    }

    public AudioSource Get()
    {
        if (_pool.Count > 0)
        {
            var source = _pool.Dequeue();
            source.gameObject.SetActive(true);
            return source;
        }
        return CreateAudioSource("Dynamic_AudioSource");
    }

    public void Return(AudioSource source)
    {
        if (source == null) return;

        source.Stop();
        source.clip = null;
        source.loop = false;
        source.volume = 1f;
        source.pitch = 1f;
        source.gameObject.SetActive(false);

        if (_pool.Count < _maxSize)
            _pool.Enqueue(source);
        else
            Object.Destroy(source.gameObject);
    }

    private AudioSource CreateAudioSource(string name)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(_parent);
        var source = obj.AddComponent<AudioSource>();
        source.playOnAwake = false;
        obj.SetActive(false);
        return source;
    }
}