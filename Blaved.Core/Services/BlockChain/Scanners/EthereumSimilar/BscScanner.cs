using Blaved.Core.Interfaces.Services.BlockChain;

namespace Blaved.Core.Services.BlockChain.Scanners.EthereumSimilar
{
    public class BscScanner : EthereumSimilarScanner
    {
        private const string MainAsset = "BNB";
        public BscScanner(IEthereumSimilarScanUsersService ethereumSimilarUsersScanService)
            : base( MainAsset, ethereumSimilarUsersScanService)
        {
        }
    }
}
