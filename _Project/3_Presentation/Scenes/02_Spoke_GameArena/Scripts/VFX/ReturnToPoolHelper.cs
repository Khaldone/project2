// Assets/_Project/Scenes/02_Spoke_GameArena/Scripts/VFX/ReturnToPoolHelper.cs
using UnityEngine;


public class ReturnToPoolHelper : MonoBehaviour
{
    private ArenaVFXPoolService _poolService;
    private ParticleSystem _particleSystem;
    private string _effectId; // Requires a way to map this, simplified here


    public void Init(ArenaVFXPoolService pool, ParticleSystem ps)
    {
        _poolService = pool;
        _particleSystem = ps;
    }


    private void OnParticleSystemStopped()
    {
        // When the spark animation finishes, quietly put it back on the shelf
        _poolService.ReturnToPool("spark_clack", _particleSystem);
    }
}
