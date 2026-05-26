// Assets/_Project/1_CoreDomain/Tests/PushTokenSyncerTests.cs
using System;
using NUnit.Framework;
using NSubstitute;
using Billiards.CoreDomain.Notifications;
using Billiards.Infrastructure.Backend;

namespace Billiards.Core.Tests
{
    [TestFixture]
    public class PushTokenSyncerTests
    {
        private IPushNotificationService _mockPushService;
        private IMessageBroker _mockMessageBroker;
        private PushTokenSyncer _sut;

        [SetUp]
        public void SetUp()
        {
            _mockPushService = Substitute.For<IPushNotificationService>();
            _mockMessageBroker = Substitute.For<IMessageBroker>();
        }

        [Test]
        public void PushTokenSyncer_OnCreation_SubscribesToDependencies()
        {
            // Act
            _sut = new PushTokenSyncer(_mockPushService, _mockMessageBroker);

            // Assert
            _mockMessageBroker.Received(1).Subscribe(Arg.Any<Action<UserAuthenticatedMessage>>());
            _mockPushService.Received(1).OnTokenUpdated += Arg.Any<Action<string>>();
        }

        [Test]
        public void PushTokenSyncer_OnDispose_UnsubscribesFromDependencies()
        {
            // Arrange
            _sut = new PushTokenSyncer(_mockPushService, _mockMessageBroker);

            // Act
            _sut.Dispose();

            // Assert
            _mockMessageBroker.Received(1).Unsubscribe(Arg.Any<Action<UserAuthenticatedMessage>>());
            _mockPushService.Received(1).OnTokenUpdated -= Arg.Any<Action<string>>();
        }
    }
}
