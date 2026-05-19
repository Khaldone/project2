// Inside Assets/Scripts/Presentation/Input/MobileTouchInput.cs
using UnityEngine;
using UnityEngine.InputSystem;


public class MobileTouchInput : MonoBehaviour, IPlayerInput
{
    [SerializeField] private InputActionReference aimDeltaAction;
    [SerializeField] private InputActionReference strikePowerAction;
    [SerializeField] private InputActionReference strikeReleaseAction;


    private void OnEnable()
    {
        aimDeltaAction.action.Enable();
        strikePowerAction.action.Enable();
        strikeReleaseAction.action.Enable();
    }


    private void OnDisable()
    {
        aimDeltaAction.action.Disable();
        strikePowerAction.action.Disable();
        strikeReleaseAction.action.Disable();
    }


    // Fulfilling the pure C# Contract
    public float GetAimDelta()
    {
        // Reads the X delta of the finger moving across the screen
        return aimDeltaAction.action.ReadValue<Vector2>().x;
    }


    public float GetStrikePower()
    {
        // Reads the Y distance of the pull-back (clamped 0 to 1)
        return Mathf.Clamp01(Mathf.Abs(strikePowerAction.action.ReadValue<Vector2>().y));
    }


    public bool IsStrikeReleased()
    {
        // Returns true exactly on the frame the finger lifts
        return strikeReleaseAction.action.WasReleasedThisFrame();
    }
}
