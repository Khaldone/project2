using System;
using System.Threading.Tasks;

namespace ibc.network
{
    /// <summary>
    /// Extension methods for Task to resume on Unity main thread.
    /// </summary>
    public static class MainThreadTaskExtensions
    {
        /// <summary>
        /// Await this to guarantee the code after 'await' runs on the Unity main thread.
        /// Usage: await SomeAsync().OnMainThread();
        /// </summary>
        public static MainThreadTaskAwaiter OnMainThread(this Task task)
        {
            return new MainThreadTaskAwaiter(task);
        }

        /// <summary>
        /// Await this to get result and then continue on Unity main thread.
        /// </summary>
        public static MainThreadTaskAwaiter<T> OnMainThread<T>(this Task<T> task)
        {
            return new MainThreadTaskAwaiter<T>(task);
        }

        /// <summary>
        /// Fire-and-forget style helper that ensures continuation runs on main thread,
        /// and logs any exceptions.
        /// </summary>
        public static async void ForgetOnMainThread(this Task task)
        {
            try
            {
                await task.OnMainThread();
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogException(ex);
            }
        }

        /// <summary>
        /// Fire-and-forget style helper for Task.
        /// </summary>
        public static async void ForgetOnMainThread<T>(this Task<T> task)
        {
            try
            {
                await task.OnMainThread();
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogException(ex);
            }
        }
    }
}