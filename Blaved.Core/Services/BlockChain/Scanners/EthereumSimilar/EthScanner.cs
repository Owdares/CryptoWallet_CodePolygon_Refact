using Blaved.Core.Interfaces.Services.BlockChain;

namespace Blaved.Core.Services.BlockChain.Scanners.EthereumSimilar
{
    public class EthScanner : EthereumSimilarScanner
    {
        private const string MainAsset = "ETH";
        public EthScanner(IEthereumSimilarScanUsersService ethereumSimilarUsersScanService) 
            : base(MainAsset, ethereumSimilarUsersScanService)
        {
        }
    }
}
