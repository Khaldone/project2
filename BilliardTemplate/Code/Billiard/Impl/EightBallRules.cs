using System;

namespace ibc.game
{
    [Serializable]
    public class EightBallRules : GameRulesBase, ITargetBallValidator
    {
        public bool HasAssignedGroups => Context.SidesAssigned;
        
        private const int CueBallId = 0;
        private const int EightBallId = 8;

        public EightBallRules(GameContext context, IBallClassifier classifier, ITurnProcessor turnProcessor) : base(context, classifier, turnProcessor)
        {
        }

        
        public bool IsValidTarget(int ballIdentifier)
        {
            if (!HasAssignedGroups)
                return true; 

            var targetBalls = _context.GetCurrentPlayerTargetBalls();
            return targetBalls.Contains(ballIdentifier);
        }
        
        public static EightBallRules Default
        {
            get
            {

                var classifier = new EightBallClassifier(CueBallId, EightBallId);
                var context = new GameContext(classifier);
                var breakValidator = new EightBallBreakValidator(CueBallId);
                var foulDetector = new EightBallFoulDetector(CueBallId, EightBallId);
                var winCondition = new EightBallWinCondition();

                var turnProcessor = new EightBallTurnProcessor(context, classifier, breakValidator, foulDetector, winCondition,
                    EightBallId);

                return new EightBallRules(context, classifier, turnProcessor);
            }
        }

    }
}