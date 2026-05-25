// Assets/_Project/2_Infrastructure/Telemetry/Serialization/TelemetryEventFactory.cs
using System.Collections.Generic;
using Billiards.Core.Analytics;

namespace Billiards.Infrastructure.Telemetry.Serialization
{
    public static class TelemetryEventFactory
    {
        public static Dictionary<string, object> BuildMatchPayload(MatchTelemetryMessage msg)
        {
            var dict = TelemetryEventPool.Get();
            dict["match_id"] = msg.MatchId;
            dict["game_mode"] = msg.GameMode;
            dict["opponent_type"] = msg.OpponentType;
            dict["is_winner"] = msg.IsWinner ? 1 : 0;
            dict["turns"] = msg.TotalTurns;
            dict["wager"] = msg.CoinsWagered;
            dict["duration"] = msg.RunningDuration;
            return dict;
        }

        public static Dictionary<string, object> BuildEconomyPayload(EconomyTelemetryMessage msg)
        {
            var dict = TelemetryEventPool.Get();
            dict["tx_type"] = msg.TransactionType;
            dict["currency"] = msg.CurrencyType;
            dict["amount"] = msg.Amount;
            dict["context"] = msg.Context;
            dict["item_id"] = msg.ItemId;
            return dict;
        }
    }
}