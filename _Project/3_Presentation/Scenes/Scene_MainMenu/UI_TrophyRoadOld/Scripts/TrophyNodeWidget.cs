// Attached to: Node_Prefab
//using System;
//using UnityEngine;
//using UnityEngine.UI;
//using TMPro;
//using Billiards.Core.Progression;

//public class TrophyNodeWidget : MonoBehaviour
//{
//    [Header("Data Display")]
//    [SerializeField] private TextMeshProUGUI _requiredTrophiesText;
//    [SerializeField] private Image _rewardIcon;

//    [Header("State Visuals")]
//    [SerializeField] private GameObject _lockedStateVisuals;
//    [SerializeField] private GameObject _unlockedStateVisuals;
//    [SerializeField] private GameObject _claimedStateVisuals;

//    [Header("Interaction")]
//    [SerializeField] private Button _claimButton;
//    [SerializeField] private Animator _nodeAnimator;


//    private int _myIndex;
//    public event Action<int> OnClaimClicked;

//    private void Awake()
//    {
//        _claimButton.onClick.AddListener(() => OnClaimClicked?.Invoke(_myIndex));
//    }

//    public void Setup(int index, TrophyMilestone milestoneData)
//    {
//        _myIndex = index;
//        _requiredTrophiesText.text = milestoneData.RequiredTrophies.ToString();

//         Turn everything off first
//        _lockedStateVisuals.SetActive(false);
//        _unlockedStateVisuals.SetActive(false);
//        _claimedStateVisuals.SetActive(false);
//        _claimButton.interactable = false;


//         Turn on the correct state based on the pure C# data struct
//        if (milestoneData.IsClaimed)
//        {
//            _claimedStateVisuals.SetActive(true);
//        }
//        else if (milestoneData.IsUnlocked)
//        {
//            _unlockedStateVisuals.SetActive(true);
//            _claimButton.interactable = true;
//            _nodeAnimator.SetBool("IsPulsing", true); // Make it obvious they can click it
//        }
//        else
//        {
//            _lockedStateVisuals.SetActive(true);
//        }
//    }

//    public void PlayClaimedAnimation()
//    {
//        _nodeAnimator.SetBool("IsPulsing", false);
//        _nodeAnimator.SetTrigger("ClaimSequence");

//         The animation will swap the active GameObjects from Unlocked -> Claimed
//         using Unity Animation Events halfway through the particle burst.
//    }
//}