using UnityEngine;

namespace ibc.game
{
    [DefaultExecutionOrder(order: -1000)]
    public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
    {
        private static T _instance;
        private bool _init;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindAnyObjectByType<T>();
                    if (_instance)
                        _instance.Initialize();
                }

                return _instance;
            }
        }

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this as T;
                Initialize();
            }
            else if (_instance != this)
            {
                Debug.LogWarning($"Multiple instances of {typeof(T)} found");
            }
            else if (!_init)
            {
                Initialize();
            }
        }

        private void Initialize()
        {
            if (!_init)
            {
                AwakeSingleton();
                _init = true;
            }
        }

        protected abstract void AwakeSingleton();
    }
}