// Assets/_Project/3_Presentation/Scene_GameArena/Scripts/ArenaNavigationHandler.cs
using Billiards.Presentation.Arena;
using UnityEngine;
using VContainer;

public class ArenaNavigationHandler : MonoBehaviour
{
    private GameArenaEntryPoint _entryPoint;

    // We can use [Inject] here if you prefer, or manual setup like the Main Menu
    [Inject]
    public void Construct(GameArenaEntryPoint entryPoint)
    {
        _entryPoint = entryPoint;
    }

    public void OnQuitButtonPressed()
    {
        if (_entryPoint == null)
        {
            Debug.LogError("[ArenaNav] EntryPoint is null! Cannot return to Main Menu.");
            return;
        }

        // Optional: Show a loading screen or confirmation popup first

        _entryPoint.ReturnToMainMenu();
    }
}