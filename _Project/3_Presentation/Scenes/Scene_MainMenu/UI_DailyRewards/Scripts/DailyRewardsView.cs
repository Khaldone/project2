// Attached to: DailyRewardsCanvas
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public interface IDailyRewardsView
{

}
public class DailyRewardsView : MonoBehaviour, IDailyRewardsView
{
    [Header("UI References")]
    [SerializeField] private Button _closeButton;
    [Tooltip("Must contain exactly 7 nodes in chronological order")]
    [SerializeField] private DailyRewardNodeWidget[] _dayNodes;


    public event Action OnCloseClicked;
    public event Action<int> OnDayClaimClicked;


    private void Awake()
    {
        _closeButton.onClick.AddListener(() => OnCloseClicked?.Invoke());

        // Wire up each node's internal button to the View's master event
        for (int i = 0; i < _dayNodes.Length; i++)
        {
            int dayIndex = i; // Capture for closure
            _dayNodes[i].OnClaimClicked += () => OnDayClaimClicked?.Invoke(dayIndex);
        }
    }


    // Called by the Presenter to set the visual state of the entire week
    //public void PopulateWeek(IReadOnlyList<DailyRewardData> weekData, int currentStreakIndex, bool canClaimToday)
    //{
    //    for (int i = 0; i < _dayNodes.Length; i++)
    //    {
    //        RewardNodeState state;


    //        if (i < currentStreakIndex)
    //        {
    //            state = RewardNodeState.Claimed; // Past days
    //        }
    //        else if (i == currentStreakIndex)
    //        {
    //            state = canClaimToday ? RewardNodeState.ReadyToClaim : RewardNodeState.Claimed;
    //        }
    //        else
    //        {
    //            state = RewardNodeState.Locked; // Future days
    //        }


    //        _dayNodes[i].Setup(weekData[i], state);
    //    }
    //}


    public void PlayClaimAnimation(int dayIndex)
    {
        _dayNodes[dayIndex].PlayBurstVFX();
        _dayNodes[dayIndex].SetState(RewardNodeState.Claimed);

        // Show the close button so the player can leave
        _closeButton.gameObject.SetActive(true);
    }

    private void OnDestroy() { _closeButton.onClick.RemoveAllListeners(); }
}
