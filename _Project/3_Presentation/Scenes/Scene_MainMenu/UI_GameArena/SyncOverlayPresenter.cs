// Assets/_Project/3_Presentation/UI_GameArena/SyncOverlayPresenter.cs
using UnityEngine;


public class SyncOverlayPresenter : MonoBehaviour
{
    [SerializeField] private CanvasGroup _curtainCanvas;


    private void Awake()
    {
        //MessageBroker.Instance.Subscribe<AppResumedMessage>(OnAppResumed);
        //MessageBroker.Instance.Subscribe<StateSuccessfullyHydratedMessage>(OnStateHydrated);
    }


    //private void OnAppResumed(AppResumedMessage msg)
    //{
    //    // Instantly turn the screen black with a "Syncing..." spinner.
    //    // This hides the ugly network fast-forwarding from the user's eyes.
    //    _curtainCanvas.alpha = 1.0f;
    //    _curtainCanvas.blocksRaycasts = true;
    //}


    //private void OnStateHydrated(StateSuccessfullyHydratedMessage msg)
    //{
    //    // The Core Domain has confirmed the state is mathematically perfect again.
    //    // Fade the curtain away smoothly.
    //    StartCoroutine(FadeOutCurtain());
    //}
}
