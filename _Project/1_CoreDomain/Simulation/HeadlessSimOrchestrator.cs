// 3. Assets/Scripts/CoreDomain/Simulation/HeadlessSimOrchestrator.cs
using System;


public class HeadlessSimOrchestrator
{
    private readonly TurnController _turnController;
    private readonly TableStateManager _stateManager;
    private readonly IAnomalyDetector _anomalyDetector;
    private readonly System.Random _rng = new System.Random();


    public HeadlessSimOrchestrator(TurnController turnController, TableStateManager stateManager, IAnomalyDetector anomalyDetector)
    {
        _turnController = turnController;
        _stateManager = stateManager;
        _anomalyDetector = anomalyDetector;
    }


    // Runs a complete game from break to finish in milliseconds
    public int RunSimulatedMatch(int maxTurns = 100)
    {
        int turnsTaken = 0;


        while (turnsTaken < maxTurns)
        {
            // 1. Generate a random, valid strike
            float randomPower = (float)_rng.NextDouble(); // 0.0 to 1.0
            float randomAngle = _rng.Next(0, 360);

            //_turnController.SimulateInput(randomAngle, randomPower);


            // 2. Fast-forward the physics tick loop until balls stop
            while (!_stateManager.AreAllBallsResting())
            {
                // We tick the pure C# logic by 16ms (simulating 60fps)
                _turnController.Tick(0.016f, 2.0f);


                // 3. ANOMALY CHECK: Validate the math on every single micro-frame
                //_anomalyDetector.ValidatePhysicsState(
                //    _stateManager.GetAllBallPositions(),
                //    _stateManager.GetAllBallVelocities()
                //);
            }


            // (Assume we call the RulesEngine here to resolve pockets and fouls)


            turnsTaken++;
        }


        return turnsTaken;
    }
}