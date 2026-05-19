// Assets/_Project/Scenes/00_Hub_Bootstrap/Scripts/AI/UnityBotProfileProvider.cs
using UnityEngine;


public class UnityBotProfileProvider : MonoBehaviour, IBotProfileProvider
{
    [SerializeField] private BotProfileDatabase _database;


    public PlayerProfile GenerateFakeOpponent(int playerCurrentLevel)
    {
        string randomName = _database.FirstNames[Random.Range(0, _database.FirstNames.Length)];
        string randomCountry = _database.CountryCodes[Random.Range(0, _database.CountryCodes.Length)];
        string randomAvatar = _database.AvatarIds[Random.Range(0, _database.AvatarIds.Length)];

        // Make the bot's level believable (e.g., +/- 2 levels from the human player)
        int botLevel = Mathf.Clamp(playerCurrentLevel + Random.Range(-2, 3), 1, 100);


        return new PlayerProfile
        {
            UserId = System.Guid.NewGuid().ToString(), // Looks like a real ID to the system
            DisplayName = randomName,
            CountryCode = randomCountry,
            Level = botLevel,
            AvatarId = randomAvatar
        };
    }
}
