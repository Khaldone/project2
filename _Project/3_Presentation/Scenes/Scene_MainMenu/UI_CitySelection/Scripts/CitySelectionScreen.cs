// Assets/_Project/3_Presentation/Scene_CitySelection/Scripts/CitySelectionScreen.cs
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Billiards.Presentation.UI;

namespace Billiards.Presentation
{
    public class CitySelectionScreen : SsBaseMenu
    {
        [Header("Scroll System")]
        [SerializeField] private CitySnapScrollView _cityScroller;

        [Header("Navigation Buttons")]
        [SerializeField] private Button _invisibleNextBtn;
        [SerializeField] private Button _invisiblePrevBtn;

        [Header("Action Buttons")]
        [SerializeField] private Button _play8BallBtn;
        [SerializeField] private Button _play9BallBtn;

        // The Events the Presenter will subscribe to
        public event Action<int> OnCityFocused;
        public event Action OnPlay8BallClicked;
        public event Action OnPlay9BallClicked;

        public override void Awake()
        {
            base.Awake();

            _invisibleNextBtn.onClick.AddListener(_cityScroller.GoToNext);
            _invisiblePrevBtn.onClick.AddListener(_cityScroller.GoToPrevious);

            _play8BallBtn.onClick.AddListener(() => OnPlay8BallClicked?.Invoke());
            _play9BallBtn.onClick.AddListener(() => OnPlay9BallClicked?.Invoke());

            // Bubble up the scroll snap event
            _cityScroller.OnCityFocused += (index) => OnCityFocused?.Invoke(index);
        }

        // The API for the Presenter to push data DOWN
        public async Cysharp.Threading.Tasks.UniTask PopulateCitiesAsync(System.Collections.Generic.List<Billiards.CoreDomain.Progression.ArenaConfig> configs)
        {
            if (_cityScroller != null)
            {
                await _cityScroller.PopulateCitiesAsync(configs);
            }
        }
    }
}