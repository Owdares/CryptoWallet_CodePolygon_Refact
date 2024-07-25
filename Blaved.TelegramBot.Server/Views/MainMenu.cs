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
    public class MainMenu : BotMenuBase, IMainMenu
    {

        private readonly ITelegramBotClient _botClient;
        private readonly InterfaceTranslatorService _interfaceTranslatorService;
        private readonly AppConfig _appConfig;
        private readonly IInfoService _infoService;
        public MainMenu(ITelegramBotClient botClient, InterfaceTranslatorService interfaceTranslatorService,
            IOptions<AppConfig> appConfig, IInfoService infoService, ILogger<MainMenu> logger) : base(botClient, logger)
        {
            _infoService = infoService;
            _appConfig = appConfig.Value;
            _botClient = botClient;
            _interfaceTranslatorService = interfaceTranslatorService;
        }
        public async Task<Message?> Main(UserModel user, CancellationToken cancellationToken, bool isEdit = true)
        {
            var menuText = _interfaceTranslatorService.GetTranslation("M.Main", user.Language);
            var buttonWalletMenuText = _interfaceTranslatorService.GetTranslation("B.Wallet", user.Language);
            var buttonBlavedPayMainMenuText = _interfaceTranslatorService.GetTranslation("B.BlavedPay", user.Language);
            var buttonConvertMenuText = _interfaceTranslatorService.GetTranslation("B.Exchange", user.Language);
            var buttonHelpMenuText = _interfaceTranslatorService.GetTranslation("B.Help", user.Language);
            var buttonSettingsMenuText = _interfaceTranslatorService.GetTranslation("B.Settings", user.Language);

            var coinList = _appConfig.AssetConfiguration.CoinList;
            decimal fullBalanceUsd = 0;
            foreach (var coin in coinList)
            {
                var coinPrice = await _infoService.GetCoinPriceUSDT(coin);

                var balance = user.BalanceModel.GetBalance(coin);
                if (balance > 0)
                {
                    fullBalanceUsd += balance.AmountToUSD(coinPrice);
                }
                var bonusBalanceModel = user.BonusBalanceModel.GetBalance(coin);
                if (bonusBalanceModel > 0)
                {
                    fullBalanceUsd += bonusBalanceModel.AmountToUSD(coinPrice);
                }
            }

            buttonWalletMenuText = string.Format(buttonWalletMenuText,
                fullBalanceUsd.AmountCutUSD());

            menuText = string.Format(menuText,
                _appConfig.UrlConfiguration.MediaUrl.GroupByLanguage[user.Language],
                _appConfig.UrlConfiguration.MediaUrl.GroupByLanguage[user.Language],
                _appConfig.UrlConfiguration.MediaUrl.ChatByLanguage[user.Language]);

            InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(text: buttonWalletMenuText, callbackData: $"{CallbackRequestRoute.WalletPutPage}:1"),
                },
                new[]
                {
                     InlineKeyboardButton.WithCallbackData(text: buttonConvertMenuText, callbackData: CallbackRequestRoute.Exchange),
                     InlineKeyboardButton.WithCallbackData(text: buttonBlavedPayMainMenuText, callbackData: CallbackRequestRoute.BlavedPay)
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(text: buttonHelpMenuText, callbackData: CallbackRequestRoute.Help),
                     InlineKeyboardButton.WithCallbackData(text: buttonSettingsMenuText, callbackData: CallbackRequestRoute.Settings)
                },
            });

            return await SendMessageAsync(user, menuText, inlineKeyboard, isEdit, cancellationToken);
        }
        public async Task<Message?> ServerError(UserModel user, CancellationToken cancellationToken, bool isEdit = true)
        {
            var menuText = _interfaceTranslatorService.GetTranslation("M.ServerError", user.Language);
            var refreshText = _interfaceTranslatorService.GetTranslation("B.Refresh", user.Language);
            var buttonHelpUrlText = _interfaceTranslatorService.GetTranslation("B.HelpUrl", user.Language);

            InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                 new[]
                 {
                     InlineKeyboardButton.WithCallbackData(text: refreshText, callbackData: CallbackRequestRoute.Main),
                    InlineKeyboardButton.WithUrl(text: buttonHelpUrlText, url: _appConfig.UrlConfiguration.MediaUrl.HelpByLanguage[user.Language]),
                 },
            });

            return await SendMessageAsync(user, menuText, inlineKeyboard, isEdit, cancellationToken);
        }
        public async Task TelegramAPIError(string callbackQueryId, CancellationToken cancellationToken)
        {
            await SendMessageAnswerAsync(null, callbackQueryId, cancellationToken, false);
        }
        public async Task<Message?> UserHasBanned(UserModel user, CancellationToken cancellationToken, bool isEdit = true)
        {
            var menuText = _interfaceTranslatorService.GetTranslation("M.UserHasBanned", user.Language);
            var buttonHelpText = _interfaceTranslatorService.GetTranslation("B.Help", user.Language);
            var buttonRefreshText = _interfaceTranslatorService.GetTranslation("B.Refresh", user.Language);

            menuText = string.Format(menuText, _appConfig.UrlConfiguration.MediaUrl.HelpByLanguage[user.Language]);

            InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(text: buttonRefreshText, callbackData: CallbackRequestRoute.Main),
                },
                new[]
                {
                     InlineKeyboardButton.WithUrl(text: buttonHelpText, _appConfig.UrlConfiguration.MediaUrl.HelpByLanguage[user.Language]),
                },
            });

            return await SendMessageAsync(user, menuText, inlineKeyboard, isEdit, cancellationToken);
        }
        public async Task AcceptTermsOfUseAnswer(UserModel user, string callbackQueryId, CancellationToken cancellationToken)
        {
            var menuText = _interfaceTranslatorService.GetTranslation("M.AcceptTermsOfUseAnswer", user.Language);

            await SendMessageAnswerAsync(menuText, callbackQueryId, cancellationToken);
        }

        public async Task SectionIsTemporarilyUnavailableAnswer(UserModel user, string callbackQueryId, CancellationToken cancellationToken)
        {
            var menuText = _interfaceTranslatorService.GetTranslation("M.SectionIsTemporarilyUnavailableAnswer", user.Language);

            await SendMessageAnswerAsync(menuText, callbackQueryId, cancellationToken);
        }

        public async Task FunctionDisabledAnswer(UserModel user, string callbackQueryId, CancellationToken cancellationToken)
        {
            var menuText = _interfaceTranslatorService.GetTranslation("M.FunctionDisabledAnswer", user.Language);

            await SendMessageAnswerAsync(menuText, callbackQueryId, cancellationToken);
        }
        public async Task AssetDisabledAnswer(UserModel user, string callbackQueryId, CancellationToken cancellationToken)
        {
            var menuText = _interfaceTranslatorService.GetTranslation("M.AssetDisabledAnswer", user.Language);

            await SendMessageAnswerAsync(menuText, callbackQueryId, cancellationToken);
        }
        public async Task NetworkDisabledAnswer(UserModel user, string callbackQueryId, CancellationToken cancellationToken)
        {
            var menuText = _interfaceTranslatorService.GetTranslation("M.NetworkDisabledAnswer", user.Language);

            await SendMessageAnswerAsync(menuText, callbackQueryId, cancellationToken);
        }
    }
}
