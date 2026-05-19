//// Assets/_Project/Editor/Tests/Progression/TrophyStateEvaluationTests.cs
//using NUnit.Framework;
//using Billiards.Core.Progression;

//namespace Billiards.Editor.Tests.Progression
//{
//    [TestFixture]
//    public class TrophyStateEvaluationTests
//    {
//        [Test]
//        public void EvaluateState_WhenAlreadyClaimed_ReturnsClaimedState()
//        {
//            // Act
//            var state = TrophyMilestone.EvaluateState(currentCups: 500, requiredCups: 100, isComplete: true, rewarded: true);

//            // Assert
//            Assert.AreEqual(MilestoneState.Claimed, state, "Milestones fully complete and rewarded on the server must evaluate as Claimed.");
//        }

//        [Test]
//        public void EvaluateState_WhenCupsSufficientButNotClaimed_ReturnsClaimableState()
//        {
//            // Act
//            var state = TrophyMilestone.EvaluateState(currentCups: 250, requiredCups: 200, isComplete: false, rewarded: false);

//            // Assert
//            Assert.AreEqual(MilestoneState.Claimable, state, "Milestones where cups surpass thresholds but remain uncollected must sit at Claimable.");
//        }

//        [Test]
//        public void EvaluateState_WhenCupsInsufficient_ReturnsLockedState()
//        {
//            // Act
//            var state = TrophyMilestone.EvaluateState(currentCups: 50, requiredCups: 200, isComplete: false, rewarded: false);

//            // Assert
//            Assert.AreEqual(MilestoneState.Locked, state, "Milestones where the player's cup tally falls short must evaluate as Locked.");
//        }
//    }
//}