using Blaved.Core.Objects.Models;
using Blaved.Core.Objects.Models.Configurations;
using Microsoft.Extensions.Options;
using Nethereum.Contracts.Standards.ERC20.ContractDefinition;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Util;
using Nethereum.Web3;
using System.Numerics;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Serilog.Context;
using Blaved.Core.Utility;
using Blaved.Core.Interfaces.Services.BlockChain;
using Blaved.Core.Interfaces.Services;
using Blaved.Core.Interfaces;

namespace Blaved.Core.Services.BlockChain.Transfers.EthereumSimilar
{
    public class EthereumSimilarHotTransferService : EthereumSimilarBaseTransfer , IEthereumSimilarHotTransferService
    {
        private readonly AppConfig _appConfig;
        private readonly IBlockChainAccountService _blockchainAccountService;
        public readonly IUnitOfWork _unitOfWork;
        public readonly IInfoService _infoService;
        public EthereumSimilarHotTransferService(IOptions<AppConfig> appConfig,
            IBlockChainAccountService blockChainAccountService, ILogger<EthereumSimilarHotTransferService> logger,
            IUnitOfWork unitOfWork, IInfoService infoService) : base(logger)
        {
            _infoService = infoService;
            _unitOfWork = unitOfWork;
            _blockchainAccountService = blockChainAccountService;
            _appConfig = appConfig.Value;
        }
        
        public async Task<HotTransferModel> TransferToHot(UserModel userModel, string network, string Asset, bool isToken)
        {
            LogContext.PushProperty("IsToken", isToken);

            _logger.LogInformation("Started sending to hot wallet");

            var hotAddress = _appConfig.BinanceConfiguration.DepositeAddress[network];

            var userAddress = userModel.BlockChainWalletModel.GetAddress(network);
            var userPrivatKey = userModel.BlockChainWalletModel.GetPrivateKey(network);

            var web3User = _blockchainAccountService.GetWeb3UserAccount(network, userPrivatKey);
            var web3Hot = _blockchainAccountService.GetWeb3HotAccount(network);
            var timePreferenceFeeSuggesionStrategy = web3Hot.FeeSuggestion.GetTimePreferenceFeeSuggestionStrategy();
            web3Hot.TransactionManager.Fee1559SuggestionStrategy = timePreferenceFeeSuggesionStrategy;
            web3User.TransactionManager.Fee1559SuggestionStrategy = timePreferenceFeeSuggesionStrategy;
            var coinDecimal = _appConfig.AssetConfiguration.CoinDecimalByNetwork[network][Asset];

            var fee = await timePreferenceFeeSuggesionStrategy.SuggestFeeAsync();
            BigInteger MaxPriorityFeePerGas;
            BigInteger MaxFeePerGas;
            if (fee.MaxFeePerGas != null && fee.MaxFeePerGas > 0 && fee.MaxPriorityFeePerGas != null && fee.MaxPriorityFeePerGas > 0)
            {
                MaxPriorityFeePerGas = fee.MaxPriorityFeePerGas.Value;
                MaxFeePerGas = fee.MaxFeePerGas.Value;
            }
            else
            {
                var gasPrice = await web3Hot.Eth.GasPrice.SendRequestAsync();
                MaxPriorityFeePerGas = gasPrice.Value;
                MaxFeePerGas = gasPrice.Value;
            }

            var transferToHot = isToken
                ? await TransferTokenToHot(web3User, web3Hot, userModel, network, Asset, userAddress, hotAddress, coinDecimal, MaxPriorityFeePerGas, MaxFeePerGas)
                : await TransferMainCoinToHot(web3User, network, Asset, userAddress, hotAddress, MaxPriorityFeePerGas, MaxFeePerGas);

            _logger.LogInformation("Completed sending to hot wallet");
            _logger.LogDebug("Completed sending to hot wallet Detailed - {@Response}", JsonConvert.SerializeObject(transferToHot, Formatting.Indented));

            return transferToHot;
        }
        private async Task<HotTransferModel> TransferMainCoinToHot(Web3 web3User, string network, string Asset, 
            string userAddress, string hotAddress, BigInteger maxPriorityFeePerGas, BigInteger maxFeePerGas)
        {
            var userBalanceWei = await web3User.Eth.GetBalance.SendRequestAsync(userAddress);
            decimal amountForTransfer = Web3.Convert.FromWei(userBalanceWei.Value) - Web3.Convert.FromWei(new BigInteger(21000) * maxFeePerGas);

            var transaction = await SendMainCoin(web3User, hotAddress, amountForTransfer, maxPriorityFeePerGas, maxFeePerGas);

            return CreateTransferToHotModel(network, Asset, userAddress, hotAddress, amountForTransfer, transaction);
        }
        private async Task<HotTransferModel> TransferTokenToHot(Web3 web3User, Web3 web3Hot, UserModel userModel,string network, string Asset,
            string userAddress, string hotAddress, int coinDecimal, BigInteger maxPriorityFeePerGas, BigInteger maxFeePerGas)
        {
            var contract = _appConfig.AssetConfiguration.CoinContractByNetwork[network][Asset];
            var balanceWei = await web3User.Eth.ERC20.GetContractService(contract)
                                                                .BalanceOfQueryAsync(userAddress);
            decimal balance = Web3.Convert.FromWei(balanceWei, coinDecimal);

            var transferHandler = web3User.Eth.GetContractTransactionHandler<TransferFunction>();
            var transferFunction = await GetTokenTransferFunctionAsync(transferHandler, hotAddress, balance, contract, coinDecimal, maxPriorityFeePerGas, maxFeePerGas);

            var amountForFee = CalculateFeeForTokenTransfer(transferFunction);
            var amountFeeForFeeTransfer = Web3.Convert.FromWei(new BigInteger(21000) * maxFeePerGas);

            var fullFee = amountForFee + amountFeeForFeeTransfer;
            var mainCoinPrice = await _infoService.GetCoinPriceUSDT(_appConfig.AssetConfiguration.MainAssetByNetwork[network]);
            var tokenCoinPrice = await _infoService.GetCoinPriceUSDT(Asset);
            if (CalculationTool.ConvertCoin(fullFee, mainCoinPrice, tokenCoinPrice) >= balance)
            {
                throw new Exception("Transfer to hot fee is over deposit amount");
            }

            var transferForFee = await SendMainCoin(web3Hot, userAddress, amountForFee, maxPriorityFeePerGas, maxFeePerGas);

            await _unitOfWork.DepositRepository.AddDeposite(new DepositModel()
            {
                AddressFrom = transferForFee.From,
                AddressTo = transferForFee.To,
                Amount = amountForFee,
                Asset = Asset,
                Fee = CalculateFeeEther(transferForFee),
                Hash = transferForFee.TransactionHash,
                Network = network,
                UserId = userModel.UserId,
                IsInside = true,
            });
            await _unitOfWork.SaveChanges();

            var transaction = await SendToken(web3User, transferHandler, transferFunction, contract);

            return CreateTransferToHotModel(network, Asset, userAddress, hotAddress, balance, transaction, transferForFee);
        }

