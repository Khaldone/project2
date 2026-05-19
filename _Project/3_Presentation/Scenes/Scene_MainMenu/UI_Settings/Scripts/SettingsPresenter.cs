// Assets/_Project/Scenes/UI_Settings/Scripts/SettingsPresenter.cs
using PlayFab.EconomyModels;
using System;
using VContainer.Unity;


public class SettingsPresenter : IStartable, IDisposable
{
    //private readonly ISettingsView _view;
    private readonly IAudioService _audioService;
    private readonly ILocalSaveService _saveService;
    private readonly ISettingsOrchestrator _settings;

    //public SettingsPresenter(ISettingsView view, IAudioService audioService, ILocalSaveService saveService)
    //{
    //    _view = view;
    //    _audioService = audioService;
    //    _saveService = saveService;
    //}

    public SettingsPresenter(IAudioService audioService, ILocalSaveService saveService, ISettingsOrchestrator settings)
    {
        _audioService = audioService;
        _saveService = saveService;
        _settings = settings;
    }

    public async void Start()
    {
        // 1. Load the saved values
        float savedMusicVol = await _saveService.LoadDataAsync("setting_music_vol", 0.8f);

        // 2. Set the UI Sliders to match the saved data
        //_view.SetMusicSliderValue(savedMusicVol);


        // 3. Listen for the player dragging the slider
        //_view.OnMusicSliderChanged += HandleMusicVolumeChanged;

        // 1. Populate initial UI state
        //_view.SetSfxToggle(_settings.CurrentDeviceSettings.SfxVolume > 0);
        //_view.SetPowerMeterToggle(_settings.CurrentDeviceSettings.IsPowerMeterOnLeft);
        //_view.SetFriendRequestsToggle(_settings.CurrentAccountSettings.AllowFriendRequests);


        // 2. Listen for UI interactions
        //_view.OnPowerMeterToggled += HandlePowerMeterToggled;
        //_view.OnFriendRequestsToggled += HandleFriendRequestsToggled;
    }


    private void HandlePowerMeterToggled(bool isLeft)
    {
        var updatedPrefs = _settings.CurrentDeviceSettings;
        updatedPrefs.IsPowerMeterOnLeft = isLeft;

        // This instantly saves to disk and fires the global event
        _settings.UpdateDeviceSettings(updatedPrefs);

    }

    private void HandleMusicVolumeChanged(float newValue)
    {
        // Tell the Hub to update the live audio mix
        _audioService.SetMusicVolume(newValue);

        // Save it encrypted to the disk!
        _saveService.SaveDataAsync("setting_music_vol", newValue);
    }
    private void HandleFriendRequestsToggled(bool allow)
    {
        var updatedPrefs = _settings.CurrentAccountSettings;
        updatedPrefs.AllowFriendRequests = allow;


        // Saves to PlayFab
        _settings.UpdateAccountSettingsAsync(updatedPrefs);
    }


    public void Dispose()
    {
        //_view.OnMusicSliderChanged -= HandleMusicVolumeChanged;
        //_view.OnPowerMeterToggled -= HandlePowerMeterToggled;
        //_view.OnFriendRequestsToggled -= HandleFriendRequestsToggled;
    }
}