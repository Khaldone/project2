using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

#if UNS_USE_ADDRESSABLES
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
#endif

namespace Animmal.NotificationSystem
{
    //To pool each variation of notification items. 
    public class NotificationItemList
    {
        public List<NotificationItem> Variation = new List<NotificationItem>();
    }

    public enum NotificationSpawnDirection
    {
        Top,
        Bottom
    }

    [AddComponentMenu("Animmal/NotificationSystem/Notification Display")]
    /// <summary>
    /// Notification Display is responsible for showing and managing notification items
    /// </summary>
    public partial class NotificationDisplay : MonoBehaviour, IDisplayable
    {

        #region VARIABLES

        [Header("Notification Manager")]
        [Tooltip("If you use NotificationManager, this ID will be used to identify the display")]
        public string UniqueID;

        [Tooltip(
            "This will determine if Notification Display will register itself to be available to Notification Manager.")]
        public bool RegisterDisplayWithNotificationManager = true;
        [Tooltip("Enable to make sure notification queue can work in unScaled time and ignoreTime.timeScale value")]
        public bool TimescaleIndependant;
        
        [Header("Notification Item Setup")]
#if UNS_USE_ADDRESSABLES
        [Tooltip(
            "Addressable Versions Will Be Favored if assigned. Notification item variations that can be spawned by this display. EX: Positive, Neutral, Negative alert.")]
        public List<NotificationItemAddressableField> AddressableNotificationItemPrefabs =
            new List<NotificationItemAddressableField>();

        [Tooltip(
            "Should notification addressable everytime notification is hidden or only when display is destroyed? Does not work when pooling is enabled")]
        public bool UnloadAddressableWhenNotificationIsHidden;
#endif
        [Tooltip(
            "Notification item variations that can be spawned by this display. EX: Positive, Neutral, Negative alert.")]
        public List<NotificationItem> NotificationItemPrefabs = new List<NotificationItem>();

        [Tooltip("Parent object for spawning notification items.")]
        public Transform NotificationItemSpawnParent;

        [Tooltip("Spawn at the top of the hierarchy list or at the end.")]
        public NotificationSpawnDirection SpawnDirection;

        [Header("Retrigger Behavior")] [Tooltip("You can enable/disable queuing behavior altogether.")]
        public bool QueuingEnabled = true;

        [Tooltip("How many notifications can be shown at once. Negative Value (-1) = Unlimited.")]
        public int MaxNotifications = -1;

        [Tooltip("How long to wait before showing next notification in queue.")] [Min(0)]
        public float NotificationCooldownTime = 1f;

        [Header("Auto Advance")]
        [Tooltip(
            "When a new notification is shown and there is no room for new notifications. First notification item will be hidden to make room for new one.")]
        public bool AutoAdvanceQueue = true;

        [Tooltip("If AutoAdvanceQueue is turned on, should the first item be hidden instantly?.")]
        public bool AutoAdvanceQueueInstantHide = true;

        [Header("Pooling")] [Tooltip("Recycle notifications?")]
        public bool UsePooling = true;

        [Tooltip("If pooling is used disable GameObject of Notification item when it has finished hiding?")]
        public bool DisableGameObjectOnHiddenItems;

        [Header("Data Processors")]
        [Tooltip(
            "Add Objects here that have monobehavior component with INotificationDataProcessor interface to do global data processing on all Notification Data passed through this display")]
        public List<GameObject> DataProcessorGameobjects = new List<GameObject>();

        [Header("Events")] [Tooltip("Called when notification item is spawned or recycled to be shown")]
        public UnityNotificationItemEvent OnItemShown = new UnityNotificationItemEvent();

        [Tooltip("Called when notification item finished hiding")]
        public UnityNotificationItemEvent OnItemHidden = new UnityNotificationItemEvent();

        [Tooltip("Called when notification display recieves show command.")]
        public UnityBoolEvent OnDisplayShow = new UnityBoolEvent();

        [Tooltip("Called when notification display recieves hide command.")]
        public UnityBoolEvent OnDisplayHide = new UnityBoolEvent();

        //tracking display state
        public bool IsDisabled { get; set; }
        public bool IsPaused { get; set; }

        //Implementing INotificationDisplayable Interface 
        public virtual UnityBoolEvent OnShowEvent
        {
            get { return OnDisplayShow; }
        }

