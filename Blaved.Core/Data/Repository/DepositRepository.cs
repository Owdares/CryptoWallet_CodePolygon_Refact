using Blaved.Core.Data.DataBase;
using Blaved.Core.Interfaces.Repository;
using Blaved.Core.Objects.Models;
using Microsoft.EntityFrameworkCore;

namespace Blaved.Core.Data.Repository
{
    public class DepositRepository : IDepositRepository
    {
        private readonly MyDbContext _dbContext;
        public DepositRepository(MyDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task AddDeposite(DepositModel deposit)
        {
            await _dbContext.Deposits.AddAsync(deposit);
        }
        public async Task<List<DepositModel>> GetDepositList(long userId, string Asset, string network)
        {
            return await _dbContext.Deposits.Where(c => c.UserId == userId && c.Network == network && c.Asset == Asset && c.IsInside == false).ToListAsync();
        }
        public async Task<List<DepositModel>> GetDepositList(long userId, string network)
        {
            return await _dbContext.Deposits.Where(c => c.UserId == userId && c.Network == network && c.IsInside == false).ToListAsync();
        }
        public async Task<List<DepositModel>> GetDepositList()
        {
            return await _dbContext.Deposits.ToListAsync();
        }
        public async Task<DepositModel?> GetDeposit(int num)
        {
            return await _dbContext.Deposits.SingleOrDefaultAsync(u => u.Id == num);
        }
    }
}
