namespace Blaved.Core.Objects.Models.Info
{
    public class InfoForConvertModel
    {
        public string FromAsset { get; set; }
        public string ToAsset { get; set; }
        public decimal MinAmount { get; set; }
        public decimal MaxAmount { get; set; }
        public decimal ConvertInternalFee { get; set; }
    }
}
