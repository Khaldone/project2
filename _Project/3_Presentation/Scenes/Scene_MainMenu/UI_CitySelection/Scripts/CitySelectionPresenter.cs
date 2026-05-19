// Assets/_Project/3_Presentation/Scene_CitySelection/Scripts/CitySelectionPresenter.cs
using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer.Unity;
using Cysharp.Threading.Tasks;
using Billiards.CoreDomain.Progression;
using Billiards.Presentation.UI; // For CitySnapScrollView

namespace Billiards.Presentation.CitySelection
{
    public class CitySelectionPresenter : IInitializable, IDisposable
    {
        private readonly CitySelectionScreen _view;
        private readonly CitySnapScrollView _scrollSnap;
        private readonly IArenaProgressionService _progressionService;

        private List<ArenaConfig> _arenaConfigs;

        public CitySelectionPresenter(
            CitySelectionScreen view,
            CitySnapScrollView scrollSnap,
            IArenaProgressionService progressionService
            )
        {
            _view = view;
            _scrollSnap = scrollSnap;
            _progressionService = progressionService;

            _scrollSnap.OnCityFocused += HandleCityFocused;
            //_view.OnPlayBtnClicked += HandlePlayClicked;
        }



        public void Initialize()
        {
            LoadCitiesFlowAsync().Forget();
        }

        private async UniTaskVoid LoadCitiesFlowAsync()
        {
            // 1. Tell View to show a loading spinner
            //_view.ShowLoading(true);

            try
            {
                // 2. Fetch the hydrated data from PlayFab/CBS
                _arenaConfigs = await _progressionService.GetHydratedArenaConfigsAsync();

                // 3. Command the View to dynamically spawn and hydrate the city panels via Addressables
                await _view.PopulateCitiesAsync(_arenaConfigs);

                // Force layout update to select the first one
                HandleCityFocused(0);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CitySelection] Failed to load cities: {ex.Message}");
            }
            finally
            {
                //_view.ShowLoading(false);
            }
        }

        private void HandleCityFocused(int index)
        {
            if (_arenaConfigs == null || index >= _arenaConfigs.Count) return;

            var config = _arenaConfigs[index];

            // Tell the main screen to enable/disable the "Play" button at the bottom
            //_view.SetPlayButtonInteractable(config.IsPlayable);
        }

        private void HandlePlayClicked()
        {
            // In reality, this transitions to the Matchmaking Scene
            Debug.Log($"Transitioning to Matchmaking...");
        }

        public void Dispose()
        {
            _scrollSnap.OnCityFocused -= HandleCityFocused;
            //_view.OnPlayBtnClicked -= HandlePlayClicked;
        }
    }
}