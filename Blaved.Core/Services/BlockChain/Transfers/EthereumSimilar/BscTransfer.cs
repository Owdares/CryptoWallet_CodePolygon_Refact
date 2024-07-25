using Blaved.Core.Interfaces.Services.BlockChain;

namespace Blaved.Core.Services.BlockChain.Transfers.EthereumSimilar
{
    public class BscTransfer : EthereumSimilarTransfer
    {
        private const string Network = "BSC";
        private const string MainAsset = "BNB";

        public BscTransfer(IEthereumSimilarHotTransferService ethereumSimilarHotService)
        : base(Network, MainAsset, ethereumSimilarHotService)
        {
        }
    }
}
