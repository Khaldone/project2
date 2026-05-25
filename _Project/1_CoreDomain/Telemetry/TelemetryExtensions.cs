// Path: Assets/_Project/1_CoreDomain/Telemetry/TelemetryExtensions.cs
using Billiards.CoreDomain.Telemetry;
using System.Collections.Generic;

public static class TelemetryExtensions
{
    private static ITelemetryService _service;

    public static void Inject(ITelemetryService service) => _service = service;

    public static void LogMatchState(string message, string matchId)
    {
        _service?.AddBreadcrumb(message, "matchmaking", "info",
            new Dictionary<string, string> { { "match_id", matchId } });
    }
}