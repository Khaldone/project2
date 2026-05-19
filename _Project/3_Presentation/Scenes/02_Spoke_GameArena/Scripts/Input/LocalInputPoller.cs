// Assets/_Project/Scenes/02_Spoke_GameArena/Scripts/Input/LocalInputPoller.cs
using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using UnityEngine;


// This runs ONLY on the local client's device
public class LocalInputPoller : MonoBehaviour, INetworkRunnerCallbacks
{
    private NetworkRunner _runner;
    private bool _wantsToStrike = false;
    private float _localAngle = 0f;
    private float _localPower = 0f;

    private void Start()
    {
        _runner = FindObjectOfType<NetworkRunner>();
        _runner.AddCallbacks(this);
    }

    private void Update()
    {
        // Read your UI touch joysticks or swipe gestures here
        // (Assume we have a UI script that sets these values)
        if (Input.GetButtonDown("Fire1")) // Or a UI "STRIKE" button
        {
            _wantsToStrike = true;
            _localAngle = 45f; // Example read from UI
            _localPower = 0.8f; // Example read from UI
        }
    }

    // Fusion calls this right before a network tick to gather inputs
    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        var myInput = new NetworkStrikeInput();


        if (_wantsToStrike)
        {
            myInput.IsStriking = true;
            myInput.Angle = _localAngle;
            myInput.Power = _localPower;

            _wantsToStrike = false; // Reset after packing
        }


        // Hand the packet to Fusion to blast across the network
        input.Set(myInput);
    }

    // (Other INetworkRunnerCallbacks omitted for brevity)
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
        throw new NotImplementedException();
    }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
        throw new NotImplementedException();
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        throw new NotImplementedException();
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        throw new NotImplementedException();
    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
        throw new NotImplementedException();
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
        throw new NotImplementedException();
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
        throw new NotImplementedException();
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
        throw new NotImplementedException();
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
    {
        throw new NotImplementedException();
    }

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
    {
        throw new NotImplementedException();
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
        throw new NotImplementedException();
    }

    public void OnConnectedToServer(NetworkRunner runner)
    {
        throw new NotImplementedException();
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        throw new NotImplementedException();
    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
        throw new NotImplementedException();
    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
        throw new NotImplementedException();
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
        throw new NotImplementedException();
    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {
        throw new NotImplementedException();
    }
    // ...
}