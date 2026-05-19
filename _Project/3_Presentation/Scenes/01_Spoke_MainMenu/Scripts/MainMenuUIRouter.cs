// Assets/_Project/Scenes/01_Spoke_MainMenu/Scripts/MainMenuUIRouter.cs
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using VContainer;
using VContainer.Unity;

public class MainMenuUIRouter : IUIRouter
{
    private readonly LifetimeScope _mainMenuScope;

    public MainMenuUIRouter(LifetimeScope mainMenuScope)
    {
        _mainMenuScope = mainMenuScope;
    }

    public async Task OpenMenuAsync(string additiveSceneName)
    {
        // 1. Tell VContainer the upcoming additive scene is a child of the Main Menu
        using (LifetimeScope.EnqueueParent(_mainMenuScope))
        {
            // 2. Load the scene additively
            var asyncLoad = SceneManager.LoadSceneAsync(additiveSceneName, LoadSceneMode.Additive);
            while (!asyncLoad.isDone) await Task.Yield();
        }
    }

    public async Task CloseMenuAsync(string additiveSceneName)
    {
        var asyncUnload = SceneManager.UnloadSceneAsync(additiveSceneName);
        while (!asyncUnload.isDone) await Task.Yield();
    }
}