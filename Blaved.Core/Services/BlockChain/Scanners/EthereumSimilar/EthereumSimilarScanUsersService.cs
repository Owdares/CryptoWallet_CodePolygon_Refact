using Blaved.Core.Interfaces;
using Blaved.Core.Interfaces.Services;
using Blaved.Core.Interfaces.Services.BlockChain;
using Blaved.Core.Objects.Models;
using Blaved.Core.Objects.Models.Configurations;
using Blaved.Core.Objects.Models.Cryptocurrency;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nethereum.Web3;
using Newtonsoft.Json;
using Serilog.Context;

namespace Blaved.Core.Services.BlockChain.Scanners.EthereumSimilar
{
    public class EthereumSimilarScanUsersService : EthereumSimilarBaseScanner, IEthereumSimilarScanUsersService
    {
        public readonly IUnitOfWork _unitOfWork;
        public readonly IBlockChainTransferFacade _blockChainTransferFacade;
        public readonly IInfoService _infoService;
        public readonly AppConfig _appConfig;
        private readonly ILogger<EthereumSimilarScanUsersService> _logger;
        public EthereumSimilarScanUsersService(IUnitOfWork unitOfWork, IBlockChainTransferFacade blockChainTransferFacade,
            IInfoService infoService, IOptions<AppConfig> appConfig, ILogger<EthereumSimilarScanUsersService> logger)
        {
            _logger = logger;
            _appConfig = appConfig.Value;
            _infoService = infoService;
            _unitOfWork = unitOfWork;
            _blockChainTransferFacade = blockChainTransferFacade;
        }
        public async Task<List<TransactionDTO>> ScanUsersDeposit(CryptoAssetModel asset, bool isToken)
        {
            LogContext.PushProperty("IsToken", isToken);

            _logger.LogInformation("Blockchain scanning started");

            if(asset.SelectNetwork == null)
            {
                throw new Exception("ScanUsersDeposit select network is null");
            }

            var web3 = new Web3(_appConfig.BlockChainConfiguration.NetworkNodesUrl[asset.SelectNetwork.Network]);
            var lastBlock = await web3.Eth.Blocks.GetBlockNumber.SendRequestAsync();
            var infoForBlockChaine = await _unitOfWork.InfoForBlockChainRepository.GetInfoForBlockChaine(asset.Asset, asset.SelectNetwork.Network);

            var depositList = await _unitOfWork.DepositRepository.GetDepositList();
            HashSet<string> depositListHash = depositList.Select(obj => obj.Hash).ToHashSet();

            var addressList = await _unitOfWork.BlockChainWalletRepository.GetUsersAddressList(asset.SelectNetwork.Network);
            HashSet<string> addressListHash = addressList.ToHashSet();

            List<TransactionDTO> transactions = await ScanTransaction(web3, addressListHash, depositListHash, infoForBlockChaine?.LastScanBlock ?? lastBlock.Value, lastBlock.Value, isToken, asset);
            
            await _unitOfWork.InfoForBlockChainRepository.UpdateLastScanBlock(asset.Asset, asset.SelectNetwork.Network, (long)lastBlock.Value);
            await _unitOfWork.SaveChanges();

            _logger.LogInformation("Blockchain scan completed");
            _logger.LogDebug("Blockchain scan completed Detailed - {@Response}", JsonConvert.SerializeObject(transactions, Formatting.Indented));

            return transactions;
        }
    }
}
