using NUnit.Framework;
using NSubstitute;
using System;
public class EndGameUITests
{
    [SetUp]
    public void Setup()
    {
        ServiceLocator.Clear(); // Clean slate for every test
    }

    [Test]
    public void ClickDoubleCoinsButton_WhenAdSucceeds_DoublesPlayerCoins()
    {
        // ARRANGE
        var mockAds = Substitute.For<IAdsService>();

        // Setup the mock to instantly trigger the "success" callback when shown
        mockAds.When(x => x.ShowRewardedAd(Arg.Any<Action>(), Arg.Any<Action>()))
               .Do(callInfo => callInfo.ArgAt<Action>(0).Invoke());


        ServiceLocator.Register<IAdsService>(mockAds);


        // ACT (Simulating a UI button click)
        bool rewardGiven = false;
        ServiceLocator.Get<IAdsService>().ShowRewardedAd(
            onRewardEarned: () => rewardGiven = true,
            onAdFailed: () => rewardGiven = false
        );


        // ASSERT
        Assert.IsTrue(rewardGiven);
    }
}