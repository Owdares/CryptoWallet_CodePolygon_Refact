using Blaved.Core.Data.DataBase;
using Blaved.Core.Interfaces.Repository;
using Blaved.Core.Objects.Models;

namespace Blaved.Core.Data.Repository
{
    public class TransferToHotRepository : ITransferToHotRepository
    {
        private readonly MyDbContext _dbContext;
        public TransferToHotRepository(MyDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task AddTransferToHot(HotTransferModel hotTransfer)
        {
            await _dbContext.HotTransfers.AddAsync(hotTransfer);
        }
    }
}
