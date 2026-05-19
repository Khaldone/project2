// Assets/_Project/CoreDomain/Settings/ISettingsOrchestrator.cs
using System;
using System.Threading.Tasks;

public interface ISettingsOrchestrator
{
    // Reactive Events: Systems listen to these to update instantly
    event Action<DeviceSettings> OnDeviceSettingsChanged;
    event Action<AccountSettings> OnAccountSettingsChanged;

    DeviceSettings CurrentDeviceSettings { get; }
    AccountSettings CurrentAccountSettings { get; }

    Task InitializeAsync(); // Called on boot to load everything

    // Commands from the UI
    void UpdateDeviceSettings(DeviceSettings newSettings);
    Task UpdateAccountSettingsAsync(AccountSettings newSettings);
}