        public virtual UnityBoolEvent OnHideEvent
        {
            get { return OnDisplayHide; }
        }

        //Init
        protected bool InitComplete = false;



        public List<NotificationStatus> ActiveNotificationItems
        {
            get { return NotificationQueuer.ActiveNotificationItems; }
        }

        #endregion

        #region SUBCLASSES

        NotificationDisplayer _NotificationDisplayer = new NotificationDisplayer();

        protected virtual INotificationDisplayer NotificationDisplayer
        {
            get { return _NotificationDisplayer; }
        }

        NotificationQueuer _NotificationQueuer = new NotificationQueuer();

        protected virtual INotificationQueuer NotificationQueuer
        {
            get { return _NotificationQueuer; }
        }

        NotificationPool _NotificationPool = new NotificationPool();

        protected virtual INotificationPool NotificationPool
        {
            get { return _NotificationPool; }
        }

        NotificationSpawner _NotificationSpawner = new NotificationSpawner();

        protected virtual INotificationSpawner NotificationSpawner
        {
            get { return _NotificationSpawner; }
        }

        #endregion

        #region UNITYFUNCTIONS

        protected virtual void Start()
        {
            Init();
        }

        protected virtual void OnEnable()
        {
            NotificationQueuer.OnDisplayEnabled();
        }

        protected virtual void OnDisable()
        {
            NotificationQueuer.OnDisplayDisabled();
        }

        protected virtual void OnDestroy()
        {
            UnRegisterDisplay();
#if UNS_USE_ADDRESSABLES
            UnloadAddressables();
#endif

        }

        #endregion

        #region INIT

        protected virtual void Init()
        {
            if (InitComplete)
                return;
            //Make sure we have at least something for ID
            if (string.IsNullOrEmpty(UniqueID))
                UniqueID = Guid.NewGuid().ToString();

            //For use with Notification Manager
            if (RegisterDisplayWithNotificationManager)
                RegisterDisplay();

            NotificationPool.Init(this);
            NotificationSpawner.Init(this);
            NotificationDisplayer.Init(this);
            NotificationQueuer.Init(this);
            NotificationQueuer.OnNotificationReady.AddListener(ShowQueuedNotification);

            InitComplete = true;
        }

        #endregion

        #region NOTIFICATIONS


        /// Call this to show new notification item with supplied data
        public virtual NotificationStatus ShowNotification(NotificationData _NotificationData)
        {
            if (InitComplete == false)
                Init();

            if (CanShowNotification() == false)
                return new NotificationStatus(NotificationStatusEnum.Skipped, null, null);

            return NotificationQueuer.AddToQueue(_NotificationData);
        }

        protected virtual void ShowQueuedNotification(NotificationStatus _NotificationStatus)
        {
            if (CanShowNotification() == false)
            {
                return;
            }

            StartCoroutine(ShowNotificationItem(_NotificationStatus));
        }

        protected virtual IEnumerator ShowNotificationItem(NotificationStatus _NotificationStatus)
        {
            yield return PrepareNotificationItem(_NotificationStatus);
            NotificationDisplayer.DisplayNotification(_NotificationStatus);
            OnItemShown.Invoke(_NotificationStatus.NotificationItem);
        }

        protected virtual IEnumerator PrepareNotificationItem(NotificationStatus _NotificationStatus)
        {
            int _StyleVariationID = Helpers.GetVariationID(this, _NotificationStatus.NotificationData.StyleVariationID);
            if (NotificationPool.HasPooledItem(_StyleVariationID))
            {
                _NotificationStatus.NotificationItem = NotificationPool.RemoveFromPool(_StyleVariationID, this);
            }
            else
            {
                yield return NotificationSpawner.SpawnNotificationItem(_NotificationStatus);
                //listen to hiding finished event so we know when the queue is cleared up
                _NotificationStatus.NotificationItem.OnHidingFinished.AddListener(ItemHidingFinished);
            }
        }

        /// <summary>
        /// Action hooken into Notification item OnItemHidingFinished to manage active items and queue 
        /// </summary>
        /// <param name="_Item">Notification Item that finished its hiding routine</param>
        protected virtual void ItemHidingFinished(NotificationItem _Item)
        {
            NotificationQueuer.ItemHidingFinished(_Item.Status);
            NotificationPool.ItemHidingFinished(_Item.Status, this);
            _Item.Status.SetStatus(NotificationStatusEnum.Hidden, false);
            OnItemHidden.Invoke(_Item);
        }

