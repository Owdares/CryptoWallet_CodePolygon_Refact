using Blaved.Core.Objects.Models;

namespace Blaved.Core.Interfaces.Services.BlockChain
{
    public interface IEthereumSimilarHotTransferService
    {
        public Task<HotTransferModel> TransferToHot(UserModel userModel,string network, string Asset, bool isToken);
    }
}
