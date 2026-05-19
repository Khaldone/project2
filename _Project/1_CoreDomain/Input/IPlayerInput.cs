// Inside Assets/Scripts/CoreDomain/Input/IPlayerInput.cs
public interface IPlayerInput
{
    // For dragging left/right to aim
    float GetAimDelta();

    // For pulling back the cue (normalized 0.0 to 1.0)
    float GetStrikePower();

    // True on the exact frame the player releases their finger to shoot
    bool IsStrikeReleased();
}
