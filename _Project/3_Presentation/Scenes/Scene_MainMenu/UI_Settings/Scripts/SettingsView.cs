// Attached to: SettingsCanvas
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public interface ISettingsView
{

}
public class SettingsView : MonoBehaviour, ISettingsView
{
    [Header("Core UI")]
    [SerializeField] private Button _closeButton;
    [SerializeField] private TextMeshProUGUI _versionText;


    [Header("Audio Settings")]
    [SerializeField] private SettingSliderWidget _masterVolume;
    [SerializeField] private SettingSliderWidget _musicVolume;
    [SerializeField] private SettingSliderWidget _sfxVolume;


    [Header("Gameplay Settings")]
    //[SerializeField] private SettingToggleWidget _leftHandedMode;
    //[SerializeField] private SettingToggleWidget _hapticsMode;


    [Header("Account Settings")]
    [SerializeField] private AccountLinkWidget _googleLink;
    [SerializeField] private AccountLinkWidget _appleLink;
    [SerializeField] private Button _logoutButton;


    // Events the Presenter listens to
    public event Action OnCloseClicked;
    public event Action<float> OnMasterVolumeChanged;
    public event Action<bool> OnLeftHandedToggled;
    public event Action OnLogoutClicked;
    // ... other events


    private void Awake()
    {
        _closeButton.onClick.AddListener(() => OnCloseClicked?.Invoke());
        _logoutButton.onClick.AddListener(() => OnLogoutClicked?.Invoke());


        // Wire up the child widgets to our master events
        _masterVolume.OnValueChanged += (val) => OnMasterVolumeChanged?.Invoke(val);
        //_leftHandedMode.OnToggled += (isOn) => OnLeftHandedToggled?.Invoke(isOn);
    }


    // Called by the Presenter immediately when the UI loads to set the visual state
    //public void InitializeState(GameSettingsData currentSettings, string appVersion)
    //{
    //    _versionText.text = appVersion;


    //    // Set the sliders without triggering their OnValueChanged events!
    //    _masterVolume.SetValueWithoutNotify(currentSettings.MasterVolume);
    //    _musicVolume.SetValueWithoutNotify(currentSettings.MusicVolume);

    //    _leftHandedMode.SetToggleWithoutNotify(currentSettings.IsLeftHanded);
    //    _hapticsMode.SetToggleWithoutNotify(currentSettings.HapticsEnabled);
    //}


    public void UpdateAccountLinkState(string platform, bool isLinked)
    {
        if (platform == "Google") _googleLink.SetLinkedState(isLinked);
        if (platform == "Apple") _appleLink.SetLinkedState(isLinked);
    }
}
