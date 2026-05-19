using System.Collections;
using UnityEngine;
using UnityEngine.Events;

#if UNS_USE_ADDRESSABLES
using UnityEngine.AddressableAssets;

using UnityEngine.ResourceManagement.AsyncOperations;
#endif

namespace Animmal.NotificationSystem
{
#if UNS_USE_ADDRESSABLES
    [System.Serializable]
    public class NotificationItemAddressableField
    {
        public class UnityNotificationDataEvent : UnityEvent<NotificationStatus> { }
        public UnityNotificationDataEvent OnLoadingComplete = new UnityNotificationDataEvent();
        public AssetReference AddressableNotificationItemPrefab;
        public NotificationItem LoadedItem { get; private set; }
        bool IsLoading;
        AsyncOperationHandle<GameObject> Handle;
        
        public IEnumerator LoadPrefabAndWait(NotificationDisplay _Display, NotificationStatus _Status)
        {
            if (LoadedItem == null)
            {
                if (IsLoading)
                {
                    while (IsLoading)
                    {
                        yield return null;
                    }
                    OnLoadingComplete.Invoke(_Status);
                }
                else
                {
                    IsLoading = true;
                    Handle = Addressables.LoadAssetAsync<GameObject>(AddressableNotificationItemPrefab);
                    yield return Handle;
                    if (Handle.Status == AsyncOperationStatus.Succeeded)
                    {
                        LoadedItem = Handle.Result.GetComponent<NotificationItem>();
                    }
                    else
                    {
                        Debug.LogError("Ultimate Notification System: Addresable Prefab not assigned on Object: " + _Display.gameObject.name + " for Notification Display " + _Display.UniqueID);
                    }
                    IsLoading = false;
                    OnLoadingComplete.Invoke(_Status);
                }
            }
            else
            {
                OnLoadingComplete.Invoke(_Status);
            }
            yield break;
        }

        public void UnloadItem()
        {
            if (Handle.IsValid() && Handle.IsDone && LoadedItem != null)
            {
                LoadedItem = null;
                Addressables.Release<GameObject>(Handle);
            }
        }
    }
#endif
}