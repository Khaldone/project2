using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Base class for all menu screens with slide-in/slide-out animations and state management.
/// </summary>
public class SsBaseMenu : MonoBehaviour
{
    /// <summary>
    /// Enumeration of directions from which a menu can appear.
    /// </summary>
    public enum enFromDirections
    {
        invalid = -1,
        fromRight,
        fromLeft,
        fromTop,
        fromBottom
    }

    /// <summary>
    /// Enumeration of directions to which a menu can disappear.
    /// </summary>
    public enum enToDirections
    {
        invalid = -1,
        toLeft,
        toRight,
        toTop,
        toBottom
    }

    /// <summary>
    /// Enumeration of possible menu states during animation and display.
    /// </summary>
    public enum enStates
    {
        invalid,
        hidden,
        comingIn,
        visibleAndIdle,
        goingOut
    }

    /// <summary>
    /// Enumeration of initial states for the menu when the scene starts.
    /// </summary>
    public enum enStartUpenStates
    {
        snapOffscreenThenHide,
        hideImmediate,
        showAndAnimateOntoScreen,
        showAndSnapOntoScreen,
        doNothing
    }

    //Menu State
    //Current state of the menu (hidden, comingIn, visibleAndIdle, goingOut).
    public enStates State => state;
    [Tooltip("Last direction from which the menu appeared.")]
    public enFromDirections LastInDirection => lastInDirection;
    [Tooltip("Last direction to which the menu disappeared.")]
    public enToDirections LastOutDirection => lastOutDirection;

    [Header("Animation Positions")]
    [Tooltip("Starting position for menu animation.")]
    Vector2 startPos = Vector2.zero;
    [Tooltip("Ending position for menu animation.")]
    Vector2 endPos = Vector2.zero;
    [Tooltip("Last direction from which the menu entered.")]
    enFromDirections lastInDirection = enFromDirections.invalid;
    [Tooltip("Last direction to which the menu exited.")]
    enToDirections lastOutDirection = enToDirections.invalid;

    [Header("Menu References")]
    [Tooltip("Parent canvas containing this menu.")]
    protected Canvas parentCanvas;
    protected int parentInitSortingOrder;
    [Tooltip("RectTransform component for menu positioning and animation.")]
    [SerializeField] protected RectTransform rectTransform;
    
    [Header("Animation Settings")]
    [Tooltip("Duration in seconds for menu slide-in animation.")]
    protected float inDuration = 0.3f;
    [Tooltip("Duration in seconds for menu slide-out animation.")]
    protected float outDuration = 0.3f;
    [Tooltip("Initial state of the menu when the scene starts.")]
    [SerializeField] protected enStartUpenStates startUpState;
    [Tooltip("Default direction from which the menu enters.")]
    [Space(10f)]
    [SerializeField] protected enFromDirections enterDirection;
    [Tooltip("Default direction to which the menu exits.")]
    [SerializeField] protected enToDirections exitDirection;
    [Tooltip("Whether to override default animation durations.")]
    [SerializeField] protected bool overrideDurations;
    [Tooltip("Custom duration for slide-in animation when override is enabled.")]
    [SerializeField] protected float overrideInDuration = 0.3f;
    [Tooltip("Custom duration for slide-out animation when override is enabled.")]
    [SerializeField] protected float overrideOutDuration = 0.3f;

    [Header("UI Components")]
    [Tooltip("ScrollRect component for scrollable menu content.")]
    [SerializeField] ScrollRect scrollView;
    [Tooltip("Current state of the menu.")]
    enStates state;
    [Tooltip("Default anchor minimum position for the menu RectTransform.")]
    Vector2 defaultRectAnchorMin;
    [Tooltip("Default anchor maximum position for the menu RectTransform.")]
    Vector2 defaultRectAnchorMax;
    [Tooltip("Time elapsed in the current state.")]
    float stateTime;
    Button[] registeredButtons;
    bool buttonHandlersInitialized;

    [Header("Menu Override")]
    [Tooltip("Whether this menu should override the active menu system.")]
    protected bool overrideActiveMenu = false;

