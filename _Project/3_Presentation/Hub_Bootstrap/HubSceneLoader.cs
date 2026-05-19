// Assets/Scripts/Presentation/Hub_Bootstrap/HubSceneLoader.cs
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using VContainer;
using VContainer.Unity;


public class HubSceneLoader : ISceneLoaderHubSpokes
{
    private readonly LifetimeScope _hubScope;


    // We inject the Hub's own scope into the loader
    public HubSceneLoader(LifetimeScope hubScope)
    {
        _hubScope = hubScope;
    }


    public async Task LoadSpokeSceneAsync(string sceneName)
    {
        // 1. Tell VContainer: "The next LifetimeScope that wakes up is my child."
        using (LifetimeScope.EnqueueParent(_hubScope))
        {
            // 2. Load the Spoke scene.
            // VContainer intercepts the awake phase of the new scene and links the scopes.
            var asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);

            while (!asyncLoad.isDone)
            {
                await Task.Yield();
            }
        }
    }
}
