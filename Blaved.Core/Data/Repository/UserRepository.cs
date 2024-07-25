using Blaved.Core.Data.DataBase;
using Blaved.Core.Interfaces.Repository;
using Blaved.Core.Objects.Models;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Blaved.Core.Data.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly MyDbContext _dbContext;
        public UserRepository(MyDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<UserModel?> GetUser(long UserId)
        {
            return await _dbContext.Users
                .Include(x => x.BalanceModel)
                .Include(x => x.BonusBalanceModel)
                .Include(x => x.BlockChainWalletModel)
                .Include(x => x.MessagesBlavedPayIDModel)
                .Include(x => x.MessagesCheckModel)
                .Include(x => x.MessagesExchangeModel)
                .Include(x => x.MessagesWithdrawModel)
                .SingleOrDefaultAsync(u => u.UserId == UserId);
        }
        public async Task<UserModel?> GetUser(string address, string network)
        {
            return await _dbContext.Users
                .Include(x=> x.BalanceModel)
                .Include(x=> x.BonusBalanceModel)
                .Include(x => x.BlockChainWalletModel)
                .Include(x => x.MessagesBlavedPayIDModel)
                .Include(x => x.MessagesCheckModel)
                .Include(x => x.MessagesExchangeModel)
                .Include(x => x.MessagesWithdrawModel)
                .Where(u => EF.Property<string>(u.BlockChainWalletModel, BlockChainWalletModel.GetAddressPropertyName(network)) == address)
                .SingleOrDefaultAsync();
        }
        public async Task AddUser(UserModel user)
        {
            await _dbContext.Users.AddAsync(user);
        }
        public async Task<List<UserModel>> GetUserList()
        {
            return await _dbContext.Users
                .Include(x => x.BalanceModel)
                .Include(x => x.BonusBalanceModel)
                .Include(x => x.BlockChainWalletModel)
                .Include(x => x.MessagesBlavedPayIDModel)
                .Include(x => x.MessagesCheckModel)
                .Include(x => x.MessagesExchangeModel)
                .Include(x => x.MessagesWithdrawModel)
                .ToListAsync();
        }
        public async Task<int> GetUsersCount()
        {
            return await _dbContext.Users.CountAsync();
        }
        public async Task<int> GetUserReferralsCount(long userId)
        {
            return await _dbContext.Users.CountAsync(x => x.WhoseReferral == userId);
        }
        public async Task<decimal> GetUserChargeAmountToReferral(long userId, string asset)
        {
            var users = await _dbContext.Users.Where(x => x.WhoseReferral == userId).ToListAsync();
            if(users.Count <= 0) { return 0; }
            var totalChargeToReferral = await _dbContext.Exchanges
                .Where(x => users.Select(u => u.UserId).Contains(x.UserId))
                .Where(x => x.FromAsset == asset)
                .Select(x => x.ChargeToReferral)
                .SumAsync();

            return totalChargeToReferral;
        }
        public async Task UpdateUserLanguage(long UserId, string language)
        {
            var user = await _dbContext.Users.SingleOrDefaultAsync(u => u.UserId == UserId);
            if (user == null)
            {
                throw new ArgumentNullException(paramName: "User", message: "User is null");
            }
            user.Language = language;
        }
        public async Task UpdateUserMessageId(long UserId, int messageId)
        {
            var user = await _dbContext.Users.SingleOrDefaultAsync(u => u.UserId == UserId);
            if (user == null)
            {
                throw new ArgumentNullException(paramName: "User", message: "User is null");
            }
            user.MessageId = messageId;
        }
        public async Task UpdateUserWhereMenu(long UserId, string whereMenu)
        {
            var user = await _dbContext.Users.SingleOrDefaultAsync(u => u.UserId == UserId);
            if (user == null)
            {
                throw new ArgumentNullException(paramName: "User", message: "User is null");
            }
            user.WhereMenu = whereMenu;
        }
        public async Task UpdateUserTermsOfUse(long userId, bool acceptedTermsOfUse)
        {
            var user = await _dbContext.Users.SingleOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
            {
                throw new ArgumentNullException(paramName: "User", message: "User is null");
            }
            user.AcceptedTermsOfUse = acceptedTermsOfUse;
        }
        public async Task UpdateEnabledNotificationsBlavedPay(long userId, bool status)
        {
            var user = await _dbContext.Users.SingleOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
            {
                throw new ArgumentNullException(paramName: "User", message: "User is null");
            }
            user.EnabledNotificationsBlavedPay = status;
        }

    }
    public class BalanceRepository : IBalanceRepository
    {
        private readonly MyDbContext _dbContext;
        public BalanceRepository(MyDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<BalanceModel?> GetBalance(long userId)
        {
            return await _dbContext.Balances.SingleOrDefaultAsync(u => u.UserId == userId);
        }
        public async Task AddToBalance(long userId, decimal amount, string Asset)
        {
            var balance = await _dbContext.Balances.SingleOrDefaultAsync(u => u.UserId == userId);

            if (balance == null)
            {
                throw new ArgumentNullException(paramName: nameof(userId), message: "User not found");
            }

            PropertyInfo property = balance.GetType().GetProperty(BalanceModel.GetBalancePropertyName(Asset))!;
            decimal currentValue = (decimal)property.GetValue(balance)!;

            decimal newBalance = currentValue + amount;
            property.SetValue(balance, newBalance);
        }
        public async Task SubtractFromBalance(long userId, decimal amount, string Asset)
        {
            var balance = await _dbContext.Balances.SingleOrDefaultAsync(u => u.UserId == userId);

            if (balance == null)
            {
                throw new ArgumentNullException(paramName: nameof(userId), message: "User not found");
            }

            PropertyInfo property = balance.GetType().GetProperty(BalanceModel.GetBalancePropertyName(Asset))!;
            decimal currentValue = (decimal)property.GetValue(balance)!;

            decimal newBalance = currentValue - amount;
            property.SetValue(balance, newBalance);
        }
    }
    public class BonusBalanceRepository : IBonusBalanceRepository
    {
        private readonly MyDbContext _dbContext;
        public BonusBalanceRepository(MyDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<BonusBalanceModel?> GetBonusBalance(long userId)
        {
            return await _dbContext.BonusBalances.SingleOrDefaultAsync(u => u.UserId == userId);
        }
        public async Task AddToBonusBalance(long userId, decimal amount, string Asset)
        {
            var balance = await _dbContext.BonusBalances.SingleOrDefaultAsync(u => u.UserId == userId);

            if (balance == null)
            {
                throw new ArgumentNullException(paramName: nameof(userId), message: "User not found");
            }

            PropertyInfo property = balance.GetType().GetProperty(BonusBalanceModel.GetBalancePropertyName(Asset))!;
            decimal currentValue = (decimal)property.GetValue(balance)!;

            decimal newBalance = currentValue + amount;
            property.SetValue(balance, newBalance);
        }
        public async Task SubtractFromBonusBalance(long userId, decimal amount, string Asset)
        {
            var bonusBalance = await _dbContext.BonusBalances.SingleOrDefaultAsync(u => u.UserId == userId);

            if (bonusBalance == null)
            {
                throw new ArgumentNullException(paramName: nameof(userId), message: "User not found");
            }

            PropertyInfo property = bonusBalance.GetType().GetProperty(BonusBalanceModel.GetBalancePropertyName(Asset))!;
            decimal currentValue = (decimal)property.GetValue(bonusBalance)!;

            decimal newBalance = currentValue - amount;
            property.SetValue(bonusBalance, newBalance);
        }
    }
    public class BlockChainWalletRepository : IBlockChainWalletRepository 
    {
        private readonly MyDbContext _dbContext;
        public BlockChainWalletRepository(MyDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task AddBlockChainWallet(BlockChainWalletModel blockChainWallet)
        {
            await _dbContext.BlockChainWallets.AddAsync(blockChainWallet);
        }
        public async Task<List<string>> GetUsersAddressList(string network)
        {
            var propertyName = BlockChainWalletModel.GetAddressPropertyName(network);
            var property = typeof(BlockChainWalletModel).GetProperty(propertyName);
            if(property == null)
            {
                throw new Exception("Network dont supported");
            }
            return await _dbContext.BlockChainWallets.Select(entity => (string)property.GetValue(entity)!).ToListAsync();
        }
    }
    public class MessagesWithdrawRepository : IMessagesWithdrawRepository
    {
        private readonly MyDbContext _dbContext;
        public MessagesWithdrawRepository(MyDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<MessagesWithdrawModel?> GetMessagesWithdraw(long userId)
        {
            return await _dbContext.MessagesWithdraws.SingleOrDefaultAsync(u => u.UserId == userId);
        }
        public async Task AddMessagesWithdraw(MessagesWithdrawModel messagesWithdraw)
        {
            await _dbContext.MessagesWithdraws.AddAsync(messagesWithdraw);
        }
        public async Task UpdateMessagesWithdrawAmount(long userId, decimal amount)
        {
            var messageWithdraw = await _dbContext.MessagesWithdraws.SingleOrDefaultAsync(u => u.UserId == userId);
            if (messageWithdraw == null)
            {
                throw new ArgumentNullException(paramName: "MessagesWithdraw", message: "MessagesWithdraw is null");
            }
            messageWithdraw.Amount = amount;
        }
        public async Task UpdateMessagesWithdrawAddress(long userId, string address)
        {
            var messagesWithdraw = await _dbContext.MessagesWithdraws.SingleOrDefaultAsync(u => u.UserId == userId);
            if (messagesWithdraw == null)
            {
                throw new ArgumentNullException(paramName: "MessagesWithdraw", message: "MessagesWithdraw is null");
            }
            messagesWithdraw.Address = address;
        }
        public async Task UpdateMessagesWithdrawAsset(long userId, string Asset)
        {
            var messagesWithdraw = await _dbContext.MessagesWithdraws.SingleOrDefaultAsync(u => u.UserId == userId);
            if (messagesWithdraw == null)
            {
                throw new ArgumentNullException(paramName: "MessagesWithdraw", message: "MessagesWithdraw is null");
            }
            messagesWithdraw.Asset = Asset;
        }
        public async Task UpdateMessagesWithdrawNetwork(long userId, string network)
        {
            var messagesWithdraw = await _dbContext.MessagesWithdraws.SingleOrDefaultAsync(u => u.UserId == userId);
            if (messagesWithdraw == null)
            {
                throw new ArgumentNullException(paramName: "MessagesWithdraw", message: "MessagesWithdraw is null");
            }
            messagesWithdraw.Network = network;
        }
    }
    public class MessagesBlavedPayIDRepository : IMessagesBlavedPayIDRepository
    {
        private readonly MyDbContext _dbContext;
        public MessagesBlavedPayIDRepository(MyDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<MessagesBlavedPayIDModel?> GetMessagesBlavedPayID(long userId)
        {
            return await _dbContext.MessagesBlavedPayIDs.SingleOrDefaultAsync(u => u.UserId == userId);
        }
        public async Task AddMessagesBlavedPayID(MessagesBlavedPayIDModel messagesBlavedPayID)
        {
            await _dbContext.MessagesBlavedPayIDs.AddAsync(messagesBlavedPayID);

        }
        public async Task UpdateMessagesBlavedPayIDAmount(long userId, decimal amount)
        {
            var messagesBlavedPayID = await _dbContext.MessagesBlavedPayIDs.SingleOrDefaultAsync(u => u.UserId == userId);
            if (messagesBlavedPayID == null)
            {
                throw new ArgumentNullException(paramName: "MessagesBlavedPayID", message: "MessagesBlavedPayID is null");
            }
            messagesBlavedPayID.Amount = amount;
        }
        public async Task UpdateMessagesBlavedPayIDID(long userId, long id)
        {
            var messagesBlavedPay = await _dbContext.MessagesBlavedPayIDs.SingleOrDefaultAsync(u => u.UserId == userId);
            if (messagesBlavedPay == null)
            {
                throw new ArgumentNullException(paramName: "MessagesBlavedPayID", message: "MessagesBlavedPayID is null");
            }
            messagesBlavedPay.ToUserId = id;
        }
        public async Task UpdateMessagesBlavedPayIDAsset(long userId, string Asset)
        {
            var messagesBlavedPay = await _dbContext.MessagesBlavedPayIDs.SingleOrDefaultAsync(u => u.UserId == userId);
            if (messagesBlavedPay == null)
            {
                throw new ArgumentNullException(paramName: "MessagesBlavedPayID", message: "MessagesBlavedPayID is null");
            }
            messagesBlavedPay.Asset = Asset;

        }
    }
    public class MessagesCheckRepository : IMessagesCheckRepository
    {
        private readonly MyDbContext _dbContext;
        public MessagesCheckRepository(MyDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<MessagesCheckModel?> GetMessagesCheck(long userId)
        {
            return await _dbContext.MessagesChecks.SingleOrDefaultAsync(u => u.UserId == userId);
        }
        public async Task AddMessagesCheck(MessagesCheckModel messagesCheck)
        {
            await _dbContext.MessagesChecks.AddAsync(messagesCheck);
        }
        public async Task UpdateMessagesCheckAmount(long userId, decimal amount)
        {
            var messagesCheck = await _dbContext.MessagesChecks.SingleOrDefaultAsync(u => u.UserId == userId);
            if (messagesCheck == null)
            {
                throw new ArgumentNullException(paramName: "MessagesCheck", message: "MessagesCheck is null");
            }
            messagesCheck.Amount = amount;
        }
        public async Task UpdateMessagesCheckCount(long userId, int count)
        {
            var messagesCheck = await _dbContext.MessagesChecks.SingleOrDefaultAsync(u => u.UserId == userId);
            if (messagesCheck == null)
            {
                throw new ArgumentNullException(paramName: "MessagesCheck", message: "MessagesCheck is null");
            }
            messagesCheck.Count = count;
        }
        public async Task UpdateMessagesCheckAsset(long userId, string Asset)
        {
            var messagesCheck = await _dbContext.MessagesChecks.SingleOrDefaultAsync(u => u.UserId == userId);
            if (messagesCheck == null)
            {
                throw new ArgumentNullException(paramName: "MessagesCheck", message: "MessagesCheck is null");
            }
            messagesCheck.Asset = Asset;
        }
    }
    public class MessagesExchangeRepository : IMessagesExchangeRepository
    {
        private readonly MyDbContext _dbContext;
        public MessagesExchangeRepository(MyDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<MessagesExchangeModel?> GetMessageExchange(long userId)
        {
            return await _dbContext.MessagesExchanges.SingleOrDefaultAsync(u => u.UserId == userId);
        }
        public async Task AddMessagesExchange(MessagesExchangeModel messagesExchange)
        {
            await _dbContext.MessagesExchanges.AddAsync(messagesExchange);
        }
        public async Task UpdateMessagesExchangeAmount(long userId, decimal amount)
        {
            var messagesExchange = await _dbContext.MessagesExchanges.SingleOrDefaultAsync(u => u.UserId == userId);
            if (messagesExchange == null)
            {
                throw new ArgumentNullException(paramName: "MessagesExchange", message: "MessagesExchange is null");
            }
            messagesExchange.Amount = amount;
        }
        public async Task UpdateMessagesExchangeToAsset(long userId, string toAsset)
        {
            var messagesExchange = await _dbContext.MessagesExchanges.SingleOrDefaultAsync(u => u.UserId == userId);
            if (messagesExchange == null)
            {
                throw new ArgumentNullException(paramName: "MessagesExchange", message: "MessagesExchange is null");
            }
            messagesExchange.ToAsset = toAsset;
        }
        public async Task UpdateMessagesExchangeFromAsset(long userId, string fromAsset)
        {
            var messagesExchange = await _dbContext.MessagesExchanges.SingleOrDefaultAsync(u => u.UserId == userId);
            if (messagesExchange == null)
            {
                throw new ArgumentNullException(paramName: "MessagesExchange", message: "MessagesExchange is null");
            }
            messagesExchange.FromAsset = fromAsset;
        }
    }
}
