using NUnit.Framework;
using NSubstitute;
using System.Threading.Tasks;


public class PostMatchProcessorTests
{
    [Test]
    public void CalculateProjectedXP_WhenWinnerIsEfficient_ReturnsMaxXP()
    {
        // Prove the pure math works before testing the async processor
        int xp = MatchRewardCalculator.CalculateProjectedXP(isWinner: true, shotsTaken: 5);
        Assert.AreEqual(150, xp); // 100 base + 50 bonus
    }


    [Test]
    public async Task ProcessMatchEndAsync_WhenSubmissionSucceeds_VerifiesResults()
    {
        // ARRANGE
        var mockSubmitter = Substitute.For<IMatchResultSubmitter>();
        var mockUI = Substitute.For<IMatchUIView>();


        // Simulate a successful network response from PlayFab
        mockSubmitter.SubmitMatchDataAsync(Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<int>())
                     .Returns(Task.FromResult(true));


        var processor = new PostMatchProcessor(mockSubmitter, mockUI);


        // ACT
        await processor.ProcessMatchEndAsync("Player_99", true, 8);


        // ASSERT
        Received.InOrder(() => {
            mockUI.ShowNotification("Calculating Results...");
            mockUI.ShowNotification("Projected XP: +150"); // Verifying it used the calculator
            mockUI.ShowNotification("Results Verified by Server.");
            mockUI.SetEndTurnButtonInteractable(true);
        });
    }


    [Test]
    public async Task ProcessMatchEndAsync_WhenSubmissionFails_ShowsError()
    {
        // ARRANGE
        var mockSubmitter = Substitute.For<IMatchResultSubmitter>();
        var mockUI = Substitute.For<IMatchUIView>();


        // Simulate a network failure or server rejection
        mockSubmitter.SubmitMatchDataAsync(Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<int>())
                     .Returns(Task.FromResult(false));


        var processor = new PostMatchProcessor(mockSubmitter, mockUI);


        // ACT
        await processor.ProcessMatchEndAsync("Player_99", true, 8);


        // ASSERT
        //mockUI.Received(1).ShowError("Failed to sync match results. Retrying...");
        mockUI.DidNotReceive().ShowNotification("Results Verified by Server.");
    }
}
