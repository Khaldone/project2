// Assets/_Project/1_CoreDomain/Tests/NotificationQueueServiceTests.cs
using NUnit.Framework;
using Billiards.CoreDomain.Notifications;

namespace Billiards.CoreDomain.Tests
{
    [TestFixture]
    public class NotificationQueueServiceTests
    {
        private NotificationQueueService _sut;

        [SetUp]
        public void SetUp()
        {
            _sut = new NotificationQueueService();
        }

        // ── UpdateActiveClassification ──────────────────────────────────────

        [Test]
        public void UpdateActiveClassification_WhenNotificationShowing_FiresEvent()
        {
            // Arrange: Enqueue and let it show (it will fire OnShowNotification)
            NotificationClassification receivedClassification = NotificationClassification.Info;
            _sut.OnClassificationUpdated += c => receivedClassification = c;

            _sut.OnShowNotification += _ => { }; // Consume the show event
            _sut.Enqueue(CreateStatusOverlay());

            // Act
            _sut.UpdateActiveClassification(NotificationClassification.Success);

            // Assert
            Assert.AreEqual(NotificationClassification.Success, receivedClassification);
        }

        [Test]
        public void UpdateActiveClassification_WhenNothingShowing_DoesNotFireEvent()
        {
            // Arrange: No notification enqueued
            bool eventFired = false;
            _sut.OnClassificationUpdated += _ => eventFired = true;

            // Act
            _sut.UpdateActiveClassification(NotificationClassification.Error);

            // Assert
            Assert.IsFalse(eventFired);
        }

        // ── UpdateActiveMessage ─────────────────────────────────────────────

        [Test]
        public void UpdateActiveMessage_WhenNotificationShowing_FiresEvent()
        {
            // Arrange
            string receivedTitle = null;
            string receivedMessage = null;
            _sut.OnMessageUpdated += (t, m) => { receivedTitle = t; receivedMessage = m; };

            _sut.OnShowNotification += _ => { };
            _sut.Enqueue(CreateStatusOverlay());

            // Act
            _sut.UpdateActiveMessage("Login Successful", "Loading Profile...");

            // Assert
            Assert.AreEqual("Login Successful", receivedTitle);
            Assert.AreEqual("Loading Profile...", receivedMessage);
        }

        [Test]
        public void UpdateActiveMessage_WhenNothingShowing_DoesNotFireEvent()
        {
            // Arrange
            bool eventFired = false;
            _sut.OnMessageUpdated += (_, __) => eventFired = true;

            // Act
            _sut.UpdateActiveMessage("Title", "Message");

            // Assert
            Assert.IsFalse(eventFired);
        }

        // ── DismissActive ───────────────────────────────────────────────────

        [Test]
        public void DismissActive_WhenNotificationShowing_FiresDismissEvent()
        {
            // Arrange
            bool dismissFired = false;
            _sut.OnDismissRequested += () => dismissFired = true;

            _sut.OnShowNotification += _ => { };
            _sut.Enqueue(CreateStatusOverlay());

            // Act
            _sut.DismissActive();

            // Assert
            Assert.IsTrue(dismissFired);
        }

        [Test]
        public void DismissActive_WhenNothingShowing_DoesNotFireEvent()
        {
            // Arrange
            bool dismissFired = false;
            _sut.OnDismissRequested += () => dismissFired = true;

            // Act
            _sut.DismissActive();

            // Assert
            Assert.IsFalse(dismissFired);
        }

        // ── Queue Integrity After Dismiss ───────────────────────────────────

        [Test]
        public void DismissActive_DoesNotUnlockScreen_UntilNotifyDisplayFinishedCalled()
        {
            // Arrange: Enqueue two notifications
            int showCount = 0;
            _sut.OnShowNotification += _ => showCount++;
            _sut.OnDismissRequested += () => { }; // Consume dismiss

            _sut.Enqueue(CreateStatusOverlay());
            _sut.Enqueue(CreateStatusOverlay());

            // Act: Dismiss the first one (but DON'T call NotifyDisplayFinished)
            _sut.DismissActive();

            // Assert: The second notification should NOT have been shown yet
            // because the screen is still "locked" (the View hasn't confirmed teardown)
            Assert.AreEqual(1, showCount);
        }

        [Test]
        public void NotifyDisplayFinished_AfterDismiss_ShowsNextQueued()
        {
            // Arrange
            int showCount = 0;
            _sut.OnShowNotification += _ => showCount++;
            _sut.OnDismissRequested += () => { };

            _sut.Enqueue(CreateStatusOverlay());
            _sut.Enqueue(CreateStatusOverlay());

            // Act: Dismiss, then tell the queue we're done displaying
            _sut.DismissActive();
            _sut.NotifyDisplayFinished();

            // Assert: Second notification should now be shown
            Assert.AreEqual(2, showCount);
        }

        // ── Helpers ─────────────────────────────────────────────────────────

        private NotificationData CreateStatusOverlay()
        {
            return new NotificationData
            {
                Type = enNotificationType.SystemWarning,
                Classification = NotificationClassification.Info,
                Layout = NotificationLayout.StatusOverlay,
                SlideIn = NotificationSlideDirection.Immediate,
                SlideOut = NotificationSlideDirection.Immediate,
                Title = "Test",
                Message = "Test Message",
                DisplayDurationSeconds = 0
            };
        }
    }
}
