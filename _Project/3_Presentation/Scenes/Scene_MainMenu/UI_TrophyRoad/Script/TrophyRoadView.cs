// Attached to: TrophyRoadCanvas
// [LEGACY] This file is commented out. It references the old TrophyNodeWidget API
// (OnClaimClicked event, PlayClaimedAnimation). The new widget lives in
// Assets/_Project/3_Presentation/Progression/Trophy Road/TrophyNodeWidget.cs
// and uses a different contract (Setup(TrophyMilestone, Action), UpdateVisualState).

//using System;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.UI;
//using TMPro;
//using Billiards.Core.Progression;
//using Billiards.Presentation.TrophyRoad.Widgets;

//public interface ITrophyRoadView
//{
//}

//public class TrophyRoadView : MonoBehaviour, ITrophyRoadView
//{
//    [Header("UI References")]
//    [SerializeField] private Button _closeButton;
//    [SerializeField] private TextMeshProUGUI _currentTrophiesText;
//    [SerializeField] private GameObject _screenBlocker;
//    [SerializeField] private ScrollRect _scrollRect;

//    [Header("Node Pooling")]
//    [SerializeField] private TrophyNodeWidget _nodePrefab;
//    [SerializeField] private Transform _contentTrack;

//    private List<TrophyNodeWidget> _activeNodes = new List<TrophyNodeWidget>();

//    public event Action OnCloseClicked;
//    public event Action<int> OnClaimNodeClicked;

//    private void Awake()
//    {
//        _closeButton.onClick.AddListener(() => OnCloseClicked?.Invoke());
//    }

//    public void RenderMilestoneNodes(IReadOnlyList<TrophyMilestone> milestones)
//    {
//        ClearNodes();
//        for (int i = 0; i < milestones.Count; i++)
//        {
//            var node = Instantiate(_nodePrefab, _contentTrack);
//            node.Setup(i, milestones[i]);
//            node.OnClaimClicked += HandleNodeClaimed;
//            _activeNodes.Add(node);
//        }
//    }

//    private void HandleNodeClaimed(int nodeIndex)
//    {
//        OnClaimNodeClicked?.Invoke(nodeIndex);
//    }

//    public void ShowLoadingBlocker(bool isVisible)
//    {
//        _screenBlocker.SetActive(isVisible);
//    }

//    public void ScrollToTrophyCount(int currentTrophies)
//    {
//        _currentTrophiesText.text = currentTrophies.ToString("N0");
//    }

//    public void PlayUnlockAnimation(int nodeIndex)
//    {
//        _activeNodes[nodeIndex].PlayClaimedAnimation();
//    }

//    private void ClearNodes() { }
//    private void OnDestroy() { _closeButton.onClick.RemoveAllListeners(); }
//}
