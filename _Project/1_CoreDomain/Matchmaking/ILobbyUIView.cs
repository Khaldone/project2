// 2. Assets/Scripts/CoreDomain/Matchmaking/ILobbyUIView.cs
public interface ILobbyUIView
{
    void ShowDefaultState();
    void ShowSearchingState(string estimatedTime);
    void ShowMatchFoundState();
    void ShowError(string message);
    void EnableInteractability(bool isInteractable);
}
