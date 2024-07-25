using Blaved.Core.Objects.Models;
using Blaved.Core.Objects.Models.Cryptocurrency;

namespace Blaved.Core.Interfaces.Services.BlockChain
{
    public interface IBlockChainScanner
    {
        public Task<List<TransactionDTO>> ScanUserDeposit(CryptoAssetModel asset);
    }
}
