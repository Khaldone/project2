// Attached to: Node_Day_1 through Node_Day_7
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public enum RewardNodeState { Locked, ReadyToClaim, Claimed }


public class DailyRewardNodeWidget : MonoBehaviour
{
    [Header("Data Display")]
    [SerializeField] private TextMeshProUGUI _dayLabelText; // e.g., "Day 1"
    [SerializeField] private TextMeshProUGUI _rewardAmountText; // e.g., "500"
    [SerializeField] private Image _rewardIcon;


    [Header("State Visuals")]
    [SerializeField] private GameObject _lockedOverlay;
    [SerializeField] private GameObject _readyGlow;
    [SerializeField] private GameObject _claimedCheckmark;

    [Header("Interaction & Polish")]
    [SerializeField] private Button _claimButton;
    [SerializeField] private Animator _nodeAnimator;
    [SerializeField] private ParticleSystem _claimVfx;


    public event Action OnClaimClicked;


    private void Awake()
    {
        _claimButton.onClick.AddListener(() => OnClaimClicked?.Invoke());
    }


    //public void Setup(DailyRewardData data, RewardNodeState state)
    //{
    //    _dayLabelText.text = $"Day {data.DayNumber}";
    //    _rewardAmountText.text = data.Amount.ToString();
    //    // _rewardIcon.sprite = ... (Load via Addressables based on data.ItemId)


    //    SetState(state);
    //}


    public void SetState(RewardNodeState state)
    {
        // Reset all states
        _lockedOverlay.SetActive(false);
        _readyGlow.SetActive(false);
        _claimedCheckmark.SetActive(false);
        _claimButton.interactable = false;
        _nodeAnimator.SetBool("IsPulsing", false);


        switch (state)
        {
            case RewardNodeState.Locked:
                _lockedOverlay.SetActive(true);
                break;
            case RewardNodeState.ReadyToClaim:
                _readyGlow.SetActive(true);
                _claimButton.interactable = true;
                _nodeAnimator.SetBool("IsPulsing", true);
                break;
            case RewardNodeState.Claimed:
                _claimedCheckmark.SetActive(true);
                break;
        }
    }

    public void PlayBurstVFX()
    {
        _nodeAnimator.SetTrigger("Pop");
        _claimVfx.Play();
    }
}