using Blaved.Core.Objects.Models;
using Telegram.Bot.Types;

namespace Blaved.Core.Interfaces.Views
{
    public interface IHelpMenu
    {
        Task<Message?> HelpPage(UserModel user, CancellationToken cancellationToken, bool isEdit = true);
    }
}
