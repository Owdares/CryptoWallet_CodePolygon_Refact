using Blaved.Core.Interfaces.Services;
using Blaved.Core.Interfaces.Views;
using Blaved.Core.Objects.Models;
using Blaved.Core.Objects.Models.Configurations;
using Blaved.Core.Objects.Models.Info;
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
    public class CheckMenu : BotMenuBase, ICheckMenu
    {
        private readonly ITelegramBotClient _botClient;
        private readonly IInfoService _infoService;
        private readonly InterfaceTranslatorService _interfaceTranslatorService;
        private readonly AppConfig _appConfig;

        public CheckMenu(ITelegramBotClient botClient, IInfoService infoService, InterfaceTranslatorService interfaceTranslatorService,
            IOptions<AppConfig> appConfig, ILogger<CheckMenu> logger) : base(botClient, logger)
        {
            _botClient = botClient;
            _infoService = infoService;
            _interfaceTranslatorService = interfaceTranslatorService;
            _appConfig = appConfig.Value;
        }

        public async Task<Message?> Check(UserModel user, CancellationToken cancellationToken, bool isEdit = true)
        {
            var menuText = _interfaceTranslatorService.GetTranslation("M.Check", user.Language);
            var myCheckMenuText = _interfaceTranslatorService.GetTranslation("B.CheckList", user.Language);
            var createCheckMenuText = _interfaceTranslatorService.GetTranslation("B.CheckСreate", user.Language);
            var buttonBackText = _interfaceTranslatorService.GetTranslation("B.Back", user.Language);

            InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(text: createCheckMenuText, callbackData: $"{CallbackRequestRoute.CheckCreateWaitAssetPutPage}:1")
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(text: myCheckMenuText, callbackData: $"{CallbackRequestRoute.CheckListPutPage}:1"),
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(text: buttonBackText,callbackData: CallbackRequestRoute.BlavedPay)
                },

            });

            return await SendMessageAsync(user, menuText, inlineKeyboard, isEdit, cancellationToken);
        }


        #region CreateCheckMenu
        public async Task<Message?> CheckCreateWaitAsset(UserModel user, CancellationToken cancellationToken, int currentPage = 1, bool isEdit = true)
        {
            var menuText = _interfaceTranslatorService.GetTranslation("M.CheckCreateWaitAsset", user.Language);
            var buttonNextPageText = _interfaceTranslatorService.GetTranslation("B.NextPage", user.Language);
            var buttonBackPageText = _interfaceTranslatorService.GetTranslation("B.BackPage", user.Language);
            var buttonBackText = _interfaceTranslatorService.GetTranslation("B.Back", user.Language);

            var AssetList = _appConfig.AssetConfiguration.CoinList;

            var buttons = AssetList.Select(name => InlineKeyboardButton.WithCallbackData(text: name, callbackData: $"{CallbackRequestRoute.CheckCreateWaitAmountPutAsset}:{name}")).ToArray();

            int buttonsPerRow = 3;
            int buttonsPerColum = 4;

            int buttonsPerPage = buttonsPerRow * buttonsPerColum;

            int startIndex = (currentPage - 1) * buttonsPerPage;
            int endIndex = Math.Min(startIndex + buttonsPerPage, AssetList.Count);

            var buttonRows = buttons
                .Skip(startIndex)
                .Take(buttonsPerPage)
                .Select((button, index) => new { Button = button, Index = index, RowIndex = index / buttonsPerRow, ColumnIndex = index % buttonsPerRow })
                .GroupBy(x => x.RowIndex)
                .Take(buttonsPerColum)
                .Select(group => group.OrderBy(x => x.ColumnIndex).Select(x => x.Button).ToArray())
                .ToList();

            if (currentPage > 1 || endIndex < AssetList.Count)
            {
                var pageNavigationRow = new List<InlineKeyboardButton>();
                if (currentPage > 1)
                {
                    pageNavigationRow.Add(InlineKeyboardButton.WithCallbackData(buttonBackPageText, $"{CallbackRequestRoute.CheckCreateWaitAssetPutPage}:{currentPage - 1}"));
                }
                if (endIndex < AssetList.Count)
                {
                    pageNavigationRow.Add(InlineKeyboardButton.WithCallbackData(buttonNextPageText, $"{CallbackRequestRoute.CheckCreateWaitAssetPutPage}:{currentPage + 1}"));
                }
                buttonRows.Add(pageNavigationRow.ToArray());
            }

            buttonRows.Add(new[] { InlineKeyboardButton.WithCallbackData(text: buttonBackText, callbackData: CallbackRequestRoute.Check) });

            var inlineKeyboard = new InlineKeyboardMarkup(buttonRows);

            return await SendMessageAsync(user, menuText, inlineKeyboard, isEdit, cancellationToken);
        }
        public async Task<Message?> CheckCreateWaitAmount(UserModel user, CancellationToken cancellationToken, bool isEdit = true)
        {
            var menuText = _interfaceTranslatorService.GetTranslation("M.CheckCreateWaitAmount", user.Language);
            var buttonBackText = _interfaceTranslatorService.GetTranslation("B.BackUpdateAsset", user.Language);
            var buttonMinAmountText = _interfaceTranslatorService.GetTranslation("B.MinAmount", user.Language);
            var buttonMaxAmountText = _interfaceTranslatorService.GetTranslation("B.MaxAmount", user.Language);

            string Asset = user.MessagesCheckModel.Asset;

            decimal coinPrice = await _infoService.GetCoinPriceUSDT(Asset);
            var checkInfo = await _infoService.GetInfoForCheck(Asset);

            decimal coinBalance = user.BalanceModel.GetBalance(Asset);
            var assetUrl = _appConfig.AssetConfiguration.CoinUrl[Asset];

            menuText = string.Format(menuText,
                Asset, assetUrl,
                checkInfo.MinAmount.AmountCut(), Asset, checkInfo.MinAmount.AmountToUSD(coinPrice).AmountCutUSD(),
                coinBalance.AmountCut(), Asset, coinBalance.AmountToUSD(coinPrice).AmountCutUSD());

            buttonMinAmountText = string.Format(buttonMinAmountText, checkInfo.MinAmount.AmountCut(), checkInfo.MinAmount.AmountToUSD(coinPrice).AmountCutUSD());
            buttonMaxAmountText = string.Format(buttonMaxAmountText, coinBalance.AmountCut(), coinBalance.AmountToUSD(coinPrice).AmountCutUSD());

            InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(text: buttonMinAmountText, callbackData: $"{CallbackRequestRoute.CheckCreatePutAmount}:{checkInfo.MinAmount}"),
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(text: buttonMaxAmountText, callbackData: $"{CallbackRequestRoute.CheckCreatePutAmount}:{coinBalance}"),
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(text: buttonBackText, callbackData: $"{CallbackRequestRoute.CheckCreateWaitAssetPutPage}:1"),
                },
            });

            return await SendMessageAsync(user, menuText, inlineKeyboard, isEdit, cancellationToken);
        }
        public async Task<Message?> CheckCreateWaitCount(UserModel user, CancellationToken cancellationToken, bool isEdit = true)
        {
            var menuText = _interfaceTranslatorService.GetTranslation("M.CheckCreateWaitCount", user.Language);
            var buttonBackText = _interfaceTranslatorService.GetTranslation("B.BackUpdateAmount", user.Language);
            var buttonMinCountText = _interfaceTranslatorService.GetTranslation("B.MinCount", user.Language);
            var buttonMaxCountText = _interfaceTranslatorService.GetTranslation("B.MaxCount", user.Language);

            string Asset = user.MessagesCheckModel.Asset;
            decimal amount = user.MessagesCheckModel.Amount;
            decimal coinPrice = await _infoService.GetCoinPriceUSDT(Asset);
            decimal coinBalance = user.BalanceModel.GetBalance(Asset);
            int maxCount = (int)Math.Floor(coinBalance / amount);
            int minCount = 1;
            decimal minFullAmount = (amount * minCount).AmountRound();
            decimal maxFullAmount = (amount * maxCount).AmountRound();

            string minFullAmountUSD = minFullAmount.AmountToUSD(coinPrice).AmountCutUSD();
            string maxFullAmountUSD = maxFullAmount.AmountToUSD(coinPrice).AmountCutUSD();

            menuText = string.Format(menuText,
                amount.AmountCut(), Asset, amount.AmountToUSD(coinPrice).AmountCutUSD(),
                minCount, minFullAmount.AmountCut(), Asset, minFullAmountUSD,
                maxCount, maxFullAmount.AmountCut(), Asset, maxFullAmountUSD,
                coinBalance, Asset, coinBalance.AmountToUSD(coinPrice).AmountCutUSD());

            buttonMinCountText = string.Format(buttonMinCountText, minCount, minFullAmount.AmountCut(), Asset, minFullAmountUSD);
            buttonMaxCountText = string.Format(buttonMaxCountText, maxCount, maxFullAmount.AmountCut(), Asset, maxFullAmountUSD);

            InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(text: buttonMinCountText, callbackData: $"{CallbackRequestRoute.CheckCreatePutCount}:{minCount}"),
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(text: buttonMaxCountText, callbackData: $"{CallbackRequestRoute.CheckCreatePutCount}:{maxCount}"),
                },
                new[]
                {
                   InlineKeyboardButton.WithCallbackData(text: buttonBackText, callbackData:$"{CallbackRequestRoute.CheckCreateWaitAmountPutAsset}:{Asset}"),
                },

            });

            return await SendMessageAsync(user, menuText, inlineKeyboard, isEdit, cancellationToken);
        }
        public async Task<Message?> CheckUpdateWaitPassword(UserModel user, string url, CancellationToken cancellationToken, bool isEdit = true)
        {
            var menuText = _interfaceTranslatorService.GetTranslation("M.CheckUpdateWaitPassword", user.Language);
            var buttonBackText = _interfaceTranslatorService.GetTranslation("B.Back", user.Language);

            InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                   InlineKeyboardButton.WithCallbackData(text: buttonBackText, callbackData:$"{CallbackRequestRoute.CheckViewPutUrl}:{url}"),
                },
            });

            return await SendMessageAsync(user, menuText, inlineKeyboard, isEdit, cancellationToken);
        }

        public async Task<Message?> CheckUpdatePasswordNotCorrect(UserModel user, string url, CancellationToken cancellationToken, bool isEdit = true)
        {
            var menuText = _interfaceTranslatorService.GetTranslation("M.CheckUpdatePasswordNotCorrect", user.Language);
            var buttonMainMenuText = _interfaceTranslatorService.GetTranslation("B.Back", user.Language);

            InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(text: buttonMainMenuText, callbackData:$"{CallbackRequestRoute.CheckViewPutUrl}:{url}"),
                },
            });

            return await SendMessageAsync(user, menuText, inlineKeyboard, isEdit, cancellationToken);
        }
        public async Task<Message?> CheckCreateCountNotCorrect(UserModel user, CancellationToken cancellationToken, bool isEdit = true)
        {
            var menuText = _interfaceTranslatorService.GetTranslation("M.CheckCreateCountNotCorrect", user.Language);
            var buttonBackText = _interfaceTranslatorService.GetTranslation("B.BackUpdateAmount", user.Language);
            var buttonMinCountText = _interfaceTranslatorService.GetTranslation("B.MinCount", user.Language);
            var buttonMaxCountText = _interfaceTranslatorService.GetTranslation("B.MaxCount", user.Language);

            string Asset = user.MessagesCheckModel.Asset;
            decimal amount = user.MessagesCheckModel.Amount;
            decimal coinPrice = await _infoService.GetCoinPriceUSDT(Asset);
            decimal coinBalance = user.BalanceModel.GetBalance(Asset);
            int maxCount = (int)Math.Floor(coinBalance / amount);
            int minCount = 1;
            decimal minFullAmount = (amount * minCount).AmountRound();
            decimal maxFullAmount = (amount * maxCount).AmountRound();

            string minFullAmountUSD = minFullAmount.AmountToUSD(coinPrice).AmountCutUSD();
            string maxFullAmountUSD = maxFullAmount.AmountToUSD(coinPrice).AmountCutUSD();

            menuText = string.Format(menuText,
                amount.AmountCut(), Asset, amount.AmountToUSD(coinPrice).AmountCutUSD(),
                minCount, minFullAmount.AmountCut(), Asset, minFullAmountUSD,
                maxCount, maxFullAmount.AmountCut(), Asset, maxFullAmountUSD,
                coinBalance, Asset, coinBalance.AmountToUSD(coinPrice).AmountCutUSD());

            buttonMinCountText = string.Format(buttonMinCountText, minCount, minFullAmount.AmountCut(), Asset, minFullAmountUSD);
            buttonMaxCountText = string.Format(buttonMaxCountText, maxCount, maxFullAmount.AmountCut(), Asset, maxFullAmountUSD);

            InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(text: buttonMinCountText, callbackData: $"{CallbackRequestRoute.CheckCreatePutCount}:{minCount}"),
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(text: buttonMaxCountText, callbackData: $"{CallbackRequestRoute.CheckCreatePutCount}:{maxCount}"),
                },
                new[]
                {
                   InlineKeyboardButton.WithCallbackData(text: buttonBackText, callbackData:$"{CallbackRequestRoute.CheckCreateWaitAmountPutAsset}:{Asset}"),
                },

            });

            return await SendMessageAsync(user, menuText, inlineKeyboard, isEdit, cancellationToken);
        }
        public async Task<Message?> CheckCreateAmountNotCorrect(UserModel user, CancellationToken cancellationToken, bool isEdit = true)
        {
            var menuText = _interfaceTranslatorService.GetTranslation("M.CheckCreateAmountNotCorrect", user.Language);
            var buttonBackText = _interfaceTranslatorService.GetTranslation("B.BackUpdateAsset", user.Language);
            var buttonMinAmountText = _interfaceTranslatorService.GetTranslation("B.MinAmount", user.Language);
            var buttonMaxAmountText = _interfaceTranslatorService.GetTranslation("B.MaxAmount", user.Language);

            string Asset = user.MessagesCheckModel.Asset;

            decimal coinPrice = await _infoService.GetCoinPriceUSDT(Asset);
            var checkInfo = await _infoService.GetInfoForCheck(Asset);

            decimal coinBalance = user.BalanceModel.GetBalance(Asset);
            var assetUrl = _appConfig.AssetConfiguration.CoinUrl[Asset];

            menuText = string.Format(menuText,
                Asset, assetUrl,
                checkInfo.MinAmount.AmountCut(), Asset, checkInfo.MinAmount.AmountToUSD(coinPrice).AmountCutUSD(),
                coinBalance.AmountCut(), Asset, coinBalance.AmountToUSD(coinPrice).AmountCutUSD());

            buttonMinAmountText = string.Format(buttonMinAmountText, checkInfo.MinAmount.AmountCut(), checkInfo.MinAmount.AmountToUSD(coinPrice).AmountCutUSD());
            buttonMaxAmountText = string.Format(buttonMaxAmountText, coinBalance.AmountCut(), coinBalance.AmountToUSD(coinPrice).AmountCutUSD());

            InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(text: buttonMinAmountText, callbackData: $"{CallbackRequestRoute.CheckCreatePutAmount}:{checkInfo.MinAmount}"),
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(text: buttonMaxAmountText, callbackData: $"{CallbackRequestRoute.CheckCreatePutAmount}:{coinBalance}"),
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(text: buttonBackText, callbackData: $"{CallbackRequestRoute.CheckCreateWaitAssetPutPage}:1"),
                },
            });

            return await SendMessageAsync(user, menuText, inlineKeyboard, isEdit, cancellationToken);
        }
        public async Task CheckCreateInsufficientBalanceAnswer(UserModel user, InfoForCheckModel checkInfoModel, string callbackQueryId, CancellationToken cancellationToken)
        {
            var menuText = _interfaceTranslatorService.GetTranslation("M.CheckCreateInsufficientBalanceAnswer", user.Language);

            var userBalance = user.BalanceModel.GetBalance(checkInfoModel.Asset);
            decimal coinPrice = await _infoService.GetCoinPriceUSDT(checkInfoModel.Asset);

            menuText = string.Format(menuText,
                checkInfoModel.MinAmount.AmountCut(), checkInfoModel.Asset, checkInfoModel.MinAmount.AmountToUSD(coinPrice).AmountCutUSD(),
                userBalance.AmountCut(), checkInfoModel.Asset, userBalance.AmountToUSD(coinPrice).AmountCutUSD());

            await SendMessageAnswerAsync(menuText, callbackQueryId, cancellationToken);
        }

        public async Task<Message?> CheckConfirm(UserModel user, CancellationToken cancellationToken, bool isEdit = true)
        {
            var menuText = _interfaceTranslatorService.GetTranslation("M.CheckConfirm", user.Language);
            var buttonCompleteCheckText = _interfaceTranslatorService.GetTranslation("B.Confirm", user.Language);
            var buttonCancelCheckText = _interfaceTranslatorService.GetTranslation("B.Cancel", user.Language);
            var buttonBackText = _interfaceTranslatorService.GetTranslation("B.BackUpdateCount", user.Language);

            string Asset = user.MessagesCheckModel.Asset;
            decimal amount = user.MessagesCheckModel.Amount;
            int count = user.MessagesCheckModel.Count;
            decimal coinPrice = await _infoService.GetCoinPriceUSDT(Asset);
            var assetUrl = _appConfig.AssetConfiguration.CoinUrl[Asset];
            var completeSum = (amount * count).AmountRound();

            var checkMoreInfoUrl = _appConfig.UrlConfiguration.MediaUrl.CheckMoreInfoByLanguage[user.Language];

            menuText = string.Format(menuText,
                Asset, assetUrl,
                amount.AmountCut(), Asset, amount.AmountToUSD(coinPrice).AmountCutUSD(),
                count,
                completeSum.AmountCut(), Asset, completeSum.AmountToUSD(coinPrice).AmountCutUSD(),
                checkMoreInfoUrl);

            InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(text: buttonCompleteCheckText, callbackData: CallbackRequestRoute.CheckConfirm),
                    InlineKeyboardButton.WithCallbackData(text: buttonCancelCheckText, callbackData: CallbackRequestRoute.Check),
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(text: buttonBackText, callbackData: CallbackRequestRoute.CheckCreateWaitCount),
                },
            });

            return await SendMessageAsync(user, menuText, inlineKeyboard, isEdit, cancellationToken);

        }
        public async Task<Message?> CheckUpdatePasswordConfirm(UserModel user, string password, string url, CancellationToken cancellationToken, bool isEdit = true)
        {
            var menuText = _interfaceTranslatorService.GetTranslation("M.CheckUpdatePasswordConfirm", user.Language);
            var buttonCompleteCheckText = _interfaceTranslatorService.GetTranslation("B.Confirm", user.Language);
            var buttonCancelCheckText = _interfaceTranslatorService.GetTranslation("B.Cancel", user.Language);
            var buttonBackText = _interfaceTranslatorService.GetTranslation("B.Back", user.Language);

            menuText = string.Format(menuText, password);

            InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                           new[]
                           {
                               InlineKeyboardButton.WithCallbackData(text: buttonCompleteCheckText, callbackData:$"{CallbackRequestRoute.CheckUpdatePasswordConfirmPutUrlPassword}:{url}_{password}"),
                               InlineKeyboardButton.WithCallbackData(text: buttonCancelCheckText, callbackData:$"{CallbackRequestRoute.CheckViewPutUrl}:{url}"),
                           },
                           new[]
                           {
                               InlineKeyboardButton.WithCallbackData(text: buttonBackText, callbackData:$"{CallbackRequestRoute.CheckUpdateWaitPasswordPutUrl}:{url}"),
                           },
            });

            return await SendMessageAsync(user, menuText, inlineKeyboard, isEdit, cancellationToken);
        }

        #endregion

        #region ActivationCheckMenu
        public async Task<Message?> CheckActivation(UserModel user, CheckModel checkModul, CancellationToken cancellationToken, bool isEdit = true)
        {
            var menuText = _interfaceTranslatorService.GetTranslation("M.CheckActivation", user.Language);
            var buttonMainMenuText = _interfaceTranslatorService.GetTranslation("B.BackMainMenu", user.Language);

            decimal coinPrice = await _infoService.GetCoinPriceUSDT(checkModul.Asset);

            menuText = string.Format(menuText, checkModul.Amount.AmountCut(), checkModul.Asset, checkModul.Amount.AmountToUSD(coinPrice).AmountCutUSD());

            InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                   InlineKeyboardButton.WithCallbackData(text: buttonMainMenuText, callbackData: CallbackRequestRoute.Main),
                },
            });

            return await SendMessageAsync(user, menuText, inlineKeyboard, isEdit, cancellationToken);
        }
        public async Task<Message?> CheckActivationCountNull(UserModel user, CancellationToken cancellationToken, bool isEdit = true)
        {
            var menuText = _interfaceTranslatorService.GetTranslation("M.CheckActivationCountNull", user.Language);
            var buttonMainMenuText = _interfaceTranslatorService.GetTranslation("B.BackMainMenu", user.Language);

            InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(text: buttonMainMenuText, callbackData: CallbackRequestRoute.Main),
                },
            });

            return await SendMessageAsync(user, menuText, inlineKeyboard, isEdit, cancellationToken);
        }
        public async Task<Message?> CheckActivationWaitPassword(UserModel user, CheckModel checkModul, CancellationToken cancellationToken, bool isEdit = true)
        {
            var menuText = _interfaceTranslatorService.GetTranslation("M.CheckActivationWaitPassword", user.Language);
            var buttonMainMenuText = _interfaceTranslatorService.GetTranslation("B.BackMainMenu", user.Language);

            decimal coinPrice = await _infoService.GetCoinPriceUSDT(checkModul.Asset);

            menuText = string.Format(menuText,
                checkModul.Amount.AmountCut(), checkModul.Asset, checkModul.Amount.AmountToUSD(coinPrice).AmountCutUSD());

            InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(text: buttonMainMenuText, callbackData: CallbackRequestRoute.Main),
                },
            });

            return await SendMessageAsync(user, menuText, inlineKeyboard, isEdit, cancellationToken);
        }
        public async Task<Message?> CheckActivationPasswordNotCorrect(UserModel user, CancellationToken cancellationToken, bool isEdit = true)
        {
            var menuText = _interfaceTranslatorService.GetTranslation("M.CheckActivationPasswordNotCorrect", user.Language);
            var buttonMainMenuText = _interfaceTranslatorService.GetTranslation("B.Back", user.Language);

            InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(text: buttonMainMenuText, callbackData: CallbackRequestRoute.Main),
                },
            });

            return await SendMessageAsync(user, menuText, inlineKeyboard, isEdit, cancellationToken);
        }
        public async Task<Message?> CheckActivationAforeUsed(UserModel user, CancellationToken cancellationToken, bool isEdit = true)
        {
            var menuText = _interfaceTranslatorService.GetTranslation("M.CheckActivationAforeUsed", user.Language);
            var buttonMainMenuText = _interfaceTranslatorService.GetTranslation("B.BackMainMenu", user.Language);

            InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(text: buttonMainMenuText, callbackData: CallbackRequestRoute.Main),
                },
            });

            return await SendMessageAsync(user, menuText, inlineKeyboard, isEdit, cancellationToken);
        }
        public async Task<Message?> CheckActivationNotFound(UserModel user, CancellationToken cancellationToken, bool isEdit = true)
        {
            var menuText = _interfaceTranslatorService.GetTranslation("M.CheckActivationNotFound", user.Language);
            var buttonMainMenuText = _interfaceTranslatorService.GetTranslation("B.BackMainMenu", user.Language);

            InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(text: buttonMainMenuText, callbackData: CallbackRequestRoute.Main),
                },
            });

            return await SendMessageAsync(user, menuText, inlineKeyboard, isEdit, cancellationToken);
        }
        #endregion

        #region MyCheckMenu
        public async Task<Message?> CheckList(UserModel user, List<CheckModel> history, CancellationToken cancellationToken, int currentPage = 1, bool isEdit = true)
        {
            var menuText = _interfaceTranslatorService.GetTranslation("M.CheckList", user.Language);
            var buttonNextPageText = _interfaceTranslatorService.GetTranslation("B.NextPage", user.Language);
            var buttonBackPageText = _interfaceTranslatorService.GetTranslation("B.BackPage", user.Language);
            var buttonBackText = _interfaceTranslatorService.GetTranslation("B.Back", user.Language);

            int buttonsPerPage = 10;

            int startIndex = (currentPage - 1) * buttonsPerPage;
            int endIndex = Math.Min(startIndex + buttonsPerPage, history.Count);

            List<InlineKeyboardButton[]> buttons = (await Task.WhenAll(history
                .Skip(startIndex)
                .Take(buttonsPerPage)
                .Select(async x =>
                {
                    var assetPrice = await _infoService.GetCoinPriceUSDT(x.Asset);
                    return new[] { InlineKeyboardButton.WithCallbackData($"{x.Amount.AmountCut()} {x.Asset} (${x.Amount.AmountToUSD(assetPrice).AmountCutUSD()}) * {x.Count}", $"{CallbackRequestRoute.CheckViewPutUrl}:{x.Url}") };
                }))).ToList();

            if (currentPage > 1 || endIndex < history.Count)
            {
                var pageNavigationRow = new List<InlineKeyboardButton>();
                if (currentPage > 1)
                {
                    pageNavigationRow.Add(InlineKeyboardButton.WithCallbackData(buttonBackPageText, $"{CallbackRequestRoute.CheckListPutPage}:{currentPage - 1}"));
                }
                if (endIndex < history.Count)
                {
                    pageNavigationRow.Add(InlineKeyboardButton.WithCallbackData(buttonNextPageText, $"{CallbackRequestRoute.CheckListPutPage}:{currentPage + 1}"));
                }
                buttons.Add(pageNavigationRow.ToArray());
            }

            buttons.Add(new[] { InlineKeyboardButton.WithCallbackData(buttonBackText, CallbackRequestRoute.Check) });


            InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(buttons);

            return await SendMessageAsync(user, menuText, inlineKeyboard, isEdit, cancellationToken);
        }
        public async Task<Message?> CheckView(UserModel user, CheckModel checkModel, CancellationToken cancellationToken, bool isEdit = true)
        {
            var menuText = _interfaceTranslatorService.GetTranslation("M.CheckView", user.Language);
            var buttonChareCheckText = _interfaceTranslatorService.GetTranslation("B.ChareCheck", user.Language);
            var buttonSetPasswordOnCheckText = _interfaceTranslatorService.GetTranslation("B.CheckSetPassword", user.Language);
            var buttonDeleteCheckText = _interfaceTranslatorService.GetTranslation("B.CheckDelete", user.Language);
            var buttonDeletePasswordCheckText = _interfaceTranslatorService.GetTranslation("B.CheckDeletePassword", user.Language);
            var buttonChangePasswordOnCheckText = _interfaceTranslatorService.GetTranslation("B.CheckUpdatePassword", user.Language);
            var buttonBackText = _interfaceTranslatorService.GetTranslation("B.Back", user.Language);

            decimal coinPrice = await _infoService.GetCoinPriceUSDT(checkModel.Asset);
            var completeSum = (checkModel.Amount * checkModel.Count).AmountRound();

            var assetUrl = _appConfig.AssetConfiguration.CoinUrl[checkModel.Asset];

            menuText = string.Format(menuText,
                checkModel.Asset, assetUrl,
                checkModel.Amount.AmountCut(), checkModel.Asset, checkModel.Amount.AmountToUSD(coinPrice).AmountCutUSD(),
                checkModel.Count,
                completeSum.AmountCut(), checkModel.Asset, completeSum.AmountToUSD(coinPrice).AmountCutUSD(),
                checkModel.CreatedAt.ToShortDateString(),
                checkModel.CreatedAt.ToShortTimeString(),
                $"{_appConfig.UrlConfiguration.CheckUrl}{checkModel.Url}").Replace("=", "\\=");


            InlineKeyboardMarkup inlineKeyboard;
            if (checkModel.Password == null)
            {
                inlineKeyboard = new InlineKeyboardMarkup(new[]
                {
                new[]
                {
                    InlineKeyboardButton.WithSwitchInlineQuery(text: buttonChareCheckText, $"{_appConfig.UrlConfiguration.CheckUrlClip}{checkModel.Url}"),
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(text: buttonSetPasswordOnCheckText, callbackData:$"{CallbackRequestRoute.CheckUpdateWaitPasswordPutUrl}:{checkModel.Url}"),
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(text: buttonDeleteCheckText, callbackData:$"{CallbackRequestRoute.CheckDeletePutUrl}:{checkModel.Url}"),
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(text: buttonBackText, callbackData:$"{CallbackRequestRoute.CheckListPutPage}:1"),
                },
                });

            }
            else
            {
                inlineKeyboard = new InlineKeyboardMarkup(new[]
                {
                new[]
                {
                    InlineKeyboardButton.WithSwitchInlineQuery(text: buttonChareCheckText, $"{_appConfig.UrlConfiguration.CheckUrlClip}{checkModel.Url}"),
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(text: buttonChangePasswordOnCheckText, callbackData:$"{CallbackRequestRoute.CheckUpdateWaitPasswordPutUrl}:{checkModel.Url}"),
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(text: buttonDeletePasswordCheckText, callbackData:$"{CallbackRequestRoute.CheckPasswordDeletePutUrl}:{checkModel.Url}"),
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(text: buttonDeleteCheckText, callbackData:$"{CallbackRequestRoute.CheckDeletePutUrl}:{checkModel.Url}"),
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(text: buttonBackText, callbackData:$"{CallbackRequestRoute.CheckListPutPage}:1"),
                },
                });

            }

            return await SendMessageAsync(user, menuText, inlineKeyboard, isEdit, cancellationToken);
        }
        public async Task CheckDontHaveAnswer(UserModel user, string callbackQueryId, CancellationToken cancellationToken)
        {
            var menuText = _interfaceTranslatorService.GetTranslation("M.CheckDontHaveAnswer", user.Language);

            await SendMessageAnswerAsync(menuText, callbackQueryId, cancellationToken);
        }
        #endregion

        public async Task InlineChareCheck(UserModel user, string inlineQueryId, CheckModel check, CancellationToken cancellationToken)
        {
            var titleText = _interfaceTranslatorService.GetTranslation("B.InlineChareCheck_Title", user.Language);
            var descriptionText = _interfaceTranslatorService.GetTranslation("B.InlineChareCheck_Description", user.Language);
            var buttonText = _interfaceTranslatorService.GetTranslation("B.InlineChareCheck_Button", user.Language);
            var messageText = _interfaceTranslatorService.GetTranslation("B.InlineChareCheck_Message", user.Language);

            decimal coinPrice = await _infoService.GetCoinPriceUSDT(check.Asset);
            var completeSum = (check.Amount * check.Count).AmountRound();
            var assetUrl = _appConfig.AssetConfiguration.CoinUrl[check.Asset];

            messageText = string.Format(messageText,
                completeSum.AmountCut(), check.Asset, completeSum.AmountToUSD(coinPrice).AmountCutUSD(),
                check.Asset, assetUrl,
                check.Amount.AmountCut(), check.Asset, check.Amount.AmountToUSD(coinPrice).AmountCutUSD(),
                check.Count).Replace("\\n", "\n").Replace(".", "\\.");

            buttonText = string.Format(buttonText,
                check.Amount.AmountCut(), check.Asset, check.Amount.AmountToUSD(coinPrice).AmountCutUSD());

            descriptionText = string.Format(descriptionText,
                check.Amount.AmountCut(), check.Asset, check.Amount.AmountToUSD(coinPrice).AmountCutUSD(),
                check.Count).Replace("\\n", "\n");

            titleText = string.Format(titleText,
                completeSum.AmountCut(), check.Asset, completeSum.AmountToUSD(coinPrice).AmountCutUSD());

            List<InlineQueryResult> results = new List<InlineQueryResult>
            {
               new InlineQueryResultArticle(
                     id: inlineQueryId,
                     title: titleText,
                     inputMessageContent: new InputTextMessageContent(messageText)
                     {
                        ParseMode = ParseMode.MarkdownV2,
                        DisableWebPagePreview = true
                     })
                         {
                             ThumbnailUrl = _appConfig.UrlConfiguration.CheckLogoUrl,
                             Description = descriptionText,
                             ReplyMarkup = new InlineKeyboardMarkup(new[]
                             {
                                 new[]
                                 {
                                     InlineKeyboardButton.WithUrl(text: buttonText, url: $"{_appConfig.UrlConfiguration.CheckUrl}{check.Url}"),
                                 },
                             }),

                         },
            };

            await _botClient.AnswerInlineQueryAsync(inlineQueryId, results, cacheTime: 0, isPersonal: true, cancellationToken: cancellationToken);
        }
    }
}
