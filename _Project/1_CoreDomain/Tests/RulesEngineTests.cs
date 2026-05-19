using NUnit.Framework;
public class RulesEngineTests
{
    private RulesEngine _engine;
    private TurnContext _context;


    [SetUp]
    public void Setup()
    {
        _engine = new RulesEngine();
        _context = new TurnContext();
    }


    [Test]
    public void EvaluateTurn_NoBallsPocketed_ReturnsEndTurn()
    {
        // ACT
        var result = _engine.EvaluateTurn(_context);


        // ASSERT
        Assert.AreEqual(RulesEngine.TurnResult.EndTurn, result);
    }


    [Test]
    public void EvaluateTurn_CueBallPocketed_ReturnsFoul()
    {
        // ARRANGE
        _context.RegisterPocketedBall(enBallType.Solid);
        _context.RegisterPocketedBall(enBallType.Cue); // The scratch


        // ACT
        var result = _engine.EvaluateTurn(_context);


        // ASSERT
        // Even though a solid was pocketed, the cue ball overrides it as a foul.
        Assert.AreEqual(RulesEngine.TurnResult.Foul, result);
    }
}
