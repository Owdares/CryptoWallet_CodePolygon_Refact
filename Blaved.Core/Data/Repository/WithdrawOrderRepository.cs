using Binance.Net.Enums;
using Blaved.Core.Data.DataBase;
using Blaved.Core.Interfaces.Repository;
using Blaved.Core.Objects.Models;
using Microsoft.EntityFrameworkCore;

namespace Blaved.Core.Data.Repository
{
    public class WithdrawOrderRepository : IWithdrawOrderRepository
    {
        private readonly MyDbContext _dbContext;

        public WithdrawOrderRepository(MyDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddWithdrawOrder(WithdrawOrderModel withdrawOrderModel)
        {
            await _dbContext.WithdrawOrders.AddAsync(withdrawOrderModel);
        }
        public async Task<WithdrawOrderModel?> GetWithdrawOrder(string idOrder)
        {
            return await _dbContext.WithdrawOrders.SingleOrDefaultAsync(u => u.IdOrder == idOrder);
        }
        public async Task<List<WithdrawOrderModel>> GetWithdrawOrderList()
        {
            return await _dbContext.WithdrawOrders.ToListAsync();
        }
        public async Task UpdateWithdrawOrderStatus(string idOrder, WithdrawalStatus newStatus)
        {
            var order = await _dbContext.WithdrawOrders.SingleOrDefaultAsync(x => x.IdOrder == idOrder);
            if (order == null)
            {
                throw new ArgumentNullException(paramName: "UpdateWithdrawOrderStatus", message: "UpdateWithdrawOrderStatus is null");
            }
            order.Status = newStatus;
        }
    }
}
