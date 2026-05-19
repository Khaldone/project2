// Assets/_Project/3_Presentation/Scene_IAP/Scripts/AchievementsPanelView.cs
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Billiards.CoreDomain.Player;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;

namespace Billiards.Presentation.Shop
{
    public class AchievementsPanelView : MonoBehaviour
    {
        [Header("Layout")]
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private RectTransform _scrollContentContainer;
        [SerializeField] private float _rowHeight = 150f;
        [SerializeField] private float _spacing = 10f;
        [SerializeField] private float _topPadding = 20f;
        [SerializeField] private float _bottomPadding = 50f;

        private List<AchievementData> _data = new();
        private readonly List<AchievementRowView> _rowPool = new();
        
        private int _visibleCount = 0;
        private int _previousTopIndex = -1;

        private void Awake()
        {
            if (_scrollRect != null)
            {
                _scrollRect.onValueChanged.AddListener(OnScroll);
            }
        }

        public async UniTask PopulateProgressAsync(List<AchievementData> achievements)
        {
            _data = achievements ?? new List<AchievementData>();
            
            if (_data.Count == 0) return;

            // 1. Set Content Height Mathematically
            float totalHeight = _data.Count * (_rowHeight + _spacing) + _topPadding + _bottomPadding;
            _scrollContentContainer.sizeDelta = new Vector2(_scrollContentContainer.sizeDelta.x, totalHeight);

            // 2. Calculate exactly how many items fit on screen + 2 for offscreen buffer
            float viewportHeight = _scrollRect.viewport != null ? _scrollRect.viewport.rect.height : _scrollRect.GetComponent<RectTransform>().rect.height;
            _visibleCount = Mathf.CeilToInt(viewportHeight / (_rowHeight + _spacing)) + 2;
            
            // 3. Pool Instantiation (Max ~10 items ever)
            int itemsToSpawn = Mathf.Min(_visibleCount, _data.Count);
            for (int i = _rowPool.Count; i < itemsToSpawn; i++)
            {
                var rowObj = await Addressables.InstantiateAsync("AchievRow", _scrollContentContainer).Task.AsUniTask();
                
                // Ensure correct pivot for math positioning and stretch horizontally
                var rect = rowObj.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0f, 1f);
                rect.anchorMax = new Vector2(1f, 1f);
                rect.pivot = new Vector2(0.5f, 1f);
                rect.offsetMin = new Vector2(0, rect.offsetMin.y);
                rect.offsetMax = new Vector2(0, rect.offsetMax.y);
                rect.sizeDelta = new Vector2(0, _rowHeight);
                
                var row = rowObj.GetComponent<AchievementRowView>();
                _rowPool.Add(row);
            }

            _previousTopIndex = -1;
            UpdateVisibleRows();
        }

        private void OnScroll(Vector2 scrollPos)
        {
            UpdateVisibleRows();
        }

        private void UpdateVisibleRows()
        {
            if (_data == null || _data.Count == 0 || _rowPool.Count == 0) return;

            // Calculate which index is currently at the top of the viewport
            float currentY = _scrollContentContainer.anchoredPosition.y;
            int topIndex = Mathf.FloorToInt(Mathf.Max(0, currentY - _topPadding) / (_rowHeight + _spacing));
            
            // Clamp topIndex so we don't try to draw out of bounds at the bottom
            topIndex = Mathf.Clamp(topIndex, 0, Mathf.Max(0, _data.Count - _visibleCount));

            // Only update if we crossed a row threshold to save CPU
            if (topIndex == _previousTopIndex) return;
            _previousTopIndex = topIndex;

            // Update the pool
            for (int i = 0; i < _rowPool.Count; i++)
            {
                int dataIndex = topIndex + i;
                var row = _rowPool[i];
                var rect = row.GetComponent<RectTransform>();

                if (dataIndex < _data.Count)
                {
                    row.gameObject.SetActive(true);
                    
                    // Manually position the row with the top padding included
                    float yPos = - (dataIndex * (_rowHeight + _spacing)) - _topPadding;
                    rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, yPos);
                    
                    // Inject data
                    row.Setup(_data[dataIndex]);
                }
                else
                {
                    row.gameObject.SetActive(false);
                }
            }
        }
    }
}