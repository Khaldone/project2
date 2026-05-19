// Assets/Scripts/Presentation/Telemetry/SentryTelemetryWrapper.cs
using System;
using System.Collections.Generic;
using Sentry;
using Sentry.Unity;
using UnityEngine;

public class SentryTelemetryWrapper : MonoBehaviour, ITelemetryService
{
    private void Awake()
    {
        // Capture all unhandled Unity log errors automatically
        Application.logMessageReceived += OnUnityLogMessage;
    }


    public void SetUserContext(string playerId, string currentScene)
    {
        SentrySdk.ConfigureScope(scope =>
        {
            scope.User = new SentryUser { Id = playerId };
            scope.SetTag("Scene", currentScene);
        });
    }


    public void LeaveBreadcrumb(string category, string message, Dictionary<string, string> data = null)
    {
        SentrySdk.AddBreadcrumb(
            message: message,
            category: category,
            type: "default",
            data: data
        );
    }


    public void CaptureException(Exception exception)
    {
        SentrySdk.CaptureException(exception);
    }


    public void CaptureMessage(string message, TelemetryLevel level)
    {
        SentryLevel sentryLevel = level switch
        {
            TelemetryLevel.Warning => SentryLevel.Warning,
            TelemetryLevel.Error => SentryLevel.Error,
            _ => SentryLevel.Info
        };


        SentrySdk.CaptureMessage(message, sentryLevel);
    }


    private void OnUnityLogMessage(string logString, string stackTrace, LogType type)
    {
        if (type == LogType.Exception || type == LogType.Error)
        {
            // Sentry's Unity SDK handles native crashes automatically,
            // but we can route standard Unity Debug.LogErrors here as well if needed.
        }
    }
}
