// 1. Assets/_Project/CoreDomain/AI/IBotBrain.cs
using System.Collections.Generic;
public interface IBotBrain
{
    // The Bot looks at the table state and decides what to do
    StrikeIntent CalculateBestShot(IReadOnlyList<BallState> currentTableState, BallState cueBallState);
}