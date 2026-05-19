// 1. Assets/Scripts/CoreDomain/Telemetry/ITelemetryService.cs
using System;
using System.Collections.Generic;

public interface ITelemetryService
{
    // Sets global context for all future logs (e.g., Player ID, Device Info)
    void SetUserContext(string playerId, string currentScene);


    // Leaves a trail of what the player is doing (e.g., "Hit cue ball with 80% power")
    void LeaveBreadcrumb(string category, string message, Dictionary<string, string> data = null);


    // Explicitly logs a non-fatal error to your dashboard
    void CaptureException(Exception exception);

    // Explicitly logs a warning (e.g., "Network latency spiked above 500ms")
    void CaptureMessage(string message, TelemetryLevel level);
}
public enum TelemetryLevel { Info, Warning, Error }
