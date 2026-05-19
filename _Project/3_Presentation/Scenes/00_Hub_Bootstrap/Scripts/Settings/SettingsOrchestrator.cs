// Assets/_Project/Scenes/00_Hub_Bootstrap/Scripts/Settings/SettingsOrchestrator.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PlayFab;
using PlayFab.ClientModels;
using VContainer.Unity;


public class SettingsOrchestrator : ISettingsOrchestrator, IStartable
{
    public event Action<DeviceSettings> OnDeviceSettingsChanged;
    public event Action<AccountSettings> OnAccountSettingsChanged;

    public DeviceSettings CurrentDeviceSettings { get; private set; }
    public AccountSettings CurrentAccountSettings { get; private set; }


    private readonly ILocalSaveService _localSave;

    public SettingsOrchestrator(ILocalSaveService localSave)
    {
        _localSave = localSave;
    }


    public async void Start()
    {
        await InitializeAsync();
    }


    public async Task InitializeAsync()
    {
        // 1. Load Local Settings (Fast)
        CurrentDeviceSettings = await _localSave.LoadDataAsync("device_prefs", new DeviceSettings
        {
            MusicVolume = 0.8f,
            SfxVolume = 1.0f,
            IsPowerMeterOnLeft = false,
            HapticsEnabled = true
        });
        OnDeviceSettingsChanged?.Invoke(CurrentDeviceSettings);


        // 2. Load Cloud Settings (Requires network)
        if (PlayFabClientAPI.IsClientLoggedIn())
        {
            FetchCloudSettings();
        }
    }

    public void UpdateDeviceSettings(DeviceSettings newSettings)
    {
        CurrentDeviceSettings = newSettings;
        OnDeviceSettingsChanged?.Invoke(CurrentDeviceSettings); // Notify AudioService and UI instantly

        // Save to disk securely in the background
        _localSave.SaveDataAsync("device_prefs", CurrentDeviceSettings);
    }


    public async Task UpdateAccountSettingsAsync(AccountSettings newSettings)
    {
        CurrentAccountSettings = newSettings;
        OnAccountSettingsChanged?.Invoke(CurrentAccountSettings);


        // Save to PlayFab Player Data
        var request = new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string>
            {
                { "AllowFriendRequests", newSettings.AllowFriendRequests.ToString() },
                { "AllowPrivateChats", newSettings.AllowPrivateChats.ToString() }
            }
        };


        PlayFabClientAPI.UpdateUserData(request,
            result => UnityEngine.Debug.Log("AAA Pipeline: Cloud settings saved."),
            error => UnityEngine.Debug.LogError("Failed to save cloud settings.")
        );
    }

    private void FetchCloudSettings()
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), result =>
        {
            bool friends = true; // Defaults
            bool chats = true;


            if (result.Data.ContainsKey("AllowFriendRequests"))
                bool.TryParse(result.Data["AllowFriendRequests"].Value, out friends);

            if (result.Data.ContainsKey("AllowPrivateChats"))
                bool.TryParse(result.Data["AllowPrivateChats"].Value, out chats);


            CurrentAccountSettings = new AccountSettings { AllowFriendRequests = friends, AllowPrivateChats = chats };
            OnAccountSettingsChanged?.Invoke(CurrentAccountSettings);


        }, error => UnityEngine.Debug.LogError("Failed to fetch cloud settings."));
    }
}