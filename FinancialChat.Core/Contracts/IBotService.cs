using FinancialChat.Core.Helpers.Messaging;
using FinancialChat.Core.Models;

namespace FinancialChat.Core.Contracts
{
    public interface IBotService
    {
        StockQuote GetStockQuote(ChatMessage chatMessage);
    }
}