        #endregion

        #region BULKOPERATIONS

        /// <summary>
        /// Disable receiving of any new notifications. To Hide as well call HideAll.
        /// </summary>
        public virtual void DisableDisplay()
        {
            IsDisabled = true;
        }

        /// <summary>
        /// Enable receiving of any new notifications. To Hide as well call HideAll.
        /// </summary>
        public virtual void EnableDisplay()
        {
            IsDisabled = false;
        }

        /// <summary>
        /// Pause showing any new notifications and queue them.
        /// </summary>
        public virtual void PauseDisplay()
        {
            IsPaused = true;
            NotificationQueuer.OnDisplayDisabled();
        }

        /// <summsary>
        /// Unpause showing new notifications. 
        /// </summary>
        public virtual void UnpauseDisplay()
        {
            IsPaused = false;
            NotificationQueuer.OnDisplayEnabled();
        }

        /// <summary>
        /// Hide All Spawned Notification Items 
        /// </summary>
        /// <param name="_Instant">Whether to call with Instant Option</param>
        public virtual void HideAll(bool _Instant = false)
        {
            ClearQueues(-1);
            for (int i = 0; i < ActiveNotificationItems.Count; i++)
            {
                ActiveNotificationItems[i].NotificationItem.Hide(_Instant);
            }
        }

        /// <summary>
        /// Show All Spawned Notification Items 
        /// </summary>
        /// <param name="_Instant">Whether to call with Instant Option</param>
        public virtual void UnhideAll(bool _Instant = false)
        {
            for (int i = 0; i < ActiveNotificationItems.Count; i++)
            {
                ActiveNotificationItems[i].NotificationItem.Show(_Instant);
            }
        }

        /// <summary>
        /// This will clear all queued notifications.
        /// </summary>
        /// <param name="_VariationID">If this is not -1, than only queued notificaiton of this index will be cleared</param>
        public virtual void ClearQueues(int _VariationID = -1)
        {
            NotificationQueuer.ClearQueues(_VariationID);
        }

        #endregion

        #region HELPERS

        protected virtual bool CanShowNotification()
        {
            if (IsDisabled)
            {
                Debug.Log("Ultimate Notification System: Notification Display - " + gameObject.name +
                          " Display is disabled, notification will be ignored.");
                return false;
            }

    #if UNS_USE_ADDRESSABLES
            if (AddressableNotificationItemPrefabs.Count == 0 && NotificationItemPrefabs.Count == 0)
            {
                Debug.LogError("Ultimate Notification System: Notification Display - " + gameObject.name +
                               "Has no Notification Item Prefabs assigned.");
                return false;
            }
    #else
            if (NotificationItemPrefabs.Count == 0)
            {
                Debug.LogError("Ultimate Notification System: Notification Display - " + gameObject.name + "Has no Notification Item Prefabs assigned.");
                return false;
            }
    #endif
            return true;
        }


    #if UNS_USE_ADDRESSABLES
        protected virtual void UnloadAddressables()
        {
            foreach (var _AddressablePrefab in AddressableNotificationItemPrefabs)
            {
                _AddressablePrefab.UnloadItem();
            }
        }
    #endif

        public bool IsAddressable(int _StyleVariationID)
        {
    #if UNS_USE_ADDRESSABLES
            return AddressableNotificationItemPrefabs.Count > _StyleVariationID;
    #endif
            return false;
        }

    #endregion

        #region DISPLAY_REGISTRATION

        protected virtual void RegisterDisplay()
        {
            if (NotificationManager.Instance != null)
                NotificationManager.Instance.RegisterDisplay(UniqueID, this);
        }

        protected virtual void UnRegisterDisplay()
        {
            if (NotificationManager.Instance != null)
                NotificationManager.Instance.UnRegisterDisplay(UniqueID);
        }

    #endregion

        #region DISPLAYVISIBILITY

        public virtual void ShowDisplay(bool _Instant)
        {
            OnDisplayShow.Invoke(_Instant);
        }

        public virtual void HideDisplay(bool _Instant)
        {
            OnDisplayHide.Invoke(_Instant);
        }

//Implementing the INotificationDisplayable interface
        public virtual void HidingFinished()
        {

        }

    #endregion
    }
}