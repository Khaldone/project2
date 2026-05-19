//// Assets/_Project/Editor/Tests/Progression/TrophyRoadTests.cs
//using NUnit.Framework;
//using Billiards.Core.Progression;

//namespace Billiards.Editor.Tests.Progression
//{
//    [TestFixture]
//    public class TrophyRoadTests
//    {
//        [Test]
//        public void Milestone_WithInsufficientTrophies_ReturnsIsUnlockedFalse()
//        {
//            // Arrange
//            var milestone = new TrophyMilestone
//            {
//                MilestoneId = "test_node",
//                RequiredTrophies = 500,
//                RewardId = "coins_100",
//                IsClaimed = false
//            };
//            int playerTrophies = 420;

//            // Act
//            bool isUnlocked = milestone.IsUnlocked(playerTrophies);

//            // Assert
//            Assert.IsFalse(isUnlocked, "Milestone should remain locked if player trophies are below required thresholds.");
//        }

//        [Test]
//        public void Milestone_WithSufficientTrophies_ReturnsIsUnlockedTrue()
//        {
//            // Arrange
//            var milestone = new TrophyMilestone
//            {
//                MilestoneId = "test_node",
//                RequiredTrophies = 500,
//                RewardId = "coins_100",
//                IsClaimed = false
//            };
//            int playerTrophies = 600;

//            // Act
//            bool isUnlocked = milestone.IsUnlocked(playerTrophies);

//            // Assert
//            Assert.IsTrue(isUnlocked, "Milestone must evaluate to unlocked when player has passed the required trophy threshold.");
//        }
//    }
//}