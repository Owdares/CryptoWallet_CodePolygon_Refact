using Blaved.Core.Interfaces.Services;
using Blaved.Core.Interfaces.Views;
using Blaved.Core.Objects.Models;
using Blaved.Core.Objects.Models.Configurations;
using Blaved.Core.Utility;
using Bleved.TelegramBot.Server.Controllers.Routes;
using Bleved.TelegramBot.Server.Services;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.ReplyMarkups;

namespace Bleved.TelegramBot.Server.Views
{
    public class HelpMenu : BotMenuBase, IHelpMenu
    {
        private readonly ITelegramBotClient _botClient;
        private readonly InterfaceTranslatorService _interfaceTranslatorService;
        private readonly AppConfig _appConfig;
        public HelpMenu(ITelegramBotClient botClient, InterfaceTranslatorService interfaceTranslatorService,
            IOptions<AppConfig> appConfig, ILogger<HelpMenu> logger) : base(botClient, logger)
        {
            _botClient = botClient;
            _interfaceTranslatorService = interfaceTranslatorService;
            _appConfig = appConfig.Value;
        }

        public async Task<Message?> HelpPage(UserModel user, CancellationToken cancellationToken, bool isEdit = true)
        {
            var menuText = _interfaceTranslatorService.GetTranslation("M.Help", user.Language);
            var buttonHelpUrlText = _interfaceTranslatorService.GetTranslation("B.HelpUrl", user.Language);
            var buttonBackText = _interfaceTranslatorService.GetTranslation("B.BackMainMenu", user.Language);

            InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithUrl(text: buttonHelpUrlText, url: _appConfig.UrlConfiguration.MediaUrl.HelpByLanguage[user.Language]),
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(text: buttonBackText,callbackData: CallbackRequestRoute.Main)
                },

            });

            return await SendMessageAsync(user, menuText, inlineKeyboard, isEdit, cancellationToken);
        }
    }
}
