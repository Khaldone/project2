// Attached to: Account_Link_Google
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class AccountLinkWidget : MonoBehaviour
{
    [SerializeField] private Button _linkButton;
    [SerializeField] private TextMeshProUGUI _buttonText;
    [SerializeField] private Image _platformIcon;
    [SerializeField] private GameObject _connectedCheckmark;


    public event Action OnClicked;


    private void Awake()
    {
        _linkButton.onClick.AddListener(() => OnClicked?.Invoke());
    }


    public void SetLinkedState(bool isLinked)
    {
        if (isLinked)
        {
            _buttonText.text = "Connected";
            _linkButton.interactable = false;
            _connectedCheckmark.SetActive(true);
        }
        else
        {
            _buttonText.text = "Link Account";
            _linkButton.interactable = true;
            _connectedCheckmark.SetActive(false);
        }
    }
}
