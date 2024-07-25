using Blaved.Core.Objects.Models;

namespace Blaved.Core.Interfaces.Repository
{
    public interface IUserRepository
    {
        Task<UserModel?> GetUser(long userId);
        Task<UserModel?> GetUser(string address, string network);
        Task AddUser(UserModel user);
        Task<List<UserModel>> GetUserList();
        Task<int> GetUsersCount();
        Task<int> GetUserReferralsCount(long userId);
        Task<decimal> GetUserChargeAmountToReferral(long userId, string asset);
        Task UpdateUserLanguage(long userId, string language);
        Task UpdateUserMessageId(long userId, int messageId);
        Task UpdateUserWhereMenu(long userId, string whereMenu);
        Task UpdateUserTermsOfUse(long userId, bool acceptedUserTermsOfUse);
        Task UpdateEnabledNotificationsBlavedPay(long userId, bool status);
    }
    public interface IBlockChainWalletRepository
    {
        Task AddBlockChainWallet(BlockChainWalletModel blockChainWallet);
        Task<List<string>> GetUsersAddressList(string network);
    }
    public interface IBalanceRepository
    {
        Task<BalanceModel?> GetBalance(long userId);
        Task AddToBalance(long userId, decimal amount, string Asset);
        Task SubtractFromBalance(long userId, decimal amount, string Asset);
    }
    public interface IBonusBalanceRepository
    {
        Task<BonusBalanceModel?> GetBonusBalance(long userId);
        Task AddToBonusBalance(long userId, decimal amount, string Asset);
        Task SubtractFromBonusBalance(long userId, decimal amount, string Asset);
    }
    public interface IMessagesWithdrawRepository
    {
        Task<MessagesWithdrawModel?> GetMessagesWithdraw(long userId);
        Task AddMessagesWithdraw(MessagesWithdrawModel messagesWithdraw);
        Task UpdateMessagesWithdrawAmount(long userId, decimal amount);
        Task UpdateMessagesWithdrawAddress(long userId, string address);
        Task UpdateMessagesWithdrawAsset(long userId, string Asset);
        Task UpdateMessagesWithdrawNetwork(long userId, string network);
    }
    public interface IMessagesBlavedPayIDRepository
    {
        Task<MessagesBlavedPayIDModel?> GetMessagesBlavedPayID(long userId);
        Task AddMessagesBlavedPayID(MessagesBlavedPayIDModel messagesBlavedPayID);
        Task UpdateMessagesBlavedPayIDAmount(long userId, decimal amount);
        Task UpdateMessagesBlavedPayIDID(long userId, long id);
        Task UpdateMessagesBlavedPayIDAsset(long userId, string Asset);
    }
    public interface IMessagesCheckRepository
    {
        Task<MessagesCheckModel?> GetMessagesCheck(long userId);
        Task AddMessagesCheck(MessagesCheckModel messagesCheck);
        Task UpdateMessagesCheckAmount(long userId, decimal amount);
        Task UpdateMessagesCheckCount(long userId, int count);
        Task UpdateMessagesCheckAsset(long userId, string Asset);
    }
    public interface IMessagesExchangeRepository
    {
        Task<MessagesExchangeModel?> GetMessageExchange(long userId);
        Task AddMessagesExchange(MessagesExchangeModel messagesExchange);
        Task UpdateMessagesExchangeAmount(long userId, decimal amount);
        Task UpdateMessagesExchangeToAsset(long userId, string toAsset);
        Task UpdateMessagesExchangeFromAsset(long userId, string fromAsset);

    }
}
