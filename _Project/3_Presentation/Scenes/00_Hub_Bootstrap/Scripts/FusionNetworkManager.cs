// Assets/_Project/Scenes/00_Hub_Bootstrap/Scripts/FusionNetworkManager.cs
using System.Threading.Tasks;
using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;


// This script sits on a GameObject in the 00_Hub_Bootstrap scene
[RequireComponent(typeof(NetworkRunner))]
public class FusionNetworkManager : MonoBehaviour, INetMatchmakingService, INetworkSpawner
{
    private NetworkRunner _runner;


    private void Awake()
    {
        _runner = GetComponent<NetworkRunner>();
        // Fusion requires a SceneManager to sync scenes between host and clients
        //gameObject.AddComponent<NetworkSceneManagerDefault>();
    }


    public async Task<bool> JoinOrCreateMatchAsync(string roomName)
    {
        var args = new StartGameArgs
        {
            GameMode = GameMode.AutoHostOrClient,
            SessionName = roomName,
            //SceneManager = GetComponent<NetworkSceneManagerDefault>()
        };


        StartGameResult result = await _runner.StartGame(args);

        if (result.Ok)
        {
            // The Host tells all clients to load the Arena Spoke!
            if (_runner.IsServer)
            {
                // Fusion's SceneManager handles loading the Spoke scene for everyone
                //_runner.SetActiveScene("02_Spoke_GameArena");
                //_runner.SceneManager.LoadScene("02_Spoke_GameArena");
            }
            return true;
        }

        return false;
    }


    public Task DisconnectAsync()
    {
        return _runner.Shutdown();
    }


    public uint SpawnNetworkedEntity(string prefabAddress, float startX, float startZ)
    {
        // In a real AAA setup, you would use Addressables here instead of Resources
        var prefab = Resources.Load<NetworkObject>(prefabAddress);
        var position = new Vector3(startX, 0, startZ);

        NetworkObject spawnedObj = _runner.Spawn(prefab, position, Quaternion.identity);

        return spawnedObj.Id.Raw; // Return Fusion's internal ID to the pure C# domain
    }


    public void DespawnNetworkedEntity(uint entityId)
    {
        var obj = _runner.FindObject(new NetworkId { Raw = entityId });
        if (obj != null)
        {
            _runner.Despawn(obj);
        }
    }

    int INetworkSpawner.SpawnNetworkedEntity(string prefabAddress, float startX, float startZ)
    {
        throw new System.NotImplementedException();
    }

    public void DespawnNetworkedEntity(int entityId)
    {
        throw new System.NotImplementedException();
    }
}