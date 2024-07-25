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
    public class SettingsMenu : BotMenuBase, ISettingsMenu
    {
        private readonly ITelegramBotClient _botClient;
        private readonly IInfoService _infoService;
        private readonly InterfaceTranslatorService _interfaceTranslatorService;
        private readonly AppConfig _appConfig;
        public SettingsMenu(ITelegramBotClient botClient, IInfoService infoService, InterfaceTranslatorService interfaceTranslatorService,
            IOptions<AppConfig> appConfig, ILogger<SettingsMenu> logger) : base(botClient, logger)
        {
            _botClient = botClient;
            _infoService = infoService;
            _interfaceTranslatorService = interfaceTranslatorService;
            _appConfig = appConfig.Value;
        }

        public async Task<Message?> Language(UserModel user, CancellationToken cancellationToken, bool isEdit = true)
        {
            var menuText = _interfaceTranslatorService.GetTranslation("M.Language", user.Language);
            var buttonBackText = _interfaceTranslatorService.GetTranslation("B.Back", user.Language);

            var languageList = _appConfig.AssetConfiguration.LanguageList.OrderByDescending(lang => lang == user.Language).ToList();

            var buttons = languageList.Select(x => 
            {
                var flagLanguageText = _interfaceTranslatorService.GetTranslation($"B.Language{x}", user.Language);
                return InlineKeyboardButton.WithCallbackData($"{flagLanguageText}", $"{CallbackRequestRoute.LanguageUpdatePutLanguage}:{x}"); 
            }).ToArray();

            int buttonsPerRow = 2;

            var buttonRows = buttons.Select((button, index) => new { Button = button, Index = index })
                .GroupBy(x => x.Index / buttonsPerRow)
                .Select(group => group.Select(x => x.Button).ToArray())
                .ToList();

            buttonRows.Add(new[] { InlineKeyboardButton.WithCallbackData(text: buttonBackText, callbackData: CallbackRequestRoute.Settings) });

            var inlineKeyboard = new InlineKeyboardMarkup(buttonRows);

            return await SendMessageAsync(user, menuText, inlineKeyboard, isEdit, cancellationToken);
        }
        public async Task<Message?> Referrals(UserModel user, int referralsCount, decimal fullAmountChargeToReferralInUSD, CancellationToken cancellationToken, bool isEdit = true)
        {
            var menuText = _interfaceTranslatorService.GetTranslation("M.Referrals", user.Language);
            var buttonChareReferralsText = _interfaceTranslatorService.GetTranslation("B.ChareReferrals", user.Language);
            var buttonBackText = _interfaceTranslatorService.GetTranslation("B.Back", user.Language);

            menuText = string.Format(menuText,
                user.RateReferralExchange, referralsCount, fullAmountChargeToReferralInUSD.AmountCutUSD(), $"{_appConfig.UrlConfiguration.ReferralUrl}{user.UserId}").Replace("=", "\\=");

            InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithSwitchInlineQuery(text: buttonChareReferralsText, $"{_appConfig.UrlConfiguration.ReferralUrlClip}{user.UserId}")
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(text: buttonBackText,callbackData: CallbackRequestRoute.Settings)
                },

            });

            return await SendMessageAsync(user, menuText, inlineKeyboard, isEdit, cancellationToken);
        }
        public async Task<Message?> Settings(UserModel user, CancellationToken cancellationToken, bool isEdit = true)
        {
            var menuText = _interfaceTranslatorService.GetTranslation("M.Settings", user.Language);
            var groupUrlText = _interfaceTranslatorService.GetTranslation("B.GroupUrl", user.Language);
            var chatUrlText = _interfaceTranslatorService.GetTranslation("B.ChatUrl", user.Language);
            var referalText = _interfaceTranslatorService.GetTranslation("B.Referrals", user.Language);
            var whatCanText = _interfaceTranslatorService.GetTranslation("B.TermsOfUse", user.Language);
            var getLanguageText = _interfaceTranslatorService.GetTranslation("B.Language", user.Language);
            var buttonBackText = _interfaceTranslatorService.GetTranslation("B.BackMainMenu", user.Language);
            var notificationsText = _interfaceTranslatorService.GetTranslation("B.Notifications", user.Language);

            InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(text: whatCanText, callbackData: $"{CallbackRequestRoute.TermsOfUsePutPage}:1")
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(text: referalText, callbackData: CallbackRequestRoute.Referrals),
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(text: notificationsText, callbackData: CallbackRequestRoute.Notifications),
                    InlineKeyboardButton.WithCallbackData(text: getLanguageText, callbackData: CallbackRequestRoute.Language)
                },
                new[]
                {   InlineKeyboardButton.WithUrl(text: groupUrlText,url: _appConfig.UrlConfiguration.MediaUrl.GroupByLanguage[user.Language]),
                    InlineKeyboardButton.WithUrl(text: chatUrlText,url: _appConfig.UrlConfiguration.MediaUrl.ChatByLanguage[user.Language])
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(text: buttonBackText,callbackData: CallbackRequestRoute.Main)
                },

            });

            return await SendMessageAsync(user, menuText, inlineKeyboard, isEdit, cancellationToken);
        }
        public async Task<Message?> TermsOfUse(UserModel user, CancellationToken cancellationToken, int currentPage = 1, bool isEdit = true)
        {
            var buttonBackText = _interfaceTranslatorService.GetTranslation("B.Back", user.Language);
            var buttonNextPageText = _interfaceTranslatorService.GetTranslation("B.NextPage", user.Language);
            var buttonBackPageText = _interfaceTranslatorService.GetTranslation("B.BackPage", user.Language);

            string menuText = _interfaceTranslatorService.GetTranslation($"M.TermsOfUse", user.Language);

            string startTag = $"PageStart {currentPage}";
            string endTag = $"PageEnd {currentPage}";

            int startIndex = menuText.IndexOf(startTag);
            int endIndex = menuText.IndexOf(endTag, startIndex + startTag.Length);

            menuText = menuText.Substring(startIndex + startTag.Length, endIndex - startIndex - startTag.Length).Trim();

            switch (currentPage)
            {
                case 1:
                    menuText = string.Format(menuText, 
                        _appConfig.UrlConfiguration.MediaUrl.GroupByLanguage[user.Language],
                        _appConfig.UrlConfiguration.MediaUrl.GroupByLanguage[user.Language],
                        _appConfig.UrlConfiguration.MediaUrl.GroupByLanguage[user.Language]);
                    break;
                case 2:
                    menuText = string.Format(menuText,
                        _appConfig.UrlConfiguration.MediaUrl.GroupByLanguage[user.Language],
                        _appConfig.UrlConfiguration.MediaUrl.GroupByLanguage[user.Language]);
                    break;
                case 3:
                    menuText = string.Format(menuText, 
                        _appConfig.UrlConfiguration.MediaUrl.GroupByLanguage[user.Language],
                        _appConfig.UrlConfiguration.MediaUrl.GroupByLanguage[user.Language]);
                    break;
                case 4:
                    menuText = string.Format(menuText, 
                        _appConfig.UrlConfiguration.MediaUrl.GroupByLanguage[user.Language],
                        _appConfig.UrlConfiguration.MediaUrl.GroupByLanguage[user.Language]);
                    break;
                default:
                    break;
            }

            List<InlineKeyboardButton[]> rows = new List<InlineKeyboardButton[]>();

            var pageNavigationRow = new List<InlineKeyboardButton>();
            if (currentPage > 1)
            {
                pageNavigationRow.Add(InlineKeyboardButton.WithCallbackData(buttonBackPageText, $"{CallbackRequestRoute.TermsOfUsePutPage}:{currentPage - 1}"));
            }
            if (currentPage < 4)
            {
                pageNavigationRow.Add(InlineKeyboardButton.WithCallbackData(buttonNextPageText, $"{CallbackRequestRoute.TermsOfUsePutPage}:{currentPage + 1}"));
            }
            rows.Add(pageNavigationRow.ToArray());


            rows.Add(new[]
            {
               InlineKeyboardButton.WithCallbackData(text: buttonBackText,callbackData: CallbackRequestRoute.Settings)
            });

            InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(rows);

            return await SendMessageAsync(user, menuText, inlineKeyboard, isEdit, cancellationToken);
        }

        public async Task<Message?> Notifications(UserModel user, CancellationToken cancellationToken, bool isEdit = true)
        {
            var menuText = _interfaceTranslatorService.GetTranslation("M.Notifications", user.Language);
            var buttonBlavedPayText = _interfaceTranslatorService.GetTranslation("B.NotificationsBlavedPay", user.Language);
            var buttonBackText = _interfaceTranslatorService.GetTranslation("B.Back", user.Language);

            InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(text: buttonBlavedPayText, CallbackRequestRoute.NotificationsBlavedPay)
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(text: buttonBackText,callbackData: CallbackRequestRoute.Settings)
                },

            });

            return await SendMessageAsync(user, menuText, inlineKeyboard, isEdit, cancellationToken);
        }
        public async Task<Message?> NotificationsBlavedPay(UserModel user, CancellationToken cancellationToken, bool isEdit = true)
        {
            var menuText = _interfaceTranslatorService.GetTranslation("M.NotificationsBlavedPay", user.Language);
            var enableText = _interfaceTranslatorService.GetTranslation("B.Enable", user.Language);
            var disableText = _interfaceTranslatorService.GetTranslation("B.Disable", user.Language);
            var buttonBackText = _interfaceTranslatorService.GetTranslation("B.Back", user.Language);

            List<InlineKeyboardButton[]> buttons = new List<InlineKeyboardButton[]>();

            if (user.EnabledNotificationsBlavedPay)
            {
                buttons.Add(new[] { InlineKeyboardButton.WithCallbackData(disableText, CallbackRequestRoute.SettingsNotificationsDisableBlavedPay) });
            }
            else
            {
                buttons.Add(new[] { InlineKeyboardButton.WithCallbackData(enableText, CallbackRequestRoute.SettingsNotificationsEnableBlavedPay) });
            }

            buttons.Add(new[] { InlineKeyboardButton.WithCallbackData(buttonBackText, CallbackRequestRoute.Notifications) });

            InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(buttons);

            return await SendMessageAsync(user, menuText, inlineKeyboard, isEdit, cancellationToken);
        }
        public async Task SettingsNotificationDisableBlavedPayAnswer(UserModel user, string callbackQueryId, CancellationToken cancellationToken)
        {
            var menuText = _interfaceTranslatorService.GetTranslation("M.SettingsNotificationDisableBlavedPayAnswer", user.Language);

            await SendMessageAnswerAsync(menuText, callbackQueryId, cancellationToken);
        }
        public async Task SettingsNotificationEnableBlavedPayAnswer(UserModel user, string callbackQueryId, CancellationToken cancellationToken)
        {
            var menuText = _interfaceTranslatorService.GetTranslation("M.SettingsNotificationEnableBlavedPayAnswer", user.Language);

            await SendMessageAnswerAsync(menuText, callbackQueryId, cancellationToken);
        }

        public async Task InlineSendReferralInvite(UserModel user, string inlineQueryId, long whoseReferalId, CancellationToken cancellationToken)
        {
            var titleText = _interfaceTranslatorService.GetTranslation("B.InlineSendReferralInvite_Title", user.Language);
            var descriptionText = _interfaceTranslatorService.GetTranslation("B.InlineSendReferralInvite_Description", user.Language);
            var buttonText = _interfaceTranslatorService.GetTranslation("B.InlineSendReferralInvite_Button", user.Language);
            var messageText = _interfaceTranslatorService.GetTranslation("B.InlineSendReferralInvite_Message", user.Language);

            messageText = string.Format(messageText,
                _appConfig.UrlConfiguration.MediaUrl.GroupByLanguage[user.Language],
                _appConfig.UrlConfiguration.MediaUrl.GroupByLanguage[user.Language],
                _appConfig.UrlConfiguration.MediaUrl.ChatByLanguage[user.Language]).Replace(".", "\\.").Replace("\\n", "\n");

            List<InlineQueryResult> results = new List<InlineQueryResult>
            {
               new InlineQueryResultArticle(
                   id: inlineQueryId,
                   title: titleText,
                   inputMessageContent: new InputTextMessageContent(messageText)
                   {
                        ParseMode = ParseMode.MarkdownV2,
                   })
                      {
                          ThumbnailUrl = _appConfig.UrlConfiguration.MainLogoUrl,
                          Description = descriptionText,
                          ReplyMarkup = new InlineKeyboardMarkup(new[]
                          {
                              new[]
                              {
                                  InlineKeyboardButton.WithUrl(text: buttonText, url: $"{_appConfig.UrlConfiguration.ReferralUrl}{user.UserId}"),
                              },
                          }),
                      },
            };

            await _botClient.AnswerInlineQueryAsync(inlineQueryId, results, cacheTime: 0, isPersonal: true, cancellationToken: cancellationToken);
        }
    }
}
