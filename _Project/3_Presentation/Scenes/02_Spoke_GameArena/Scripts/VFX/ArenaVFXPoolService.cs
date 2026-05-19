// Assets/_Project/Scenes/02_Spoke_GameArena/Scripts/VFX/ArenaVFXPoolService.cs
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using VContainer;
using VContainer.Unity;


public class ArenaVFXPoolService : MonoBehaviour, IVisualEffectPool
{
    [System.Serializable]
    public class PoolSetup
    {
        public string EffectId;
        public ParticleSystem Prefab;
        public int PrewarmCount = 10;
    }


    [SerializeField] private List<PoolSetup> _poolSetups;

    // VContainer's object resolver allows us to inject dependencies into spawned prefabs if needed
    private IObjectResolver _resolver;

    // A dictionary holding a distinct ObjectPool for each type of effect
    private Dictionary<string, IObjectPool<ParticleSystem>> _pools = new Dictionary<string, IObjectPool<ParticleSystem>>();


    [Inject]
    public void Construct(IObjectResolver resolver)
    {
        _resolver = resolver;
    }


    private void Awake()
    {
        // Initialize the pools based on our Inspector setup
        foreach (var setup in _poolSetups)
        {
            var pool = new ObjectPool<ParticleSystem>(
                createFunc: () => InstantiateEffect(setup.Prefab),
                actionOnGet: (ps) => ps.gameObject.SetActive(true),
                actionOnRelease: (ps) => ps.gameObject.SetActive(false),
                actionOnDestroy: (ps) => Destroy(ps.gameObject),
                collectionCheck: false,
                defaultCapacity: setup.PrewarmCount,
                maxSize: 50
            );


            _pools.Add(setup.EffectId, pool);


            // Pre-warm the pool to prevent stuttering on the first collision
            var prewarmedItems = new List<ParticleSystem>();
            for (int i = 0; i < setup.PrewarmCount; i++) prewarmedItems.Add(pool.Get());
            foreach (var item in prewarmedItems) pool.Release(item);
        }
    }


    private ParticleSystem InstantiateEffect(ParticleSystem prefab)
    {
        // We use VContainer to instantiate so the prefab can use [Inject] if it needs to!
        ParticleSystem instance = _resolver.Instantiate(prefab, transform);

        // Attach a helper script to auto-return the particle to the pool when it finishes playing
        var returnHelper = instance.gameObject.AddComponent<ReturnToPoolHelper>();
        returnHelper.Init(this, instance);

        return instance;
    }


    public void PlayEffect(string effectId, Vector3 position)
    {
        if (_pools.TryGetValue(effectId, out var pool))
        {
            ParticleSystem ps = pool.Get();
            ps.transform.position = position;
            ps.Play();
        }
        else
        {
            Debug.LogWarning($"AAA Pipeline: VFX Pool missed request for {effectId}");
        }
    }


    // Called by the helper script when the particle dies
    public void ReturnToPool(string effectId, ParticleSystem ps)
    {
        if (_pools.TryGetValue(effectId, out var pool))
        {
            pool.Release(ps);
        }
    }
}
