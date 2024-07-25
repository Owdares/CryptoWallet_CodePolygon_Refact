using Blaved.Core.Objects.Models;
using Blaved.Core.Objects.Models.Cryptocurrency;
using Nethereum.Contracts.Standards.ERC20.ContractDefinition;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;
using System.Numerics;

namespace Blaved.Core.Services.BlockChain.Scanners.EthereumSimilar
{
    public class EthereumSimilarBaseScanner
    {
        protected async Task<List<TransactionDTO>> ScanTransaction(Web3 web3, HashSet<string> addresses, HashSet<string> transfersHash,
            BigInteger startBlock, BigInteger lastBlock, bool isToken, CryptoAssetModel asset)
        {
            var transactions = isToken 
                ? await ScanTokenTransaction(web3, addresses, transfersHash, startBlock, lastBlock, asset.SelectNetwork!.AssetContract!, asset.SelectNetwork.AssetDecimals, asset.SelectNetwork.CryptoDepositConfig.DepositMin)
                : await ScanMainTransaction(web3, addresses, transfersHash, startBlock, lastBlock, asset.SelectNetwork!.AssetDecimals, asset.SelectNetwork.CryptoDepositConfig.DepositMin);

            return transactions;
        }
        protected async Task<List<TransactionDTO>> ScanTokenTransaction(Web3 web3, HashSet<string> addresses, HashSet<string> transfersHash,
            BigInteger startBlock, BigInteger lastBlock, string contract, int coinDecimal, decimal minAmount)
        {
            try
            {
                var transferEventHandler = web3.Eth.GetEvent<TransferEventDTO>(contract);

                var filter = transferEventHandler.CreateFilterInput(
                    fromBlock: new BlockParameter(new HexBigInteger(startBlock)),
                    toBlock: new BlockParameter(new HexBigInteger(lastBlock)));

                var transactions = await transferEventHandler.GetAllChangesAsync(filter);

                return transactions
                    .Where(x =>
                    !transfersHash.Contains(x.Log.TransactionHash)
                    && addresses.Contains(x.Event.To)
                    && Web3.Convert.FromWei(x.Event.Value, coinDecimal) >= minAmount)
                    .Select(x => new TransactionDTO()
                    {
                        isToken = true,
                        From = x.Event.From,
                        To = x.Event.To,
                        Value = Web3.Convert.FromWei(x.Event.Value, coinDecimal),
                        TransactionHash = x.Log.TransactionHash

                    }).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while scanning the token - ", ex);
            }
        }
        protected async Task<List<TransactionDTO>> ScanMainTransaction(Web3 web3, HashSet<string> addresses, HashSet<string> transfersHash,
            BigInteger startBlock, BigInteger lastBlock, int coinDecimal, decimal minAmount)
        {
            try
            {
                var blocks = await web3.Eth.Blocks.GetBlockWithTransactionsByNumber.SendBatchRequestAsync();

                var transactions = new List<Transaction>();
                for (BigInteger i = startBlock; i <= lastBlock; i++)
                {
                    var block = await web3.Eth.Blocks.GetBlockWithTransactionsByNumber.SendRequestAsync(new HexBigInteger(i));
                    if (block != null)
                    {
                        transactions.AddRange(block.Transactions
                            .Where(x =>
                            !transfersHash.Contains(x.TransactionHash)
                            && addresses.Select(a => a.ToLower()).Contains(x.To)
                            && Web3.Convert.FromWei(x.Value, coinDecimal) >= minAmount));
                    }
                }
                return transactions.Select(x => new TransactionDTO()
                {
                    isToken = false,
                    From = x.From,
                    To = x.To,
                    Value = Web3.Convert.FromWei(x.Value, coinDecimal),
                    TransactionHash = x.TransactionHash

                }).ToList();
            }
            catch (Exception ex)
            { 
                throw new Exception("An error occurred while scanning the main coin", ex); 
            }
        }
    }
}
