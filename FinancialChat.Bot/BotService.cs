using FileHelpers;
using FinancialChat.Core.Contracts;
using FinancialChat.Core.Helpers.Messaging;
using FinancialChat.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace FinancialChat.Bot
{
    public class BotService : IBotService
    {
        private string GetStockCodeFromMessage(string message)
        {
            var stockCode = string.Empty;
            Regex proccesor = new(@"\/stock=(?<code>.*)");
            Match matches = proccesor.Match(message);

            if (matches.Success)
                stockCode = matches.Groups["code"].Value;

            return stockCode;
        }

        private List<StockQuote> GetStockQuoteFromAPI(string stockCode)
        {
            string requestUrl = $"https://stooq.com/q/l/?s={stockCode}&f=sd2t2ohlcv&h&e=csv";
            var request = (HttpWebRequest)WebRequest.Create(requestUrl);
            var response = (HttpWebResponse)request.GetResponse();

            TextReader reader = new StreamReader(response.GetResponseStream());

            var engine = new FileHelperEngine<StockQuote>();

            var records = engine.ReadStream(reader);

            return records.ToList();
        }

        public StockQuote GetStockQuote(ChatMessage chatMessage)
        {
            if (chatMessage is null)
                throw new ArgumentNullException(nameof(chatMessage));

            string stockCode = GetStockCodeFromMessage(chatMessage.Message);

            if (!string.IsNullOrWhiteSpace(stockCode))
            {
                try
                {
                    List<StockQuote> stockQuotes = GetStockQuoteFromAPI(stockCode);

                    if (stockQuotes is not null && stockQuotes.Any())
                    {
                        return stockQuotes.First();
                    }
                }
                catch (Exception e)
                {
                    throw new FormatException(e.Message);
                }
            }

            return null;
        }
    }
}