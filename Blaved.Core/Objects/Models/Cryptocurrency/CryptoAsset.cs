using Blaved.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blaved.Core.Objects.Models.Cryptocurrency
{
    public class CryptoAssetModel
    {
        public string ProviderName { get; set; } = string.Empty;
        public string Asset { get; set; } = string.Empty;
        public string AssetUrl { get; set; } = string.Empty;
        public bool DepositAllEnable { get; set; }
        public bool WithdrawAllEnable { get; set; }
        public string AssetName { get; set; } = string.Empty;
        public List<CryptoNetworkModel> NetworkList { get; set; } = new List<CryptoNetworkModel>();
        public CryptoNetworkModel? SelectNetwork { get; set; } = null;
    }
    public class CryptoNetworkModel
    {
        public string? AssetContract { get; set; } = null;
        public int AssetDecimals { get; set; }
        public string NetworkAsset { get; set; } = string.Empty;
        public string Asset { get; set; } = string.Empty;
        public string NetworkName { get; set; } = string.Empty;
        public string Network { get; set; } = string.Empty;
        public string NetworkUrl { get; set; } = string.Empty;
        public CryptoWithdrawConfigModel CryptoWithdrawConfig { get; set; } = new();
        public CryptoDepositConfigModel CryptoDepositConfig { get; set; } = new();
    }
    public class CryptoWithdrawConfigModel
    {
        public bool WithdrawEnabled { get; set; }
        public decimal WithdrawFee { get; set; }
        public decimal WithdrawMin { get; set; }
        public decimal WithdrawInternalFee { get; set; }
        public decimal WithdrawCombineFee { get; set; }
        public decimal WithdrawMax { get; set; }
    }
    public class CryptoDepositConfigModel
    {
        public bool DepositEnabled { get; set; }
        public int MinConfirmations { get; set; }
        public decimal DepositMin { get; set; }
    }
}
