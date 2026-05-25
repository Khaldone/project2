// Assets/_Project/3_Presentation/Scene_Login/Scripts/LoginEntryPoint.cs
using Billiards.CoreDomain.Services;
using Billiards.Infrastructure.Monetization;
using Billiards.Presentation.Login;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer.Unity;

namespace Billiards.Presentation
{
    public class LoginEntryPoint : IAsyncStartable
    {
        private const string LAST_LOGIN_METHOD_KEY = "LastLoginMethod";
        
        private readonly IAuthenticationService_New _authService;
        private readonly PlayerSession _playerSession;
        private readonly IPlayerDataService _playerDataService;
        private readonly IEnumerable<INativeAuthService> _nativeAuthServices;
        private readonly ILocalSaveService _localSaveService;
        private readonly IPlatformServicesGate _platformGate;

        private INotificationQueue _notificationQueue;

        public LoginEntryPoint(
            IAuthenticationService_New authService,
            PlayerSession playerSession,
            IPlayerDataService playerDataService,
            IEnumerable<INativeAuthService> nativeAuthServices,
            ILocalSaveService localSaveService,
            IPlatformServicesGate platformGate)
        {
            _authService = authService;
            _playerSession = playerSession;
            _playerDataService = playerDataService;
            _nativeAuthServices = nativeAuthServices;
            _localSaveService = localSaveService;
            _platformGate = platformGate;
        }

        private async UniTask TryResolveQueueAsync(CancellationToken token)
        {
            if (_notificationQueue != null) return;

            while (_notificationQueue == null && !token.IsCancellationRequested)
            {
                var scopes = Object.FindObjectsByType<VContainer.Unity.LifetimeScope>(FindObjectsSortMode.None);
                foreach (var scope in scopes)
                {
                    if (scope.Container == null) continue;
                    try
                    {
                        _notificationQueue = (INotificationQueue)scope.Container.Resolve(typeof(INotificationQueue));
                        if (_notificationQueue != null) break;
                    }
                    catch { /* Ignore */ }
                }

                if (_notificationQueue != null) break;
                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }
        }

        private async UniTask EnqueueStatusOverlayAsync(string title, string message, CancellationToken token)
        {
            await TryResolveQueueAsync(token);
            if (_notificationQueue == null) return;

            _notificationQueue.Enqueue(new NotificationData
            {
                Type = enNotificationType.SystemWarning,
                Classification = NotificationClassification.Info,
                Layout = NotificationLayout.StatusOverlay,
                SlideIn = NotificationSlideDirection.Immediate,
                SlideOut = NotificationSlideDirection.Immediate,
                Title = title,
                Message = message,
                DisplayDurationSeconds = 0 // Stays open indefinitely until DismissActive() is called
            });
        }

