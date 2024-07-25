using Blaved.Core.Objects.Models.Configurations;
using Microsoft.Extensions.Options;
using Nethereum.Web3.Accounts;
using Nethereum.Web3;
using Blaved.Core.Interfaces.Services.BlockChain;
using Nethereum.HdWallet;

namespace Blaved.Core.Services.BlockChain
{
    public class BlockChainAccountService : IBlockChainAccountService
    {
        private readonly AppConfig _appConfig;
        public BlockChainAccountService(IOptions<AppConfig> appConfig)
        {
            _appConfig = appConfig.Value;
        }
        public Account GetNewChainAccount(int allUsersCount, string network, string password = "")
        {
            string Words = _appConfig.BlockChainConfiguration.UserMnemonics[network];
            var wallet = new Wallet(Words, password);

            int numberNewAccount = allUsersCount + 1;
            return wallet.GetAccount(numberNewAccount);
            
        }
        public Web3 GetWeb3HotAccount(string network, string password = "")
        {
            string mnemonic = _appConfig.BlockChainConfiguration.HotMnemonics[network];
            var wallet = new Wallet(mnemonic, password);
            var account = wallet.GetAccount(0);

            return new Web3(account, _appConfig.BlockChainConfiguration.NetworkNodesUrl[network]);
        }
        public Web3 GetWeb3UserAccount(string network, string privateKey)
        {
            var account = new Account(privateKey, _appConfig.AssetConfiguration.NetworkId[network]);
            var web3 = new Web3(account, _appConfig.BlockChainConfiguration.NetworkNodesUrl[network]);
            
            return web3;
        }
    }
}
