// Assets/_Project/Scenes/02_Spoke_GameArena/Scripts/Input/NetworkStrikeInput.cs
using Fusion;

// This is the actual packet sent over the internet
public struct NetworkStrikeInput : INetworkInput
{
    public float Angle;
    public float Power;
    public NetworkBool IsStriking; // Acts as a "fire button" trigger
}