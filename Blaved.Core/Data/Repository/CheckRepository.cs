using Blaved.Core.Data.DataBase;
using Blaved.Core.Interfaces.Repository;
using Blaved.Core.Objects.Models;
using Microsoft.EntityFrameworkCore;

namespace Blaved.Core.Data.Repository
{
    public class CheckRepository : ICheckRepository
    {
        private readonly MyDbContext _dbContext;
        public CheckRepository(MyDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<CheckModel?> GetCheck(int num)
        {
            return await _dbContext.Checks
        .SingleOrDefaultAsync(u => u.Id == num && u.IsDeleted == false);
        }
        public async Task<CheckModel?> GetCheck(string url)
        {
            return await _dbContext.Checks
        .SingleOrDefaultAsync(u => u.Url == url && u.IsDeleted == false);
        }
        public async Task<List<CheckModel>> GetCheckList(long userId)
        {
            return await _dbContext.Checks.Where(c => c.UserId == userId && c.IsDeleted == false).ToListAsync();
        }
        public async Task<List<CheckModel>> GetCheckList()
        {
            return await _dbContext.Checks.ToListAsync();
        }
        public async Task AddCheck(CheckModel check)
        {
            await _dbContext.Checks.AddAsync(check);
        }
        public async Task SubtractFromCheckCount(string url, int count)
        {
            var check = await _dbContext.Checks.SingleOrDefaultAsync(u => u.Url == url);
            if (check == null)
            {
                throw new ArgumentNullException(paramName: "Check", message: "Check is null");
            }
            check.Count -= count;
        }
        public async Task<CheckModel> UpdateCheckPassword(string url, string password)
        {
            var check = await _dbContext.Checks.SingleOrDefaultAsync(u => u.Url == url);
            if (check == null)
            {
                throw new ArgumentNullException(paramName: "Check", message: "Check is null");
            }
            check.Password = password;
            return check;
        }
        public async Task<CheckModel> DeleteCheckPassword(string url)
        {
            var check = await _dbContext.Checks.SingleOrDefaultAsync(u => u.Url == url);
            if (check == null)
            {
                throw new ArgumentNullException(paramName: "Check", message: "Check is null");
            }
            check.Password = null;
            return check;
        }
        public async Task DeleteCheck(string url)
        {
            var check = await _dbContext.Checks.SingleOrDefaultAsync(u => u.Url == url);
            if (check == null)
            {
                throw new ArgumentNullException(paramName: "Check", message: "Check is null");
            }
            check.IsDeleted = true;
        }
    }
}