    [Header("UI Audio")]
    [SerializeField] protected bool playPanelTransitionSfx = true;
    [SerializeField] protected bool playButtonPressSfx = true;
    private bool isUiSfxConfigLoaded;

    [Space, Header("User Currencies (Parent)")]
    [Tooltip("Text component displaying the player's gold coins amount.")]
    [SerializeField] protected TextMeshProUGUI goldCoinsText;
    [Tooltip("Text component displaying the player's cash amount.")]
    [SerializeField] protected TextMeshProUGUI cashText;
    
    /// <summary>
    /// Subscribes to currency update events when the menu is enabled.
    /// </summary>
    private void OnEnable()
    {
        //ss_man_CurrencyManager.OnGetUserCurrencies += OnGetUserCurrencies;
        InitializeButtonAudioHandlers();
    }

    /// <summary>
    /// Handles currency updates and updates the UI text components.
    /// </summary>
    /// <param name="goldCurrency">The current gold currency amount.</param>
    /// <param name="cashCurrency">The current cash currency amount.</param>
    private void OnGetUserCurrencies(int goldCurrency, int cashCurrency)
    {
        if (goldCoinsText)
        {
            goldCoinsText.SetText($"{goldCurrency}");
        }
        if (cashText)
        {
            cashText.SetText($"{cashCurrency}");
        }
    }

    /// <summary>
    /// Unsubscribes from currency update events when the menu is disabled.
    /// </summary>
    private void OnDisable()
    {
        //ss_man_CurrencyManager.OnGetUserCurrencies -= OnGetUserCurrencies;
    }

    private void OnDestroy()
    {
        UnregisterButtonAudioHandlers();
    }
    
    /// <summary>
    /// Initializes the base menu component, sets up RectTransform references and animation durations.
    /// </summary>
    public virtual void Awake()
    {
        parentCanvas = base.gameObject.GetComponentInParent<Canvas>(includeInactive: true);
        parentInitSortingOrder = parentCanvas.sortingOrder;
        if (rectTransform == null) rectTransform = base.gameObject.GetComponent<RectTransform>();

        if (rectTransform != null)
        {
            defaultRectAnchorMin = rectTransform.anchorMin;
            defaultRectAnchorMax = rectTransform.anchorMax;
        }
        if (overrideDurations)
        {
            inDuration = overrideInDuration;
            outDuration = overrideOutDuration;
        }

        InitializeButtonAudioHandlers();

    }

    /// <summary>
    /// Initializes the menu's initial state based on startUpState setting.
    /// </summary>
    public virtual void Start()
    {

        switch (startUpState)
        {
            case enStartUpenStates.hideImmediate:
                if (startUpState == enStartUpenStates.hideImmediate)
                {
                    HideImmediate();
                }
                break;

            case enStartUpenStates.snapOffscreenThenHide:
                if (rectTransform != null)
                {
                    Vector2 b = new Vector2(1f, 0f);
                    rectTransform.anchorMin = defaultRectAnchorMin + b;
                    rectTransform.anchorMax = defaultRectAnchorMax + b;
                }
                StartCoroutine(HideMenuInSeconds(0.1f));
                break;

            case enStartUpenStates.showAndSnapOntoScreen:
                Show(enFromDirections.invalid, snap: true);
                break;

            case enStartUpenStates.showAndAnimateOntoScreen:
                Show(enFromDirections.invalid, snap: false);
                break;
        }

    }

    /// <summary>
    /// Sets the menu's current state and resets the state timer.
    /// </summary>
    /// <param name="newState">The new state to set.</param>
    protected virtual void SetState(enStates newState)
    {
        state = newState;
        stateTime = 0;
    }

    /// <summary>
    /// Handles connection timeout by hiding the menu if it's currently visible.
    /// </summary>
    protected virtual void OnConnectionTimeout()
    {
        if (state == enStates.visibleAndIdle)
        {
            Hide(true, enToDirections.toRight);
        }
    }

