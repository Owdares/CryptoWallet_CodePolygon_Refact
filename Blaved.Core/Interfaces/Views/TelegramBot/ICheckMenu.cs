using Telegram.Bot.Types;
using Blaved.Core.Objects.Models;
using Blaved.Core.Objects.Models.Info;


namespace Blaved.Core.Interfaces.Views
{
    public interface ICheckMenu
    {
        Task<Message?> Check(UserModel user, CancellationToken cancellationToken, bool isEdit = true);


        #region CreateCheckMenu
        Task<Message?> CheckCreateWaitAsset(UserModel user, CancellationToken cancellationToken, int currentPage = 1, bool isEdit = true);
        Task<Message?> CheckCreateWaitAmount(UserModel user, CancellationToken cancellationToken, bool isEdit = true);
        Task<Message?> CheckCreateWaitCount(UserModel user, CancellationToken cancellationToken, bool isEdit = true);
        Task<Message?> CheckUpdateWaitPassword(UserModel user, string url, CancellationToken cancellationToken, bool isEdit = true);

        Task<Message?> CheckUpdatePasswordNotCorrect(UserModel user, string url, CancellationToken cancellationToken, bool isEdit = true);
        Task<Message?> CheckCreateCountNotCorrect(UserModel user, CancellationToken cancellationToken, bool isEdit = true);
        Task<Message?> CheckCreateAmountNotCorrect(UserModel user, CancellationToken cancellationToken, bool isEdit = true);
        Task CheckCreateInsufficientBalanceAnswer(UserModel user, InfoForCheckModel checkInfoModel, string callbackQueryId, CancellationToken cancellationToken);
        Task<Message?> CheckConfirm(UserModel user, CancellationToken cancellationToken, bool isEdit = true);
        Task<Message?> CheckUpdatePasswordConfirm(UserModel user, string password, string url, CancellationToken cancellationToken, bool isEdit = true);

        #endregion

        #region ActivationCheckMenu
        Task<Message?> CheckActivation(UserModel user, CheckModel checkModul, CancellationToken cancellationToken, bool isEdit = true);
        Task<Message?> CheckActivationCountNull(UserModel user, CancellationToken cancellationToken, bool isEdit = true);
        Task<Message?> CheckActivationWaitPassword(UserModel user, CheckModel checkModul, CancellationToken cancellationToken, bool isEdit = true);
        Task<Message?> CheckActivationPasswordNotCorrect(UserModel user, CancellationToken cancellationToken, bool isEdit = true);
        Task<Message?> CheckActivationAforeUsed(UserModel user, CancellationToken cancellationToken, bool isEdit = true);
        Task<Message?> CheckActivationNotFound(UserModel user, CancellationToken cancellationToken, bool isEdit = true);
        #endregion

        #region MyCheckMenu
        Task<Message?> CheckList(UserModel user, List<CheckModel> history, CancellationToken cancellationToken, int currentPage = 1, bool isEdit = true);
        Task<Message?> CheckView(UserModel user, CheckModel checkModel, CancellationToken cancellationToken, bool isEdit = true);
        Task CheckDontHaveAnswer(UserModel user, string callbackQueryId, CancellationToken cancellationToken);
        #endregion


        Task InlineChareCheck(UserModel user, string inlineQueryId, CheckModel check, CancellationToken cancellationToken);
    }
}
