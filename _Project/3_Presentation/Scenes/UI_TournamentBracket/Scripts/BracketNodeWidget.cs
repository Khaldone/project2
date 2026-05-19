// Attached to: All 15 Node GameObjects
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public enum NodeStatus { Empty, Waiting, Playing, Winner, Eliminated }

public class BracketNodeWidget : MonoBehaviour
{
    [SerializeField] private Image _avatarImage;
    [SerializeField] private TextMeshProUGUI _playerNameText;
    [SerializeField] private GameObject _eliminatedOverlay;
    [SerializeField] private Animator _nodeAnimator;


    //private TournamentPlayerProfile _currentProfile;

    //public void SetProfile(TournamentPlayerProfile profile)
    //{
    //    _currentProfile = profile;
    //    _playerNameText.text = profile.Name;
    //    _avatarImage.sprite = profile.Avatar; // Usually loaded via Addressables
    //}



    //public TournamentPlayerProfile GetProfile() => _currentProfile;


    public void SetStatus(NodeStatus status)
    {
        _eliminatedOverlay.SetActive(status == NodeStatus.Eliminated);
        _nodeAnimator.SetBool("IsPlaying", status == NodeStatus.Playing);

        if (status == NodeStatus.Winner)
        {
            _nodeAnimator.SetTrigger("Celebrate");
        }
    }
}