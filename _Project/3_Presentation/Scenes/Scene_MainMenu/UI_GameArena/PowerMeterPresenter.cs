// Assets/_Project/3_Presentation/UI_GameArena/PowerMeterPresenter.cs
using UnityEngine;
using UnityEngine.EventSystems;


public class PowerMeterPresenter : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("Dependencies")]
    [SerializeField] private RectTransform _meterHandle;
    [SerializeField] private RectTransform _meterBackground;
    [SerializeField] private CueVisualizer _cueVisualizer; // Handles the 3D stick model


    private PowerMeterOrchestrator _orchestrator;
    private Vector2 _dragStartPosition;
    private float _maxDragHeight;


    public void Inject(PowerMeterOrchestrator orchestrator)
    {
        _orchestrator = orchestrator;
        _maxDragHeight = _meterBackground.rect.height;
    }


    public void OnPointerDown(PointerEventData eventData)
    {
        _dragStartPosition = eventData.position;
    }


    public void OnDrag(PointerEventData eventData)
    {
        // Calculate how far down they dragged their finger
        float dragDistance = _dragStartPosition.y - eventData.position.y;

        // Ask the Core Domain to evaluate this input
        ShotPowerResult result = _orchestrator.CalculateShotPower(dragDistance, _maxDragHeight);


        // Update the 2D UI Handle position
        _meterHandle.anchoredPosition = new Vector2(0, -result.NormalizedPullback * _maxDragHeight);


        // Tell the 3D Presentation script to physically pull the cue stick backward
        _cueVisualizer.UpdateVisualPullback(result.NormalizedPullback);


        // Play a clicking sound if they enter the Sweet Spot
        if (result.HitSweetSpot)
        {
            //MessageBroker.Instance.Publish(new PlayAudioMessage("SFX_Meter_Max"));
        }
    }


    public void OnPointerUp(PointerEventData eventData)
    {
        float dragDistance = _dragStartPosition.y - eventData.position.y;
        ShotPowerResult result = _orchestrator.CalculateShotPower(dragDistance, _maxDragHeight);


        // The player released the shot! Reset visuals and command the network.
        _meterHandle.anchoredPosition = Vector2.zero;
        //_cueVisualizer.PlayStrikeAnimation();


        // Pass the final, approved math to the Network wrapper
        //MessageBroker.Instance.Publish(new ExecuteShotCommand(result.AppliedImpulse));
    }

    //private void OnTurnStateChanged(MappedTurnStateMessage msg)
    //{
    //    if (msg.IsMyTurn)
    //    {
    //        // 1. Enable Raycasts (allow dragging)
    //        _canvasGroup.blocksRaycasts = true;

    //        // 2. Fade the meter in
    //        FadeMeterTo(1.0f);
    //    }
    //    else
    //    {
    //        // 1. Disable human input!
    //        _canvasGroup.blocksRaycasts = false;

    //        // 2. Fade the meter out, or switch it to "Ghost Mode" to watch the opponent's network inputs
    //        FadeMeterTo(0.0f);
    //    }
    //}

}
