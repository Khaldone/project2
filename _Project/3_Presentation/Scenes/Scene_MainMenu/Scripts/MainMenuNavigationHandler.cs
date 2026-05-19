using Billiards.Presentation.Shop;
using Billiards.Presentation.MainMenu;
using UnityEngine;
using Cysharp.Threading.Tasks;
using static MainMenuRouter;
using Billiards.Presentation;

public class MainMenuNavigationHandler : MonoBehaviour
{
    // 1. Remove the [Inject] attribute!
    private MainMenuRouter _router;
    private MainMenuEntryPoint _entryPoint;

    // 2. Add a manual setup method
    public void InitializeRouter(MainMenuRouter sharedRouter, MainMenuEntryPoint entryPoint)
    {
        _router = sharedRouter;
        _entryPoint = entryPoint;
        Debug.Log($"[NavHandler] Received Router & EntryPoint: {_router.GetHashCode()}");
       
    }

    // Your existing button methods remain the same
    public void GoToShop()
    {
        if (_router == null)
        {
            Debug.LogError("[NavHandler] Router is null! Transition failed.");
            return;
        }

        _router.TransitionTo<IAP_Screen>(ShowStyle.FromRight, HideStyle.ToLeft);
    }

    public void GoToMatchmaking()
    {
        if (_router == null)
        {
            Debug.LogError("[NavHandler] Router is null! Transition failed.");
            return;
        }

        _router.TransitionTo<MatchmakingScreen>(ShowStyle.FromRight, HideStyle.ToLeft);
    }

    public void GoToPlayerProfileMenu()
    {
        if (_router == null)
        {
            Debug.LogError("[NavHandler] Router is null! Transition failed.");
            return;
        }

        _router.TransitionTo<PlayerProfileScreen>(ShowStyle.FromRight, HideStyle.ToLeft);
    }
    public void GoToCitySelection()
    {
        if (_router == null)
        {
            Debug.LogError("[NavHandler] Router is null! Transition failed.");
            return;
        }

        _router.TransitionTo<CitySelectionScreen>(ShowStyle.FromRight, HideStyle.ToLeft);
    }

    public void GoToTrophyRoad()
    {
        if (_router == null)
        {
            Debug.LogError("[NavHandler] Router is null! Transition failed.");
            return;
        }

        _router.TransitionTo<Billiards.Presentation.TrophyRoad.TrophyRoadScreen>(ShowStyle.FromRight, HideStyle.ToLeft);
    }
    public void GoToGameArena()
    {
        if (_entryPoint == null)
        {
            Debug.LogError("[NavHandler] EntryPoint is null! Cannot load Arena.");
            return;
        }

        // Disable the button to prevent double-clicking while scenes load
        // (Optional: You could also show a quick "Loading..." overlay here)

        _entryPoint.TransitionToGameArenaAsync().Forget();
    }
}