        private HotTransferModel CreateTransferToHotModel(string network, string Asset, string addressUser, string hotAddress,
            decimal amount, TransactionReceipt transactionReceipt, TransactionReceipt? transactionReceiptForFee = null)
        {
            decimal feeEther = CalculateFeeEther(transactionReceipt, transactionReceiptForFee);

            var withdrawTransactionToHotModel = new HotTransferModel()
            {
                FromAddress = addressUser,
                ToAddress = hotAddress,
                Network = network,
                Asset = Asset,
                Fee = feeEther,
                Hash = transactionReceipt.TransactionHash,
                Status = (long)transactionReceipt.Status.Value,
                Amount = amount,
            };

            return withdrawTransactionToHotModel;
        }
        private decimal CalculateFeeEther(TransactionReceipt transactionReceipt, TransactionReceipt? transactionReceiptForFee = null)
        {
            var gasPriceGwei = Web3.Convert.FromWei(transactionReceipt.EffectiveGasPrice.Value, UnitConversion.EthUnit.Gwei);
            var feeEther = Web3.Convert.FromWei(new BigInteger(gasPriceGwei) * transactionReceipt.GasUsed.Value, UnitConversion.EthUnit.Gwei);

            if (transactionReceiptForFee != null)
            {
                var gasPriceGweiForFee = Web3.Convert.FromWei(transactionReceiptForFee.EffectiveGasPrice.Value, UnitConversion.EthUnit.Gwei);

                return feeEther += Web3.Convert.FromWei(new BigInteger(gasPriceGweiForFee) * transactionReceiptForFee.GasUsed.Value, UnitConversion.EthUnit.Gwei);
            }
            return feeEther;
        }
    }
}