    /// <summary>
    /// Shows the menu with slide-in animation from the specified direction.
    /// </summary>
    /// <param name="fromDirection">The direction from which the menu should appear.</param>
    /// <param name="snap">Whether to snap the menu into position immediately without animation.</param>
    protected virtual void Show(enFromDirections fromDirection, bool snap)
    {
        if (state == enStates.comingIn || state == enStates.visibleAndIdle)
        {
            return;
        }
        //base.gameObject.SetActive(value: true);//Changes By Jahanzaib For testing
        rectTransform.gameObject.SetActive(value: true);//Changes By Jahanzaib For testing

        if (snap)
        {
            SetState(enStates.visibleAndIdle);
        }
        else
        {
            SetState(enStates.comingIn);
        }
        endPos = Vector2.zero;
        if (fromDirection == enFromDirections.invalid)
        {
            fromDirection = enterDirection;
        }
        lastInDirection = fromDirection;
        startPos = Vector2.zero;
        switch (fromDirection)
        {
            case enFromDirections.fromLeft:
                startPos = new Vector2(-1f, 0f);
                break;
            case enFromDirections.fromRight:
                startPos = new Vector2(1f, 0f);
                break;
            case enFromDirections.fromTop:
                startPos = new Vector2(0f, 1f);
                break;
            case enFromDirections.fromBottom:
                startPos = new Vector2(0f, -1f);
                break;
        }
        if (rectTransform != null)
        {
            if (snap)
            {
                rectTransform.anchorMin = defaultRectAnchorMin + endPos;
                rectTransform.anchorMax = defaultRectAnchorMax + endPos;
            }
            else
            {
                rectTransform.anchorMin = defaultRectAnchorMin + startPos;
                rectTransform.anchorMax = defaultRectAnchorMax + startPos;
            }
        }

    }

    /// <summary>
    /// Hides the menu with slide-out animation to the specified direction.
    /// </summary>
    /// <param name="snap">Whether to hide the menu immediately without animation.</param>
    /// <param name="outDirection">The direction to which the menu should disappear.</param>
    protected virtual void Hide(bool snap, enToDirections outDirection)
    {

        if (state == enStates.goingOut || state == enStates.hidden)
        {
            return;
        }
        if (snap)
        {
            HideImmediate();
            return;
        }
        SetState(enStates.goingOut);

        if (rectTransform != null)
        {
            startPos = rectTransform.anchorMin - defaultRectAnchorMin;
        }
        else
        {
            startPos = Vector2.zero;
        }
        if (outDirection == enToDirections.invalid)
        {
            outDirection = exitDirection;
        }
        lastOutDirection = outDirection;
        switch (outDirection)
        {
            case enToDirections.toLeft:
                endPos = new Vector2(-1f, 0f);
                break;
            case enToDirections.toRight:
                endPos = new Vector2(1f, 0f);
                break;
            case enToDirections.toTop:
                endPos = new Vector2(0f, 1f);
                break;
            case enToDirections.toBottom:
                endPos = new Vector2(0f, -1f);
                break;
        }

    }

    /// <summary>
    /// Immediately hides the menu without animation and sets state to hidden.
    /// </summary>
    public virtual void HideImmediate()
    {
        //base.gameObject.SetActive(value: false);//Changes By Jahanzaib For testing
        rectTransform.gameObject.SetActive(value: false);//Changes By Jahanzaib For testing
        SetState(enStates.hidden);
        parentCanvas.sortingOrder = -1;
    }

    /// <summary>
    /// Shows the menu sliding in from the left side.
    /// </summary>
    public void ShowFromLeft()
    {
        Show(enFromDirections.fromLeft, snap: false);
        Debug.Log("Showing from Left");
    }

    /// <summary>
    /// Shows the menu sliding in from the right side.
    /// </summary>
    public void ShowFromRight()
    {
        Show(enFromDirections.fromRight, snap: false);
        Debug.Log("Showing from Right");

    }

    /// <summary>
    /// Shows the menu sliding in from the bottom.
    /// </summary>
    public void ShowFromBottom()
    {
        Show(enFromDirections.fromBottom, snap: false);
        Debug.Log("Showing from Bottom");

    }

    /// <summary>
    /// Shows the menu sliding in from the top.
    /// </summary>
    public void ShowFromTop()
    {
        Show(enFromDirections.fromTop, snap: false);
        Debug.Log("Showing from Top");

    }

