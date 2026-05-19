using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Animmal.NotificationSystem
{
    public class NotificationQueuer: INotificationQueuer
    {
        //Active item handling 
        protected List<NotificationStatus> _ActiveNotificationItems = new List<NotificationStatus>();
        public List<NotificationStatus> ActiveNotificationItems { get { return _ActiveNotificationItems; }}
        protected List<NotificationStatus> QueuedNotifications = new List<NotificationStatus>();
        protected NotificationDisplay NotificationDisplay = null;
        public UnityNotificationStatusEvent _OnNotificationReady = new UnityNotificationStatusEvent();

        public UnityNotificationStatusEvent OnNotificationReady { get { return _OnNotificationReady; } }

        protected IEnumerator RetriggerCoroutine;
        protected bool IsQueueRoutineRunning = false;
        protected float LastNotificationTime = -1;
        protected float NotificationQueueTimer = 0;
        public virtual void Init(NotificationDisplay _NotificationDisplay)
        {
            NotificationDisplay = _NotificationDisplay;
        }

        public virtual NotificationStatus AddToQueue(NotificationData _NotificationData)
        {
            if (CanQueueNotification() == false)
                return new NotificationStatus(NotificationStatusEnum.Skipped, null, null);
                
            NotificationStatus _NotificationStatus = new NotificationStatus(NotificationStatusEnum.Queued, _NotificationData, null);
            QueuedNotifications.Add(_NotificationStatus);
            StartQueueCheck();
            return _NotificationStatus;
        }

        protected virtual bool CanQueueNotification()
        {
            if (NotificationDisplay.QueuingEnabled == false
                && NotificationDisplay.MaxNotifications > 0
                && ActiveNotificationItems.Count + QueuedNotifications.Count >= NotificationDisplay.MaxNotifications )
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public virtual void OnDisplayEnabled()
        {
            if (QueuedNotifications.Count > 0)
                StartQueueCheck();
        }

        public virtual void OnDisplayDisabled()
        {
            if (RetriggerCoroutine != null)
               NotificationDisplay.StopCoroutine(RetriggerCoroutine);
        }

        public virtual void ItemHidingFinished(NotificationStatus _NotificationStatus)
        {
            if (ActiveNotificationItems.Contains(_NotificationStatus))
            {
                ActiveNotificationItems.Remove(_NotificationStatus);
            }
            StartQueueCheck();
        }

        public virtual void ClearQueues(int _VariationID)
        {
            if (_VariationID == -1)
            {
                QueuedNotifications.Clear();
            }
            else
            {
                List<NotificationStatus> _FilteredNotifications = new List<NotificationStatus>();
                for (int i = 0; i < QueuedNotifications.Count; i++)
                {
                    if (QueuedNotifications[i].NotificationData.StyleVariationID != _VariationID)
                        _FilteredNotifications.Add(QueuedNotifications[i]);
                }
                QueuedNotifications.Clear();
                QueuedNotifications.AddRange(_FilteredNotifications);
                _FilteredNotifications = null;
            }
        }

        protected virtual void StartQueueCheck()
        {
            if (IsQueueRoutineRunning == false)
            {
                float _CooldownTime = ActiveNotificationItems.Count == 0 ? 0 : NotificationDisplay.NotificationCooldownTime;
                RetriggerCoroutine = NotificationRetriggerCheckDelayed(_CooldownTime);
                NotificationDisplay.StartCoroutine(RetriggerCoroutine);
            }
        }
        
        
        /// <summary>
        /// Coroutine used to advance queue after cooldown 
        /// </summary>
        /// <param name="_Delay">Time until cooldown is finished</param>
        /// <returns></returns>
        protected virtual IEnumerator NotificationRetriggerCheckDelayed(float _Delay)
        {
            NotificationQueueTimer = 0;
            _Delay += 0.01f;
            while (NotificationQueueTimer < _Delay)
            {
                NotificationQueueTimer +=
                    NotificationDisplay.TimescaleIndependant ? Time.unscaledDeltaTime : Time.deltaTime;
                yield return null;
            }
            NotificationQueueCheck();
        }

        /// <summary>
        /// Check if display is ready to show queued item 
        /// </summary>
        /// <param name="_CheckAutoAdvance"></param>
        protected virtual void NotificationQueueCheck(bool _CheckAutoAdvance = true)
        {
            IsQueueRoutineRunning = true;
            if (NotificationDisplay.IsDisabled || NotificationDisplay.IsPaused)
            {
                IsQueueRoutineRunning = false;
                return;
            }

            if (QueuedNotifications.Count == 0)
            {
                IsQueueRoutineRunning = false;
                return;
            }

            // If display is on cooldown and we are allowed to queue launch coroutine to retry after the cooldown
            if (IsDisplayOnCooldown())
            {
                RetriggerCoroutine = NotificationRetriggerCheckDelayed(Time.time - LastNotificationTime);
                NotificationDisplay.StartCoroutine(RetriggerCoroutine);
                if (NotificationDisplay.QueuingEnabled == false)
                    LastNotificationTime = -1;
                IsQueueRoutineRunning = false;
                return;
            }


            // if we have room to show new notification show it from queued items
            if (IsThereARoomInQueue())
            {
                PushNotificationOutOfQueue();
                IsQueueRoutineRunning = false;
                return;
            }
            else
            {
                if (ShouldAutoAdvanceQueue(_CheckAutoAdvance))
                {
                    AutoHideFirstNotification();
                }
                IsQueueRoutineRunning = false;
                return;
            }

        }

        protected virtual void PushNotificationOutOfQueue()
        {
            OnNotificationReady.Invoke(QueuedNotifications[0]);
            ActiveNotificationItems.Add(QueuedNotifications[0]);
            QueuedNotifications.RemoveAt(0);
            if (IsQueueCheckRequired())
            {
                RetriggerCoroutine =
                    NotificationRetriggerCheckDelayed(NotificationDisplay.NotificationCooldownTime);
                NotificationDisplay.StartCoroutine(RetriggerCoroutine);
            }

            LastNotificationTime = Time.time;
        }
        
        protected virtual bool ShouldAutoAdvanceQueue(bool _CheckAutoAdvance)
        {
            return NotificationDisplay.AutoAdvanceQueue && _CheckAutoAdvance;
        }

        protected virtual void AutoHideFirstNotification()
        {
            ActiveNotificationItems[0].NotificationItem.Hide(NotificationDisplay.AutoAdvanceQueueInstantHide);
        }

        protected virtual bool IsQueueCheckRequired()
        {
            return QueuedNotifications.Count > 0 &&
                   ActiveNotificationItems.Count < NotificationDisplay.MaxNotifications ||
                   NotificationDisplay.AutoAdvanceQueue;
        }

        protected virtual bool IsThereARoomInQueue()
        {
            return NotificationDisplay.MaxNotifications < 0 ||
                   ActiveNotificationItems.Count < NotificationDisplay.MaxNotifications;
        }

        protected virtual bool IsNotificationQueueFull()
        {
            return NotificationDisplay.MaxNotifications < 0 ? false : ActiveNotificationItems.Count >= NotificationDisplay.MaxNotifications;
        }

        protected virtual bool IsDisplayOnCooldown()
        {
            float _Timepassed = NotificationDisplay.TimescaleIndependant
                ? (Time.realtimeSinceStartup - LastNotificationTime)
                : (Time.time - LastNotificationTime);
            return (LastNotificationTime != -1 && ( _Timepassed < NotificationDisplay.NotificationCooldownTime));
        }
    
      
    }
}