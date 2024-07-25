using Blaved.Core.Objects.Models;
using Blaved.Core.Objects.Models.Info;
using Telegram.Bot.Types;

namespace Blaved.Core.Interfaces.Views
{
    public interface IBlavedPayMenu
    {
        Task<Message?> BlavedPay(UserModel user, CancellationToken cancellationToken, bool isEdit = true);
        Task<Message?> BlavedPayID(UserModel user, CancellationToken cancellationToken, bool isEdit = true);
        Task<Message?> BlavedPayIDTransferCreateWaitAsset(UserModel user, CancellationToken cancellationToken, int currentPage = 1, bool isEdit = true);
        Task<Message?> BlavedPayIDTransferCreateWaitID(UserModel user, string Asset, CancellationToken cancellationToken, bool isEdit = true);
        Task<Message?> BlavedPayIDTransferCreateWaitAmount(UserModel user, CancellationToken cancellationToken, bool isEdit = true);
        Task BlavedPayIDTransferCreateInsufficientBalanceAnswer(UserModel user, InfoForBlavedPayIDModel blavedPayInfoID, string callbackQueryId, CancellationToken cancellationToken);
        Task<Message?> BlavedPayIDTransferCreateNotCorrectAmount(UserModel user, CancellationToken cancellationToken, bool isEdit = true);
        Task<Message?> BlavedPayIDTransferCreateUserNotFound(UserModel user, CancellationToken cancellationToken, bool isEdit = true);
        Task<Message?> BlavedPayIDTransferCreateNotCorrectID(UserModel user, CancellationToken cancellationToken, bool isEdit = true);
        Task<Message?> BlavedPayIDTransferConfirm(UserModel user, CancellationToken cancellationToken, bool isEdit = true);
        Task BlavedPayIDTransferCompleted(UserModel user, BlavedPayIDTransferModel blavedPayIDTransfer, CancellationToken cancellationToken, bool isEdit = true);

        Task<Message?> BlavedPayIDTransferHistoryList(UserModel user, List<BlavedPayIDTransferModel> history, CancellationToken cancellationToken, int currentPage = 1, bool isEdit = true);
        Task<Message?> BlavedPayIDTransferHistoryView(UserModel user, BlavedPayIDTransferModel blavedPayIDTransfer, CancellationToken cancellationToken, bool isEdit = true);
        Task BlavedPayIDTransferHistoryDontHaveAnswer(UserModel user, string callbackQueryId, CancellationToken cancellationToken);
        Task BlavedPayNotificationDisableAnswer(UserModel user, string callbackQueryId, CancellationToken cancellationToken);
        Task<Message?> BlavedPayIDTransferAlert(UserModel user, BlavedPayIDTransferModel blavedPayIDTransfer, CancellationToken cancellationToken, bool isEdit = true);


    }
}
