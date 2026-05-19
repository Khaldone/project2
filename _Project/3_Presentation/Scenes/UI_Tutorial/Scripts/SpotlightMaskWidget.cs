// Attached to: Dark_Mask_Overlay
using UnityEngine;
using UnityEngine.UI;


[RequireComponent(typeof(Image))]
public class SpotlightMaskWidget : MonoBehaviour, ICanvasRaycastFilter
{
    [SerializeField] private float _holeRadius = 150f;
    private Vector2 _holeCenterScreenPos;
    private Material _maskMaterial;


    private void Awake()
    {
        // We use a custom unlit shader that draws a transparent circle based on a Vector2 parameter
        _maskMaterial = GetComponent<Image>().material;
    }


    public void SetHolePosition(Vector2 screenPos)
    {
        _holeCenterScreenPos = screenPos;

        // Pass the coordinates to the Shader to visually cut the hole
        _maskMaterial.SetVector("_HolePosition", screenPos);
        _maskMaterial.SetFloat("_HoleRadius", _holeRadius);
    }


    // THIS IS THE MAGIC METHOD
    // Unity's UI system calls this every time the player touches the screen
    public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
    {
        // Calculate how far the player's tap is from the center of our Spotlight hole
        float distanceFromHole = Vector2.Distance(sp, _holeCenterScreenPos);


        if (distanceFromHole <= _holeRadius)
        {
            // The player tapped INSIDE the hole.
            // Return FALSE. This tells this UI panel to ignore the touch,
            // allowing the touch to pass perfectly down to the GameArena's CueInputListener!
            return false;
        }
        else
        {
            // The player tapped the black overlay.
            // Return TRUE. This UI panel eats the touch, preventing the player
            // from clicking the Pause menu or swiping the wrong ball.
            return true;
        }
    }
}
