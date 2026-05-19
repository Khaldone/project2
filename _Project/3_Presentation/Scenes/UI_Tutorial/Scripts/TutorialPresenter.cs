// Inside Assets/_Project/Scenes/UI_Tutorial/Scripts/TutorialPresenter.cs

// Assets/_Project/Scenes/UI_Tutorial/Scripts/TutorialPresenter.cs
using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer.Unity;


public class TutorialPresenter : IStartable, IDisposable
{
    private readonly ITutorialView _view;
    private readonly IMessageBroker _broker;
    private readonly IUIRouter _uiRouter; // To close the scene when finished

    private readonly ICueInputListener _inputListener; // The input abstraction we built
    private readonly IFTUETracker _ftueTracker;

    private int _tutorialStep = 0;


    // The sequence of the tutorial
    private readonly List<TutorialStepData> _tutorialSteps;
    private int _currentStepIndex = 0;
    private float _tutorialStartTime;

    public TutorialPresenter(ITutorialView view, IMessageBroker broker, IUIRouter uiRouter, ICueInputListener inputListener, IFTUETracker ftueTracker, IUIRouter router)
    {
        _view = view;
        _broker = broker;
        _uiRouter = uiRouter;

        _inputListener = inputListener;
        _ftueTracker = ftueTracker;

        // In a massive AAA game, this list would be passed in from a TutorialOrchestrator.
        // For standard UI flows, defining it in the Presenter is acceptable.
        _tutorialSteps = new List<TutorialStepData>
        {
            new TutorialStepData { StepIndex = 1, InternalName = "Learn_Camera", DisplayText = "Swipe the screen to look around the table.", RequiresGameplay = false },
            new TutorialStepData { StepIndex = 2, InternalName = "Learn_Power", DisplayText = "Pull back the cue stick to set your power.", RequiresGameplay = false },
            new TutorialStepData { StepIndex = 3, InternalName = "Learn_Strike", DisplayText = "Release to strike the cue ball!", RequiresGameplay = true }
        };
    }

    public void Start()
    {
        // 1. Start the stopwatch the millisecond the scene loads
        _tutorialStartTime = Time.realtimeSinceStartup;

        // 2. Wire up the user input
        _view.OnNextButtonClicked += AdvanceTutorial;
        _view.OnSkipButtonClicked += SkipTutorial;


        // 3. Start the flow
        _currentStepIndex = 0;
        ApplyCurrentStep();


        // 4. ANALYTICS: Track that the player actually started the FTUE
        _broker.Publish(new TutorialStepMessage
        {
            StepIndex = 0,
            StepName = "Tutorial_Started"
        });

        _inputListener.OnAimingUpdated += HandleAiming;
        _inputListener.OnStrikeExecuted += HandleStrike;


        // Step 0: Lock the screen, focus the spotlight on the cue ball
        AdvanceTutorial();
    }

    private void AdvanceTutorial()
    {
        // 1. ANALYTICS: Track the completion of the *current* step before moving on
        TutorialStepData completedStep = _tutorialSteps[_currentStepIndex];

        _broker.Publish(new TutorialStepMessage
        {
            StepIndex = completedStep.StepIndex,
            StepName = completedStep.InternalName
        });

        // 2. Increment the state
        _currentStepIndex++;

        // 3. Check if we are finished
        if (_currentStepIndex >= _tutorialSteps.Count)
        {
            FinishTutorial();
        }
        else
        {
            ApplyCurrentStep();
        }

        _tutorialStep++;


        switch (_tutorialStep)
        {
            case 1:
                // Find the cue ball's 3D position and map it to the 2D screen UI
                //Vector2 cueBallScreenPos = GetScreenPositionOfCueBall();

                //_view.MoveSpotlightTo(cueBallScreenPos);
                //_view.ShowInstructionText("Pull back to aim.");
                break;

            case 2:
                //_view.ShowInstructionText("Release to strike!");
                break;

            case 3:
                //_view.FadeOutOverlay();
                //_view.ShowInstructionText("Great shot! You're ready.");
                CompleteTutorial();
                break;
        }
    }

    private void ApplyCurrentStep()
    {
        TutorialStepData currentStep = _tutorialSteps[_currentStepIndex];

        _view.UpdateInstructions(currentStep.DisplayText);

        // Example logic: Only show the highlight mask on the first step
        _view.ShowHighlightMask(currentStep.StepIndex == 1);
    }

    private void SkipTutorial()
    {
        // ANALYTICS: Track if players find the tutorial annoying and skip it early
        _broker.Publish(new TutorialStepMessage
        {
            StepIndex = 999,
            StepName = "Tutorial_Skipped_Early"
        });


        FinishTutorial();
    }

    private void FinishTutorial()
    {
        _view.ShowTutorialPanel(false);


        // 2. Stop the stopwatch and calculate the difference
        float totalTimeElapsed = Time.realtimeSinceStartup - _tutorialStartTime;

        // 3. Drop the envelope in the mail with the new data
        _broker.Publish(new TutorialStepMessage
        {
            StepIndex = 1000,
            StepName = "Tutorial_Fully_Completed",
            TimeSpentSeconds = Mathf.RoundToInt(totalTimeElapsed) // Send as a clean integer
        });


        _uiRouter.CloseMenuAsync("UI_Tutorial");
    }


    private void HandleAiming(enStrikeCommand cmd)
    {
        if (_tutorialStep == 1 && cmd.Power > 0.2f)
        {
            // They pulled back far enough! Move to the next text prompt.
            AdvanceTutorial();
        }
    }


    private void HandleStrike(enStrikeCommand cmd)
    {
        if (_tutorialStep == 2)
        {
            // They fired the shot. The physics engine handles the rest.
            // (In a real implementation, you'd wait for the 'MatchConcluded' message here)
            AdvanceTutorial();
        }
    }


    private async void CompleteTutorial()
    {
        // 1. Tell PlayFab they finished, so they never see this again
        await _ftueTracker.MarkTutorialCompleteAsync();


        // 2. Tear down the Arena and load the Main Menu
        //_uiRouter.LoadSceneAsync("01_Spoke_MainMenu");
    }


    public void Dispose()
    {
        _inputListener.OnAimingUpdated -= HandleAiming;
        _inputListener.OnStrikeExecuted -= HandleStrike;

        // Golden Rule: Always unsubscribe when the scene unloads
        if (_view != null)
        {
            _view.OnNextButtonClicked -= AdvanceTutorial;
            _view.OnSkipButtonClicked -= SkipTutorial;
        }
    }
}