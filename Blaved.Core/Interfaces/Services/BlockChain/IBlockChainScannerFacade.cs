namespace Blaved.Core.Interfaces.Services.BlockChain
{
    public interface IBlockChainScannerFacade
    {
        public Dictionary<string, IBlockChainScanner> Scanners { get; }

        public IBlockChainScanner GetNetworkScanner(string network)
        {
            if (Scanners.TryGetValue(network, out var scanner))
            {
                return scanner;
            }

            throw new NotSupportedException($"Network '{network}' is not supported.");
        }
    }
}
