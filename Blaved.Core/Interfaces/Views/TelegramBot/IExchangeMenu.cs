using Telegram.Bot.Types;
using Blaved.Core.Objects.Models;
using Blaved.Core.Objects.Models.Info;

namespace Blaved.Core.Interfaces.Views
{
    public interface IExchangeMenu
    {
        Task<Message?> ExchangeCreateWaitFromAsset(UserModel user, CancellationToken cancellationToken, int currentPage = 1, bool isEdit = true);
        Task<Message?> ExchangeCreateWaitToAsset(UserModel user, string fromAsset, List<InfoForExchangeModel> infoForExchangeUnionList, CancellationToken cancellationToken, int currentPage = 1, bool isEdit = true);
        Task<Message?> ExchangeCreateWaitAmount(UserModel user, InfoForExchangeModel exchangeCoinInfo, CancellationToken cancellationToken, bool isEdit = true);
        Task<Message?> Exchange(UserModel user, CancellationToken cancellationToken, bool isEdit = true);
        Task<Message?> ExchangeCreateNotCorrectAmount(UserModel user, InfoForExchangeModel exchangeCoinInfoModel, CancellationToken cancellationToken, bool isEdit = true);

        Task<Message?> ExchangeConfirm(UserModel user, ExchangeQuoteModel exchangeQuoteModel, CancellationToken cancellationToken, bool isEdit = true);
        Task<Message?> ExchangeCompleted(UserModel user, ExchangeModel exchangeModel, CancellationToken cancellationToken, bool isEdit = true);
        Task ExchangeCreateErrorAnswer(UserModel user, string callbackQueryId, CancellationToken cancellationToken);
        Task ExchangeCreateErrorOverdueConfirmAnswer(UserModel user, string callbackQueryId, CancellationToken cancellationToken);
        Task<Message?> ExchangeCreateErrorQuote(UserModel user, CancellationToken cancellationToken, bool isEdit = true);
        Task ExchangeCreateCoinDontHavePairAnswer(UserModel user, string callbackQueryId, CancellationToken cancellationToken);
        Task ExchangeCreateInsufficientBalanceAnswer(UserModel user, InfoForExchangeModel infoForExchange, string callbackQueryId, CancellationToken cancellationToken);


        Task<Message?> ExchangeHistoryList(UserModel user, List<ExchangeModel> history, CancellationToken cancellationToken, int currentPage = 1, bool isEdit = true);
        Task<Message?> ExchangeHistoryView(UserModel user, ExchangeModel exchangeModel, CancellationToken cancellationToken, bool isEdit = true);
        Task ExchangeHistoryDontHaveAnswer(UserModel user, string callbackQueryId, CancellationToken cancellationToken);
    }
}
