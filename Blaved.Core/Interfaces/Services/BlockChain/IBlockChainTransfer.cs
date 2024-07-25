using Blaved.Core.Objects.Models;

namespace Blaved.Core.Interfaces.Services.BlockChain
{
    public interface IBlockChainTransfer
    {
        public Task<HotTransferModel> TransferCoinToHot(UserModel user, string Asset);
    }
}
