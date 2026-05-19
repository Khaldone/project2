// Assets/Scripts/CoreDomain/Services/IAdsService.cs
using System;
public interface IAdsService
{
    bool IsRewardedAdReady();
    void ShowRewardedAd(Action onRewardEarned, Action onAdFailed);
}