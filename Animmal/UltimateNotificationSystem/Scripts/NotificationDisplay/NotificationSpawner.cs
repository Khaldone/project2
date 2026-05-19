using System.Collections;
using UnityEngine;

namespace Animmal.NotificationSystem
{
    public class NotificationSpawner: INotificationSpawner
    {
        protected NotificationDisplay NotificationDisplay;
        public virtual void Init(NotificationDisplay _NotificationDisplay)
        {
            NotificationDisplay = _NotificationDisplay;
        }
        
        protected virtual IEnumerator PreLoadAddressableItem(NotificationStatus _NotificationStatus)
        {
            //We are using addressable and item is not loaded
#if UNS_USE_ADDRESSABLES
            if (NotificationDisplay.IsAddressable(_NotificationStatus.NotificationData.StyleVariationID)
                && IsAddressableItemLoaded(_NotificationStatus.NotificationData.StyleVariationID) == false)
            {
                yield return NotificationDisplay
                    .AddressableNotificationItemPrefabs[_NotificationStatus.NotificationData.StyleVariationID]
                    .LoadPrefabAndWait(NotificationDisplay, _NotificationStatus);
            }
#endif
            yield break;
        }

        public virtual IEnumerator SpawnNotificationItem(NotificationStatus _NotificationStatus)
        {

            NotificationItem _Item = null;
            int _StyleVariationID = _NotificationStatus.NotificationData.StyleVariationID;
            yield return PreLoadAddressableItem(_NotificationStatus);
#if UNS_USE_ADDRESSABLES
            if (IsAddressable(_StyleVariationID, NotificationDisplay))
                _Item = MonoBehaviour.Instantiate(NotificationDisplay.AddressableNotificationItemPrefabs[_StyleVariationID].LoadedItem, NotificationDisplay.NotificationItemSpawnParent) as NotificationItem;
            else
                _Item = MonoBehaviour.Instantiate(NotificationDisplay.NotificationItemPrefabs[_StyleVariationID], NotificationDisplay.NotificationItemSpawnParent) as NotificationItem;

#else
            _Item =  MonoBehaviour.Instantiate(NotificationDisplay.NotificationItemPrefabs[_StyleVariationID], NotificationDisplay.NotificationItemSpawnParent) as NotificationItem;
#endif
            _NotificationStatus.NotificationItem = _Item;

        }
        
        
        protected virtual bool IsAddressable(int _StyleVariationID, NotificationDisplay _NotificationDisplay)
        {
#if UNS_USE_ADDRESSABLES
            return _NotificationDisplay.AddressableNotificationItemPrefabs.Count > _StyleVariationID;
#endif
            return false;
        }
        
        
#if UNS_USE_ADDRESSABLES
        protected virtual bool IsAddressableItemLoaded(int _StyleVariationID)
        {
            
            return NotificationDisplay.AddressableNotificationItemPrefabs[_StyleVariationID].LoadedItem != null;
        }
#endif


    }
}