// Assets/_Project/1_CoreDomain/Tests/SettingsOrchestratorTests.cs
using NUnit.Framework;
using NSubstitute;
using System.Threading.Tasks;


[TestFixture]
public class SettingsOrchestratorTests
{
    private ILocalSaveService _subSaveService;
    private ICloudDataService _subCloudService;
    private IMessageBroker_New _subBroker;
    //private SettingsOrchestrator _orchestrator;


    [SetUp]
    public void Setup()
    {
        // ARRANGE: Create perfect, clean substitutes for every test
        _subSaveService = Substitute.For<ILocalSaveService>();
        _subCloudService = Substitute.For<ICloudDataService>();
        _subBroker = Substitute.For<IMessageBroker_New>();


        // Inject the substitutes into our pure C# orchestrator
        //_orchestrator = new SettingsOrchestrator(_subSaveService, _subCloudService, _subBroker);
    }


    [Test]
    public void SetMasterVolume_WhenChanged_SavesLocallyAndPublishesMessage()
    {
        // ACT
        float newVolume = 0.45f;
        //_orchestrator.SetMasterVolume(newVolume);


        // ASSERT
        // 1. Verify the Message Broker announced the change to the Audio Orchestrator
        _subBroker.Received(1).Publish(Arg.Is<VolumeChangedMessage>(msg => msg.NewVolume == newVolume));


        // 2. Verify the AES Save Service was commanded to write to disk
        //_subSaveService.Received(1).SaveSettingsAsync(Arg.Is<GameSettingsData>(data => data.MasterVolume == newVolume));


        // 3. Verify it attempted to sync the new profile to PlayFab
        _subCloudService.Received(1).SyncSettingsToPlayFabAsync(Arg.Any<GameSettingsData>());
    }


    [Test]
    public async Task InitializeAsync_WhenLocalDataExists_HydratesStateCorrectly()
    {
        // ARRANGE
        var savedData = new GameSettingsData { MasterVolume = 0.2f, IsLeftHandedMode = true };

        // Force the substitute save service to return our dummy data when asked
        //_subSaveService.LoadSettingsAsync().Returns(Task.FromResult(savedData));


        // ACT
        //await _orchestrator.InitializeAsync();
        //float currentVolume = _orchestrator.GetCurrentSettings().MasterVolume;


        // ASSERT
        //Assert.AreEqual(0.2f, currentVolume, "Orchestrator did not hydrate the volume from local storage.");

        // Verify it broadcasted the loaded volume so the AudioService boots up quietly!
        _subBroker.Received(1).Publish(Arg.Is<VolumeChangedMessage>(msg => msg.NewVolume == 0.2f));
    }


    [Test]
    public void SetLeftHandedMode_WhenCloudSyncFails_StillSavesLocally()
    {
        // ARRANGE
        // Simulate a scenario where the player is offline or PlayFab is down
        _subCloudService.When(x => x.SyncSettingsToPlayFabAsync(Arg.Any<GameSettingsData>()))
                        .Throw(new System.Exception("Network Timeout"));


        // ACT & ASSERT
        // We use Assert.DoesNotThrow to ensure the Orchestrator safely catches the cloud error
        // and doesn't crash the game.
        //Assert.DoesNotThrow(() => _orchestrator.SetLeftHandedMode(true));


        // Even though the cloud failed, the local AES save must STILL have executed!
        //_subSaveService.Received(1).SaveSettingsAsync(Arg.Is<GameSettingsData>(data => data.IsLeftHandedMode == true));
    }
}
