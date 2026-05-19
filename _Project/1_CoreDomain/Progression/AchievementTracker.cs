// Assets/_Project/CoreDomain/Progression/AchievementTracker.cs
public class AchievementTracker
{
    private readonly IMessageBroker _broker;

    public void OnPlayFabAchievementUnlocked(string achName, string desc)
    {
        // Drop the envelope in the global mail!
        //_broker.Publish(new AchievementUnlockedMessage
        //{
        //    Name = achName,
        //    Description = desc
        //});
    }
}