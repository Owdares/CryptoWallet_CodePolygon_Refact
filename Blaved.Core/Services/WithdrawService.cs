using Binance.Net.Enums;
using Blaved.Core.Interfaces;
using Blaved.Core.Interfaces.Services;
using Blaved.Core.Interfaces.Services.Binance;
using Blaved.Core.Objects.Models;
using Blaved.Core.Objects.Models.Configurations;
using Blaved.Core.Objects.Models.Cryptocurrency;
using Blaved.Core.Utility;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Blaved.Core.Services
{
    public class WithdrawService : IWithdrawService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBinanceService _binanceService;
        private readonly ILogger<WalletService> _logger;
        private readonly IInfoService _infoService;
        private readonly AppConfig _appConfig;

        public WithdrawService(IUnitOfWork unitOfWork, IBinanceService binanceService, ILogger<WalletService> logger,
            IInfoService infoService, IOptions<AppConfig> appConfig)
        {
            _unitOfWork = unitOfWork;
            _binanceService = binanceService;
            _logger = logger;
            _infoService = infoService;
            _appConfig = appConfig.Value;
        }
        public async Task BinanceWithdraw(UserModel user, CryptoAssetModel assetModel)
        {
            decimal amount = user.MessagesWithdrawModel.Amount;
            string asset = user.MessagesWithdrawModel.Asset;
            string network = user.MessagesWithdrawModel.Network;
            string address = user.MessagesWithdrawModel.Address;

            decimal feeInternal = assetModel.SelectNetwork.CryptoWithdrawConfig.WithdrawInternalFee;
            decimal feeBinance = assetModel.SelectNetwork.CryptoWithdrawConfig.WithdrawFee;
            decimal combineFee = assetModel.SelectNetwork.CryptoWithdrawConfig.WithdrawCombineFee;

            decimal amountForSend = (amount + feeBinance).AmountRound();
            decimal amountForSubtract = (amount + combineFee).AmountRound();

            var orderId = await _binanceService.SendCoin(asset, network, address, amountForSend);
            if (!orderId.Status || orderId.Data == null)
            {
                throw new Exception("Sending coins from Binance failed - Status = false || Data = null");
            }
            var orderModel = new WithdrawOrderModel
            {
                UserId = user.UserId,
                IdOrder = orderId.Data,
                AddressTo = address,
                Amount = amount,
                Asset = asset,
                ChargeToCapital = feeInternal,
                Fee = combineFee,
                Network = network,
                Status = WithdrawalStatus.Processing,
            };
            await _unitOfWork.BalanceRepository.SubtractFromBalance(user.UserId, amountForSubtract, asset);
            await _unitOfWork.WithdrawOrderRepository.AddWithdrawOrder(orderModel);
            await _unitOfWork.SaveChanges();

            _logger.LogDebug("Coin sent {@Order}", JsonConvert.SerializeObject(orderModel, Formatting.Indented));
        }
        public async Task<List<WithdrawModel>> BinanceWithdrawValidateOrder()
        {
            var withdrawHistory = await _binanceService.GetWithdrawHistory();
            var withdrawOrders = await _unitOfWork.WithdrawOrderRepository.GetWithdrawOrderList();
            if (!withdrawHistory.Status || withdrawHistory.Data == null)
            {
                throw new Exception("Retrieving output history is not successful");
            }
            var withdrawOrdersFilt = withdrawOrders.Where(x => x.Status != WithdrawalStatus.Completed);
            var withdrawHistoryFilt = withdrawHistory.Data.Where(x => x.Status == WithdrawalStatus.Completed);

            var verifyTransactions = new List<WithdrawModel>();

            foreach (var order in withdrawOrdersFilt)
            {
                var withdraw = withdrawHistoryFilt.SingleOrDefault(x => x.Id == order.IdOrder);
                if (withdraw is not null)
                {
                    var withdrawModel = new WithdrawModel()
                    {
                        AddressFrom = "Default.Binance",
                        AddressTo = order.AddressTo,
                        Network = order.Network,
                        Asset = order.Asset,
                        Fee = order.Fee,
                        UserId = order.UserId,
                        Status = withdraw.Status,
                        Amount = order.Amount,
                        Hash = withdraw.TransactionId,
                        OrderId = order.IdOrder,
                        ChargeToCapital = order.ChargeToCapital

                    };
                    verifyTransactions.Add(withdrawModel);
                }
            }

            return verifyTransactions;
        }
    }
}
