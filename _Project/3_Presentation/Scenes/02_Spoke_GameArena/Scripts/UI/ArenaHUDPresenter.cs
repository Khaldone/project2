// Assets/_Project/Scenes/02_Spoke_GameArena/Scripts/UI/ArenaHUDPresenter.cs
using PlayFab.EconomyModels;
using System;
using VContainer.Unity;


public class ArenaHUDPresenter : IStartable, IDisposable
{
    //private readonly IArenaHUDView _view;
    private readonly ISettingsOrchestrator _settings;

    //public ArenaHUDPresenter(IArenaHUDView view, ISettingsOrchestrator settings)
    //{
    //    _view = view;
    //    _settings = settings;
    //}

    public ArenaHUDPresenter(ISettingsOrchestrator settings)
    {
        _settings = settings;
    }

    public void Start()
    {
        // 1. Apply the setting immediately when the Arena loads
        ApplyPowerMeterLayout(_settings.CurrentDeviceSettings);


        // 2. Subscribe to the event in case they change it MID-MATCH
        _settings.OnDeviceSettingsChanged += ApplyPowerMeterLayout;
    }

    private void ApplyPowerMeterLayout(DeviceSettings settings)
    {
        if (settings.IsPowerMeterOnLeft)
        {
            //_view.AnchorPowerMeterToLeft();
        }
        else
        {
            //_view.AnchorPowerMeterToRight();
        }
    }

    public void Dispose()
    {
        _settings.OnDeviceSettingsChanged -= ApplyPowerMeterLayout;
    }
}