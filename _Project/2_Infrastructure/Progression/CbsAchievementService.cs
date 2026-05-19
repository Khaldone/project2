// Assets/_Project/2_Infrastructure/Progression/CbsAchievementService.cs
using System.Collections.Generic;
using System.Linq;
using CBS;
using CBS.Models;
using Cysharp.Threading.Tasks;
using Billiards.CoreDomain.Player;
using Billiards.CoreDomain.Progression;
using UnityEngine;

namespace Billiards.Infrastructure.Progression
{
    public class CbsAchievementService : IAchievementDataSource
    {
        private IAchievements AchievementModule { get; set; }

        public async UniTask<List<AchievementData>> FetchAchievementsAsync()
        {
            var tcs = new UniTaskCompletionSource<List<AchievementData>>();
            
            // Get the module right before fetching to ensure CBS is fully initialized
            AchievementModule = CBSModule.Get<CBSAchievementsModule>();

            AchievementModule.GetAchievementsTable(result =>
            {
                if (result.IsSuccess)
                {
                    var domainAchievements = new List<AchievementData>();
                    var achievements = result.AchievementsData?.Tasks;

                    if (achievements != null)
                    {
                        foreach (var achievement in achievements)
                        {
                            domainAchievements.Add(new AchievementData
                            {
                                Id = achievement.ID,
                                Title = achievement.Title,
                                Description = achievement.Description,
                                CurrentProgress = achievement.CurrentStep,
                                MaxProgress = achievement.Steps,
                                IsClaimed = achievement.Rewarded
                            });
                        }
                    }

                    // RESOLVE THE TASK: This tells the UI to stop waiting and display the panels!
                    tcs.TrySetResult(domainAchievements);
                }
                else
                {
                    // If PlayFab fails (e.g. not logged in), log the error and return an empty list so the UI doesn't hang forever.
                    UnityEngine.Debug.LogError($"[CBS] Failed to fetch achievements: {result.Error?.Message}. Ensure PlayFabAuthService is logged in first!");
                    tcs.TrySetResult(new List<AchievementData>());
                }
            });

            return await tcs.Task;
        }
    }
}
