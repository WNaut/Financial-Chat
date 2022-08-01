using System;
using FinancialChat.Core.Helpers.Messaging;
using FinancialChat.Core.Models;
using NUnit.Framework;

namespace FinancialChat.Bot.Facts
{
    [TestFixture]
    public sealed class BotServiceFacts
    {
        private BotService _botService;

        [OneTimeSetUp]
        public void SetUp()
        {
            _botService = new BotService();
        }

        [Test]
        public void With_Null_Command_Throws_ArgumentNullException() =>
            Assert.Throws<ArgumentNullException>(() => _botService.GetStockQuote(null));

        [TestCase("/stock=AAPL.US")]
        [TestCase("/stock=MSFT.UK")]
        [TestCase("/stock=GOOGL.US")]
        public void With_Valid_Command_Returns_StockQuote(string command)
        {
            ChatMessage message = new()
            {
                Message = command
            };

            StockQuote response = _botService.GetStockQuote(message);

            Assert.That(response, Is.InstanceOf(typeof(StockQuote)));
        }

        [TestCase("")]
        [TestCase("   ")]
        [TestCase("/stock=")]
        public void With_Invalid_Command_Returns_Null(string command)
        {
            ChatMessage message = new()
            {
                Message = command
            };

            StockQuote response = _botService.GetStockQuote(message);

            Assert.That(response, Is.Null);
        }

        [TestCase("/stock=No spaces allowed")]
        [TestCase("/stock=No_speci@l_chars_allowed")]
        public void With_Malformed_Command_Throws_FormatException(string command)
        {
            ChatMessage message = new()
            {
                Message = command
            };

            Assert.Throws<FormatException>(() => _botService.GetStockQuote(message));
        }
    }
}
