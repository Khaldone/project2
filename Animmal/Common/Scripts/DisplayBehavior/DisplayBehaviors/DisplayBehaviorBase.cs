using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
namespace Animmal
{
    public abstract partial  class DisplayBehaviorBase : MonoBehaviour
    {
        public bool AutoHide;
        [Tooltip("Only used if AutoHide is toggled.")]
        [Min(0)]
        public float AutoHideTriggerDelay;
        [Tooltip("Enable to make sure animation can call auto hide when time-scale is 0. FE: Game pause scenario")]
        public bool AutoHideTimescaleIndependant;

        [Tooltip("Called when IDisplayable Show event is called.")]
        public UnityEvent OnHideStarted = new UnityEvent();
        [Tooltip("Called when IDisplayable Hide event is called or Autohide behavior is enabled.")]
        public UnityEvent OnShowStarted = new UnityEvent();

        [Tooltip("GameObject that has IDisplayable interface")]
        public GameObject DisplayableGameObject;
        public IDisplayable _Displayable;
        protected IDisplayable Displayable
        {
            get
            {
                if (_Displayable == null)
                {
                    _Displayable = DisplayableGameObject == null ? GetComponent<IDisplayable>() : DisplayableGameObject.GetComponent<IDisplayable>();
                }

                return _Displayable;
            }
        }

        public enum CURRENTSTATE { Idle, Showing, Hiding };
        public CURRENTSTATE CurrentState { get; set; }

        IEnumerator AutoHideCoroutine;

        protected float WaitForAutoHideTimer = 0;
        protected WaitForEndOfFrame WaitForEndOfFrame = new WaitForEndOfFrame();
        protected virtual void Awake()
        {
            Init();
        }

        protected virtual void Init()
        {
            if (Displayable == null)
            {
                Debug.LogError("Animmal: " + this.name + " on gameobject " + gameObject.name + "Displayable not set.");
                return;
            }
            Displayable.OnShowEvent.AddListener(ShowItem);
            Displayable.OnHideEvent.AddListener(HideItem);
        }

        protected virtual void ShowItem(bool _Instant)
        {
            EnsureGameObjectIsActive();
            
            OnShowStarted.Invoke();
            if (AutoHide)
            {
                if (AutoHideCoroutine != null)
                    StopCoroutine(AutoHideCoroutine);

                AutoHideCoroutine = DelayedHide();
                StartCoroutine(AutoHideCoroutine);
            }
        }
        protected virtual void EnsureGameObjectIsActive()
        {
            if (DisplayableGameObject != null && DisplayableGameObject.activeInHierarchy == false)
            {
                DisplayableGameObject.SetActive(true);
            }
            else if (gameObject != null && gameObject.activeInHierarchy == false)
            {
                gameObject.SetActive(true);
            }
        }

        

        protected IEnumerator DelayedHide()
        {
            WaitForAutoHideTimer = 0;
            while (WaitForAutoHideTimer < AutoHideTriggerDelay)
            {
                WaitForAutoHideTimer += AutoHideTimescaleIndependant ? Time.unscaledDeltaTime : Time.deltaTime;
                yield return null;
            }
            HideItem(false);
        }

        protected virtual void HideItem(bool _Instant)
        {
            if (IsGameobjectDeActivated())
            {
                OnHideStarted.Invoke();
                Displayable.HidingFinished();
                return;
            }

            OnHideStarted.Invoke();
            Displayable.HidingFinished();
        }
        
        protected virtual bool IsGameobjectDeActivated()
        {
            if (DisplayableGameObject != null && DisplayableGameObject.activeInHierarchy == false)
            {
                return true;
            }
            else if (gameObject != null && gameObject.activeInHierarchy == false)
            {
                return true;
            }

            return false;
        }

        public virtual IEnumerator HidingFinishedDelayed()
        {
            CurrentState = CURRENTSTATE.Idle;
            yield return WaitForEndOfFrame;
            Displayable.HidingFinished();
        }
    }
}