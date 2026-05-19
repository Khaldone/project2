using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class AudioOrchestrator : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private AudioSource _sourcePrefab;
    [SerializeField] private AudioClip _ballHitBallClip;

    // The Pool Instance
    private AudioSourcePool _audioPool;
    private List<PhysicsCollisionAudioMessage> _frameCollisions = new List<PhysicsCollisionAudioMessage>();


    private const float MAX_VELOCITY_REF = 15.0f;
    private const float MIN_PITCH = 0.85f;
    private const float MAX_PITCH = 1.15f;


    private void Awake()
    {
        // Pre-warm 10 audio sources on startup.
        // 10 is usually the maximum hardware voices an old mobile phone can handle anyway.
        _audioPool = new AudioSourcePool(_sourcePrefab, this.transform, initialSize: 10);

        //MessageBroker.Instance.Subscribe<PhysicsCollisionAudioMessage>(OnPhysicsCollision);
    }


    private void OnPhysicsCollision(PhysicsCollisionAudioMessage msg)
    {
        _frameCollisions.Add(msg);
    }


    private void LateUpdate()
    {
        if (_frameCollisions.Count == 0) return;


        _frameCollisions.Sort((a, b) => b.ImpactVelocity.CompareTo(a.ImpactVelocity));
        int soundsToPlay = Mathf.Min(_frameCollisions.Count, 3);


        for (int i = 0; i < soundsToPlay; i++)
        {
            var msg = _frameCollisions[i];
            float force = Mathf.Clamp01(msg.ImpactVelocity / MAX_VELOCITY_REF);
            float attenuation = i == 0 ? 1.0f : (1.0f / (i + 1));

            float volume = Mathf.Pow(force, 0.5f) * attenuation;
            float pitch = Mathf.Lerp(MIN_PITCH, MAX_PITCH, force) + Random.Range(-0.05f, 0.05f);


            Play3DSound(msg.Position, volume, pitch);
        }


        _frameCollisions.Clear();
    }


    private void Play3DSound(Vector3 worldPos, float vol, float pitch)
    {
        // 1. Ask the pool for a ready-to-use AudioSource (NO INSTANTIATION HERE!)
        AudioSource instance = _audioPool.GetSource();

        instance.transform.position = worldPos;
        instance.clip = _ballHitBallClip;
        instance.volume = vol;
        instance.pitch = pitch;
        instance.spatialBlend = 1.0f;

        instance.Play();

        // 2. Start a countdown timer to recycle it (NO DESTROY HERE!)
        StartCoroutine(RecycleSourceAfterDelay(instance, _ballHitBallClip.length));
    }


    private IEnumerator RecycleSourceAfterDelay(AudioSource source, float delay)
    {
        // Wait for the exact length of the audio clip
        yield return new WaitForSeconds(delay);

        // 3. Hand it back to the pool to be used for the next collision
        _audioPool.ReturnSource(source);
    }

    private void Play2DSound(Vector2 tablePosition, float vol, float pitch)
    {
        AudioSource instance = _audioPool.GetSource();

        // Map the X position of the table (-1.0 left to 1.0 right) directly to the audio pan
        float pan = Mathf.Clamp(tablePosition.x, -1.0f, 1.0f);

        instance.clip = _ballHitBallClip;
        instance.volume = vol;
        instance.pitch = pitch;
        instance.spatialBlend = 0.0f; // Force strict 2D audio
        instance.panStereo = pan;     // Hard pan left or right based on collision location

        instance.Play();
        StartCoroutine(RecycleSourceAfterDelay(instance, _ballHitBallClip.length));

        // Does this function belong to the other audio orchestrator?
    }

}
