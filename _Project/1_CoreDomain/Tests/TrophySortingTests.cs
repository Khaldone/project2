//// Assets/_Project/Editor/Tests/Progression/TrophySortingTests.cs
//using NUnit.Framework;
//using System.Collections.Generic;
//using Billiards.Core.Progression;

//namespace Billiards.Editor.Tests.Progression
//{
//    [TestFixture]
//    public class TrophySortingTests
//    {
//        [Test]
//        public void MilestoneList_WhenSorted_ArrangesByRequiredCupsAscending()
//        {
//            // Arrange
//            var unsortedTrack = new List<TrophyMilestone>
//            {
//                new TrophyMilestone { TaskId = "node_high", RequiredCups = 500 },
//                new TrophyMilestone { TaskId = "node_low", RequiredCups = 100 },
//                new TrophyMilestone { TaskId = "node_mid", RequiredCups = 250 }
//            };

//            // Act
//            unsortedTrack.Sort((a, b) => a.RequiredCups.CompareTo(b.RequiredCups));

//            // Assert
//            Assert.AreEqual(100, unsortedTrack[0].RequiredCups);
//            Assert.AreEqual(250, unsortedTrack[1].RequiredCups);
//            Assert.AreEqual(500, unsortedTrack[2].RequiredCups);
//        }
//    }
//}