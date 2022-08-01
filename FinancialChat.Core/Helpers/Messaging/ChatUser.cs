namespace FinancialChat.Core.Helpers.Messaging
{
    public sealed class ChatUser
    {
        public string ConnectionId { get; set; }
        public string Username { get; set; }
        public string CurrentRoom { get; set; }
    }
}
