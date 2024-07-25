using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blaved.Core.Interfaces
{
    public interface ICryptoAssetModel
    {
        public string ProviderName { get; set; } 
        public string Asset { get; set; } 
        public string AssetUrl { get; set; } 
        public bool DepositAllEnable { get; set; }
        public bool WithdrawAllEnable { get; set; }
        public string AssetName { get; set; } 
        public List<ICryptoNetworkModel> NetworkList { get; set; }
        public ICryptoNetworkModel? SelectNetwork { get; set; } 
    }
    public interface ICryptoNetworkModel
    {
        public string? AssetContract { get; set; } 
        public int AssetDecimals { get; set; }
        public string NetworkAsset { get; set; }
        public string Asset { get; set; } 
        public string NetworkName { get; set; } 
        public string Network { get; set; } 
        public string NetworkUrl { get; set; }
    }
    public interface ICryptoWithdrawConfigModel
    {
        public bool WithdrawEnabled { get; set; }
        public decimal WithdrawFee { get; set; }
        public decimal WithdrawMin { get; set; }
        public decimal WithdrawInternalFee { get; set; }
        public decimal WithdrawCombineFee { get; set; }
        public decimal WithdrawMax { get; set; }
    }
    public interface ICryptoDepositConfigModel
    {
        public bool DepositEnabled { get; set; }
        public int MinConfirmations { get; set; }
        public decimal DepositMin { get; set; }
    }
}
