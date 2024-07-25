using Blaved.Core.Objects.Models;
using Microsoft.EntityFrameworkCore;
using Blaved.Core.Data.DataBase;
using Blaved.Core.Interfaces.Repository;

namespace Blaved.Core.Data.Repository
{
    public class BlavedPayIDRepository : IBlavedPayIDRepository
    {
        private readonly MyDbContext _dbContext;
        public BlavedPayIDRepository(MyDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task AddBlavedPayIDTransfer(BlavedPayIDTransferModel blavedPayIDTransfer)
        {
            await _dbContext.BlavedPayIDTransfers.AddAsync(blavedPayIDTransfer);
        }
        public async Task<List<BlavedPayIDTransferModel>> GetBlavedPayIDTransferList(long userId)
        {
            return await _dbContext.BlavedPayIDTransfers.Where(c => c.UserId == userId).ToListAsync();
        }
        public async Task<BlavedPayIDTransferModel?> GetBlavedPayIDTransfer(int num)
        {
            return await _dbContext.BlavedPayIDTransfers.SingleOrDefaultAsync(u => u.Id == num);
        }
    }
}
