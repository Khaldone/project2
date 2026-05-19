// 2. Assets/Scripts/CoreDomain/Loadout/LoadoutCoordinator.cs
// The Brain. This handles the logic of spawning what the player owns.
using System.Threading.Tasks;
public class LoadoutCoordinator
{
    private readonly IAssetLoader _assetLoader;
    private readonly IPlayerDataService _playerData; // From our Save System!

    private object _currentCueInstance;


    public LoadoutCoordinator(IAssetLoader assetLoader, IPlayerDataService playerData)
    {
        _assetLoader = assetLoader;
        _playerData = playerData;
    }


    public async Task<bool> EquipCueAsync(string cueId)
    {
        // 1. Validate ownership using the pure C# economy rules
        if (!_playerData.CurrentProfile.UnlockedCues.Contains(cueId))
        {
            return false;
        }


        // 2. Clean up the old 3D model from memory
        if (_currentCueInstance != null)
        {
            _assetLoader.Release(_currentCueInstance);
        }


        // 3. Command the engine to fetch and build the new 3D model
        _currentCueInstance = await _assetLoader.LoadAndInstantiateAsync(cueId);


        // 4. Update the local save cache
        _playerData.CurrentProfile.EquippedCueId = cueId;

        return _currentCueInstance != null;
    }
}
