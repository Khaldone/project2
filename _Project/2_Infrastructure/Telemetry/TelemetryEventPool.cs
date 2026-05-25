// Assets/_Project/2_Infrastructure/Telemetry/Serialization/TelemetryEventPool.cs
using System.Collections.Generic;
using UnityEngine.Pool;

namespace Billiards.Infrastructure.Telemetry.Serialization
{
    public static class TelemetryEventPool
    {
        private static readonly ObjectPool<Dictionary<string, object>> _pool =
            new ObjectPool<Dictionary<string, object>>(
                createFunc: () => new Dictionary<string, object>(16),
                actionOnGet: dict => dict.Clear(),
                actionOnRelease: dict => dict.Clear(),
                defaultCapacity: 10
            );

        public static Dictionary<string, object> Get() => _pool.Get();
        public static void Release(Dictionary<string, object> dict) => _pool.Release(dict);
    }
}