// 1. Assets/_Project/CoreDomain/Social/ChatMessage.cs
using System;
public struct ChatMessage
{
    public string SenderId { get; set; }
    public string Text { get; set; }
    public long TimestampTicks { get; set; }
    public global::CBS.Models.MessageContent ContentType { get; set; }
    public object Sender { get; set; }
    public DateTime CreationDateUTC { get; set; }
    public global::CBS.Models.ChatTarget Target { get; set; }
    public object TaggedProfile { get; set; }
    public global::CBS.Models.MessageState State { get; set; }
    public string MessageID { get; set; }
    public string ChatID { get; set; }

    public string GetMessageBody()
    {
        throw new NotImplementedException();
    }
}