using System;
using UnityEngine;
#if UNITY_ANDROID && !UNITY_EDITOR
using UnityEngine.Android;
#endif
using Firebase.Messaging;
using Billiards.CoreDomain.Notifications;
using Cysharp.Threading.Tasks;
using VContainer.Unity;

namespace Billiards.Infrastructure.Notifications
{
    public sealed class FirebasePushWrapper : IPushNotificationService, IInitializable, IDisposable
    {
        private readonly IMessageBroker _messageBroker;
        private string _currentToken = string.Empty;
        private bool _isReady;

        public event Action<string> OnTokenUpdated;
        public bool IsReady => _isReady;

        public FirebasePushWrapper(IMessageBroker messageBroker)
        {
            _messageBroker = messageBroker;
        }

        public void Initialize()
        {
            InitializeAsync().Forget();
        }

        public async UniTask InitializeAsync()
        {
            if (_isReady) return;

            // Wait for MobileMonetizationPro's initialization sequence to complete dependencies safely
            int maxTimeoutFrames = 300;
            while (Firebase.FirebaseApp.DefaultInstance == null && maxTimeoutFrames > 0)
            {
                await UniTask.Yield(PlayerLoopTiming.Update);
                maxTimeoutFrames--;
            }

            if (Firebase.FirebaseApp.DefaultInstance == null)
            {
                Debug.LogError("[PushACL] Firebase App context initialization timed out.");
                return;
            }

            try
            {
                // Attach directly to global Firebase static events safely inside the Infrastructure boundary
                FirebaseMessaging.MessageReceived += OnFirebaseMessageReceived;
                FirebaseMessaging.TokenReceived += OnFirebaseTokenReceived;

#if UNITY_ANDROID && !UNITY_EDITOR
                if (!Permission.HasUserAuthorizedPermission("android.permission.POST_NOTIFICATIONS"))
                {
                    Debug.Log("[PushACL] android.permission.POST_NOTIFICATIONS not authorized. Requesting...");
                    Permission.RequestUserPermission("android.permission.POST_NOTIFICATIONS");
                }
                else
                {
                    Debug.Log("[PushACL] android.permission.POST_NOTIFICATIONS already authorized.");
                }
#endif

                // Enable FCM token registration since it was suppressed/disabled in AndroidManifest.xml
                FirebaseMessaging.TokenRegistrationOnInitEnabled = true;

                // Request permission which also triggers token delivery via OnFirebaseTokenReceived
                await FirebaseMessaging.RequestPermissionAsync().AsUniTask();
                _isReady = true;

                if (!string.IsNullOrEmpty(_currentToken))
                {
                    OnTokenUpdated?.Invoke(_currentToken);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PushACL] Critical native initialization binding failure: {ex.Message}");
            }
        }

        public string GetDeviceToken()
        {
            return _currentToken;
        }

        private void OnFirebaseMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            if (e?.Message == null) return;

            string title = string.Empty;
            string body = string.Empty;

            if (e.Message.Notification != null)
            {
                title = e.Message.Notification.Title;
                body = e.Message.Notification.Body;
            }

            // Translate the native payload to a pure domain object and publish it cleanly over your architecture's broker
            var domainMessage = new RemotePushMessage(title, body, e.Message.Data);
            _messageBroker.Publish(domainMessage);
        }

        private void OnFirebaseTokenReceived(object sender, TokenReceivedEventArgs e)
        {
            if (e == null || string.IsNullOrEmpty(e.Token)) return;
            if (string.Equals(_currentToken, e.Token, StringComparison.Ordinal)) return;

            _currentToken = e.Token;
            OnTokenUpdated?.Invoke(_currentToken);
        }

        public void Dispose()
        {
            // Robust cleanup avoids persistent lifecycle reference leaks across scene loads
            FirebaseMessaging.MessageReceived -= OnFirebaseMessageReceived;
            FirebaseMessaging.TokenReceived -= OnFirebaseTokenReceived;
        }
    }
}