// Attached to: LootboxRevealCanvas
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using TMPro;


public class LootboxRevealView : MonoBehaviour, ILootboxRevealView
{
    [Header("Sub-Widgets")]
    [SerializeField] private FullScreenTapWidget _tapZone;
    [SerializeField] private TheatricalStageWidget _theatricalStage;
    //[SerializeField] private SummaryGridWidget _summaryGrid;


    [Header("UI Elements")]
    [SerializeField] private Animator _backgroundAnimator;
    [SerializeField] private TextMeshProUGUI _promptText;


    // The Presenter listens to this!
    public event Action OnScreenTapped;


    private void Awake()
    {
        // Route the dumb tap zone's event up to the View's event
        _tapZone.OnTapped += () => OnScreenTapped?.Invoke();
    }


    public void DimBackground(float duration)
    {
        _backgroundAnimator.SetFloat("Speed", 1f / duration);
        _backgroundAnimator.SetTrigger("Dim");
    }


    public void PlayBoxSpawnAnimation() => _theatricalStage.SpawnBox();

    public void PlayBoxEruptionVFX(string rarity) => _theatricalStage.PlayEruption(rarity);


    public async Task PlayItemRevealAnimationAsync(GameObject itemPrefab, int quantity)
    {
        // We await this so the Presenter knows exactly when the animation finishes
        // before it allows the player to tap again.
        await _theatricalStage.AnimateItemPopAsync(itemPrefab, quantity);
    }


    public void HideBox() => _theatricalStage.HideBox();


    public void ShowFinalSummaryGrid(IReadOnlyList<LootboxReward> rewards)
    {
        //_summaryGrid.gameObject.SetActive(true);
        //_summaryGrid.PopulateGrid(rewards);
    }


    public void ChangeTapPromptTo(string text)
    {
        _promptText.text = text;
        _promptText.GetComponent<Animator>().SetTrigger("Bounce");
    }


    private void OnDestroy() { _tapZone.OnTapped -= () => OnScreenTapped?.Invoke(); }
}
