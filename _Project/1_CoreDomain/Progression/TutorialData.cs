// Assets/_Project/CoreDomain/Progression/TutorialData.cs
public struct TutorialStepData
{
    public int StepIndex;
    public string InternalName;   // e.g., "Learn_Aiming" (Used for Analytics)
    public string DisplayText;    // e.g., "Drag your finger to aim the cue."
    public bool RequiresGameplay; // Does this step require them to actually hit a ball?
}