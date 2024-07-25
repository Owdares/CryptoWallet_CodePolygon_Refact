using Blaved.Core.Data.DataBase;
using Blaved.Core.Interfaces.Repository;
using Blaved.Core.Objects.Models;
using Microsoft.EntityFrameworkCore;

namespace Blaved.Core.Data.Repository
{
    public class WithdrawRepository : IWithdrawRepository
    {
        private readonly MyDbContext _dbContext;
        public WithdrawRepository(MyDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task AddWithdraw(WithdrawModel withdraw)
        {
            await _dbContext.Withdraws.AddAsync(withdraw);
        }
        public async Task<List<WithdrawModel>> GetWithdrawList(long userId, string Asset, string network)
        {
            return await _dbContext.Withdraws.Where(c => c.UserId == userId && c.Network == network && c.Asset == Asset).ToListAsync();

        }
        public async Task<List<WithdrawModel>> GetWithdrawList(long userId, string network)
        {
            return await _dbContext.Withdraws.Where(c => c.UserId == userId && c.Network == network).ToListAsync();
        }
        public async Task<WithdrawModel?> GetWithdraw(int num)
        {
            return await _dbContext.Withdraws.SingleOrDefaultAsync(u => u.Id == num);
        }
    }
}
