// Attached to: MatchmakingCanvas
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public interface IMatchmakingView
{

}
public class MatchmakingView : MonoBehaviour, IMatchmakingView
{
    [Header("Local Player UI")]
    [SerializeField] private Image _localAvatar;
    [SerializeField] private TextMeshProUGUI _localName;


    [Header("Opponent UI")]
    [SerializeField] private Image _opponentAvatar;
    [SerializeField] private TextMeshProUGUI _opponentName;
    [SerializeField] private GameObject _opponentLevelBadge;

    [Header("State UI")]
    [SerializeField] private TextMeshProUGUI _statusText;
    [SerializeField] private Button _cancelButton;
    [SerializeField] private Animator _matchmakingAnimator; // Handles the dramatic "Match Found" slam animation


    public event Action OnCancelClicked;


    private void Awake()
    {
        _cancelButton.onClick.AddListener(() => OnCancelClicked?.Invoke());
    }


    public void SetupLocalPlayer(Sprite avatar, string playerName)
    {
        _localAvatar.sprite = avatar;
        _localName.text = playerName;
    }


    // Called by the Presenter during the search phase
    public void UpdateStatus(string status)
    {
        _statusText.text = status;
    }


    // Called by the Presenter the exact millisecond Photon finds a player (or a Bot is assigned)
    public void LockInOpponent(Sprite avatar, string opponentName)
    {
        _opponentAvatar.sprite = avatar;
        _opponentName.text = opponentName;
        _opponentLevelBadge.SetActive(true);


        // 1. Physically prevent the player from cancelling now
        _cancelButton.interactable = false;

        // 2. Trigger the juicy visual slam animation (screen shakes, avatars crash together)
        _matchmakingAnimator.SetTrigger("MatchFound");
    }


    private void OnDestroy()
    {
        _cancelButton.onClick.RemoveAllListeners();
    }
}