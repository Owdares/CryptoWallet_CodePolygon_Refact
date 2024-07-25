using Blaved.Core.Interfaces.Services.BlockChain;
using Blaved.Core.Objects.Models;
using Blaved.Core.Objects.Models.Cryptocurrency;

namespace Blaved.Core.Services.BlockChain.Scanners.EthereumSimilar
{
    public class EthereumSimilarScanner : IBlockChainScanner
    {
        private readonly string MainAsset;
        private readonly IEthereumSimilarScanUsersService _ethereumSimilarScanUsersService;
        public EthereumSimilarScanner(string mainAsset, IEthereumSimilarScanUsersService ethereumSimilarScanUsersService)
        {
            MainAsset = mainAsset;
            _ethereumSimilarScanUsersService = ethereumSimilarScanUsersService;
        }
        public virtual async Task<List<TransactionDTO>> ScanUserDeposit(CryptoAssetModel asset)
        {
            if (asset.Asset == MainAsset)
            {
                return await _ethereumSimilarScanUsersService.ScanUsersDeposit(asset, false);
            }
            return await _ethereumSimilarScanUsersService.ScanUsersDeposit(asset, true);
        }

    }
}
