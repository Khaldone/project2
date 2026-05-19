// 4. Assets/Scripts/CoreDomain/SaveSystem/PlayerDataService.cs
// The Brain. This is the target of our unit tests.
using Billiards.CoreDomain.Player;
using Billiards.CoreDomain.Progression;
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
public class PlayerDataService : IPlayerDataService, IPlayerDataServiceProgression
{
    private readonly ICloudSaveBackend _cloudBackend;
    private readonly IAchievementDataSource _achievementDataSource;
    public PlayerProfileSave CurrentProfile { get; private set; } = new PlayerProfileSave();

    private bool _isDirty = false;

    // IPlayerDataServiceProgression implementation
    public int CurrentCoins => CurrentProfile.Coins;
    public string PlayerName { get; private set; } = "Player";

    public event Action<int> OnCoinsChanged;
    public event Action<string> OnNameChanged;

    // We inject the PlayFab backend into this pure C# logic
    public PlayerDataService(ICloudSaveBackend cloudBackend, IAchievementDataSource achievementDataSource)
    {
        _cloudBackend = cloudBackend;
        _achievementDataSource = achievementDataSource;
    }


    // Called during the Bootstrapper sequence
    public async UniTask InitializeAsync()
    {
        var profile = await _cloudBackend.LoadProfileAsync();
        if (profile != null)
        {
            CurrentProfile = profile;
        }
    }


    public void AddCoins(int amount)
    {
        CurrentProfile.Coins += amount;
        _isDirty = true; // Mark that local changes haven't been pushed to the cloud
        OnCoinsChanged?.Invoke(CurrentProfile.Coins);
    }

    public void SetPlayerName(string name)
    {
        PlayerName = name;
        OnNameChanged?.Invoke(name);
    }


    public void UnlockCue(string cueId)
    {
        if (!CurrentProfile.UnlockedCues.Contains(cueId))
        {
            CurrentProfile.UnlockedCues.Add(cueId);
            _isDirty = true;
        }
    }


    // The background worker or a manual UI button calls this
    public async UniTask ForceSyncAsync()
    {
        if (!_isDirty) return; // Save network bandwidth!


        bool success = await _cloudBackend.SaveProfileAsync(CurrentProfile);
        if (success)
        {
            _isDirty = false; // Sync complete, cache is clean
        }
    }

    private List<AchievementData> _cachedAchievements;

    public async UniTask<List<AchievementData>> GetAchievementsAsync()
    {
        if (_cachedAchievements == null)
        {
            _cachedAchievements = await _achievementDataSource.FetchAchievementsAsync();
        }
        return _cachedAchievements;
    }

    public void ClearAchievementsCache()
    {
        _cachedAchievements = null;
    }
}