using System.Collections.Generic;
using ibc.solvers;

namespace ibc.game
{
    public interface ITurnProcessor
    {
        TurnResult ProcessBreak(List<PhysicsSolver.Event> events);
        TurnResult ProcessRegularTurn(List<PhysicsSolver.Event> events);
    }
}