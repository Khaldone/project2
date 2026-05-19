// 4. Assets/Scripts/CoreDomain/Boot/GameBootstrapper.cs
// This is the target of our test. It orchestrates the entire startup.
using System.Threading.Tasks;

public class GameBootstrapper
{
    private readonly IAuthenticationService_Old _authService;
    private readonly INetworkService _networkService;
    private readonly ISceneLoader _sceneLoader;


    public GameBootstrapper(
        IAuthenticationService_Old authService,
        INetworkService networkService,
        ISceneLoader sceneLoader)
    {
        _authService = authService;
        _networkService = networkService;
        _sceneLoader = sceneLoader;
    }


    public async Task StartGameAsync()
    {
        // Step 1: Log in via Google/PlayFab
        bool authSuccess = await _authService.AuthenticateAsync();
        if (!authSuccess)
        {
            // In a real app, you would command the UI to show an error dialog here
            return;
        }


        // Step 2: Connect to Photon Fusion's master server
        bool networkSuccess = await _networkService.ConnectToMasterServerAsync();
        if (!networkSuccess)
        {
            return;
        }


        // Step 3: We are online and authenticated. Load the Main Menu.
        _sceneLoader.LoadMainMenu();
    }
}
