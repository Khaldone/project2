// Assets/_Project/CoreDomain/Analytics/TelemetryMessages.cs

// Pillar 1: Economy
public struct EconomyTransactionMessage
{
    public string CurrencyType; // "Coins", "Gems"
    public int Amount;          // Positive for gain, negative for spend
    public string SourceSinkId; // "IAP_Purchase", "Match_Entry_Fee", "Match_Win", "Tournament_Entry", "Daily_Spin"
    public string Currency; //Coins/ gold/ crystals, etc.
    public int AmountChange;

}
// Pillar 2: Matchmaking Funnel
public struct MatchmakingFunnelMessage
{
    public string FunnelStep;    // "Search_Started", "Match_Found", "Arena_Loaded"
    public int WaitTimeSeconds;  // How long did they wait before this step?
}

// Pillar 3: Bot Deception & Match Results
public struct MatchConcludedMessage
{
    public bool WasBotOpponent;
    public bool DidLocalPlayerWin;
    public bool IsRanked;
    public int TotalTurnsTaken;
    public string EndReason;     // "8_Ball_Sunk", "Opponent_Quit", "Time_Expired"
}
// Pillar 4: First-Time User Experience (FTUE)
public struct TutorialStepMessage
{
    public int StepIndex;        // 1, 2, 3...
    public string StepName;      // "Learn_Camera", "Learn_Power_Slider", "Learn_Spin"
    public int TimeSpentSeconds; // NEW: Carries the duration payload
}