        public async UniTask StartAsync(CancellationToken cancellation)
        {
            // Await the platform services gate to ensure Firebase (which runs GMS resolution)
            // has finished initializing and released any native GMS locks.
            await _platformGate.WaitUntilReadyAsync(cancellation);

            var nativeAuth = System.Linq.Enumerable.FirstOrDefault(_nativeAuthServices);

            if (nativeAuth != null)
            {
                await EnqueueStatusOverlayAsync("Authenticating", "Connecting to server...", cancellation);
                try
                {
                    string silentToken = await nativeAuth.TrySilentAuthenticateAsync();
                    if (!string.IsNullOrEmpty(silentToken))
                    {
                        Debug.Log($"[LoginEntryPoint] Silent native auth succeeded. Logging in with {nativeAuth.PlatformName}...");
                        bool success = await _authService.AuthenticateWithNativeTokenAsync(nativeAuth.PlatformName, silentToken);
                        if (success)
                        {
                            await SaveLoginMethod("Native");
                            _notificationQueue?.UpdateActiveClassification(NotificationClassification.Success);
                            _notificationQueue?.UpdateActiveMessage("Login Successful", "Loading Profile...");
                            await _playerDataService.InitializeAsync();
                            await UniTask.Delay(1500, cancellationToken: cancellation);
                            _notificationQueue?.DismissActive();
                            await SceneManager.LoadSceneAsync("Scene_MainMenu_Bootstrap", LoadSceneMode.Additive);
                            return;
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"[LoginEntryPoint] Silent native auth exception (non-fatal): {ex.Message}");
                }
                _notificationQueue?.DismissActive();
            }

            string lastLoginMethod = await LoadLoginMethod();
            if (lastLoginMethod == "Native")
            {
                await EnqueueStatusOverlayAsync("Authenticating", "Connecting to server...", cancellation);
                bool success = await _authService.AutoLoginAsync();
                if (success)
                {
                    _notificationQueue?.UpdateActiveClassification(NotificationClassification.Success);
                    _notificationQueue?.UpdateActiveMessage("Login Successful", "Loading Profile...");
                    await _playerDataService.InitializeAsync();
                    await UniTask.Delay(1500, cancellationToken: cancellation);
                    _notificationQueue?.DismissActive();
                    await SceneManager.LoadSceneAsync("Scene_MainMenu_Bootstrap", LoadSceneMode.Additive);
                    return;
                }
                _notificationQueue?.DismissActive();
            }

            // Manual login
            await UniTask.Yield();
        }

        private async UniTask SaveLoginMethod(string method)
        {
            await _localSaveService.SaveDataAsync(LAST_LOGIN_METHOD_KEY, method);
        }

        private async UniTask<string> LoadLoginMethod()
        {
            return await _localSaveService.LoadDataAsync<string>(LAST_LOGIN_METHOD_KEY, string.Empty);
        }

        public async UniTask TryLoginSequenceAsync()
        {
            await EnqueueStatusOverlayAsync("Authenticating", "Logging in as Guest...", default);

            bool success = await _authService.AuthenticateAsync();

            if (success)
            {
                await SaveLoginMethod("Guest");

                _notificationQueue?.UpdateActiveClassification(NotificationClassification.Success);
                _notificationQueue?.UpdateActiveMessage("Login Successful", "Loading Profile...");
                
                await _playerDataService.InitializeAsync();

                await UniTask.Delay(1500);
                _notificationQueue?.DismissActive();
                await SceneManager.LoadSceneAsync("Scene_MainMenu_Bootstrap", LoadSceneMode.Additive);
            }
            else
            {
                _notificationQueue?.UpdateActiveClassification(NotificationClassification.Error);
                _notificationQueue?.UpdateActiveMessage("Connection Failed", "Try again.");
                await UniTask.Delay(2500);
                _notificationQueue?.DismissActive();
            }
        }

        public async UniTask TryNativeLoginSequenceAsync()
        {
            var authService = System.Linq.Enumerable.FirstOrDefault(_nativeAuthServices);
            if (authService == null)
            {
                await EnqueueStatusOverlayAsync("Error", "Native Auth Not Supported.", default);
                _notificationQueue?.UpdateActiveClassification(NotificationClassification.Error);
                await UniTask.Delay(2500);
                _notificationQueue?.DismissActive();
                return;
            }

            string platformName = authService.PlatformName;
            await EnqueueStatusOverlayAsync("Authenticating", $"Logging in with {platformName}...", default);

            try
            {
                string token = await authService.AuthenticateAsync();
                Debug.Log($"[LoginEntryPoint] Received native token for {platformName}. Forwarding to CBS...");

                bool success = await _authService.AuthenticateWithNativeTokenAsync(platformName, token);

                if (success)
                {
                    await SaveLoginMethod("Native");

                    _notificationQueue?.UpdateActiveClassification(NotificationClassification.Success);
                    _notificationQueue?.UpdateActiveMessage("Login Successful", "Loading Profile...");
                    await _playerDataService.InitializeAsync();

                    await UniTask.Delay(1500);
                    _notificationQueue?.DismissActive();
                    await SceneManager.LoadSceneAsync("Scene_MainMenu_Bootstrap", LoadSceneMode.Additive);
                }
                else
                {
                    _notificationQueue?.UpdateActiveClassification(NotificationClassification.Error);
                    _notificationQueue?.UpdateActiveMessage("Login Failed", "Native Login Failed.");
                    await UniTask.Delay(2500);
                    _notificationQueue?.DismissActive();
                }
            }
            catch (System.Exception ex)
            {
                if (_notificationQueue == null) await EnqueueStatusOverlayAsync("Error", "Auth Cancelled or Failed.", default);
                _notificationQueue?.UpdateActiveClassification(NotificationClassification.Error);
                _notificationQueue?.UpdateActiveMessage("Error", "Auth Cancelled or Failed.");
                Debug.LogError($"[LoginEntryPoint] Native auth exception: {ex.Message}");
                await UniTask.Delay(2500);
                _notificationQueue?.DismissActive();
            }
        }
    }
}