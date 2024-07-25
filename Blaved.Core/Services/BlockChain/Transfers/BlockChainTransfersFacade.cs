using Blaved.Core.Interfaces.Services.BlockChain;
using Blaved.Core.Services.BlockChain.Transfers.EthereumSimilar;


namespace Blaved.Core.Services.BlockChain.Transfers
{
    public class BlockChainTransfersFacade : IBlockChainTransferFacade
    {
        public Dictionary<string, IBlockChainTransfer> Transfers { get; }

        public BlockChainTransfersFacade(BscTransfer bscTransfer, EthTransfer ethTransfer, MaticTransfer maticTransfer)
        {
            Transfers = new Dictionary<string, IBlockChainTransfer>
            {
                ["BSC"] = bscTransfer,
                ["ETH"] = ethTransfer,
                ["MATIC"] = maticTransfer
            };
        }

        public IBlockChainTransfer GetNetworkTransfer(string network)
        {
            if (Transfers.TryGetValue(network, out var transfer))
            {
                return transfer;
            }

            throw new NotSupportedException($"Network '{network}' is not supported.");
        }
    }
}

