using System;
using System.Collections.Generic;
using System.Threading;
using ibc.game;
using UnityEngine;

namespace ibc.network
{
    public class MainThreadDispatcher : Singleton<MainThreadDispatcher>
    {
        public bool IsMainThread => Thread.CurrentThread.ManagedThreadId == _mainThreadId;

        private static MainThreadDispatcher _instance;

        private readonly Queue<Action> _actions = new Queue<Action>();
        private int _mainThreadId;
        
        protected override void AwakeSingleton()
        {
            _mainThreadId = Thread.CurrentThread.ManagedThreadId;
        }

        private void Update()
        {
            lock (_actions)
            {
                while (_actions.Count > 0)
                {
                    var action = _actions.Dequeue();
                    try
                    {
                        action?.Invoke();
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex);
                    }
                }
            }
        }


        public void Enqueue(Action action)
        {
            if (action == null)
                return;

            if (IsMainThread)
            {
                action();
                return;
            }

            lock (_actions)
            {
                _actions.Enqueue(action);
            }
        }
    }
}