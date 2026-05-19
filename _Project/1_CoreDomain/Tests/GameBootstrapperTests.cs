using NUnit.Framework;
using NSubstitute;
using System.Threading.Tasks;
public class GameBootstrapperTests
{
    [Test]
    public async Task StartGameAsync_WhenAllServicesSucceed_LoadsMainMenu()
    {
        // ARRANGE
        var mockAuth = Substitute.For<IAuthenticationService_Old>();
        var mockNetwork = Substitute.For<INetworkService>();
        var mockLoader = Substitute.For<ISceneLoader>();


        // Configure the happy path
        mockAuth.AuthenticateAsync().Returns(Task.FromResult(true));
        mockNetwork.ConnectToMasterServerAsync().Returns(Task.FromResult(true));


        //var bootstrapper = new GameBootstrapper(mockAuth, mockNetwork, mockLoader);


        // ACT
        //await bootstrapper.StartGameAsync();


        // ASSERT
        // NSubstitute verifies the strict execution order
        Received.InOrder(() =>
        {
            mockAuth.AuthenticateAsync();
            mockNetwork.ConnectToMasterServerAsync();
            mockLoader.LoadMainMenu();
        });
    }


    [Test]
    public async Task StartGameAsync_WhenAuthFails_AbortsBootSequence()
    {
        // ARRANGE
        var mockAuth = Substitute.For<IAuthenticationService_Old>();
        var mockNetwork = Substitute.For<INetworkService>();
        var mockLoader = Substitute.For<ISceneLoader>();


        // Configure PlayFab/Google to fail (e.g., no internet)
        mockAuth.AuthenticateAsync().Returns(Task.FromResult(false));
        

        //var bootstrapper = new GameBootstrapper(mockAuth, mockNetwork, mockLoader);


        // ACT
        //await bootstrapper.StartGameAsync();


        // ASSERT
        await mockAuth.Received(1).AuthenticateAsync();

        // Prove that we never try to connect to multiplayer if we aren't logged in
        await mockNetwork.DidNotReceive().ConnectToMasterServerAsync();
        mockLoader.DidNotReceive().LoadMainMenu();
    }
}
