using Microsoft.Extensions.Logging;
using Nethereum.Contracts.ContractHandlers;
using Nethereum.Contracts.Standards.ERC20.ContractDefinition;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Util;
using Nethereum.Web3;
using Newtonsoft.Json;
using System.Numerics;

namespace Blaved.Core.Services.BlockChain.Transfers.EthereumSimilar
{
    public class EthereumSimilarBaseTransfer
    {
        public readonly ILogger _logger;
        public EthereumSimilarBaseTransfer(ILogger logger)
        {
            _logger = logger;
        }
        public async Task<TransactionReceipt> SendMainCoin(Web3 web3, string toAddress, decimal amountInEther, BigInteger maxPriorityFeePerGas, BigInteger maxFeePerGas, int gasUse = 21000)
        {
            try
            {
                int maxAttempts = 1000;
                int currentAttempt = 0;
                bool transactionSuccessful = false;
                TransactionReceipt? transactionReceipt = null;

                var transfer = await web3.Eth.GetEtherTransferService().TransferEtherAsync(toAddress, amountInEther, maxPriorityFee: maxPriorityFeePerGas, maxFeePerGas: maxFeePerGas, gasUse);

                while (currentAttempt < maxAttempts)
                {
                    currentAttempt++;
                    transactionReceipt = await web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(transfer);

                    if (transactionReceipt != null && transactionReceipt.Succeeded())
                    {
                        _logger.LogDebug("Completed sending main coin - {0}", JsonConvert.SerializeObject(transactionReceipt, Formatting.Indented));
                        transactionSuccessful = true;
                        break;
                    }

                    await Task.Delay(1000);
                }

                if (!transactionSuccessful)
                {
                    throw new Exception($"Sending main coin failed or timed out");
                }

                return transactionReceipt ?? throw new Exception($"Unknown error when sending main coin");
            }
            catch(Exception ex)
            {
                throw new Exception("An error occurred while sending the main coin - ", ex);
            }
        }
        public async Task<TransactionReceipt> SendToken(Web3 web3, IContractTransactionHandler<TransferFunction> transferHandler,
            TransferFunction transferFunction, string contract)
        {
            try
            {
                int maxAttempts = 1000;
                int currentAttempt = 0;
                bool transactionSuccessful = false;
                TransactionReceipt? transactionReceipt = null;

                var transfer = await transferHandler.SendRequestAsync(contract, transferFunction);

                while (currentAttempt < maxAttempts)
                {
                    currentAttempt++;
                    transactionReceipt = await web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(transfer);

                    if (transactionReceipt != null && transactionReceipt.Succeeded())
                    {
                        _logger.LogDebug("Completed sending token - {0}", JsonConvert.SerializeObject(transactionReceipt, Formatting.Indented));
                        transactionSuccessful = true;
                        break;
                    }

                    await Task.Delay(1000);
                }

                if (!transactionSuccessful)
                {
                    throw new Exception($"Sending token failed or timed out");
                }

                return transactionReceipt ?? throw new Exception($"Unknown error when sending token");
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while sending the token - ", ex);
            }

        }
        protected async Task<TransferFunction> GetTokenTransferFunctionAsync(IContractTransactionHandler<TransferFunction> transferHandler, 
            string toAddress, decimal amountInEther, string contract, int coinDecimal, BigInteger maxPriorityFeePerGas, BigInteger maxFeePerGas)
        {
            var amountInWei = Web3.Convert.ToWei(amountInEther, coinDecimal);

            TransferFunction transfer = new TransferFunction()
            {
                To = toAddress,
                Value = amountInWei,
                MaxFeePerGas = maxFeePerGas,
                MaxPriorityFeePerGas = maxPriorityFeePerGas,
            };

            var estimate = await transferHandler.EstimateGasAsync(contract, transfer);
            transfer.Gas = estimate;

            return transfer;
        }
        protected decimal CalculateFeeForTokenTransfer(TransferFunction transfer)
        {
            var gasPriceGwei = Web3.Convert.FromWei(transfer.MaxFeePerGas!.Value, UnitConversion.EthUnit.Gwei);
            return Web3.Convert.FromWei(new BigInteger(gasPriceGwei) * transfer.Gas!.Value, UnitConversion.EthUnit.Gwei);

        }
    }
}
