using System.Collections.Generic;
using UnityEngine;

namespace Animmal.NotificationSystem
{
    public class NotificationPool: INotificationPool
    {
        //pooling 
        protected List<NotificationItemList> PooledNotificationItems = new List<NotificationItemList>();
        public virtual void Init(NotificationDisplay _NotificationDisplay)
        {
            if (_NotificationDisplay.UsePooling)
            {
#if UNS_USE_ADDRESSABLES
                
                for (int i = 0; i < _NotificationDisplay.AddressableNotificationItemPrefabs.Count; i++)
                {
                    PooledNotificationItems.Add(new NotificationItemList());
                }
                for (int i = _NotificationDisplay.AddressableNotificationItemPrefabs.Count; i < _NotificationDisplay.NotificationItemPrefabs.Count; i++)
                {
                    PooledNotificationItems.Add(new NotificationItemList());
                }
#else
                for (int i = 0; i < _NotificationDisplay.NotificationItemPrefabs.Count; i++)
                {
                    PooledNotificationItems.Add(new NotificationItemList());
                }
#endif
            }
        }
        
        public virtual NotificationItem RemoveFromPool(int _VariationID, NotificationDisplay _NotificationDisplay)
        {
            if (HasPooledItem(_VariationID))
            {
                NotificationItem _NotificationItem = PooledNotificationItems[_VariationID].Variation[0];
                PooledNotificationItems[_VariationID].Variation.RemoveAt(0);
                return _NotificationItem;
            }
            else
            {
                return null;
            }
        }

        public virtual void AddToPool(int _VariationID, NotificationItem _NotificationItem)
        {
            PooledNotificationItems[_VariationID].Variation.Add(_NotificationItem);
        }

        public virtual bool HasPooledItem(int _VariationID)
        {
            return PooledNotificationItems.Count > _VariationID && PooledNotificationItems[_VariationID].Variation.Count > 0;
        }

        public virtual void ItemHidingFinished(NotificationStatus _NotificationStatus, NotificationDisplay _NotificationDisplay)
        {
            int _StyleVariationID = _NotificationStatus.NotificationData.StyleVariationID;
            if (_NotificationDisplay.UsePooling)
            {
                PooledNotificationItems[_StyleVariationID].Variation.Add(_NotificationStatus.NotificationItem);
                if (_NotificationDisplay.DisableGameObjectOnHiddenItems)
                    _NotificationStatus.NotificationItem.gameObject.SetActive(false);
            }
            else
            {
                MonoBehaviour.Destroy(_NotificationStatus.NotificationItem.gameObject);
#if UNS_USE_ADDRESSABLES

                if (_NotificationDisplay.UnloadAddressableWhenNotificationIsHidden && _NotificationDisplay.IsAddressable(_StyleVariationID))
                {
                    bool _AnotherInstanceExists = false;
                    for (int i = 0; i < _NotificationDisplay.ActiveNotificationItems.Count; i++)
                    {
                        if (_NotificationDisplay.ActiveNotificationItems[i].NotificationData.StyleVariationID == _StyleVariationID)
                        {
                            _AnotherInstanceExists = true;
                            break;
                        }
                    }
                    if (_AnotherInstanceExists == false)
                        _NotificationDisplay.AddressableNotificationItemPrefabs[_StyleVariationID].UnloadItem();
                }
#endif
            }
        }
    }
}