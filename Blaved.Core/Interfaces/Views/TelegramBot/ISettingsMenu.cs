using Telegram.Bot.Types;
using Blaved.Core.Objects.Models;

namespace Blaved.Core.Interfaces.Views
{
    public interface ISettingsMenu
    {
        Task<Message?> Language(UserModel user, CancellationToken cancellationToken, bool isEdit = true);
        Task<Message?> Referrals(UserModel user, int referralsCount, decimal fullAmountChargeToReferralInUSD, CancellationToken cancellationToken, bool isEdit = true);
        Task<Message?> Settings(UserModel user, CancellationToken cancellationToken, bool isEdit = true);
        Task<Message?> TermsOfUse(UserModel user, CancellationToken cancellationToken, int currentPage = 1, bool isEdit = true);

        Task<Message?> Notifications(UserModel user, CancellationToken cancellationToken, bool isEdit = true);
        Task<Message?> NotificationsBlavedPay(UserModel user, CancellationToken cancellationToken, bool isEdit = true);
        Task SettingsNotificationDisableBlavedPayAnswer(UserModel user, string callbackQueryId, CancellationToken cancellationToken);
        Task SettingsNotificationEnableBlavedPayAnswer(UserModel user, string callbackQueryId, CancellationToken cancellationToken);
        Task InlineSendReferralInvite(UserModel user, string inlineQueryId, long whoseReferalId, CancellationToken cancellationToken);
    }
}
