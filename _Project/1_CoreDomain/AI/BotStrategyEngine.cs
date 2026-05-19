// Assets/_Project/1_CoreDomain/AI/BotStrategyEngine.cs
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class BotStrategyEngine
{
    private readonly IPhysicsSolver _physicsSolver; // The tool that reverse-engineers the vectors


    public BotStrategyEngine(IPhysicsSolver physicsSolver)
    {
        _physicsSolver = physicsSolver;
    }


    public ShotOption DecideBestShot(MatchState state, PhysicsBall[] table, BotProfile_New profile)
    {
        // 1. 8-BALL AWARENESS: Filter out illegal targets instantly.
        List<int> legalTargetIndices = GetLegalTargetIndices(state, table);
        List<ShotOption> consideredShots = new List<ShotOption>();


        // 2. GENERATE ALL POTTING OPTIONS
        foreach (int targetIndex in legalTargetIndices)
        {
            for (int pocketIndex = 0; pocketIndex < 6; pocketIndex++)
            {
                // Ask the Physics Solver if this ball can even reach this pocket cleanly
                if (_physicsSolver.TryCalculatePottingShot(table, targetIndex, pocketIndex, out ShotOption shot))
                {
                    // If it's physically possible, score it!
                    shot.ProbabilityOfSuccess = CalculateShotDifficulty(table, shot);
                    shot.PositionalValue = CalculatePositionalValue(state, table, shot);
                    //shot.ClusterBreakValue = CalculateClusterBreakBonus(state, table, shot);

                    // Utility Math: Combine the scores based on the Bot's Profile
                    shot.TotalUtilityScore =
                        (shot.ProbabilityOfSuccess * profile.PottingWeight) +
                        (shot.PositionalValue * profile.PositionalWeight) +
                        (shot.ClusterBreakValue * 0.1f); // Minor bonus for breaking clusters

                    consideredShots.Add(shot);
                }
            }
        }


        // 3. FIND THE BEST OFFENSIVE SHOT
        ShotOption bestOffensiveShot = consideredShots
            .OrderByDescending(s => s.TotalUtilityScore)
            .FirstOrDefault();


        // 4. SAFETY PLAY DECISION
        // If the best shot is too risky (e.g., < 30% confidence), switch to defense!
        if (bestOffensiveShot.TotalUtilityScore < profile.SafetyThreshold || consideredShots.Count == 0)
        {
            return CalculateBestSafetyShot(state, table, legalTargetIndices);
        }


        return bestOffensiveShot;
    }


    private List<int> GetLegalTargetIndices(MatchState state, PhysicsBall[] table)
    {
        List<int> legalTargets = new List<int>();
        BallSuit mySuit = state.GetActivePlayerSuit();
        int myBallsLeft = state.PlayerBallsRemaining[state.ActivePlayerId];


        for (int i = 1; i < table.Length; i++)
        {
            if (!table[i].IsActive) continue;


            // 8-Ball Awareness: Never target the 8-ball unless it's the only ball left
            if (table[i].Suit == BallSuit.EightBall)
            {
                if (myBallsLeft == 0) legalTargets.Add(i);
                continue;
            }


            // Open table: Everything is legal
            if (state.IsOpenTable)
            {
                legalTargets.Add(i);
            }
            // Closed table: Only target my suit
            else if (table[i].Suit == mySuit)
            {
                legalTargets.Add(i);
            }
        }
        return legalTargets;
    }


    // --- Heuristic Calculators (The "Instincts") ---


    private float CalculateShotDifficulty(PhysicsBall[] table, ShotOption shot)
    {
        // Math: The sharper the cut angle and the longer the distance, the lower the score.
        Vector3 cuePos = table[0].Position;
        Vector3 targetPos = table[shot.TargetBallIndex].Position;
        Vector3 pocketPos = GetPocketPosition(shot.TargetPocketIndex);


        float cueToTargetDist = Vector3.Distance(cuePos, targetPos);
        float targetToPocketDist = Vector3.Distance(targetPos, pocketPos);

        Vector3 aimDir = (targetPos - cuePos).normalized;
        Vector3 pocketDir = (pocketPos - targetPos).normalized;

        // 1.0 = straight in. 0.0 = impossible 90-degree cut.
        float cutAngleFactor = Mathf.Max(0, Vector3.Dot(aimDir, pocketDir));

        float distancePenalty = (cueToTargetDist + targetToPocketDist) * 0.1f;


        return Mathf.Clamp01(cutAngleFactor - distancePenalty);
    }


    private float CalculatePositionalValue(MatchState state, PhysicsBall[] table, ShotOption shot)
    {
        // Simulates where the cue ball will stop.
        // If it stops in the center of the table, high score!
        // If it stops near a rail or behind an opponent's ball, low score.
        Vector3 predictedCueBallEndPos = _physicsSolver.PredictCueBallRestPosition(table, shot);
        float distanceToCenter = Vector3.Distance(predictedCueBallEndPos, Vector3.zero);

        // Normalize: Center = 1.0, Rails = 0.0
        return Mathf.Clamp01(1.0f - (distanceToCenter / 1.5f));
    }

    private ShotOption CalculateBestSafetyShot(MatchState state, PhysicsBall[] table, List<int> legalTargets)
    {
        // A safety shot ignores pockets. It calculates a soft hit on a legal target
        // designed to leave the cue ball hidden behind an opponent's ball.
        // (Implementation details of snooker geometry omitted for brevity)
        return new ShotOption
        {
            Type = ShotType.Safety,
            TotalUtilityScore = 1.0f // We are committed to defense now
        };
    }

    private Vector3 GetPocketPosition(int index) { /* Returns fixed table coordinates */ return Vector3.zero; }
}