    /// <summary>
    /// Shows the menu immediately without animation at its default position.
    /// </summary>
    public void JustShow()
    {
        Show(enFromDirections.invalid, snap: true);
        parentCanvas.sortingOrder = parentInitSortingOrder;
    }
    
    /// <summary>
    /// Hides the menu sliding out to the left side.
    /// </summary>
    public void HideToLeft()
    {
        Hide(snap: false, enToDirections.toLeft);
    }

    /// <summary>
    /// Hides the menu sliding out to the right side.
    /// </summary>
    public void HideToRight()
    {
        Hide(snap: false, enToDirections.toRight);
    }

    /// <summary>
    /// Hides the menu sliding out to the bottom.
    /// </summary>
    public void HideToBottom()
    {
        Hide(snap: false, enToDirections.toBottom);
    }

    /// <summary>
    /// Hides the menu sliding out to the top.
    /// </summary>
    public void HideToTop()
    {
        Hide(snap: false, enToDirections.toTop);
    }

    /// <summary>
    /// Updates the menu state and animation each frame.
    /// </summary>
    protected virtual void Update()
    {

        UpdateState(Time.deltaTime);

    }

    /// <summary>
    /// Updates the menu's animation state based on elapsed time.
    /// </summary>
    /// <param name="dt">Delta time since last frame.</param>
    void UpdateState(float dt)
    {
        Vector2 a = endPos - startPos;
        float num;
        stateTime += dt;
        switch (state)
        {
            case enStates.comingIn:
                num = Mathf.Clamp(stateTime / inDuration, 0f, 1f);
                num = Tweener.LinearEaseIn(num, 0f, 1f, 1f);
                if (rectTransform != null)
                {
                    rectTransform.anchorMin = defaultRectAnchorMin + startPos + a * num;
                    rectTransform.anchorMax = defaultRectAnchorMax + startPos + a * num;
                }
                if (num >= 1f)
                {
                    parentCanvas.sortingOrder = parentInitSortingOrder;
                    SetState(enStates.visibleAndIdle);

                }
                break;
            case enStates.goingOut:
                num = Mathf.Clamp(stateTime / outDuration, 0f, 1f);
                num = Tweener.LinearEaseInOut(num, 0f, 1f, 1f);
                if (rectTransform != null)
                {
                    rectTransform.anchorMin = defaultRectAnchorMin + startPos + a * num;
                    rectTransform.anchorMax = defaultRectAnchorMax + startPos + a * num;
                }
                if (num >= 1f)
                {
                    HideImmediate();
                    
                }
                break;
        }

    }

    /// <summary>
    /// Coroutine that hides the menu after a specified delay.
    /// </summary>
    /// <param name="timeBeforeHide">Time in seconds to wait before hiding the menu.</param>
    /// <returns>IEnumerator for coroutine execution.</returns>
    protected IEnumerator HideMenuInSeconds(float timeBeforeHide)
    {

        yield return new WaitForSeconds(timeBeforeHide);

        HideImmediate();
    }

    private void InitializeButtonAudioHandlers()
    {
        if (!playButtonPressSfx || buttonHandlersInitialized)
        {
            return;
        }

        RegisterButtonAudioHandlers();
        buttonHandlersInitialized = true;
    }

    private void RegisterButtonAudioHandlers()
    {
        if (!playButtonPressSfx)
        {
            return;
        }

        UnregisterButtonAudioHandlers();
        registeredButtons = GetComponentsInChildren<Button>(true);
        for (int i = 0; i < registeredButtons.Length; i++)
        {
            var button = registeredButtons[i];
            if (button != null)
            {
                //button.onClick.AddListener(OnAnyButtonClicked);
            }
        }
    }

    private void UnregisterButtonAudioHandlers()
    {
        if (registeredButtons == null)
        {
            return;
        }

        for (int i = 0; i < registeredButtons.Length; i++)
        {
            var button = registeredButtons[i];
            if (button != null)
            {
                //button.onClick.RemoveListener(OnAnyButtonClicked);
            }
        }
    }





}