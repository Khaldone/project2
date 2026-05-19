// 2. Assets/_Project/CoreDomain/Networking/INetworkSpawner.cs
// The Arena Spoke will use this to spawn the cue ball across all clients
public interface INetworkSpawner
{
    // Returns a generic ID that the pure C# logic can track
    int SpawnNetworkedEntity(string prefabAddress, float startX, float startZ);
    void DespawnNetworkedEntity(int entityId);
}