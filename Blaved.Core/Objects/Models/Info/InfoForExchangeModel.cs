namespace Blaved.Core.Objects.Models.Info
{
    public class InfoForExchangeModel
    {
        public string Method { get; set; }
        public string FromAsset { get; set; }
        public string ToAsset { get; set; }
        public decimal MinAmount { get; set; }
        public decimal MaxAmount { get; set; }
        public decimal ExchangeInternalFee { get; set; }
    }
}
