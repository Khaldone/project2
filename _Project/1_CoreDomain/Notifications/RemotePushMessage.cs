// 2. Assets/_Project/CoreDomain/Notifications/RemotePushMessage.cs
// The envelope we drop into the Message Broker when a push arrives while the app is open
public struct RemotePushMessage
{
    public string Title;
    public string Body;
    public string DeepLinkAction; // e.g., "open_shop", "join_tournament"
}