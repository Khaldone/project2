using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Fusion;
using Fusion.Sockets;
using UnityEngine;

namespace ibc.network
{
    public class FusionConnectInfo
    {
        
    }

    public class FusionDisconnectInfo
    {
        
    }
    
    public sealed class FusionClient : MonoBehaviour, INetworkRunnerCallbacks
    {
        public bool IsRunning => _runner != null && _runner.IsRunning;

        public event Action<FusionDisconnectInfo> Disconnected;
        public event Action<FusionConnectInfo> Connected;

        /// <summary>Fires whenever the cached session list changes (after lobby joined).</summary>
        public event Action<IReadOnlyList<SessionInfo>> SessionListChanged;

        [Header("Runner")]
        [SerializeField] private NetworkRunner _hostPrefab;
        
        [Header("Lobby")]
        [SerializeField] private SessionLobby _defaultLobby = SessionLobby.Shared;

        private NetworkRunner _runner;

        // Serialize lifecycle operations (Start/Stop/Lobby).
        private readonly SemaphoreSlim _gate = new SemaphoreSlim(1, 1);

        private Task _currentStartTask;
        private Task _currentStopTask;
        private Task _currentLobbyTask;

        private bool _callbacksAdded;
        private bool _inLobby;

        // Session cache (rooms).
        private readonly Dictionary<string, SessionInfo> _sessionsByName = new Dictionary<string, SessionInfo>();
        private int _sessionListVersion;

        // Awaiters
        private TaskCompletionSource<bool> _tcsConnected;
        private TaskCompletionSource<bool> _tcsShutdown;
        private TaskCompletionSource<bool> _tcsFirstSessionList;

        private void Awake()
        {
            EnsureRunnerInstance();
            EnsureRunnerConfigured();
        }

        private void OnDestroy()
        {
            if (_runner != null && _callbacksAdded)
            {
                _runner.RemoveCallbacks(this);
                _callbacksAdded = false;
            }
        }

        // =========================
        // Public API
        // =========================

        public async Task StartAsync(StartGameArgs args, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            if (IsRunning)
                return;

            await _gate.WaitAsync(ct).ConfigureAwait(false);
            try
            {
                if (IsRunning)
                    return;

                if (_currentStartTask != null && !_currentStartTask.IsCompleted)
                {
                    await _currentStartTask.ConfigureAwait(false);
                    return;
                }

                _currentStartTask = StartInternalAsync(args, ct);
                await _currentStartTask.ConfigureAwait(false);
            }
            finally
            {
                _gate.Release();
            }
        }

        public async Task StopAsync(CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            if (!IsRunning && _runner == null)
                return;

            await _gate.WaitAsync(ct).ConfigureAwait(false);
            try
            {
                if (_currentStopTask != null && !_currentStopTask.IsCompleted)
                {
                    await _currentStopTask.ConfigureAwait(false);
                    return;
                }

                _currentStopTask = StopInternalAsync(ct);
                await _currentStopTask.ConfigureAwait(false);
            }
            finally
            {
                _gate.Release();
            }
        }

        /// <summary>
        /// Join a lobby so we can receive <see cref="OnSessionListUpdated"/> and query rooms.
        /// Safe to call multiple times.
        /// </summary>
        public async Task JoinLobbyAsync(SessionLobby lobby, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            EnsureRunnerInstance();
            EnsureRunnerConfigured();

            await _gate.WaitAsync(ct).ConfigureAwait(false);
            try
            {
                if (_currentLobbyTask != null && !_currentLobbyTask.IsCompleted)
                {
                    await _currentLobbyTask.ConfigureAwait(false);
                    return;
                }

                _currentLobbyTask = JoinLobbyInternalAsync(lobby, ct);
                await _currentLobbyTask.ConfigureAwait(false);
            }
            finally
            {
                _gate.Release();
            }
        }

