using Blaved.Core.Interfaces.Repository;

namespace Blaved.Core.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IUserRepository UserRepository { get; }
        ICheckRepository CheckRepository { get; }
        IDepositRepository DepositRepository { get; }
        IMessagesCheckRepository MessagesCheckRepository { get; }
        IMessagesWithdrawRepository MessagesWithdrawRepository { get; }
        IWithdrawRepository WithdrawRepository { get; }
        ITransferToHotRepository TransferToHotRepository { get; }
        ICheckActivatedRepository CheckActivatedRepository { get; }
        IMessagesExchangeRepository MessagesExchangeRepository { get; }
        IWithdrawOrderRepository WithdrawOrderRepository { get; }
        IMessagesBlavedPayIDRepository MessagesBlavedPayIDRepository { get; }
        IBlavedPayIDRepository BlavedPayIDRepository { get; }
        IBalanceRepository BalanceRepository { get; }
        IBonusBalanceRepository BonusBalanceRepository { get; }
        IBlockChainWalletRepository BlockChainWalletRepository { get; }
        IInfoForBlockChainRepository InfoForBlockChainRepository { get; }
        IExchangeRepository ExchangeRepository { get; }
        Task SaveChanges();
    }
}
