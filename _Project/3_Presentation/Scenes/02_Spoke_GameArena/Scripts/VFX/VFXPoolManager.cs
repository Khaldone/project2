// Assets/_Project/Scenes/02_Spoke_GameArena/Scripts/VFX/VFXPoolManager.cs
using System.Collections.Generic;
using UnityEngine;


public class VFXPoolManager : MonoBehaviour
{
    [Header("Pool Settings")]
    [SerializeField] private GameObject _sparkPrefab;
    [SerializeField] private int _poolSize = 30;


    private Queue<GameObject> _sparkPool = new Queue<GameObject>();


    private void Awake()
    {
        // PRE-WARM THE POOL (Happens during the loading screen)
        for (int i = 0; i < _poolSize; i++)
        {
            GameObject spark = Instantiate(_sparkPrefab, transform);
            spark.SetActive(false); // Keep it hidden
            _sparkPool.Enqueue(spark);
        }
    }


    public void PlaySparkAt(Vector3 position)
    {
        // Fail-safe: If we run out of sparks (massive collision), just ignore it to save performance
        if (_sparkPool.Count == 0) return;


        // 1. Take an inactive spark from the queue
        GameObject spark = _sparkPool.Dequeue();


        // 2. Move it and activate it
        spark.transform.position = position;
        spark.SetActive(true);


        // 3. We must ensure it returns to the pool when it's done!
        // We do this by attaching a helper script to the prefab itself.
        var returnScript = spark.GetComponent<ReturnToPoolAfterDelay>();
        returnScript.StartTimer(this, 1.5f); // Return after 1.5 seconds
    }


    public void ReturnSparkToPool(GameObject spark)
    {
        // Turn it off and put it back in line
        spark.SetActive(false);
        _sparkPool.Enqueue(spark);
    }
}