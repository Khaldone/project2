// Assets/Scripts/Presentation/Services/UnityAdsServiceWrapper.cs
using UnityEngine;
using System;
public class UnityAdsServiceWrapper : MonoBehaviour, IAdsService
{
    public bool IsRewardedAdReady()
    {
        // Example integration logic
        // return Advertisement.IsReady("rewardedVideo");
        return true;
    }
    public void ShowRewardedAd(Action onRewardEarned, Action onAdFailed)
    {
        Debug.Log("Playing real Unity Ad...");
        // Implement Unity Ads SDK callback listeners here
        onRewardEarned?.Invoke();
    }
}
