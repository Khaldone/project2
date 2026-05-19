using NUnit.Framework;
using NSubstitute;
using System.Threading.Tasks;
using System.Collections.Generic;


public class LoadoutCoordinatorTests
{
    [Test]
    public async Task EquipCueAsync_WhenNewCueEquipped_ReleasesOldCue()
    {
        // ARRANGE
        var mockLoader = Substitute.For<IAssetLoader>();
        var mockData = Substitute.For<IPlayerDataService>();


        // Give the player two cues
        var profile = new PlayerProfileSave { UnlockedCues = new List<string> { "cue_basic", "cue_dragon" } };
        mockData.CurrentProfile.Returns(profile);


        // Create dummy objects to represent the 3D models in memory
        object firstCueModel = new object();
        object secondCueModel = new object();


        // Configure the mock to return our dummy objects
        mockLoader.LoadAndInstantiateAsync("cue_basic").Returns(Task.FromResult(firstCueModel));
        mockLoader.LoadAndInstantiateAsync("cue_dragon").Returns(Task.FromResult(secondCueModel));


        var coordinator = new LoadoutCoordinator(mockLoader, mockData);


        // ACT
        await coordinator.EquipCueAsync("cue_basic"); // Equip the first one
        await coordinator.EquipCueAsync("cue_dragon"); // Equip the second one


        // ASSERT
        // Verify that before the Dragon cue was loaded, the Basic cue was purged from RAM
        Received.InOrder(() => {
            mockLoader.LoadAndInstantiateAsync("cue_basic");
            mockLoader.Release(firstCueModel); // MEMORY LEAK PREVENTED
            mockLoader.LoadAndInstantiateAsync("cue_dragon");
        });
    }
}
