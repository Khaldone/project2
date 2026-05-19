// Attached to: Economy_Tally_Widget
using UnityEngine;
using TMPro;
using System.Collections;
using System;


public class RewardTallyWidget : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _payoutText;
    [SerializeField] private ParticleSystem _coinBurstVfx;
    [SerializeField] private AudioSource _tickSound; // A rapid clicking sound


    public void AnimatePayout(int targetAmount, Action onComplete)
    {
        StartCoroutine(RollNumberRoutine(targetAmount, onComplete));
    }


    private IEnumerator RollNumberRoutine(int target, Action onComplete)
    {
        float duration = 1.5f;
        float elapsed = 0f;
        int currentDisplay = 0;


        _tickSound.Play();


        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            currentDisplay = Mathf.RoundToInt(Mathf.Lerp(0, target, elapsed / duration));
            _payoutText.text = $"+{currentDisplay:N0}";
            yield return null;
        }


        _payoutText.text = $"+{target:N0}";
        _tickSound.Stop();

        if (target > 0)
        {
            _coinBurstVfx.Play();
        }


        yield return new WaitForSeconds(0.5f); // Let the particles fly before continuing
        onComplete?.Invoke();
    }
}