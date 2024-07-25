using Binance.Net.Objects.Models.Spot;
using Binance.Net.Objects.Models.Spot.Convert;
using Blaved.Core.Objects.Models;
using Blaved.Core.Objects.Models.Cryptocurrency;
using Blaved.Core.Objects.Models.Info;

namespace Blaved.Core.Interfaces.Services.Binance
{
    public interface IBinanceService
    {
        public Task<Result<List<InfoForConvertModel>>> GetCoinConvertInfo();
        public Task<Result<BinanceConvertQuote>> ConvertQuote(string fromAsset, string toAsset, decimal amount);
        public Task<Result<BinanceConvertResult>> ConvertQuoteAccept(string quoteId);
        public Task<Result<BinanceConvertOrderStatus>> GetConvertOrderStatus(string orderId);

        public Task<Result<string>> SendCoin(string Asset, string network, string address, decimal amount);
        public Task<Result<List<BinanceWithdrawal>>> GetWithdrawHistory();
        public Task<Result<Dictionary<string, decimal>>> GetCoinPriceUSD();
        public Task<Result<List<CryptoAssetModel>>> GetAssetInfo();
    }
}
