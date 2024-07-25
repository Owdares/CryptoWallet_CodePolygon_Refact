using Blaved.Core.Objects.Models;
using Blaved.Core.Objects.Models.Info;

namespace Blaved.Core.Interfaces.Services
{
    public interface IExchangeService
    {
        Task<Result<ExchangeQuoteModel>> ConvertQuoteRequest(UserModel user, InfoForExchangeModel infoForExchangeModel);
        Task<Result<ExchangeModel>> ConvertQuoteAccept(UserModel user, string quoteId, InfoForExchangeModel infoForExchangeModel);
    }
}
