using Blaved.Core.Objects.Models;

namespace Blaved.Core.Interfaces.Repository
{
    public interface IExchangeRepository
    {
        Task AddExchange(ExchangeModel exchange);
        Task<List<ExchangeModel>> GetExchangeList(long userId);
        Task<ExchangeModel?> GetExchange(int num);
    }
}
