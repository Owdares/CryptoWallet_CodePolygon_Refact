using Blaved.Core.Objects.Models.Configurations;
using Blaved.Core.Objects.Models.Cryptocurrency;
using Blaved.Core.Objects.Models.Info;
using Blaved.Core.Utility;
using Blaved.Core.Interfaces.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Blaved.Core.Interfaces;
using System.Linq;

namespace Blaved.Core.Services
{
    public class InfoService : IInfoService
    {
        private readonly AppConfig _appConfig;
        private readonly ILogger<InfoService> _logger;
        public InfoService(IOptions<AppConfig> appConfig, ILogger<InfoService> logger) 
        {
            _appConfig = appConfig.Value;
            _logger = logger;
        }

        public async Task<List<CryptoAssetModel>> GetCryptoAssetList()
        {
            _logger.LogTrace("Request to GetCryptoAssetList");

            var content = await JsonFileManager.GetFromJsonAsync<List<CryptoAssetModel>>(_appConfig.PathConfiguration.CryptoAssetList);

            if (content == null)
            {
                throw new Exception($"Error while fetching GetCryptoAssetList - {_appConfig.PathConfiguration.CryptoAssetList}");
            }

            _logger.LogTrace("Detailed response received for GetCryptoAssetList - {@Response}", JsonConvert.SerializeObject(content, Formatting.Indented));

            return content;
        }
        public async Task<CryptoAssetModel> GetCryptoAsset(string asset, string network)
        {
            _logger.LogTrace("Request to GetCryptoAssetList");

            var content = await JsonFileManager.GetFromJsonAsync<List<CryptoAssetModel>>(_appConfig.PathConfiguration.CryptoAssetList);

            if (content == null)
            {
                throw new Exception($"Error while fetching GetCryptoAssetList - {_appConfig.PathConfiguration.CryptoAssetList}");
            }

            _logger.LogTrace("Detailed response received for GetCryptoAssetList - {@Response}", JsonConvert.SerializeObject(content, Formatting.Indented));

            return content.Single(x => x.Asset == asset && x.NetworkList.Select(x => x.Network).Contains(network));
        }
        public async Task<List<CryptoAssetModel>> GetCryptoAssetList(string network)
        {
            _logger.LogTrace("Request to GetCryptoAssetList");

            var content = await JsonFileManager.GetFromJsonAsync<List<CryptoAssetModel>>(_appConfig.PathConfiguration.CryptoAssetList);

            if (content == null)
            {
                throw new Exception($"Error while fetching GetCryptoAssetList - {_appConfig.PathConfiguration.CryptoAssetList}");
            }

            _logger.LogTrace("Detailed response received for GetCryptoAssetList - {@Response}", JsonConvert.SerializeObject(content, Formatting.Indented));

            return content.Where(x => x.NetworkList.Select(x => x.Network).Contains(network)).ToList();
        }

        public async Task<InfoForConvertModel> GetInfoForConvert(string fromAsset, string toAsset)
        {
            _logger.LogTrace("Request to GetInfoForConvert - {@Request}", new { fromAsset, toAsset });

            var content = await JsonFileManager.GetFromJsonAsync<List<InfoForConvertModel>>(_appConfig.PathConfiguration.InfoForConvert);

            if (content == null)
            {
                throw new Exception($"Error while fetching GetInfoForConvert - {_appConfig.PathConfiguration.InfoForConvert}");
            }

            var filterContent = content.SingleOrDefault(a => a.FromAsset == fromAsset && a.ToAsset == toAsset);
            if (filterContent == null)
            {
                throw new Exception($"Error while fetching GetInfoForConvert with parameters - fromAsset: {fromAsset}, toAsset: {toAsset}");
            }

            _logger.LogTrace("Detailed response received for GetInfoForConvert - {@Response}", JsonConvert.SerializeObject(filterContent, Formatting.Indented));

            return filterContent;
        }
        public async Task<List<InfoForConvertModel>> GetInfoForConvert(string fromAsset)
        {
            _logger.LogTrace("Request to GetInfoForConvert - {@Request}", new { fromAsset });

            var content = await JsonFileManager.GetFromJsonAsync<List<InfoForConvertModel>>(_appConfig.PathConfiguration.InfoForConvert);

            if (content == null)
            {
                throw new Exception($"Error while fetching GetInfoForConvert - {_appConfig.PathConfiguration.InfoForConvert}");
            }

            var infoForConvertAsset = content.Where(a => a.FromAsset == fromAsset).ToList();

            _logger.LogTrace("Detailed response received for GetInfoForConvert - {@Response}", JsonConvert.SerializeObject(infoForConvertAsset, Formatting.Indented));

            return infoForConvertAsset;
        }

