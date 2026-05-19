/*
 *  Created by Dragutin Sredojevic.
 *  https://www.nitugard.com
 */
using System;
using Unity.Mathematics;
using UnityEngine;

namespace ibc.commands
{
    using objects;

    
    public interface IBilliardCommand
    {

        /// <summary>
        /// Executes command.
        /// </summary>
        /// <param name="state">The billiard state on which command can act upon.</param>
        bool Execute(BilliardState state);
    }
    
    
    public struct NullCommand : IBilliardCommand
    {
        private readonly string _tag;
        public NullCommand(string tag = null) => _tag = tag;
        public override string ToString() => _tag ?? "NullCommand";
        public bool Execute(BilliardState state) => true;
    }
    

    /// <summary>Implements strike command</summary>
    [Serializable]
    public struct StrikeCommand : IBilliardCommand
    {
        public int BallIdentifier;
        public float Velocity;
        public Cue Cue;
        public float Orientation;
        public float Elevation;
        public float2 Offset;

        
        public StrikeCommand(int ballIdentifier, float velocity, float2 offset, Cue cue, float orientation, float elevation = 0)
        {
            BallIdentifier = ballIdentifier;
            Velocity = velocity;
            Cue = cue;
            Orientation = orientation;
            Elevation = elevation;
            Offset = offset;
        }

        public bool Execute(BilliardState state)
        {
            if (!state.TryGetPhysicsBall(BallIdentifier, out var ball))
            {
                Debug.LogError($"Could not find the ball: {BallIdentifier}");
            }
            if (ball.State != Ball.StateType.Normal)
            {
                Debug.LogWarning("Could not execute strike command, ball is not in normal state!");
                return false;
            }

            if (state.Solver.ResolveBallCueImpact(ref ball, Cue, Velocity, Orientation, Elevation, Offset)){
                ball.State = Ball.StateType.Struck;
                //Debug.Log("Strike ball velocity: " + ball.Velocity);
                state.SetPhysicsBall(ball);
                return true;
            }

            return false;
        }

        public bool HasChanged(StrikeCommand cmd, float eventThreshold)
        {
            if (BallIdentifier != cmd.BallIdentifier)
                return true;
            if (math.abs(Orientation - cmd.Orientation) > eventThreshold)
                return true;
            if (math.abs(Elevation - cmd.Elevation) > eventThreshold)
                return true;
            if (math.abs(Velocity - cmd.Velocity) > eventThreshold)
                return true;
            if (math.length(Offset - cmd.Offset) > eventThreshold)
                return true;

            return false;
        }

        public void Log(BilliardState state)
        {
            state.TryGetPhysicsBall(BallIdentifier, out var ball);

            Debug.Log($"<color=green>Ball {ball.Identifier} was struck </color>\n" +
                    $"\t<color=grey>Prior to impact cue stick velocity: {Velocity:F2}\n" +
                    $"\tPost impact cue ball velocity: {math.length(ball.Velocity):F2}\n" +
                    $"\tPost impact cue ball angular velocity: {math.length(ball.AngularVelocity):F2}\n</color>");
        }
    }
}