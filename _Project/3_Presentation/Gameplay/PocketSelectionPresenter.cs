// Assets/_Project/3_Presentation/Gameplay/PocketSelectionPresenter.cs
using UnityEngine;
using System.Collections.Generic;


public class PocketSelectionPresenter : MonoBehaviour
{
    [Header("Pocket 3D Colliders")]
    [SerializeField] private List<Collider> _pocketRaycastColliders; // The 6 invisible triggers
    [SerializeField] private List<GameObject> _pocketHighlightVisuals; // The glowing rings


    private bool _isMyTurn;
    private bool _isAwaitingCall;
    [SerializeField] private List<BoxCollider2D> _pocket2DColliders;


    private void Awake()
    {
        // Listen to the Perspective Mapper (Am I the active player?)
        //MessageBroker.Instance.Subscribe<MappedTurnStateMessage>(OnTurnStateChanged);

        // Listen to the Core Domain Rules (Does the 8-ball require a call?)
        //MessageBroker.Instance.Subscribe<CallPocketRequestedMessage>(OnCallPocketRequested);

        // Listen for the final decision
        //MessageBroker.Instance.Subscribe<PocketSuccessfullyCalledMessage>(OnPocketCalled);


        // Default state: Everything off
        DisableAllPocketInteractions();
    }


    private void OnTurnStateChanged(MappedTurnStateMessage msg)
    {
        _isMyTurn = msg.IsMyTurn;
        EvaluateInteractionState();
    }


    private void OnCallPocketRequested(CallPocketRequestedMessage msg)
    {
        _isAwaitingCall = true;
        EvaluateInteractionState();
    }


    // THE SHIELD LOGIC
    private void EvaluateInteractionState()
    {
        // We ONLY enable the physics colliders if BOTH conditions are true:
        // 1. The game is paused waiting for a pocket.
        // 2. I am the one holding the cue stick right now.
        bool shouldAllowRaycasts = _isAwaitingCall && _isMyTurn;


        foreach (var col in _pocketRaycastColliders)
        {
            col.enabled = shouldAllowRaycasts;
        }


        if (shouldAllowRaycasts)
        {
            // Optionally trigger a subtle pulsing animation on all pockets to say "Click me!"
            PlayPulseAnimationOnAllPockets();
        }
    }


    //private void OnPocketCalled(PocketSuccessfullyCalledMessage msg)
    //{
    //    _isAwaitingCall = false;

    //    // 1. Turn off all colliders for EVERYONE. The decision is made.
    //    DisableAllPocketInteractions();


    //    // 2. Turn on the specific visual ring for the chosen pocket
    //    // This ensures the opponent sees the glowing ring even though they couldn't click it.
    //    _pocketHighlightVisuals[msg.PocketId].SetActive(true);
    //}


    private void DisableAllPocketInteractions()
    {
        foreach (var col in _pocketRaycastColliders) col.enabled = false;
        foreach (var vis in _pocketHighlightVisuals) vis.SetActive(false);
    }


    // Example of how Unity's Input System uses the collider
    private void Update()
    {
        // If the colliders are disabled, this Raycast will cleanly ignore the pockets.
        // The player can tap the screen a million times and nothing will happen.
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider.CompareTag("Pocket"))
                {
                    int pocketId = GetPocketIdFromCollider(hit.collider);

                    // Send the request to the network
                    //MessageBroker.Instance.Publish(new TryCallPocketCommand(pocketId));
                }
            }
        }

        // Check for mobile touch or mouse click
        if (Input.GetMouseButtonDown(0))
        {
            // Convert screen click to 2D world space
            Vector2 clickPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            // Cast a strict 2D ray
            RaycastHit2D hit = Physics2D.Raycast(clickPosition, Vector2.zero);


            if (hit.collider != null && hit.collider.CompareTag("Pocket"))
            {
                //int pocketId = GetPocketIdFromCollider(hit.collider);
                //MessageBroker.Instance.Publish(new TryCallPocketCommand(pocketId));
            }
        }

    }

    private int GetPocketIdFromCollider(Collider col) { return 0; /* logic omitted */ }
    private void PlayPulseAnimationOnAllPockets() { /* logic omitted */ }
}
