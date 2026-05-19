using System.Collections.Generic;
using ibc.solvers;

namespace ibc.game
{
    public interface IFoulDetector
    {
        FoulType DetectFoul(List<int> pocketedBalls, List<PhysicsSolver.Event> events, GameContext context);
    }
}