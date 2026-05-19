// Assets/_Project/1_CoreDomain/Tests/PowerMeterTests.cs
using NUnit.Framework;
using NSubstitute;


[TestFixture]
public class PowerMeterTests
{
    //private ILocalInventoryCache _subInventory;
    private PowerMeterOrchestrator _orchestrator;


    [SetUp]
    public void Setup()
    {
        //_subInventory = Substitute.For<ILocalInventoryCache>();
        //_orchestrator = new PowerMeterOrchestrator(_subInventory);
    }


    [Test]
    public void CalculateShotPower_WithLegendaryCue_AppliesStatMultiplier()
    {
        // ARRANGE: Player equips a Legendary Cue with +50% Force
        var legendaryStats = new CueStats { ForceMultiplier = 0.5f };
        //_subInventory.GetEquippedCueStats().Returns(legendaryStats);


        // ACT: Player pulls the meter back exactly 50% (no sweet spot)
        var result = _orchestrator.CalculateShotPower(currentDragDistance: 100f, maxDragDistance: 200f);


        // ASSERT:
        // Base Max Impulse = 40.0f
        // 50% Pullback = 20.0f Base Force
        // Legendary Multiplier (+50%) = 20.0f * 1.5f = 30.0f Total Force
        Assert.AreEqual(0.5f, result.NormalizedPullback);
        Assert.AreEqual(30.0f, result.AppliedImpulse);
        Assert.IsFalse(result.HitSweetSpot);
    }


    [Test]
    public void CalculateShotPower_InSweetSpot_AppliesBonusSnap()
    {
        // ARRANGE: Basic Cue (0% bonus)
        //_subInventory.GetEquippedCueStats().Returns(new CueStats { ForceMultiplier = 0.0f });


        // ACT: Player pulls the meter back 100% (Into the > 0.96 Sweet Spot)
        var result = _orchestrator.CalculateShotPower(currentDragDistance: 200f, maxDragDistance: 200f);


        // ASSERT:
        // Base Max Impulse = 40.0f
        // 100% Pullback = 40.0f Base Force
        // Sweet Spot Multiplier (+5%) = 40.0f * 1.05f = 42.0f Total Force
        Assert.IsTrue(result.HitSweetSpot);
        Assert.AreEqual(42.0f, result.AppliedImpulse);
    }
}
