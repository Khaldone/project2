// Assets/_Project/1_CoreDomain/Progression/IAchievementDataSource.cs
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Billiards.CoreDomain.Player;

namespace Billiards.CoreDomain.Progression
{
    public interface IAchievementDataSource
    {
        UniTask<List<AchievementData>> FetchAchievementsAsync();
    }
}
