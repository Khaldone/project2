// Assets/_Project/Scenes/00_Hub_Bootstrap/Scripts/AI/BotProfileDatabase.cs
using UnityEngine;

[CreateAssetMenu(fileName = "BotProfileDatabase", menuName = "Billiards/Bot Profile Database")]
public class BotProfileDatabase : ScriptableObject
{
    public string[] FirstNames;
    public string[] LastNames; // Optional: combine them, or just use cool nicknames
    public string[] CountryCodes = { "US", "UK", "BR", "IN", "JP", "FR" };
    public string[] AvatarIds;
}