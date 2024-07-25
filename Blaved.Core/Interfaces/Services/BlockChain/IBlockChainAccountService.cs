using Nethereum.Web3.Accounts;
using Nethereum.Web3;

namespace Blaved.Core.Interfaces.Services.BlockChain
{
    public interface IBlockChainAccountService
    {
        Account GetNewChainAccount(int allUsersCount, string network, string password = "");
        Web3 GetWeb3HotAccount(string network, string password = "");
        Web3 GetWeb3UserAccount(string network, string privateKey);
    }
}
