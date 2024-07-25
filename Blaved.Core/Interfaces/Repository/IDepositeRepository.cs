using Blaved.Core.Objects.Models;

namespace Blaved.Core.Interfaces.Repository
{
    public interface IDepositRepository
    {
        Task AddDeposite(DepositModel deposite);
        Task<List<DepositModel>> GetDepositList(long userId, string Asset, string network);
        Task<List<DepositModel>> GetDepositList(long userId, string network);
        Task<List<DepositModel>> GetDepositList();
        Task<DepositModel?> GetDeposit(int num);
    }
}
