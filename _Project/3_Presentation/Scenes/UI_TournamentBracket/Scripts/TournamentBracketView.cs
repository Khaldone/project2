// Attached to: TournamentCanvas
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public interface ITournamentBracketView
{

}

public class TournamentBracketView : MonoBehaviour, ITournamentBracketView
{
    [Header("Bracket Structure")]
    [Tooltip("Index 0-7 are QF, 8-11 are SF, 12-13 are Finals, 14 is Champion")]
    [SerializeField] private BracketNodeWidget[] _allNodes;
    //[SerializeField] private BracketLineAnimator[] _connectingLines;

    [Header("Controls")]
    [SerializeField] private Button _playMatchButton;
    [SerializeField] private Button _forfeitButton;
    [SerializeField] private TextMeshProUGUI _playButtonText;


    public event Action OnPlayMatchClicked;
    public event Action OnForfeitClicked;


    private void Awake()
    {
        _playMatchButton.onClick.AddListener(() => OnPlayMatchClicked?.Invoke());
        _forfeitButton.onClick.AddListener(() => OnForfeitClicked?.Invoke());
    }


    // Called by the Presenter to instantly set up the bracket
    //public void PopulateNode(int nodeIndex, TournamentPlayerProfile profile)
    //{
    //    _allNodes[nodeIndex].SetProfile(profile);
    //}


    // Called by the Presenter to animate a player advancing to the next round
    public void AnimateAdvancement(int fromNodeIndex, int toNodeIndex, int lineIndex)
    {
        _allNodes[fromNodeIndex].SetStatus(NodeStatus.Winner);
        //_connectingLines[lineIndex].AnimateLineFill();

        // Copy the profile from the winner's old node to their new node
        //var winningProfile = _allNodes[fromNodeIndex].GetProfile();
        //_allNodes[toNodeIndex].SetProfile(winningProfile);
        //_allNodes[toNodeIndex].SetStatus(NodeStatus.Waiting);
    }


    public void SetPlayButtonState(bool interactable, string text)
    {
        _playMatchButton.interactable = interactable;
        _playButtonText.text = text;
    }
}