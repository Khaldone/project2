using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace ibc.network
{
    /// <summary>
    /// Generic awaiter for Task that resumes on Unity's main thread.
    /// </summary>
    public struct MainThreadTaskAwaiter<T> : INotifyCompletion
    {
        private readonly Task<T> _task;

        public MainThreadTaskAwaiter(Task<T> task)
        {
            _task = task ?? throw new ArgumentNullException(nameof(task));
        }
        public MainThreadTaskAwaiter<T> GetAwaiter() => this;

        public bool IsCompleted =>
            _task.IsCompleted && MainThreadDispatcher.Instance.IsMainThread;

        public void OnCompleted(Action continuation)
        {
            if (continuation == null)
                throw new ArgumentNullException(nameof(continuation));

            _task.GetAwaiter().OnCompleted(() =>
            {
                if (MainThreadDispatcher.Instance.IsMainThread)
                {
                    continuation();
                }
                else
                {
                    MainThreadDispatcher.Instance.Enqueue(continuation);
                }
            });
        }

        public T GetResult()
        {
            return _task.GetAwaiter().GetResult();
        }
    }

    /// <summary>
    /// Awaiter that ensures the continuation after 'await' runs on Unity's main thread.
    /// </summary>
    public struct MainThreadTaskAwaiter : INotifyCompletion
    {
        private readonly Task _task;

        public MainThreadTaskAwaiter(Task task)
        {
            _task = task ?? throw new ArgumentNullException(nameof(task));
        }

        public MainThreadTaskAwaiter GetAwaiter() => this;

        /// <summary>
        /// If the task is already completed AND we're on main thread, we can run synchronously.
        /// </summary>
        public bool IsCompleted =>
            _task.IsCompleted && MainThreadDispatcher.Instance.IsMainThread;

        public void OnCompleted(Action continuation)
        {
            if (continuation == null)
                throw new ArgumentNullException(nameof(continuation));

            // When the underlying task completes, forward the continuation to the main thread.
            _task.GetAwaiter().OnCompleted(() =>
            {
                if (MainThreadDispatcher.Instance.IsMainThread)
                {
                    continuation();
                }
                else
                {
                    MainThreadDispatcher.Instance.Enqueue(continuation);
                }
            });
        }

        public void GetResult()
        {
            _task.GetAwaiter().GetResult();
        }
    }
}