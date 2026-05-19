// Assets/_Project/Scenes/02_Spoke_GameArena/Scripts/Input/CueInputPresenter.cs
using System;
using UnityEngine;
using VContainer.Unity;


public class CueInputPresenter : ICueInputListener, IStartable, IDisposable
{
    public event Action<enStrikeCommand> OnAimingUpdated;
    public event Action<enStrikeCommand> OnStrikeExecuted;
    public event Action OnStrikeCanceled;


    private readonly DragInputSurface _dragSurface;

    private Vector2 _startDragPosition;
    private const float MAX_DRAG_DISTANCE_PIXELS = 300f; // Screen pixels needed for 100% power
    private const float MIN_POWER_THRESHOLD = 0.05f;     // Deadzone to prevent accidental taps


    public CueInputPresenter(DragInputSurface dragSurface)
    {
        _dragSurface = dragSurface;
    }

    event Action<enStrikeCommand> ICueInputListener.OnAimingUpdated
    {
        add
        {
            throw new NotImplementedException();
        }

        remove
        {
            throw new NotImplementedException();
        }
    }

    event Action<enStrikeCommand> ICueInputListener.OnStrikeExecuted
    {
        add
        {
            throw new NotImplementedException();
        }

        remove
        {
            throw new NotImplementedException();
        }
    }

    public void Start()
    {
        _dragSurface.OnDragStarted += HandleDragStart;
        _dragSurface.OnDragMoved += HandleDragMoved;
        _dragSurface.OnDragEnded += HandleDragEnded;
    }


    private void HandleDragStart(Vector2 screenPosition)
    {
        _startDragPosition = screenPosition;
    }


    private void HandleDragMoved(Vector2 screenPosition)
    {
        enStrikeCommand currentCommand = CalculateStrike(screenPosition);
        OnAimingUpdated?.Invoke(currentCommand);
    }


    private void HandleDragEnded(Vector2 screenPosition)
    {
        enStrikeCommand finalCommand = CalculateStrike(screenPosition);


        // Deadzone check: If they barely pulled back, cancel the shot
        if (finalCommand.Power < MIN_POWER_THRESHOLD)
        {
            OnStrikeCanceled?.Invoke();
        }
        else
        {
            OnStrikeExecuted?.Invoke(finalCommand);
        }
    }


    private enStrikeCommand CalculateStrike(Vector2 currentScreenPos)
    {
        // 1. Calculate the vector pulling AWAY from the start point
        // (If you pull down, the ball should go up, so we reverse it)
        Vector2 dragVector = _startDragPosition - currentScreenPos;


        // 2. Calculate Power (Magnitude)
        float dragDistance = dragVector.magnitude;
        float power = Mathf.Clamp01(dragDistance / MAX_DRAG_DISTANCE_PIXELS);


        // 3. Calculate Angle (Atan2 returns Radians, we convert to Degrees)
        // Note: In Unity 3D space, X is right/left, Z is forward/back.
        // We map screen X to world X, and screen Y to world Z.
        float angleRadians = Mathf.Atan2(dragVector.y, dragVector.x);
        float angleDegrees = angleRadians * Mathf.Rad2Deg;


        // Ensure angle is always positive (0 to 360)
        if (angleDegrees < 0) angleDegrees += 360f;


        return new enStrikeCommand
        {
            Angle = angleDegrees,
            Power = power
        };
    }

    public void Dispose()
    {
        if (_dragSurface != null)
        {
            _dragSurface.OnDragStarted -= HandleDragStart;
            _dragSurface.OnDragMoved -= HandleDragMoved;
            _dragSurface.OnDragEnded -= HandleDragEnded;
        }
    }
}