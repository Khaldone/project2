// Assets/_Project/Scenes/02_Spoke_GameArena/Scripts/Input/DragInputSurface.cs
using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class DragInputSurface : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    // The View only reports raw 2D pixel coordinates
    public event Action<Vector2> OnDragStarted;
    public event Action<Vector2> OnDragMoved;
    public event Action<Vector2> OnDragEnded;

    public void OnPointerDown(PointerEventData eventData)
    {
        OnDragStarted?.Invoke(eventData.position);
    }

    public void OnDrag(PointerEventData eventData)
    {
        OnDragMoved?.Invoke(eventData.position);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        OnDragEnded?.Invoke(eventData.position);
    }
}