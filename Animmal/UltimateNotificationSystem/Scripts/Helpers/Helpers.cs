using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Animmal.NotificationSystem
{
    public static partial class Helpers 
    {
        public static NotificationData GetACopyOfNotificationData(NotificationData _Data)
        {
            NotificationData _NewData = new NotificationData();
            _NewData.StyleVariationID = _Data.StyleVariationID;
            _NewData.Texts.AddRange(_Data.Texts);
            _NewData.Sprites.AddRange(_Data.Sprites);
            _NewData.VideoClips.AddRange(_Data.VideoClips);
            _NewData.AudioClips.AddRange(_Data.AudioClips);
            return _NewData;
        }
        
        public static int GetVariationID(NotificationDisplay _NotificationDisplay, int _StyleVariationID)
        {

#if UNS_USE_ADDRESSABLES
            if (_NotificationDisplay.IsAddressable(_StyleVariationID) == false && _NotificationDisplay.NotificationItemPrefabs.Count <= _StyleVariationID)
            {
                Debug.LogError("Ultimate Notification System: Notification Display - " + _NotificationDisplay.gameObject.name +
                               " Style Variation ID in Notification Data is higher than number of NotificationItem prefabs ssigned to this Notification Display. To avoid error system is defaulting to first NotificationItem Prefab on the list."
                               + "To Fix this error either lower StyleVariationID in notification data you sent into this Display, or add new NotificationItem Prefabs to NotificationItemPrefabs list on this display."
                               + "Please note that notification count starts with 0 index. StyleVariationID of 0 = first prefab assigned in NotificationItemPrefabs list, StyleVariationID of 1 = second item etc.");
                return 0;
            }

#else
            if (_NotificationDisplay.NotificationItemPrefabs.Count <= _StyleVariationID)
            {
                Debug.LogError("Ultimate Notification System: Notification Display - " + _NotificationDisplay.gameObject.name +
                    " Style Variation ID in Notification Data is higher than number of NotificationItem prefabs ssigned to this Notification Display. To avoid error system is defaulting to first NotificationItem Prefab on the list."
                    + "To Fix this error either lower StyleVariationID in notification data you sent into this Display, or add new NotificationItem Prefabs to NotificationItemPrefabs list on this display."
                    + "Please note that notification count starts with 0 index. StyleVariationID of 0 = first prefab assigned in NotificationItemPrefabs list, StyleVariationID of 1 = second item etc.");
                return 0;
            }
#endif
            else return _StyleVariationID;
        }
    }
}