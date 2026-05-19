// Attached to: CueInventoryCanvas
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public interface ICueInventoryView
{

}
public class CueInventoryView : MonoBehaviour, ICueInventoryView
{
    [Header("Core UI")]
    [SerializeField] private Button _closeButton;
    [SerializeField] private Button _equipButton;
    [SerializeField] private TextMeshProUGUI _equipButtonText;


    [Header("Grid System")]
    //[SerializeField] private CueSlotWidget _slotPrefab;
    [SerializeField] private Transform _gridContent;

    [Header("Inspection Panel")]
    [SerializeField] private TextMeshProUGUI _inspectionNameText;
    [SerializeField] private StatBarWidget _powerBar;
    [SerializeField] private StatBarWidget _spinBar;
    [SerializeField] private StatBarWidget _aimBar;
    [SerializeField] private Cue3DViewerWidget _3dViewer;


    //private List<CueSlotWidget> _activeSlots = new List<CueSlotWidget>();
    private string _currentlyInspectedCueId;


    public event Action OnCloseClicked;
    public event Action<string> OnCueSelected; // Fired when tapping a grid item
    public event Action<string> OnEquipClicked; // Fired when clicking "Equip"


    private void Awake()
    {
        _closeButton.onClick.AddListener(() => OnCloseClicked?.Invoke());
        _equipButton.onClick.AddListener(() => OnEquipClicked?.Invoke(_currentlyInspectedCueId));
    }


    //public void PopulateGrid(IReadOnlyList<CueInventoryItem> cues, string currentlyEquippedId)
    //{
    //    ClearGrid();


    //    foreach (var cue in cues)
    //    {
    //        var slot = Instantiate(_slotPrefab, _gridContent);
    //        slot.Setup(cue, isEquipped: cue.Id == currentlyEquippedId);

    //        // Route the individual slot's click up to the Presenter
    //        slot.OnClicked += () => OnCueSelected?.Invoke(cue.Id);

    //        _activeSlots.Add(slot);
    //    }
    //}


    //public void UpdateInspectionPanel(CueInventoryItem cue, bool isEquipped)
    //{
    //    _currentlyInspectedCueId = cue.Id;
    //    _inspectionNameText.text = cue.DisplayName;


    //    // Animate the stat bars filling up
    //    _powerBar.SetStat(cue.PowerStat);
    //    _spinBar.SetStat(cue.SpinStat);
    //    _aimBar.SetStat(cue.AimStat);


    //    // Tell the 3D viewer to download and render the heavy mesh
    //    _3dViewer.LoadAndRenderCueAsync(cue.AddressableModelKey);


    //    // Update Equip button state
    //    _equipButton.interactable = cue.IsUnlocked && !isEquipped;
    //    _equipButtonText.text = isEquipped ? "EQUIPPED" : (cue.IsUnlocked ? "EQUIP" : "LOCKED");
    //}


    private void ClearGrid() { /* Pool or destroy active slots */ }

    private void OnDestroy() { /* Clean up listeners */ }
}