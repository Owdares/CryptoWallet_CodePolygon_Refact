namespace Blaved.Core.Objects.Models
{
    public class ExchangeQuoteModel
    {
        public string? QuoteId { get; set; }
        public string ToAsset { get; set; }

        public string FromAsset { get; set; }

        public decimal ToAmount { get; set; }

        public decimal FromAmount { get; set; }

        public decimal Fee { get; set; }
    }
}
