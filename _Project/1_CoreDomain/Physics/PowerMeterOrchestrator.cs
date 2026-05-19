// Assets/_Project/1_CoreDomain/Physics/PowerMeterOrchestrator.cs
using System;
using UnityEngine;


public struct ShotPowerResult
{
    public float NormalizedPullback; // 0.0 to 1.0 (Used strictly for visual animation)
    public float AppliedImpulse;     // The actual Newtons of force to hit the ball with
    public bool HitSweetSpot;        // Did they release in the "Max Power" green zone?
    public float FinalImpulse;    // The actual Newtons to apply
}


public class PowerMeterOrchestrator
{
    //private readonly ILocalInventoryCache _inventory;

    // Baseline physics constants (Tuned to feel like standard 8-ball)
    private const float BASE_MAX_IMPULSE = 40.0f;
    private const float SWEET_SPOT_MIN = 0.96f; // The top 4% of the meter is the sweet spot
    private const float SWEET_SPOT_MULTIPLIER = 1.05f; // 5% bonus force for perfect timing
    private const float SWEET_SPOT_MAX = 0.95f;
    private const float MAX_PHYSICS_FORCE = 45.0f;
    //public PowerMeterOrchestrator(ILocalInventoryCache inventory)
    //{
    //    _inventory = inventory;
    //}


    public ShotPowerResult CalculateShotPower(float currentDragDistance, float maxDragDistance, float dragDistance = 1)
    {
        // 1. Normalize the human input to a clean 0.0 to 1.0 percentage
        float rawPower = Math.Clamp(currentDragDistance / maxDragDistance, 0.0f, 1.0f);


        // 2. Fetch the RPG Stats from the Gacha System
        // A beginner cue might have a ForceMultiplier of 0.0. A Legendary cue might be 0.4 (+40% power).
        //CueStats equippedCue = _inventory.GetEquippedCueStats();


        // 3. Check for the "Sweet Spot"
        bool isPerfect = rawPower >= SWEET_SPOT_MIN;


        // 4. Calculate the final deterministic math
        float calculatedImpulse = rawPower * BASE_MAX_IMPULSE;

        // Apply the Cue's RPG stat bonus
        //calculatedImpulse *= (1.0f + equippedCue.ForceMultiplier);


        // Apply the skill-based sweet spot bonus
        if (isPerfect)
        {
            calculatedImpulse *= SWEET_SPOT_MULTIPLIER;
        }




        // Apply a small "Snap" or "Boost" if they hit the sweet spot
        float finalPower = isPerfect ? rawPower * 1.05f : rawPower;

        return new ShotPowerResult
        {
            NormalizedPullback = rawPower,
            AppliedImpulse = calculatedImpulse,
            HitSweetSpot = isPerfect,
            FinalImpulse = finalPower * MAX_PHYSICS_FORCE,
        };
    }
}
