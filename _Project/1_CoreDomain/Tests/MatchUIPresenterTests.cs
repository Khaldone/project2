using NUnit.Framework;
using NSubstitute;
using System;


public class MatchUIPresenterTests
{
    [Test]
    public void OnPlayerTurnChanged_WhenPlayer2_UpdatesIndicatorAndDisablesButton()
    {
        // ARRANGE
        var mockView = Substitute.For<IMatchUIView>();

        // (Mocking the dependencies for MatchCoordinator omitted for brevity,
        // assume it is created as 'mockCoordinator')
        var mockCoordinator = Substitute.For<MatchCoordinator>();

        //var presenter = new MatchUIPresenter(mockView, mockCoordinator);


        // ACT
        // Simulate the internal game logic firing the event
        mockCoordinator.OnPlayerTurnChanged += Raise.Event<Action<int>>(2);


        // ASSERT
        // Verify the pure C# Presenter commanded the UI correctly
        mockView.Received(1).UpdateTurnIndicator(2);
        mockView.Received(1).SetEndTurnButtonInteractable(false);
    }
}
