// Attached to: Setting_MasterVolume
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class SettingSliderWidget : MonoBehaviour
{
    [SerializeField] private Slider _slider;
    [SerializeField] private TextMeshProUGUI _valueText;


    public event Action<float> OnValueChanged;


    private void Awake()
    {
        _slider.onValueChanged.AddListener(HandleSliderChange);
    }


    private void HandleSliderChange(float value)
    {
        _valueText.text = $"{Mathf.RoundToInt(value * 100)}%";
        OnValueChanged?.Invoke(value);
    }


    // Critical: Sets the visual state on boot without firing an event back to the Presenter
    public void SetValueWithoutNotify(float value)
    {
        _slider.SetValueWithoutNotify(value);
        _valueText.text = $"{Mathf.RoundToInt(value * 100)}%";
    }
}