        public async Task LeaveLobbyAsync(CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            await _gate.WaitAsync(ct).ConfigureAwait(false);
            try
            {
                if (_runner == null || !_inLobby)
                    return;

                // No explicit "Leave lobby" in Fusion in the same sense; you typically just stop listening by shutting down
                // or joining/starting a session. We model "leaving" as clearing state + marking not in lobby.
                _inLobby = false;
                ClearSessionCache();
            }
            finally
            {
                _gate.Release();
            }
        }

        // =========================
        // Room enquiries (data methods)
        // =========================

        public bool IsInLobby => _inLobby;

        public int SessionListVersion => _sessionListVersion;

        public IReadOnlyList<SessionInfo> GetSessionSnapshot()
        {
            // Return a stable copy for callers (UI, matchmaking selection, etc.).
            lock (_sessionsByName)
                return _sessionsByName.Values.ToList();
        }

        public bool TryGetSession(string sessionName, out SessionInfo info)
        {
            lock (_sessionsByName)
                return _sessionsByName.TryGetValue(sessionName, out info);
        }

        /// <summary>
        /// Wait until we get the FIRST session list update from Fusion (requires lobby joined).
        /// Useful for UI: "loading rooms..."
        /// </summary>
        public Task WaitForFirstSessionListAsync(CancellationToken ct)
        {
            if (_tcsFirstSessionList != null && _tcsFirstSessionList.Task.IsCompleted)
                return Task.CompletedTask;

            _tcsFirstSessionList ??= new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            if (ct.CanBeCanceled)
            {
                ct.Register(() => _tcsFirstSessionList.TrySetCanceled(ct));
            }

            return _tcsFirstSessionList.Task;
        }

        // =========================
        // Internal Start/Stop/Lobby
        // =========================

