using Blaved.Core.Interfaces;
using Blaved.Core.Interfaces.Services.BlockChain;
using Blaved.Core.Interfaces.Services;
using Blaved.Core.Objects.Models;
using Blaved.Core.Objects.Models.Configurations;
using Blaved.Core.Utility;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Serilog.Context;
using Blaved.Core.Objects.Models.Cryptocurrency;

namespace Blaved.Core.Services
{
    public class WalletService : IWalletService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBlockChainScannerFacade _blockChainScannerFacade;
        private readonly IBlockChainTransferFacade _blockChainTransferFacade;
        private readonly ILogger<WalletService> _logger;
        private readonly IInfoService _infoService;
        private readonly AppConfig _appConfig;
        private readonly IWithdrawService _withdrawService;
        private readonly IAlertsService _alertsService;

        public WalletService(IUnitOfWork unitOfWork, ILogger<WalletService> logger,IBlockChainScannerFacade blockChainScannerFacade, IBlockChainTransferFacade blockChainTransferFacade,
            IInfoService infoService, IOptions<AppConfig> appConfig, IWithdrawService withdrawService, IAlertsService alertsService)
        {
            _alertsService = alertsService;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _blockChainScannerFacade = blockChainScannerFacade;
            _blockChainTransferFacade = blockChainTransferFacade;
            _infoService = infoService;
            _appConfig = appConfig.Value;
            _withdrawService = withdrawService;
        }

        #region Bonus Balance
        public async Task<bool> BonusBalanceToBalance(UserModel user)
        {
            int x = 0;
            foreach (var coin in _appConfig.AssetConfiguration.CoinList)
            {
                var coinBonusBalance = user.BonusBalanceModel.GetBalance(coin);
                if (coinBonusBalance > 0)
                {
                    await _unitOfWork.BonusBalanceRepository.SubtractFromBonusBalance(user.UserId, coinBonusBalance, coin);
                    await _unitOfWork.BalanceRepository.AddToBalance(user.UserId, coinBonusBalance, coin);
                    x++;
                }
            }
            if (x > 0)
            {
                await _unitOfWork.SaveChanges();

                _logger.LogInformation($"The bonus balance has been transferred to the main balance");
                return true;
            }
            return false;
        }
        #endregion

        #region Withdraw
        public async Task WithdrawConfirm(UserModel user, CryptoAssetModel assetModel)
        {
            if (assetModel.ProviderName == "Binance")
            {
                await _withdrawService.BinanceWithdraw(user, assetModel);
            }
        }
        public async Task WithdrawValidate()
        {
            var verifyTransactions = await _withdrawService.BinanceWithdrawValidateOrder();

            foreach (var transaction in verifyTransactions)
            {
                await _unitOfWork.WithdrawOrderRepository.UpdateWithdrawOrderStatus(transaction.OrderId, transaction.Status);
                await _unitOfWork.WithdrawRepository.AddWithdraw(transaction);
                await _unitOfWork.SaveChanges();

                _logger.LogInformation("Coin sending confirmation received {0}", JsonConvert.SerializeObject(transaction, Formatting.Indented));

                await _alertsService.WithdrawCompletedAlert(transaction);
            }
        }

        #endregion

        #region Deposit
        private async Task DepositReception(DepositModel deposit)
        {
            var userModel = await _unitOfWork.UserRepository.GetUser(deposit.AddressTo, deposit.Network);

            await _unitOfWork.DepositRepository.AddDeposite(deposit);
            await _unitOfWork.BalanceRepository.AddToBalance(deposit.UserId, deposit.Amount, deposit.Asset);
            await _unitOfWork.SaveChanges();

            _logger.LogInformation("The user has received a deposit - {@Deposit}", new { deposit.Hash });
            _logger.LogDebug("The user has received a deposit - {@Deposit}", JsonConvert.SerializeObject(deposit, Formatting.Indented));

            await _alertsService.DepositCompletedAlert(deposit);
        }
        public async Task DepositScanTransaction(string network)
        {
            LogContext.PushProperty("Network", network);
            var infoForDepositAsset = await _infoService.GetCryptoAssetList(network);

            var scanner = _blockChainScannerFacade.Scanners[network];
            foreach (var asset in infoForDepositAsset)
            {
                try
                {
                    LogContext.PushProperty("Asset", asset.Asset);

                    var depositeModel = await scanner.ScanUserDeposit(asset);

                    await DepositProcessing(depositeModel, asset.Asset, network);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while scanning and/or processing the deposit");
                }
            }
        }

        private async Task DepositProcessing(List<TransactionDTO> transactions, string asset, string network)
        {
            var depositSplit = DepositSplit(transactions);

            foreach (var deposits in depositSplit)
            {
                try
                {
                    _logger.LogInformation("Deposit processing has started - {@Count}", new { deposits.Count });

                    var user = await _unitOfWork.UserRepository.GetUser(deposits.First().To, network);
                    var amount = deposits.Sum(x => x.Value);

                    LogContext.PushProperty("UserId", user!.UserId);
                    LogContext.PushProperty("Amount", amount);

                    var transferToHot = await _blockChainTransferFacade.Transfers[network].TransferCoinToHot(user!, asset);
                    await _unitOfWork.TransferToHotRepository.AddTransferToHot(transferToHot);
                    await _unitOfWork.SaveChanges();

                    var feeForOneDeposit = (transferToHot.Fee / deposits.Count).AmountRound();
                    decimal feeInDepositCoin = deposits.First().isToken ? await DepositFeeInCoin(feeForOneDeposit, asset, network) : feeForOneDeposit;

                    foreach (var deposit in deposits)
                    {
                        var depositModel = DepositCreateModel(user!, deposit, feeInDepositCoin, asset, network);
                        await DepositReception(depositModel);
                    }

                    _logger.LogInformation("Deposit processing completed - {@Count}", new { deposits.Count });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while processing deposits - {@Deposits} ", JsonConvert.SerializeObject(deposits.Select(x => x.TransactionHash)));
                }
            }
        }

        private List<List<TransactionDTO>> DepositSplit(List<TransactionDTO> sourceList)
        {
            Dictionary<string, List<TransactionDTO>> propertyListsDictionary = new Dictionary<string, List<TransactionDTO>>();

            foreach (var item in sourceList)
            {
                string propertyValue = item.To;

                if (!propertyListsDictionary.ContainsKey(propertyValue))
                {
                    propertyListsDictionary[propertyValue] = new List<TransactionDTO>();
                }

                propertyListsDictionary[propertyValue].Add(item);
            }

            return propertyListsDictionary.Values.ToList();
        }

        private async Task<decimal> DepositFeeInCoin(decimal fee, string Asset, string network)
        {
            var mainAssetByNetwork = _appConfig.AssetConfiguration.MainAssetByNetwork[network];
            var mainCoinPriceUSD = await _infoService.GetCoinPriceUSDT(mainAssetByNetwork);
            var depositCoinPriceUSD = await _infoService.GetCoinPriceUSDT(Asset);

            return CalculationTool.ConvertCoin(fee, mainCoinPriceUSD, depositCoinPriceUSD);
        }

        private DepositModel DepositCreateModel(UserModel user, TransactionDTO transaction, decimal fee, string Asset, string network)
        {
            decimal amount = (transaction.Value - fee).AmountRound();

            return new DepositModel()
            {
                AddressFrom = transaction.From,
                AddressTo = transaction.To,
                Amount = amount,
                Asset = Asset,
                Fee = fee,
                Hash = transaction.TransactionHash,
                Network = network,
                UserId = user.UserId,
                IsInside = false

            };
        }
        #endregion
    }
}
