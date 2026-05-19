// Attached to: Screen_Tap_Zone
using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class FullScreenTapWidget : MonoBehaviour, IPointerDownHandler
{
    public event Action OnTapped;


    public void OnPointerDown(PointerEventData eventData)
    {
        OnTapped?.Invoke();
    }
}