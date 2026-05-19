using System.Collections.Generic;
using UnityEngine;

namespace Animmal.NotificationSystem
{
    public class NotificationDisplayer: INotificationDisplayer
    {
        //Data processors 
        protected List<INotificationDataProcessor> DataProcessors = new List<INotificationDataProcessor>();
        protected NotificationDisplay NotificationDisplay;
        
        public virtual void Init(NotificationDisplay _NotificationDisplay)
        {
            NotificationDisplay = _NotificationDisplay;
            //Populate data processors with components from supplied gameobjects
            InitializeDataProcessors();
        }
        
        protected virtual void InitializeDataProcessors()
        {
            for (int i = 0; i < NotificationDisplay.DataProcessorGameobjects.Count; i++)
            {
                if (NotificationDisplay.DataProcessorGameobjects[i] == null)
                {
                    Debug.LogError("Ultimate Notification System - Notification Display: "+ NotificationDisplay.gameObject.name +" No gameobject assigned to DataProcessorGameobjects list at index " + i.ToString());
                    continue; // Skip this loop iteration 
                }

                INotificationDataProcessor _Data = NotificationDisplay.DataProcessorGameobjects[i].GetComponent<INotificationDataProcessor>();
                if (_Data == null)
                {
                    Debug.LogError("Ultimate Notification System - Notification Manager: " + NotificationDisplay.gameObject.name + " No INotificationDataProcessor Component found on gameobject " + NotificationDisplay.DataProcessorGameobjects[i].name);
                    continue; // Skip this loop iteration 
                }

                DataProcessors.Add(_Data);
            }
        }
        
        public virtual void DisplayNotification(NotificationStatus _NotificationStatus)
        {
      
            //Process data through assigned data processors 
            _NotificationStatus.NotificationData = ProcessData(_NotificationStatus.NotificationData);
            NotificationItem _Item = _NotificationStatus.NotificationItem;

            if (_Item.gameObject.activeSelf == false) //Fixed the autoadvance
                _Item.gameObject.SetActive(true);
            _Item.Show(_NotificationStatus); // Feed new data into notification 
            _NotificationStatus.SetStatus(NotificationStatusEnum.Shown, false);

            //handle spawn direction
            if (NotificationDisplay.SpawnDirection == NotificationSpawnDirection.Top)
                _Item.transform.SetAsFirstSibling();
            else
                _Item.transform.SetAsLastSibling();
        }



        /// <summary>
        /// Process Data through any custom data processors assigned to Notification Manager
        /// </summary>
        /// <param name="_NotificationData">Data to process</param>
        /// <returns></returns>
        protected virtual NotificationData ProcessData(NotificationData _NotificationData)
        {
            for (int i = 0; i < DataProcessors.Count; i++)
            {
                if (DataProcessors[i] == null)
                {
                    Debug.LogError("Ultimate Notification System: Notification Manager - DataProcessor is null at index:" + i.ToString());
                }
                else
                {
                    _NotificationData = DataProcessors[i].GetProcessedData(_NotificationData);
                }
            }
            return _NotificationData;
        }
    }
}