using CBS.Core;
using CBS.Models;
using CBS.Scriptable;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CBS.UI
{
    public class LootBoxWindow : MonoBehaviour
    {
        [SerializeField]
        private GameObject OpenBtn;
        [SerializeField]
        private BaseScroller Scroller;
        [SerializeField]
        private ToggleGroup Group;

        private ICBSInventory CBSInventory { get; set; }

        private List<CBSInventoryItem> CurrentBoxes { get; set; }

        private LootboxPrefabs Prefabs { get; set; }

        private CBSInventoryItem SelectedBox { get; set; }

        private void Awake()
        {
            CBSInventory = CBSModule.Get<CBSInventoryModule>();
            Prefabs = CBSScriptable.Get<LootboxPrefabs>();
            Scroller.OnSpawn += OnItemSpawn;
        }

        private void OnDestroy()
        {
            Scroller.OnSpawn -= OnItemSpawn;
        }

        private void OnEnable()
        {
            Scroller.Clear();
            GetLootBoxes();
        }

        private void GetLootBoxes()
        {
            OpenBtn.SetActive(false);
            CBSInventory.GetLootboxes(OnLootBoxGetted);
        }

        private void OnLootBoxGetted(CBSGetLootboxesResult result)
        {
            if (result.IsSuccess)
            {
                Group.SetAllTogglesOff();
                var slotPrefab = Prefabs.LootBoxSlot;
                CurrentBoxes = result.Lootboxes;
                int count = CurrentBoxes == null ? 0 : CurrentBoxes.Count;
                Scroller.SpawnItems(slotPrefab, count);
            }
        }

        private void OnItemSpawn(GameObject uiItem, int index)
        {
            var box = CurrentBoxes[index];
            var slot = uiItem.GetComponent<LootBoxSlot>();
            slot.Configurate(box, Group);
            slot.SetSelectAction(OnBoxSelected);
            if (index == 0)
            {
                slot.SetToggleValue(true);
            }
        }

        private void OnBoxSelected(CBSInventoryItem box)
        {
            SelectedBox = box;
            OpenBtn.SetActive(SelectedBox != null);
        }

        public void OpenLootBox()
        {
            if (SelectedBox == null)
                return;
            CBSInventory.OpenLootbox(SelectedBox.InstanceID, OnOpenLootBox);
        }

        private void OnOpenLootBox(CBSOpenLootboxResult result)
        {
            if (result.IsSuccess)
            {
                GetLootBoxes();
                var resultPrefab = Prefabs.LootBoxResult;
                var resultObject = UIView.ShowWindow(resultPrefab);
                resultObject.GetComponent<LootBoxResult>().Display(result);
            }
            else
            {
                new PopupViewer().ShowFabError(result.Error);
            }
        }
    }
}
