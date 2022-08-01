using FinancialChat.Core.Helpers.Messaging;

namespace FinancialChat.Core.Contracts
{
    public interface IMessageSender
    {
        void SendMessage(ChatMessage chatMessage);
    }
}
