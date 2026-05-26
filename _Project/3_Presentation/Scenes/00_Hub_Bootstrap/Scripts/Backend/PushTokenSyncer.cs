using System;
using System.Collections.Generic;
using Billiards.CoreDomain.Notifications;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;

namespace Billiards.Infrastructure.Backend
{
    /// <summary>
    /// Syncs the device's FCM/APNs push token to PlayFab Player Data so that
    /// server-side CloudScript / Azure Functions can target this device.
    /// </summary>
    public sealed class PushTokenSyncer : IDisposable
    {
        private readonly IPushNotificationService _pushService;
        private readonly IMessageBroker _messageBroker;
        private string _lastSyncedToken = string.Empty;

        public PushTokenSyncer(IPushNotificationService pushService, IMessageBroker messageBroker)
        {
            _pushService = pushService;
            _messageBroker = messageBroker;

            _messageBroker.Subscribe<UserAuthenticatedMessage>(OnUserSessionAuthenticated);
            _pushService.OnTokenUpdated += UploadTokenToServerProfile;
        }

        private void OnUserSessionAuthenticated(UserAuthenticatedMessage message)
        {
            string currentToken = _pushService.GetDeviceToken();
            if (!string.IsNullOrEmpty(currentToken))
            {
                UploadTokenToServerProfile(currentToken);
            }
        }

        private void UploadTokenToServerProfile(string token)
        {
            if (string.IsNullOrEmpty(token)) return;

            // Dedup: skip if we already synced this exact token this session
            if (string.Equals(_lastSyncedToken, token, StringComparison.Ordinal)) return;

            if (!PlayFabClientAPI.IsClientLoggedIn()) return;

            // Platform-specific key so CloudScript knows which service to target
#if UNITY_ANDROID
            string platformKey = "FCMToken_Android";
#elif UNITY_IOS
            string platformKey = "APNsToken_iOS";
#else
            string platformKey = "PushToken_Editor";
#endif

            var request = new UpdateUserDataRequest
            {
                Data = new Dictionary<string, string>
                {
                    { platformKey, token }
                },
                // Public so server CloudScript / Azure Functions can read it
                Permission = UserDataPermission.Private
            };

            PlayFabClientAPI.UpdateUserData(request,
                result =>
                {
                    _lastSyncedToken = token;
                    Debug.Log($"[PushTokenSyncer] Token synced to PlayFab under '{platformKey}' ({token.Length} chars).");
                },
                error =>
                {
                    Debug.LogError($"[PushTokenSyncer] Failed to sync token: {error.GenerateErrorReport()}");
                });
        }

        public void Dispose()
        {
            _messageBroker.Unsubscribe<UserAuthenticatedMessage>(OnUserSessionAuthenticated);
            _pushService.OnTokenUpdated -= UploadTokenToServerProfile;
        }
    }
}