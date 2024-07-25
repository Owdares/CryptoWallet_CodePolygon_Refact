namespace Blaved.Core.Objects.Models.Configurations
{
    public class FunctionConfiguration
    {
        public FunctionEnable FunctionEnable { get; set; }
        public AssetEnable AssetEnable { get; set; }
        public NetworkEnable NetworkEnable { get; set; }
        public Dictionary<string, Dictionary<string, decimal>> WithdrawInternalFee { get; set; }
        public Dictionary<string, decimal> ExchangeInternalFee { get; set; }
        public decimal ExchangeHiddenFeeInPercent { get; set; }
    }
    public class AssetEnable
    {
        public Dictionary<string, bool> Deposit { get; set; }
        public Dictionary<string, bool> Withdraw { get; set; }
        public Dictionary<string, bool> BlavedPay { get; set; }
        public Dictionary<string, bool> Check { get; set; }
        public Dictionary<string, bool> Exchange { get; set; }
    }
    public class NetworkEnable
    {
        public Dictionary<string, bool> Deposit { get; set; }
        public Dictionary<string, bool> Withdraw { get; set; }
    }
    public class FunctionEnable
    {
        public bool Deposit { get; set; }
        public bool Withdraw { get; set; }
        public bool BlavedPay { get; set; }
        public bool Check { get; set; }
        public bool Exchange { get; set; }
    }
}
