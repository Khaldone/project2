// Assets/_Project/CoreDomain/UI/MainMenuUIRouter.cs
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using VContainer.Unity;

public class MainMenuUIRouter : IUIRouter
{
    private readonly LifetimeScope _mainMenuScope;
    private readonly ITransitionService _transitionService;

    // Inject the global transition service
    public MainMenuUIRouter(LifetimeScope mainMenuScope, ITransitionService transitionService)
    {
        _mainMenuScope = mainMenuScope;
        _transitionService = transitionService;
    }

    public async Task OpenMenuAsync(string additiveSceneName)
    {
        // 1. Pull the curtain over the screen
        await _transitionService.FadeOutAsync(0.3f);

        // 2. Do the heavy lifting (Load the additive scene)
        using (LifetimeScope.EnqueueParent(_mainMenuScope))
        {
            var asyncLoad = SceneManager.LoadSceneAsync(additiveSceneName, LoadSceneMode.Additive);

            // Wait for Unity to finish loading the heavy assets into RAM
            while (!asyncLoad.isDone)
            {
                await Task.Yield();
            }
        }

        // 3. Optional: Give the new scene 1 frame to initialize its Canvas
        await Task.Yield();

        // 4. Open the curtain
        await _transitionService.FadeInAsync(0.3f);
    }

    public async Task CloseMenuAsync(string additiveSceneName)
    {
        await _transitionService.FadeOutAsync(0.2f);

        var asyncUnload = SceneManager.UnloadSceneAsync(additiveSceneName);
        while (!asyncUnload.isDone) await Task.Yield();

        await _transitionService.FadeInAsync(0.2f);
    }
}