        public async Task<InfoForExchangeModel> GetInfoForExchange(string fromAsset, string toAsset)
        {
            _logger.LogTrace("Request to GetInfoForExchange - {@Request}", new { fromAsset, toAsset });

            var exchangeList = await GetInfoForExchange(fromAsset);

            var filterContent = exchangeList.Where(x => x.FromAsset == fromAsset && x.ToAsset == toAsset).FirstOrDefault();
            if (filterContent == null)
            {
                throw new Exception($"Error while fetching GetInfoForExchange with parameters - fromAsset: {fromAsset}, toAsset: {toAsset}");
            }

            _logger.LogTrace("Detailed response received for GetInfoForExchange - {@Response}", JsonConvert.SerializeObject(filterContent, Formatting.Indented));

            return filterContent;
        }
        public async Task<List<InfoForExchangeModel>> GetInfoForExchange(string fromAsset)
        {
            _logger.LogTrace("Request to GetInfoForExchange - {@Request}", new { fromAsset });

            var convertInfo = await GetInfoForConvert(fromAsset);

            var exchangeList = convertInfo.Select(coinInfo => new InfoForExchangeModel
            {
                Method = "Binance.Convert",
                FromAsset = coinInfo.FromAsset,
                ToAsset = coinInfo.ToAsset,
                MaxAmount = coinInfo.MaxAmount,
                MinAmount = coinInfo.MinAmount,
                ExchangeInternalFee = coinInfo.ConvertInternalFee
            }).ToList();

            _logger.LogTrace("Detailed response received for GetInfoForExchange - {@Response}", JsonConvert.SerializeObject(exchangeList, Formatting.Indented));

            return exchangeList;
        }
        
        public async Task<decimal> GetCoinPriceUSDT(string Asset)
        {
            _logger.LogTrace("Request to GetCoinPriceUSDT - {@Request}", new { Asset });

            var content = await JsonFileManager.GetFromJsonAsync<Dictionary<string, decimal>>(_appConfig.PathConfiguration.InfoForPriceCoin);

            if (content == null)
            {
                throw new Exception($"Error while fetching GetCoinPriceUSDT - {_appConfig.PathConfiguration.InfoForPriceCoin}");
            }

            var assetPrice = content.SingleOrDefault(a => a.Key.StartsWith(Asset)).Value;

            _logger.LogTrace("Detailed response received for GetCoinPriceUSDT - {@Response}", JsonConvert.SerializeObject(assetPrice, Formatting.Indented));

            return assetPrice;
        }

        public async Task<InfoForCheckModel> GetInfoForCheck(string Asset)
        {
            _logger.LogTrace("Request to GetInfoForCheck - {@Request}", new { Asset });

            var content = await JsonFileManager.GetFromJsonAsync<List<InfoForCheckModel>>(_appConfig.PathConfiguration.InfoForCheck);

            if (content == null)
            {
                throw new Exception($"Error while fetching GetInfoForCheck - {_appConfig.PathConfiguration.InfoForCheck}");
            }

            var filterContent = content.SingleOrDefault(a => a.Asset == Asset);
            if (filterContent == null)
            {
                throw new Exception($"Error while fetching GetInfoForCheck with parameters - Asset: {Asset}");
            }

            _logger.LogTrace("Detailed response received for GetInfoForCheck - {@Response}", JsonConvert.SerializeObject(filterContent, Formatting.Indented));

            return filterContent;
        }
        public async Task<InfoForBlavedPayIDModel> GetInfoForBlavedPayID(string asset)
        {
            _logger.LogTrace("Request to GetInfoForBlavedPayID - {@Request}", new { asset });

            var content = await JsonFileManager.GetFromJsonAsync<List<InfoForBlavedPayIDModel>>(_appConfig.PathConfiguration.InfoForBlavedPayID);

            if (content == null)
            {
                throw new Exception($"Error while fetching GetInfoForBlavedPayID - {_appConfig.PathConfiguration.InfoForBlavedPayID}");
            }

            var filterContent = content.SingleOrDefault(a => a.Asset == asset);
            if (filterContent == null)
            {
                throw new Exception($"Error while fetching GetInfoForBlavedPayID with parameters - Asset: {asset}");
            }

            _logger.LogTrace("Detailed response received for GetInfoForBlavedPayID - {@Response}", JsonConvert.SerializeObject(filterContent, Formatting.Indented));

            return filterContent;
        }
    }
}
