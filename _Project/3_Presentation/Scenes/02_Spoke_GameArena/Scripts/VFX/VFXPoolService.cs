////// Assets/_Project/Scenes/02_Spoke_GameArena/Scripts/VFX/VFXPoolService.cs
////using System.Collections;
////using System.Collections.Generic;
////using UnityEngine;


////public class VFXPoolService : MonoBehaviour, IVFXService
////{
////    [SerializeField] private GameObject _sparkPrefab;
////    private Queue<GameObject> _sparkPool = new Queue<GameObject>();

////    public void InitializePool(int count)
////    {
////        for (int i = 0; i < count; i++)
////        {
////            GameObject spark = Instantiate(_sparkPrefab, transform);
////            spark.SetActive(false);
////            _sparkPool.Enqueue(spark);
////        }
////    }

////    // The Physics Engine calls this when balls hit
////    public void PlaySparkAt(Vector3 position)
////    {
////        if (_sparkPool.Count == 0) return; // Fail safe


////        GameObject spark = _sparkPool.Dequeue();
////        spark.transform.position = position;
////        spark.SetActive(true);

////        // Return it to the pool after 1 second
////        StartCoroutine(ReturnToPool(spark, 1f));
////    }

////    private IEnumerator ReturnToPool(GameObject spark, float delay)
////    {
////        yield return new WaitForSeconds(delay);
////        spark.SetActive(false);
////        _sparkPool.Enqueue(spark);
////    }
////}