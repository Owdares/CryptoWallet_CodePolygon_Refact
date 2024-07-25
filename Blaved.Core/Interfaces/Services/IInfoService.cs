
using Blaved.Core.Objects.Models.Cryptocurrency;
using Blaved.Core.Objects.Models.Info;

namespace Blaved.Core.Interfaces.Services
{
    public interface IInfoService
    {
        Task<List<CryptoAssetModel>> GetCryptoAssetList();
        Task<CryptoAssetModel> GetCryptoAsset(string asset, string network);
        Task<List<CryptoAssetModel>> GetCryptoAssetList(string network);

        Task<InfoForConvertModel> GetInfoForConvert(string fromAsset, string toAsset);
        Task<List<InfoForConvertModel>> GetInfoForConvert(string fromAsset);

        Task<InfoForExchangeModel> GetInfoForExchange(string fromAsset, string toAsset);
        Task<List<InfoForExchangeModel>> GetInfoForExchange(string fromAsset);

        Task<decimal> GetCoinPriceUSDT(string Asset);

        Task<InfoForCheckModel> GetInfoForCheck(string Asset);
        Task<InfoForBlavedPayIDModel> GetInfoForBlavedPayID(string Asset);
    }
}
