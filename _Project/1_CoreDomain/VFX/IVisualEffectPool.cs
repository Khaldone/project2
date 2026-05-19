// Assets/_Project/CoreDomain/VFX/IVisualEffectPool.cs
using UnityEngine; // Allowed for Vector3

public interface IVisualEffectPool
{
    // Requests an effect from the pool, places it, and plays it
    void PlayEffect(string effectId, Vector3 position);
}