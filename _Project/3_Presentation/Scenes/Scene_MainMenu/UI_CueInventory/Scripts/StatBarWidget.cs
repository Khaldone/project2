// Attached to: Stat_Power, Stat_Spin, Stat_AimLine
using UnityEngine;
using UnityEngine.UI;
using System.Collections;


public class StatBarWidget : MonoBehaviour
{
    [SerializeField] private Image _fillImage;
    [SerializeField] private float _animationSpeed = 2f;


    public void SetStat(float normalizedValue) // Value between 0.0 and 1.0
    {
        StopAllCoroutines();
        StartCoroutine(AnimateFill(normalizedValue));
    }


    private IEnumerator AnimateFill(float targetFill)
    {
        float currentFill = _fillImage.fillAmount;
        float elapsed = 0f;


        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime * _animationSpeed;
            // Use a smooth step for a juicy easing effect
            _fillImage.fillAmount = Mathf.SmoothStep(currentFill, targetFill, elapsed);
            yield return null;
        }


        _fillImage.fillAmount = targetFill;
    }
}