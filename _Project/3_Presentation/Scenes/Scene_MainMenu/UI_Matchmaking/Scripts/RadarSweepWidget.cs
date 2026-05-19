// Attached to: Radar_Spinner_VFX
using UnityEngine;


public class RadarSweepWidget : MonoBehaviour
{
    [SerializeField] private RectTransform _radarSweepGraphic;
    [SerializeField] private float _spinSpeed = -150f;
    private bool _isSearching = true;

    private void Update()
    {
        if (_isSearching)
        {
            _radarSweepGraphic.Rotate(0, 0, _spinSpeed * Time.deltaTime);
        }
    }


    // An Animation Event or the View can call this to stop the spinning when a match is found
    public void StopSweep()
    {
        _isSearching = false;
    }
}