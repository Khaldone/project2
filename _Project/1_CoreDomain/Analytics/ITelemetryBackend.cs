// Assets/_Project/CoreDomain/Analytics/ITelemetryBackend.cs
using Cysharp.Threading.Tasks;
using System.Collections.Generic;

namespace Billiards.Core.Analytics
{
    public interface ITelemetryBackend
    {
        UniTask InitializeAsync();
        void LogEvent(string eventName, Dictionary<string, object> parameters);
    }
}