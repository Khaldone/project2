using System;

namespace ibc.network
{
    public enum NetFailCode
    {
        None,
        Cancelled,
        Timeout,
        AuthFailed,
        MatchmakingFailed,
        StartFailed,
        JoinFailed,
        SpectateFailed,
        RejoinNotPossible,
        Disconnected,
        Unknown,
    }
    
    public sealed class NetException : Exception
    {
        public NetFailCode Code { get; }

        public NetException(NetFailCode code, string message, Exception inner = null)
            : base(message, inner)
        {
            Code = code;
        }

        public override string ToString() => $"{Code}: {Message}\n{base.ToString()}";
    }
    
    public static class NetErrors
    {
        public static NetException Cancelled() =>
            new(NetFailCode.Cancelled, "Operation cancelled.");

        public static NetException Timeout(string what) =>
            new(NetFailCode.Timeout, $"{what} timed out.");

        public static NetException StartFailed(string why, Exception inner = null) =>
            new(NetFailCode.StartFailed, $"Fusion start failed: {why}", inner);

        public static NetException JoinFailed(string why, Exception inner = null) =>
            new(NetFailCode.JoinFailed, $"Fusion join failed: {why}", inner);

        public static NetException Disconnected(string why, Exception inner = null) =>
            new(NetFailCode.Disconnected, $"Disconnected: {why}", inner);
    }
}
