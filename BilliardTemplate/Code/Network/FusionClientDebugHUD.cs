using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Fusion;
using UnityEngine;

namespace ibc.network
{
    /// <summary>
    /// Drop-in debug HUD for FusionClient + NetworkRunner.
    /// - Works with IMGUI for fastest iteration (no UI setup required).
    /// - Provides state view, session list view, and common test actions (start/stop/join lobby).
    /// - Captures callback ordering into a ring buffer log.
    /// </summary>
    public sealed class FusionClientDebugHUD : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private FusionClient _client;

        [Tooltip("If assigned, we will also show direct runner info here.")]
        [SerializeField] private NetworkRunner _runnerOverride;

        [Header("UI")]
        [SerializeField] private bool _visible = true;
        [SerializeField] private KeyCode _toggleKey = KeyCode.F1;
        [SerializeField] private bool _autoScroll = true;
        [SerializeField] private bool _verbose = true;
        [SerializeField] private int _maxLogLines = 300;

        [Header("Quick Start Args (optional)")]
        [Tooltip("Session name used by quick Start buttons (Host/Client).")]
        [SerializeField] private string _quickSessionName = "debug_room";

        [Tooltip("If true, Start buttons will use SceneManager to load current active scene.")]
        [SerializeField] private bool _useActiveScene = true;

        [Tooltip("Optional region (depends on your transport/setup). Keep empty if you don’t use it.")]
        [SerializeField] private string _region = "";

        [Header("Lobby")]
        [SerializeField] private SessionLobby _lobby = SessionLobby.Shared;

        // Simple ring buffer log
        private readonly LinkedList<string> _log = new LinkedList<string>();
        private Vector2 _scroll;
        private DateTime _lastSessionListTimeUtc;

        // Simple "operation in progress" flags
        private bool _busy;
        private string _busyLabel = "";

        // Join-by-name testing
        private string _joinNameInput = "debug_room";

        private NetworkRunner Runner => _runnerOverride != null
            ? _runnerOverride
            : (_client != null ? TryGetRunnerFromClient(_client) : null);

        private void Reset()
        {
            _client = FindFirstObjectByType<FusionClient>();
        }

        private void Awake()
        {
            if (_client == null)
                _client = FindFirstObjectByType<FusionClient>();

            HookClientEvents(_client);
            Log("FusionClientDebugHUD ready. Toggle with " + _toggleKey);
        }

        private void OnDestroy()
        {
            UnhookClientEvents(_client);
        }

        private void Update()
        {
            if (Input.GetKeyDown(_toggleKey))
                _visible = !_visible;
        }

        private void HookClientEvents(FusionClient c)
        {
            if (c == null) return;

            c.Connected += OnClientConnected;
            c.Disconnected += OnClientDisconnected;

            // If you added SessionListChanged event in your FusionClient, hook it:
            // (If you did not, this won't compile. In that case comment the next 2 lines.)
            c.SessionListChanged += OnSessionListChanged;
        }

        private void UnhookClientEvents(FusionClient c)
        {
            if (c == null) return;

            c.Connected -= OnClientConnected;
            c.Disconnected -= OnClientDisconnected;

            // If exists
            c.SessionListChanged -= OnSessionListChanged;
        }

        private void OnClientConnected(FusionConnectInfo info)
        {
            Log("CLIENT: Connected event fired");
        }

        private void OnClientDisconnected(FusionDisconnectInfo info)
        {
            Log("CLIENT: Disconnected event fired");
        }

        private void OnSessionListChanged(IReadOnlyList<SessionInfo> list)
        {
            _lastSessionListTimeUtc = DateTime.UtcNow;
            Log($"LOBBY: Session list updated ({list.Count} sessions).");
            if (_verbose && list.Count > 0)
            {
                // Print first few
                foreach (var s in list.Take(5))
                    Log($"  - {s.Name} | players={s.PlayerCount}/{s.MaxPlayers} | open={s.IsOpen} vis={s.IsVisible}");
                if (list.Count > 5) Log("  ...");
            }
        }

        private void OnGUI()
        {
            if (!_visible) return;

            var w = Mathf.Min(620, Screen.width - 20);
            var h = Mathf.Min(900, Screen.height - 20);
            GUILayout.BeginArea(new Rect(10, 10, w, h), GUI.skin.window);

            DrawHeader();
            GUILayout.Space(6);

            DrawStatePanel();
            GUILayout.Space(6);

            DrawActionsPanel();
            GUILayout.Space(6);

            DrawSessionListPanel();
            GUILayout.Space(6);

            DrawLogPanel();

            GUILayout.EndArea();
        }

