// Assets/_Project/1_CoreDomain/Services/IPlatformServicesGate.cs
using Cysharp.Threading.Tasks;
using System.Threading;

namespace Billiards.CoreDomain.Services
{
    /// <summary>
    /// Coordination gate that blocks consumers (e.g., login flow) until
    /// platform-level services (Firebase) have finished initializing
    /// and released shared OS resources (Google Play Services client).
    /// </summary>
    public interface IPlatformServicesGate
    {
        /// <summary>
        /// Signal that all platform services are ready.
        /// Called once by the analytics/telemetry bootstrapper after backends init.
        /// </summary>
        void MarkReady();

        /// <summary>
        /// Await this before using any service that contends for the
        /// same OS-level resource (e.g., Google Play Games on Android).
        /// Returns immediately if already marked ready.
        /// </summary>
        UniTask WaitUntilReadyAsync(CancellationToken cancellation = default);
    }
}