        private async Task StartInternalAsync(StartGameArgs args, CancellationToken ct)
        {
            EnsureRunnerInstance();
            EnsureRunnerConfigured();

            _tcsConnected = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            _tcsShutdown  = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            if (ct.CanBeCanceled)
            {
                ct.Register(() =>
                {
                    _tcsConnected.TrySetCanceled(ct);
                    _tcsShutdown.TrySetCanceled(ct);
                });
            }

            try
            {
                // StartGame is the "session start/join" entry point.
                // args must include GameMode, SessionName, Scene, etc. as your higher-level facade decides.
                var result = await _runner.StartGame(args);

                ct.ThrowIfCancellationRequested();

                if (!result.Ok)
                    throw NetErrors.StartFailed(result.ShutdownReason.ToString());

                // Wait for OnConnectedToServer (or equivalent) to confirm transport connection.
                // Depending on mode, you may also gate on OnPlayerJoined for local player;
                // keep this simple and let upper layers decide if they need more.
                await _tcsConnected.Task.ConfigureAwait(false);

                Connected?.Invoke(new FusionConnectInfo());

                {
                    // Optional: join lobby right away so UI can browse rooms.
                    // If you start/join an actual game session instead, you may not need lobby at the same time.
                    await JoinLobbyInternalAsync(_defaultLobby, ct).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw NetErrors.StartFailed(ex.Message, ex);
            }
        }

        private async Task StopInternalAsync(CancellationToken ct)
        {
            try
            {
                if (_runner != null && _runner.IsRunning)
                {
                    _runner.Shutdown();

                    // Give Fusion a moment to dispatch OnShutdown/OnDisconnected callbacks.
                    // We also await the shutdown callback if it arrives.
                    await Task.Yield();

                    if (_tcsShutdown != null)
                    {
                        // If the callback never comes (edge cases), don't hang forever.
                        // Upper layers should have their own timeout.
                        await _tcsShutdown.Task.ConfigureAwait(false);
                    }
                }

                _inLobby = false;
                ClearSessionCache();
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new NetException(NetFailCode.Unknown, "Stop failed.", ex);
            }
        }

        private async Task JoinLobbyInternalAsync(SessionLobby lobby, CancellationToken ct)
        {
            if (_runner == null)
                throw new NetException(NetFailCode.Unknown, "Runner not available.");

            // This awaiter is completed when first OnSessionListUpdated arrives.
            _tcsFirstSessionList = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            if (ct.CanBeCanceled)
                ct.Register(() => _tcsFirstSessionList.TrySetCanceled(ct));

            try
            {
                // JoinSessionLobby enables session list updates.
                var joinTask = _runner.JoinSessionLobby(lobby, cancellationToken: ct);

                // JoinSessionLobby is async but returns void/Task depending on Fusion version;
                // if it's not awaitable in your version, remove this await and rely on first list callback.
                if (joinTask is Task t)
                    await t.ConfigureAwait(false);

                _inLobby = true;

                // Wait for the first list so callers can safely query.
                await _tcsFirstSessionList.Task.ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new NetException(NetFailCode.Unknown, $"Join lobby failed: {ex.Message}", ex);
            }
        }

        private void ClearSessionCache()
        {
            lock (_sessionsByName)
            {
                _sessionsByName.Clear();
                _sessionListVersion++;
            }

            SessionListChanged?.Invoke(Array.Empty<SessionInfo>());
        }

        // =========================
        // Runner creation/config
        // =========================

        private void EnsureRunnerInstance()
        {
            if (_runner != null)
                return;

            // Prefer an existing runner on this GO.
            _runner = GetComponent<NetworkRunner>();
            if (_runner != null)
                return;
            
            if (_hostPrefab == null)
                throw new NetException(NetFailCode.Unknown, "Host prefab not assigned (NetworkRunner).");

            // Instantiate prefab as child of this object, or in scene root - your choice.
            var inst = Instantiate(_hostPrefab, transform);
            _runner = inst;

            // Optional: keep runner alive across scene loads (common for menus).
            DontDestroyOnLoad(_runner.gameObject);
        }

        private void EnsureRunnerConfigured()
        {
            if (_runner == null)
                return;

            // ProvideInput should generally be true on clients that call SetPlayerInput / OnInput.
            // If you have a dedicated server runner, you can flip this elsewhere.
            _runner.ProvideInput = true;

            // Ensure callbacks only added once.
            if (!_callbacksAdded)
            {
                _runner.AddCallbacks(this);
                _callbacksAdded = true;
            }

            // If you use Fusion scene management, ensure a scene manager exists.
            // (Only add if your project uses it; otherwise remove this block.)
            if (_runner.GetComponent<NetworkSceneManagerDefault>() == null)
            {
                _runner.gameObject.AddComponent<NetworkSceneManagerDefault>();
            }
        }

        // =========================
        // INetworkRunnerCallbacks
        // =========================

        public void OnConnectedToServer(NetworkRunner runner)
        {
            // Transport-level connection established.
            _tcsConnected?.TrySetResult(true);
        }

        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
        {
            _tcsConnected?.TrySetException(NetErrors.JoinFailed(reason.ToString()));
            Disconnected?.Invoke(new FusionDisconnectInfo());
        }

        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
        {
            Disconnected?.Invoke(new FusionDisconnectInfo());
        }

        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
            _tcsShutdown?.TrySetResult(true);
            Disconnected?.Invoke(new FusionDisconnectInfo());
        }

        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
        {
            // This is only called while in a lobby.
            lock (_sessionsByName)
            {
                _sessionsByName.Clear();
                for (int i = 0; i < sessionList.Count; i++)
                {
                    var si = sessionList[i];
                    // SessionInfo.Name is the identifier you’ll use to join.
                    _sessionsByName[si.Name] = si;
                }

                _sessionListVersion++;
            }

            _tcsFirstSessionList?.TrySetResult(true);
            SessionListChanged?.Invoke(GetSessionSnapshot());
        }

        // --- Unused callbacks (keep for completeness; you can remove or forward to debug logger) ---

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }
        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }

        public void OnInput(NetworkRunner runner, NetworkInput input) { }
        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }

        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }

        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }

        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }

        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }

        public void OnSceneLoadStart(NetworkRunner runner) { }
        public void OnSceneLoadDone(NetworkRunner runner) { }
    }
}
