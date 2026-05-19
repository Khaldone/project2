// Attached to: TutorialCanvas
using System;
using UnityEngine;
using TMPro;


public class TutorialView : MonoBehaviour, ITutorialView
{
    [SerializeField] private SpotlightMaskWidget _maskWidget;
    [SerializeField] private TextMeshProUGUI _promptText;
    [SerializeField] private Animator _handPointerAnimator;

    public event Action OnNextButtonClicked;
    public event Action OnSkipButtonClicked;

    public void MoveSpotlightTo(Vector2 screenPosition)
    {
        // Tell the mask to cut a hole at this exact pixel coordinate
        _maskWidget.SetHolePosition(screenPosition);

        // Move the floating pointing hand next to the hole
        _handPointerAnimator.transform.position = screenPosition + new Vector2(100, -100);
    }

    public void ShowInstructionText(string text)
    {
        _promptText.text = text;

        // Trigger a slight pop animation on the text
        _promptText.GetComponent<Animator>().SetTrigger("Pop");
    }

    public void FadeOutOverlay()
    {
        // The Presenter calls this when the tutorial is complete
        GetComponent<Animator>().SetTrigger("FadeOut");
    }

    public void ShowTutorialPanel(bool isVisible)
    {
        throw new NotImplementedException();
    }

    public void UpdateInstructions(string text)
    {
        throw new NotImplementedException();
    }

    public void ShowHighlightMask(bool isVisible)
    {
        throw new NotImplementedException();
    }
}