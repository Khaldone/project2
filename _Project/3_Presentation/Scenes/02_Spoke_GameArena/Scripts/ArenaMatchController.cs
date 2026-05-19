// Assets/_Project/Scenes/02_Spoke_GameArena/Scripts/ArenaMatchController.cs
using UnityEngine;
using VContainer;
public class ArenaMatchController : MonoBehaviour
{
    private INetworkSpawner _networkSpawner;

    [Inject]
    public void Construct(INetworkSpawner networkSpawner)
    {
        // VContainer automatically climbs the hierarchy to the Hub and injects the Fusion Manager
        _networkSpawner = networkSpawner;
    }


    public void OnRoundStart()
    {
        Debug.Log("Arena: Spawning the 8-Ball via the Hub's Network Service...");

        // The overseas developer just calls the interface.
        // Fusion handles the actual network syncing behind the scenes.
        int ballId = _networkSpawner.SpawnNetworkedEntity("Prefabs/Networked_8Ball", 0f, 5f);
    }
}