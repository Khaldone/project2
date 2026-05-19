// Assets/_Project/CoreDomain/UI/ITutorialView.cs
using System;
public interface ITutorialView
{
    event Action OnNextButtonClicked;
    event Action OnSkipButtonClicked;
    void ShowTutorialPanel(bool isVisible);
    void UpdateInstructions(string text);
    void ShowHighlightMask(bool isVisible); // Dims the screen except for a specific UI element
}