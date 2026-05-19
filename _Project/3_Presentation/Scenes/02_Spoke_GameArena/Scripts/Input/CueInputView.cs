using System;
using UnityEngine;
using UnityEngine.EventSystems;


public class CueInputView : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public event Action<enStrikeCommand> OnAimingUpdated;
    public event Action<enStrikeCommand> OnStrikeExecuted;


    private float _currentAngle;
    private float _currentPower;


    public void OnDrag(PointerEventData eventData)
    {
        // ... (Calculate angle and power from swipe delta) ...

        // Fire the event. The pure C# AimPredictionService listens to this!
        OnAimingUpdated?.Invoke(new enStrikeCommand { Angle = _currentAngle, Power = _currentPower });
    }


    public void OnPointerUp(PointerEventData eventData)
    {
        // The finger released! Send the command to the network.
        OnStrikeExecuted?.Invoke(new enStrikeCommand { Angle = _currentAngle, Power = _currentPower });
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        throw new NotImplementedException();
    }
}