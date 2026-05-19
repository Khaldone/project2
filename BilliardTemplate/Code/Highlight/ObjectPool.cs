using System.Collections.Generic;
using UnityEngine;

namespace ibc.highlight
{
    public class ObjectPool : MonoBehaviour
    {
        public List<GameObject> PooledObjects;
        public GameObject ObjectToPool;
        public int InitialPoolSize = 8;

        void Awake()
        {
            PooledObjects = new List<GameObject>();
            GameObject tmp;
            for(int i = 0; i < InitialPoolSize; i++)
            {
                CreatePooledObject();
            }
        }


        private GameObject CreatePooledObject()
        {
            GameObject tmp;
            tmp = Instantiate(ObjectToPool, ObjectToPool.transform.position, ObjectToPool.transform.rotation);
            tmp.transform.localScale = ObjectToPool.transform.localScale;
            tmp.SetActive(false);
            tmp.transform.SetParent(transform);
            PooledObjects.Add(tmp);
            return tmp;
        }

        public GameObject GetPooledObject()
        {
            for(int i = 0; i < InitialPoolSize; i++)
            {
                if(!PooledObjects[i].activeInHierarchy)
                {
                    return PooledObjects[i];
                }
            }
            
            return CreatePooledObject();
        }
    }
}