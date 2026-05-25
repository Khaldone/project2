// Assets/_Project/CoreDomain/Analytics/TelemetryMessages.cs

namespace Billiards.Core.Analytics
{
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

    /// <summary>
    /// Telemetry data collected when a match lifecycle completes.
    /// </summary>
    public struct MatchTelemetryMessage
    {
        public string MatchId;
        public string GameMode;        // "8Ball", "9Ball"
        public string OpponentType;    // "Human", "Bot"
        public bool IsWinner;
        public int TotalTurns;
        public int CoinsWagered;
        public float RunningDuration;
    }

    /// <summary>
    /// Telemetry tracking currency sinks, item upgrades, and shop purchases.
    /// </summary>
    public struct EconomyTelemetryMessage
    {
        public string TransactionType; // "Sink", "Source"
        public string CurrencyType;    // "Coins", "PremiumGems"
        public int Amount;
        public string Context;         // "CueUpgrade", "EntryFee", "IAPShop"
        public string ItemId;
    }

    /// <summary>
    /// Telemetry snapshot tracking player input status. Used specifically to trap device/OS state 
    /// inside platform crash dumps during critical gameplay interactions like cue stick pullback.
    /// </summary>
    public struct InputTactileTelemetryMessage
    {
        public string CurrentStateName; // "Pullback", "Aiming", "Striking"
        public float InputIntensity;    // Fixed-point conversion value
        public string EquippedCueId;
    }

    public struct PhysicsDriftTelemetryMessage
    {
        public string MatchId;
        public uint TickNumber;
        public long ExpectedStateHash;
        public long ActualStateHash;
        public string PlatformSpecificModel;
    }


}