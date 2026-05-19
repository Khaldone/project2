using System.Collections.Generic;
using ibc.solvers;

namespace ibc.game
{
    public interface IBreakValidator
    {
        bool IsValidBreak(List<int> pocketedBalls, List<PhysicsSolver.Event> events);
    }
}