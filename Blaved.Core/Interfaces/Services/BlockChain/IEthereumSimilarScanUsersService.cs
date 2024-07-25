using Blaved.Core.Objects.Models;
using Blaved.Core.Objects.Models.Cryptocurrency;

namespace Blaved.Core.Interfaces.Services.BlockChain
{
    public interface IEthereumSimilarScanUsersService
    {
        public Task<List<TransactionDTO>> ScanUsersDeposit(CryptoAssetModel asset, bool isToken);
    }
}
