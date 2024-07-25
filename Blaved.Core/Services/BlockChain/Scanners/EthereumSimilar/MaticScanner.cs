using Blaved.Core.Interfaces.Services.BlockChain;

namespace Blaved.Core.Services.BlockChain.Scanners.EthereumSimilar
{
    public class MaticScanner : EthereumSimilarScanner
    {
        private const string MainAsset = "MATIC";
        public MaticScanner(IEthereumSimilarScanUsersService ethereumSimilarUsersScanService)
            : base(MainAsset, ethereumSimilarUsersScanService)
        {
        }
    }
}
