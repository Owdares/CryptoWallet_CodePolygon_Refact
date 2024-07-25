using Blaved.Core.Interfaces.Services.BlockChain;
using Blaved.Core.Services.BlockChain.Scanners.EthereumSimilar;

namespace Blaved.Core.Services.BlockChain.Scanners
{
    public class BlockChainScannersFacade : IBlockChainScannerFacade
    {
        public Dictionary<string, IBlockChainScanner> Scanners { get; }

        public BlockChainScannersFacade(BscScanner bscScanner, EthScanner ethScanner, MaticScanner maticScanner)
        {
            Scanners = new Dictionary<string, IBlockChainScanner>
            {
                ["BSC"] = bscScanner,
                ["ETH"] = ethScanner,
                ["MATIC"] = maticScanner
            };
        }

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