        private void DrawHeader()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("<b>Fusion Client Debug HUD</b>", RichLabel());
            GUILayout.FlexibleSpace();

            _autoScroll = GUILayout.Toggle(_autoScroll, "AutoScroll", GUILayout.Width(90));
            _verbose = GUILayout.Toggle(_verbose, "Verbose", GUILayout.Width(80));

            if (GUILayout.Button("Clear Log", GUILayout.Width(90)))
                _log.Clear();

            GUILayout.EndHorizontal();

            if (_busy)
                GUILayout.Label($"<color=yellow>BUSY:</color> {_busyLabel}", RichLabel());
        }

        private void DrawStatePanel()
        {
            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label("<b>State</b>", RichLabel());

            if (_client == null)
            {
                GUILayout.Label("<color=red>FusionClient reference missing.</color>", RichLabel());
                if (GUILayout.Button("Find FusionClient in Scene"))
                {
                    _client = FindFirstObjectByType<FusionClient>();
                    HookClientEvents(_client);
                }
                GUILayout.EndVertical();
                return;
            }

            var r = Runner;
            var isRunning = _client.IsRunning;

            GUILayout.Label($"Client.IsRunning: <b>{isRunning}</b>", RichLabel());

            // Lobby state requires your FusionClient to expose it; if you have IsInLobby/SessionListVersion methods, show them.
            // If you don't, comment these out.
            GUILayout.Label($"Client.IsInLobby: <b>{_client.IsInLobby}</b>", RichLabel());
            GUILayout.Label($"SessionListVersion: <b>{_client.SessionListVersion}</b> | LastUpdate(UTC): {_lastSessionListTimeUtc:HH:mm:ss}", RichLabel());

            if (r == null)
            {
                GUILayout.Label("<color=orange>Runner not found (runnerOverride not set and could not resolve from client).</color>", RichLabel());
                GUILayout.EndVertical();
                return;
            }

            GUILayout.Space(4);
            GUILayout.Label($"Runner.IsRunning: <b>{r.IsRunning}</b> | ProvideInput: {r.ProvideInput}", RichLabel());
            GUILayout.Label($"Mode: <b>{r.GameMode}</b> | Session: <b>{Safe(r.SessionInfo.Name)}</b>", RichLabel());

            // Tick/Ping info (safe guards, may differ by Fusion version)
            try
            {
                GUILayout.Label($"Tick: {r.Tick} | RTT(ms): {r.GetPlayerRtt(r.LocalPlayer)}", RichLabel());
            }
            catch { /* ignore if API differs */ }

            // Players
            try
            {
                var players = r.ActivePlayers.ToArray();
                GUILayout.Label($"Players: <b>{players.Length}</b> | LocalPlayer: {r.LocalPlayer}", RichLabel());
                if (_verbose && players.Length > 0)
                    GUILayout.Label("Active: " + string.Join(", ", players.Select(p => p.ToString())), RichLabel());
            }
            catch { /* ignore */ }

            GUILayout.EndVertical();
        }

        private void DrawActionsPanel()
        {
            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label("<b>Actions</b>", RichLabel());

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Start Host (Quick)"))
                _ = RunOp("Start Host", ct => QuickStart(GameMode.Host, ct));

            if (GUILayout.Button("Start Client (Quick)"))
                _ = RunOp("Start Client", ct => QuickStart(GameMode.Client, ct));

            if (GUILayout.Button("Stop"))
                _ = RunOp("Stop", ct => _client.StopAsync(ct));

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Join Lobby"))
                _ = RunOp("Join Lobby", ct => _client.JoinLobbyAsync(_lobby, ct));

            if (GUILayout.Button("Refresh Lobby (rejoin)"))
                _ = RunOp("Refresh Lobby", async ct =>
                {
                    await _client.LeaveLobbyAsync(ct);
                    await _client.JoinLobbyAsync(_lobby, ct);
                });

            if (GUILayout.Button("Leave Lobby"))
                _ = RunOp("Leave Lobby", ct => _client.LeaveLobbyAsync(ct));
            GUILayout.EndHorizontal();

            GUILayout.Space(6);
            GUILayout.Label("<b>Join/Host by session name</b>", RichLabel());
            GUILayout.BeginHorizontal();
            _joinNameInput = GUILayout.TextField(_joinNameInput, GUILayout.Width(220));

            if (GUILayout.Button("Host"))
                _ = RunOp("Host by name", ct => StartWithName(GameMode.Host, _joinNameInput, ct));

            if (GUILayout.Button("Client"))
                _ = RunOp("Client by name", ct => StartWithName(GameMode.Client, _joinNameInput, ct));

            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }

        private void DrawSessionListPanel()
        {
            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label("<b>Sessions (cached)</b>", RichLabel());

            if (_client == null)
            {
                GUILayout.EndVertical();
                return;
            }

            IReadOnlyList<SessionInfo> list;
            try
            {
                list = _client.GetSessionSnapshot();
            }
            catch
            {
                GUILayout.Label("<color=orange>Client does not expose GetSessionSnapshot() (or compile mismatch). Add it or comment this panel.</color>", RichLabel());
                GUILayout.EndVertical();
                return;
            }

            GUILayout.Label($"Count: <b>{list.Count}</b>", RichLabel());

            if (list.Count == 0)
            {
                GUILayout.Label("No sessions. (Join lobby first, or there are no rooms.)", RichLabel());
                GUILayout.EndVertical();
                return;
            }

            // Show first N for sanity
            const int maxShow = 12;
            foreach (var s in list.Take(maxShow))
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label($"{s.Name}  ({s.PlayerCount}/{s.MaxPlayers})  open={s.IsOpen} vis={s.IsVisible}", GUILayout.Width(420));
                if (GUILayout.Button("Join", GUILayout.Width(60)))
                    _ = RunOp("Join selected", ct => StartWithName(GameMode.Client, s.Name, ct));
                GUILayout.EndHorizontal();
            }

            if (list.Count > maxShow)
                GUILayout.Label($"... ({list.Count - maxShow} more)", RichLabel());

            GUILayout.EndVertical();
        }

        private void DrawLogPanel()
        {
            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label("<b>Log</b>", RichLabel());

            _scroll = GUILayout.BeginScrollView(_scroll, GUILayout.Height(260));
            foreach (var line in _log)
                GUILayout.Label(line, GUI.skin.label);
            GUILayout.EndScrollView();

            GUILayout.EndVertical();

            if (_autoScroll)
                _scroll.y = 999999;
        }

        // =========================
        // Ops
        // =========================

        private async Task RunOp(string label, Func<CancellationToken, Task> op)
        {
            if (_busy)
            {
                Log($"IGNORED op '{label}' (busy).");
                return;
            }

            _busy = true;
            _busyLabel = label;

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));
            try
            {
                Log($"OP: {label} ...");
                await op(cts.Token);
                Log($"OP: {label} OK");
            }
            catch (OperationCanceledException)
            {
                Log($"OP: {label} CANCEL/TIMEOUT");
            }
            catch (Exception ex)
            {
                Log($"OP: {label} FAIL: {ex.GetType().Name}: {ex.Message}");
            }
            finally
            {
                _busy = false;
                _busyLabel = "";
            }
        }

        private Task QuickStart(GameMode mode, CancellationToken ct)
            => StartWithName(mode, _quickSessionName, ct);

        private async Task StartWithName(GameMode mode, string sessionName, CancellationToken ct)
        {
            // You likely already build FusionStartArgs in your facade.
            // For quick debugging, we keep it simple and rely on defaults.
            var args = new StartGameArgs()
            {
                GameMode = mode,
                SessionName = sessionName,
            };

            // Optional: if you want to sync scenes via Fusion.
            // If you use scenes, you probably want args.Scene = SceneRef.FromIndex(...)
            // and a scene manager on the runner.
            if (_useActiveScene)
            {
                // This may vary by Fusion version; if you manage scenes differently, remove.
                try
                {
                    var idx = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
                    args.Scene = SceneRef.FromIndex(idx);
                }
                catch { /* ignore */ }
            }

            // Region config depends on your Fusion setup; keep it empty if unused.
            // args.CustomLobbyName etc also depends; keep minimal.

            await _client.StartAsync(args, ct);

            // If you want to always join lobby after start (menu usage), call JoinLobby explicitly:
            // await _client.JoinLobbyAsync(_lobby, ct);
        }

        // =========================
        // Helpers
        // =========================

        private void Log(string msg)
        {
            var line = $"[{DateTime.Now:HH:mm:ss.fff}] {msg}";
            _log.AddLast(line);
            while (_log.Count > _maxLogLines)
                _log.RemoveFirst();
        }

        private static GUIStyle RichLabel()
        {
            var s = new GUIStyle(GUI.skin.label) { richText = true, wordWrap = true };
            return s;
        }

        private static string Safe(string s) => string.IsNullOrEmpty(s) ? "-" : s;

        private static NetworkRunner TryGetRunnerFromClient(FusionClient client)
        {
            // If your FusionClient exposes the runner, use that instead.
            // Otherwise, we try to find it on the same GO.
            return client.GetComponentInChildren<NetworkRunner>();
        }
    }
}
