// Assets/Scripts/CoreDomain/UI/IMatchUIView.cs
public interface IMatchUIView
{
    void UpdateTurnIndicator(int playerId);
    void ShowNotification(string message);
    void SetEndTurnButtonInteractable(bool isInteractable);
}
