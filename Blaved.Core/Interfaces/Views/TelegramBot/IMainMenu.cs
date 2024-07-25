using Blaved.Core.Objects.Models;
using Telegram.Bot.Types;

namespace Blaved.Core.Interfaces.Views
{
    public interface IMainMenu
    {
        Task<Message?> Main(UserModel user, CancellationToken cancellationToken, bool isEdit = true);
        Task<Message?> ServerError(UserModel user, CancellationToken cancellationToken, bool isEdit = true);
        Task<Message?> UserHasBanned(UserModel user, CancellationToken cancellationToken, bool isEdit = true);
        Task AcceptTermsOfUseAnswer(UserModel user, string callbackQueryId, CancellationToken cancellationToken);
        Task TelegramAPIError(string callbackQueryId, CancellationToken cancellationToken);
        Task SectionIsTemporarilyUnavailableAnswer(UserModel user, string callbackQueryId, CancellationToken cancellationToken);

        Task FunctionDisabledAnswer(UserModel user, string callbackQueryId, CancellationToken cancellationToken);
        Task AssetDisabledAnswer(UserModel user, string callbackQueryId, CancellationToken cancellationToken);
        Task NetworkDisabledAnswer(UserModel user, string callbackQueryId, CancellationToken cancellationToken);
    }
}
