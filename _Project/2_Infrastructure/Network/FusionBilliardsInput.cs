// Assets/_Project/2_Infrastructure/Network/FusionBilliardsInput.cs
using Fusion;
using UnityEngine;

public struct StrikeInput : INetworkInput
{
    public Vector3 AimDirection;
    public float PowerModifier; // 0.0 to 1.0
    public Vector2 SpinEnglish; // X, Y coordinates on the cue ball
    public NetworkBool IsStriking;
}