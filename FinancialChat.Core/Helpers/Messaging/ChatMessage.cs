using System;

namespace FinancialChat.Core.Helpers.Messaging
{
    public sealed class ChatMessage
    {
        public string SentBy { get; set; }
        public DateTime SentOn { get; set; }
        public string Message { get; set; }
        public string Room { get; set; }
    }
}
