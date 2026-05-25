// Assets/_Project/1_CoreDomain/Services/PlatformServicesGate.cs
using Cysharp.Threading.Tasks;
using System.Threading;

namespace Billiards.CoreDomain.Services
{
    /// <summary>
    /// One-shot async gate backed by a <see cref="UniTaskCompletionSource"/>.
    /// The producer calls <see cref="MarkReady"/> once; all consumers awaiting
    /// <see cref="WaitUntilReadyAsync"/> are released simultaneously.
    /// Thread-safe via TrySetResult semantics.
    /// </summary>
    public sealed class PlatformServicesGate : IPlatformServicesGate
    {
        private readonly UniTaskCompletionSource _source = new UniTaskCompletionSource();
        private bool _isReady;

        public void MarkReady()
        {
            _isReady = true;
            UnityEngine.Debug.Log("[PlatformGate] MarkReady() — Firebase init complete, releasing consumers.");
            _source.TrySetResult();
        }

        public UniTask WaitUntilReadyAsync(CancellationToken cancellation = default)
        {
            if (_isReady)
            {
                UnityEngine.Debug.Log("[PlatformGate] WaitUntilReadyAsync — already ready, passing through.");
                return UniTask.CompletedTask;
            }
            UnityEngine.Debug.Log("[PlatformGate] WaitUntilReadyAsync — NOT ready, suspending caller...");
            return _source.Task;
        }
    }
}
