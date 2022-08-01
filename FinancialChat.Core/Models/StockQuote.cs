using FileHelpers;
using System;

namespace FinancialChat.Core.Models
{
    [DelimitedRecord(","), IgnoreFirst]
    public sealed class StockQuote
    {
        public string Symbol { get; set; }
        [FieldConverter(ConverterKind.Date, "yyyy-MM-dd")]
        public DateTime Date { get; set; }
        public string Time { get; set; }
        public decimal Open { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Close { get; set; }
        public long Volume { get; set; }
    }
}
