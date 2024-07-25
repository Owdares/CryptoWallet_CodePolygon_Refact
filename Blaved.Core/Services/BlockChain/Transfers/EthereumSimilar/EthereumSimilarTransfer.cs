using Blaved.Core.Interfaces.Services.BlockChain;
using Blaved.Core.Objects.Models;

namespace Blaved.Core.Services.BlockChain.Transfers.EthereumSimilar
{
    public abstract class EthereumSimilarTransfer : IBlockChainTransfer
    {
        private readonly string Network;
        private readonly string MainAsset;
        private readonly IEthereumSimilarHotTransferService _ethereumSimilarHotService;

        public EthereumSimilarTransfer(string network, string mainAsset, IEthereumSimilarHotTransferService ethereumSimilarHotService)
        {
            Network = network;
            MainAsset = mainAsset;
            _ethereumSimilarHotService = ethereumSimilarHotService;
        }

        public virtual async Task<HotTransferModel> TransferCoinToHot(UserModel user, string Asset)
        {
            if (Asset == MainAsset)
            {
                return await _ethereumSimilarHotService.TransferToHot(user, Network, Asset, false);
            }
            return await _ethereumSimilarHotService.TransferToHot(user, Network, Asset, true);
        }
        
    }
}
