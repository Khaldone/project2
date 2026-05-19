// Attached to: Currency_Node_Coins AND Currency_Node_Gems
using UnityEngine;
using TMPro;


public class CurrencyWidgetView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _amountText;
    [SerializeField] private Animator _animator; // To play a little "bump" animation when currency is added


    public void SetAmount(int amount)
    {
        _amountText.text = amount.ToString("N0"); // Formats with commas (e.g., 1,500)
    }


    public void PlayAddAnimation()
    {
        _animator.SetTrigger("OnCurrencyAdded");
    }
}