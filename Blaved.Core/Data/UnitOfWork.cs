using Blaved.Core.Data.DataBase;
using Blaved.Core.Interfaces.Repository;

namespace Blaved.Core.Interfaces
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly MyDbContext _dbContext;
        public UnitOfWork(MyDbContext myDbContext, IUserRepository userRepository,IDepositRepository depositRepository,
            IMessagesBlavedPayIDRepository messagesBlavedPayIDRepository, IMessagesWithdrawRepository messagesWithdrawRepository, 
            IMessagesExchangeRepository messagesExchangeRepository, IMessagesCheckRepository messagesCheckRepository,
            IWithdrawRepository withdrawRepository, IWithdrawOrderRepository withdrawOrderRepository, ITransferToHotRepository transferToHotRepository,
            ICheckActivatedRepository checkActivatedRepository, IBalanceRepository balanceRepository, IBonusBalanceRepository bonusBalanceRepository,
            IExchangeRepository exchangeRepository, IBlockChainWalletRepository blockChainWalletRepository, IInfoForBlockChainRepository infoForBlockChainRepository,
            IBlavedPayIDRepository blavedPayIDRepository, ICheckRepository checkRepository) 
        {
            UserRepository = userRepository;
            CheckRepository = checkRepository;
            MessagesBlavedPayIDRepository = messagesBlavedPayIDRepository;
            MessagesWithdrawRepository = messagesWithdrawRepository;
            MessagesExchangeRepository = messagesExchangeRepository;
            MessagesCheckRepository = messagesCheckRepository;
            WithdrawRepository = withdrawRepository;
            DepositRepository = depositRepository;
            ExchangeRepository = exchangeRepository;
            CheckActivatedRepository = checkActivatedRepository;
            BalanceRepository = balanceRepository;
            BonusBalanceRepository = bonusBalanceRepository;
            InfoForBlockChainRepository = infoForBlockChainRepository;
            BlockChainWalletRepository = blockChainWalletRepository;
            BlavedPayIDRepository = blavedPayIDRepository;
            TransferToHotRepository = transferToHotRepository;
            WithdrawOrderRepository = withdrawOrderRepository;

            _dbContext = myDbContext;
        }
        public IUserRepository UserRepository { get; }
        public ICheckRepository CheckRepository { get; }
        public IDepositRepository DepositRepository { get; }
        public IMessagesCheckRepository MessagesCheckRepository { get; }
        public IMessagesWithdrawRepository MessagesWithdrawRepository { get; }
        public IWithdrawRepository WithdrawRepository { get; }
        public ITransferToHotRepository TransferToHotRepository { get; }
        public ICheckActivatedRepository CheckActivatedRepository { get; }
        public IMessagesExchangeRepository MessagesExchangeRepository { get; }
        public IWithdrawOrderRepository WithdrawOrderRepository { get; }
        public IMessagesBlavedPayIDRepository MessagesBlavedPayIDRepository { get; }
        public IBlavedPayIDRepository BlavedPayIDRepository { get; }
        public IBalanceRepository BalanceRepository { get; }
        public IBonusBalanceRepository BonusBalanceRepository { get; }
        public IBlockChainWalletRepository BlockChainWalletRepository { get; }
        public IInfoForBlockChainRepository InfoForBlockChainRepository { get; }
        public IExchangeRepository ExchangeRepository { get; }

        public async Task SaveChanges()
        {
            await _dbContext.SaveChangesAsync();
        }
        public async void Dispose()
        {
            _dbContext.Dispose();
        }
    }
}
