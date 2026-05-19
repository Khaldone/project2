// Assets/_Project/3_Presentation/Audio/AudioOrchestrator.cs
using UnityEngine;
using System.Collections.Generic;


public class AudioOrchestrator : MonoBehaviour
{
    [SerializeField] private AudioSource _sourcePrefab;
    [SerializeField] private AudioClip _ballHitBallClip;

    // AAA Tuning Constants
    private const float MAX_VELOCITY_REF = 15.0f; // Speed of a max-power break
    private const float MIN_PITCH = 0.85f;
    private const float MAX_PITCH = 1.15f;


    private void Awake()
    {
        //MessageBroker.Instance.Subscribe<PhysicsCollisionAudioMessage>(OnPhysicsCollision);
    }


    private void OnPhysicsCollision(PhysicsCollisionAudioMessage msg)
    {
        // 1. Calculate Normalized Force (0.0 to 1.0)
        float force = Mathf.Clamp01(msg.ImpactVelocity / MAX_VELOCITY_REF);


        // 2. Logarithmic Volume (Human ears don't hear linearly)
        // This ensures quiet taps are still audible but heavy hits feel powerful.
        float volume = Mathf.Pow(force, 0.5f);


        // 3. Dynamic Pitch (Harder hits = Higher 'cracking' pitch)
        float pitch = Mathf.Lerp(MIN_PITCH, MAX_PITCH, force) + Random.Range(-0.05f, 0.05f);


        Play3DSound(msg.Position, volume, pitch);
    }


    private void Play3DSound(Vector3 worldPos, float vol, float pitch)
    {
        // In a real project, use an Object Pool here to avoid Instantiate overhead!
        AudioSource instance = Instantiate(_sourcePrefab, worldPos, Quaternion.identity);

        instance.clip = _ballHitBallClip;
        instance.volume = vol;
        instance.pitch = pitch;
        instance.spatialBlend = 1.0f; // 100% 3D Spatial Audio

        instance.Play();
        Destroy(instance.gameObject, _ballHitBallClip.length);
    }
}
