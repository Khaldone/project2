// Attached to: MatchResultsCanvas
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public interface IMatchResultsView
{

}
public class MatchResultsView : MonoBehaviour, IMatchResultsView
{
    [Header("Sub-Widgets")]
    //[SerializeField] private PlayerResultWidget _winnerCard;
    //[SerializeField] private PlayerResultWidget _loserCard;
    [SerializeField] private RewardTallyWidget _economyWidget;
    //[SerializeField] private TrophyUpdateWidget _trophyWidget;
    [SerializeField] private LootboxDropWidget _lootboxWidget;


    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI _titleText;
    [SerializeField] private Animator _headerAnimator;
    [SerializeField] private Button _returnButton;


    public event Action OnReturnClicked;


    private void Awake()
    {
        _returnButton.onClick.AddListener(() => OnReturnClicked?.Invoke());
        _returnButton.gameObject.SetActive(false); // Hide until the sequence is done
    }


    // 1. The Presenter calls this immediately when the scene loads
    //public void InitializeData(MatchResultData data)
    //{
    //    _titleText.text = data.IsLocalPlayerWinner ? "YOU WIN!" : "YOU LOSE!";
    //    _headerAnimator.SetTrigger(data.IsLocalPlayerWinner ? "PlayWin" : "PlayLoss");


    //    _winnerCard.SetPlayer(data.WinnerName, data.WinnerAvatar);
    //    _loserCard.SetPlayer(data.LoserName, data.LoserAvatar);
    //}


    // 2. The Presenter calls this to start the theatrical rollout
    public void PlayRewardSequence(int coinPayout, int oldTrophies, int newTrophies, LootboxInstance? droppedBox)
    {
        // In a real AAA game, this would be an async UniTask sequence
        // yielding on the completion of each widget's animation.

        _economyWidget.AnimatePayout(coinPayout, onComplete: () =>
        {
            //_trophyWidget.AnimateTrophyChange(oldTrophies, newTrophies, onComplete: () =>
            //{
            //    if (droppedBox.HasValue)
            //    {
            //        _lootboxWidget.PlayDropAnimation(droppedBox.Value, ShowReturnButton);
            //    }
            //    else
            //    {
            //        ShowReturnButton();
            //    }
            //});
        });
    }


    private void ShowReturnButton()
    {
        _returnButton.gameObject.SetActive(true);
    }
}