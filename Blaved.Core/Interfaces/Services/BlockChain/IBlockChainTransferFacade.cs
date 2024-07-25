namespace Blaved.Core.Interfaces.Services.BlockChain
{
    public interface IBlockChainTransferFacade
    {
        public Dictionary<string, IBlockChainTransfer> Transfers {  get; }

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
