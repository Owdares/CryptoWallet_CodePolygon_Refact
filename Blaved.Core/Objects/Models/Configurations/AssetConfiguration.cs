namespace Blaved.Core.Objects.Models.Configurations
{
    public class AssetConfiguration
    {
        public Dictionary<string, List<string>> CoinListByNetwork { get; set; }
        public List<string> CoinList { get; set; }
        public List<string> LanguageList { get; set; }
        public Dictionary<string, string> LanguageAbbreviations { get; set; }
        public List<string> NetworkList { get; set; }
        public List<string> CoinSymbolsUSD { get; set; }
        public Dictionary<string, List<string>> NetworkListByCoin { get; set; }
        public Dictionary<string, List<string>> NetworkViewStandartListByCoin { get; set; }
        public Dictionary<string, string> MainAssetByNetwork { get; set; }
        public Dictionary<string, string> NetworkViewName { get; set; }
        public Dictionary<string, string> NetworkViewNameStandart { get; set; }
        public Dictionary<string, string> NetworkScanHashUrl { get; set; }
        public Dictionary<string, string> CoinViewName { get; set; }
        public Dictionary<string, string> CoinUrl { get; set; }
        public Dictionary<string, string> NetworkUrl { get; set; }
        public Dictionary<string, Dictionary<string, string>> CoinContractByNetwork { get; set; }
        public Dictionary<string, Dictionary<string, int>> CoinDecimalByNetwork { get; set; }
        public Dictionary<string, int> NetworkId { get; set; }
    }
}
