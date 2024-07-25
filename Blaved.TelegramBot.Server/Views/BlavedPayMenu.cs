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
using Telegram.Bot.Types.ReplyMarkups;

namespace Bleved.TelegramBot.Server.Views
{
    public class BlavedPayMenu : BotMenuBase, IBlavedPayMenu
    {
        private readonly ITelegramBotClient _botClient;
        private readonly IInfoService _infoService;
        private readonly InterfaceTranslatorService _interfaceTranslatorService;
        private readonly AppConfig _appConfig;
        public BlavedPayMenu(ITelegramBotClient botClient, IInfoService infoService, InterfaceTranslatorService interfaceTranslatorService, 
            IOptions<AppConfig> appConfig, ILogger<BlavedPayMenu> logger) : base(botClient, logger)
        {
            _botClient = botClient;
            _infoService = infoService;
            _interfaceTranslatorService = interfaceTranslatorService;
            _appConfig = appConfig.Value;
        }

        public async Task<Message?> BlavedPay(UserModel user, CancellationToken cancellationToken, bool isEdit = true)
        {
            var menuText = _interfaceTranslatorService.GetTranslation("M.BlavedPay", user.Language);
            var buttonCheckMenuText = _interfaceTranslatorService.GetTranslation("B.Check", user.Language);
            var buttonBlavedPayIDMenuText = _interfaceTranslatorService.GetTranslation("B.BlavedPayID", user.Language);
            var buttonBinancePayIDMenuText = _interfaceTranslatorService.GetTranslation("B.BinancePayID", user.Language);
            var buttonMainMenuText = _interfaceTranslatorService.GetTranslation("B.BackMainMenu", user.Language);

            InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(text: buttonCheckMenuText, callbackData: CallbackRequestRoute.Check),
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(text: buttonBlavedPayIDMenuText, callbackData:CallbackRequestRoute.BlavedPayID),
                },
                new[]
                {

                     InlineKeyboardButton.WithCallbackData(text: buttonBinancePayIDMenuText, callbackData: CallbackRequestRoute.CommingSoon),
                },
                new[]
                {
                     InlineKeyboardButton.WithCallbackData(text: buttonMainMenuText, callbackData: CallbackRequestRoute.Main),
                },
            });

            return await SendMessageAsync(user, menuText, inlineKeyboard, isEdit, cancellationToken);
        }

        public async Task<Message?> BlavedPayID(UserModel user, CancellationToken cancellationToken, bool isEdit = true)
        {
            var menuText = _interfaceTranslatorService.GetTranslation("M.BlavedPayID", user.Language);
            var transferMenuText = _interfaceTranslatorService.GetTranslation("B.BlavedPayIDTransferCreate", user.Language);
            var historyMenuText = _interfaceTranslatorService.GetTranslation("B.BlavedPayIDTransferHistoryList", user.Language);
            var buttonBackText = _interfaceTranslatorService.GetTranslation("B.Back", user.Language);

            InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(text: transferMenuText, callbackData: $"{CallbackRequestRoute.BlavedPayIDTransferCreateWaitAssetPutPage}:1")
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(text: historyMenuText, callbackData: $"{CallbackRequestRoute.BlavedPayIDTransferHistoryListPutPage}:1"),
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(text: buttonBackText,callbackData: CallbackRequestRoute.BlavedPay)
                },

            });

            return await SendMessageAsync(user, menuText, inlineKeyboard, isEdit, cancellationToken);
        }

        public async Task<Message?> BlavedPayIDTransferCreateWaitAsset(UserModel user, CancellationToken cancellationToken, int currentPage = 1, bool isEdit = true)
        {
            var menuText = _interfaceTranslatorService.GetTranslation("M.BlavedPayIDTransferCreateWaitAsset", user.Language);
            var buttonNextPageText = _interfaceTranslatorService.GetTranslation("B.NextPage", user.Language);
            var buttonBackPageText = _interfaceTranslatorService.GetTranslation("B.BackPage", user.Language);
            var buttonBackText = _interfaceTranslatorService.GetTranslation("B.Back", user.Language);

            var AssetList = _appConfig.AssetConfiguration.CoinList;

            var buttons = AssetList.Select(name => InlineKeyboardButton.WithCallbackData(text: name, callbackData: $"{CallbackRequestRoute.BlavedPayIDTransferCreateWaitIdPutAsset}:{name}")).ToList();

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
                    pageNavigationRow.Add(InlineKeyboardButton.WithCallbackData(buttonBackPageText, $"{CallbackRequestRoute.BlavedPayIDTransferCreateWaitAssetPutPage}:{currentPage - 1}"));
                }
                if (endIndex < AssetList.Count)
                {
                    pageNavigationRow.Add(InlineKeyboardButton.WithCallbackData(buttonNextPageText, $"{CallbackRequestRoute.BlavedPayIDTransferCreateWaitAssetPutPage}:{currentPage + 1}"));
                }
                buttonRows.Add(pageNavigationRow.ToArray());
            }

            buttonRows.Add(new[] { InlineKeyboardButton.WithCallbackData(text: buttonBackText, callbackData: CallbackRequestRoute.BlavedPayID) });

            var inlineKeyboard = new InlineKeyboardMarkup(buttonRows);

            return await SendMessageAsync(user, menuText, inlineKeyboard, isEdit, cancellationToken);
        }
        public async Task<Message?> BlavedPayIDTransferCreateWaitID(UserModel user, string Asset, CancellationToken cancellationToken, bool isEdit = true)
        {
            var menuText = _interfaceTranslatorService.GetTranslation("M.BlavedPayIDTransferCreateWaitID", user.Language);
            var buttonBackText = _interfaceTranslatorService.GetTranslation("B.BackUpdateAsset", user.Language);

            var assetUrl = _appConfig.AssetConfiguration.CoinUrl[Asset];

            menuText = string.Format(menuText, Asset, assetUrl);

            InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                     InlineKeyboardButton.WithCallbackData(text: buttonBackText, callbackData: $"{CallbackRequestRoute.BlavedPayIDTransferCreateWaitAssetPutPage}:1"),
                },
            });

            return await SendMessageAsync(user, menuText, inlineKeyboard, isEdit, cancellationToken);
        }
        public async Task<Message?> BlavedPayIDTransferCreateWaitAmount(UserModel user, CancellationToken cancellationToken, bool isEdit = true)
        {
            var menuText = _interfaceTranslatorService.GetTranslation("M.BlavedPayIDTransferCreateWaitAmount", user.Language);
            var buttonBackText = _interfaceTranslatorService.GetTranslation("B.BackUpdateID", user.Language);
            var buttonMinAmountText = _interfaceTranslatorService.GetTranslation("B.MinAmount", user.Language);
            var buttonMaxAmountText = _interfaceTranslatorService.GetTranslation("B.MaxAmount", user.Language);

            var Asset = user.MessagesBlavedPayIDModel.Asset;

            decimal coinBalnace = user.BalanceModel.GetBalance(Asset);
            var blavedPayInfo = await _infoService.GetInfoForBlavedPayID(Asset);
            var coinPriceUSDT = await _infoService.GetCoinPriceUSDT(Asset);
            var assetUrl = _appConfig.AssetConfiguration.CoinUrl[Asset];

            menuText = string.Format(menuText,
                Asset, assetUrl,
                user.MessagesBlavedPayIDModel.ToUserId,
                blavedPayInfo.MinAmount.AmountCut(), Asset, blavedPayInfo.MinAmount.AmountToUSD(coinPriceUSDT).AmountCutUSD(),
                coinBalnace.AmountCut(), Asset, coinBalnace.AmountToUSD(coinPriceUSDT).AmountCutUSD());

            buttonMinAmountText = string.Format(buttonMinAmountText, blavedPayInfo.MinAmount.AmountCut(), blavedPayInfo.MinAmount.AmountToUSD(coinPriceUSDT).AmountCutUSD());
            buttonMaxAmountText = string.Format(buttonMaxAmountText, coinBalnace.AmountCut(), coinBalnace.AmountToUSD(coinPriceUSDT).AmountCutUSD());

            InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
               new[]
               {
                   InlineKeyboardButton.WithCallbackData(text: buttonMinAmountText, callbackData: $"{CallbackRequestRoute.BlavedPayIDTransferCreatePutAmount}:{blavedPayInfo.MinAmount}"),
               },
               new[]
               {
                   InlineKeyboardButton.WithCallbackData(text: buttonMaxAmountText, callbackData: $"{CallbackRequestRoute.BlavedPayIDTransferCreatePutAmount}:{coinBalnace}"),
               },
               new[]
               {
                   InlineKeyboardButton.WithCallbackData(text: buttonBackText, callbackData:$"{CallbackRequestRoute.BlavedPayIDTransferCreateWaitIdPutAsset}:{Asset}"),
               },
            });

            return await SendMessageAsync(user, menuText, inlineKeyboard, isEdit, cancellationToken);


        }
        public async Task BlavedPayIDTransferCreateInsufficientBalanceAnswer(UserModel user, InfoForBlavedPayIDModel blavedPayIDInfo, string callbackQueryId, CancellationToken cancellationToken)
        {
            var menuText = _interfaceTranslatorService.GetTranslation("M.BlavedPayIDTransferCreateInsufficientBalanceAnswer", user.Language);

            decimal coinPriceUSDT = await _infoService.GetCoinPriceUSDT(blavedPayIDInfo.Asset);
            var coinBalance = user.BalanceModel.GetBalance(blavedPayIDInfo.Asset);

            menuText = string.Format(menuText,
                blavedPayIDInfo.MinAmount.AmountCut(), blavedPayIDInfo.Asset, blavedPayIDInfo.MinAmount.AmountToUSD(coinPriceUSDT).AmountCutUSD(),
                coinBalance.AmountCut(), blavedPayIDInfo.Asset, coinBalance.AmountToUSD(coinPriceUSDT).AmountCutUSD());

            await SendMessageAnswerAsync(menuText, callbackQueryId, cancellationToken);
        }
        public async Task<Message?> BlavedPayIDTransferCreateNotCorrectAmount(UserModel user, CancellationToken cancellationToken, bool isEdit = true)
        {
            var menuText = _interfaceTranslatorService.GetTranslation("M.BlavedPayIDTransferCreateNotCorrectAmount", user.Language);
            var buttonBackText = _interfaceTranslatorService.GetTranslation("B.BackUpdateID", user.Language);

            var buttonMinAmountText = _interfaceTranslatorService.GetTranslation("B.MinAmount", user.Language);
            var buttonMaxAmountText = _interfaceTranslatorService.GetTranslation("B.MaxAmount", user.Language);

            var Asset = user.MessagesBlavedPayIDModel.Asset;

            decimal coinBalnace = user.BalanceModel.GetBalance(Asset);
            var blavedPayInfo = await _infoService.GetInfoForBlavedPayID(Asset);
            var coinPriceUSDT = await _infoService.GetCoinPriceUSDT(Asset);
            var assetUrl = _appConfig.AssetConfiguration.CoinUrl[Asset];

            menuText = string.Format(menuText,
                Asset, assetUrl,
                user.MessagesBlavedPayIDModel.ToUserId,
                blavedPayInfo.MinAmount.AmountCut(), Asset, blavedPayInfo.MinAmount.AmountToUSD(coinPriceUSDT).AmountCutUSD(),
                coinBalnace.AmountCut(), Asset, coinBalnace.AmountToUSD(coinPriceUSDT).AmountCutUSD());

            buttonMinAmountText = string.Format(buttonMinAmountText, blavedPayInfo.MinAmount.AmountCut(), blavedPayInfo.MinAmount.AmountToUSD(coinPriceUSDT).AmountCutUSD());
            buttonMaxAmountText = string.Format(buttonMaxAmountText, coinBalnace.AmountCut(), coinBalnace.AmountToUSD(coinPriceUSDT).AmountCutUSD());

            InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
               new[]
               {
                   InlineKeyboardButton.WithCallbackData(text: buttonMinAmountText, callbackData: $"{CallbackRequestRoute.BlavedPayIDTransferCreatePutAmount}:{blavedPayInfo.MinAmount}"),
               },
               new[]
               {
                   InlineKeyboardButton.WithCallbackData(text: buttonMaxAmountText, callbackData: $"{CallbackRequestRoute.BlavedPayIDTransferCreatePutAmount}:{coinBalnace}"),
               },
               new[]
               {
                   InlineKeyboardButton.WithCallbackData(text: buttonBackText, callbackData:$"{CallbackRequestRoute.BlavedPayIDTransferCreateWaitIdPutAsset}:{Asset}"),
               },
            });

            return await SendMessageAsync(user, menuText, inlineKeyboard, isEdit, cancellationToken);
        }
        public async Task<Message?> BlavedPayIDTransferCreateUserNotFound(UserModel user, CancellationToken cancellationToken, bool isEdit = true)
        {
            var menuText = _interfaceTranslatorService.GetTranslation("M.BlavedPayIDTransferCreateUserNotFound", user.Language);
            var buttonBackText = _interfaceTranslatorService.GetTranslation("B.BackUpdateAsset", user.Language);

            InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(text: buttonBackText, callbackData: $"{CallbackRequestRoute.BlavedPayIDTransferCreateWaitAssetPutPage}:1"),
                },
            });

            return await SendMessageAsync(user, menuText, inlineKeyboard, isEdit, cancellationToken);
        }
        public async Task<Message?> BlavedPayIDTransferCreateNotCorrectID(UserModel user, CancellationToken cancellationToken, bool isEdit = true)
        {
            var menuText = _interfaceTranslatorService.GetTranslation("M.BlavedPayIDTransferCreateNotCorrectID", user.Language);
            var buttonBackText = _interfaceTranslatorService.GetTranslation("B.BackUpdateAsset", user.Language);

            InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(text: buttonBackText, callbackData: $"{CallbackRequestRoute.BlavedPayIDTransferCreateWaitAssetPutPage}:1"),
                },
            });

            return await SendMessageAsync(user, menuText, inlineKeyboard, isEdit, cancellationToken);
        }

        public async Task<Message?> BlavedPayIDTransferConfirm(UserModel user, CancellationToken cancellationToken, bool isEdit = true)
        {
            var menuText = _interfaceTranslatorService.GetTranslation("M.BlavedPayIDTransferConfirm", user.Language);
            var buttonConfirmText = _interfaceTranslatorService.GetTranslation("B.Confirm", user.Language);
            var buttonCancelText = _interfaceTranslatorService.GetTranslation("B.Cancel", user.Language);
            var buttonBackText = _interfaceTranslatorService.GetTranslation("B.BackUpdateAmount", user.Language);

            var asset = user.MessagesBlavedPayIDModel.Asset;
            decimal coinPrice = await _infoService.GetCoinPriceUSDT(asset);
            decimal coinBalance = user.BalanceModel.GetBalance(asset);

            var assetUrl = _appConfig.AssetConfiguration.CoinUrl[asset];

            var blavedPayIDMoreInfoUrl = _appConfig.UrlConfiguration.MediaUrl.BlavedPayIDMoreInfoByLanguage[user.Language];

            menuText = string.Format(menuText,
                asset, assetUrl,
                user.MessagesBlavedPayIDModel.ToUserId,
                user.MessagesBlavedPayIDModel.Amount.AmountCut(), asset, user.MessagesBlavedPayIDModel.Amount.AmountToUSD(coinPrice).AmountCutUSD(),
                coinBalance.AmountCut(), asset, coinBalance.AmountToUSD(coinPrice).AmountCutUSD(), blavedPayIDMoreInfoUrl);

            InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                 new[]
                 {
                     InlineKeyboardButton.WithCallbackData(text: buttonConfirmText, callbackData: CallbackRequestRoute.BlavedPayIDTransferConfirm),
                     InlineKeyboardButton.WithCallbackData(text: buttonCancelText, callbackData: CallbackRequestRoute.BlavedPay),
                 },
                 new[]
                 {
                     InlineKeyboardButton.WithCallbackData(text: buttonBackText, callbackData: CallbackRequestRoute.BlavedPayIDTransferCreateWaitAmount),
                 },
            });

            return await SendMessageAsync(user, menuText, inlineKeyboard, isEdit, cancellationToken);
        }
        public async Task BlavedPayIDTransferCompleted(UserModel user, BlavedPayIDTransferModel blavedPayIDTransfer, CancellationToken cancellationToken, bool isEdit = true)
        {
            var menuText = _interfaceTranslatorService.GetTranslation("M.BlavedPayIDTransferCompleted", user.Language);
            var buttonBackText = _interfaceTranslatorService.GetTranslation("B.BackMainMenu", user.Language);

            decimal assetPrice = await _infoService.GetCoinPriceUSDT(blavedPayIDTransfer.Asset);

            var assetUrl = _appConfig.AssetConfiguration.CoinUrl[blavedPayIDTransfer.Asset];

            menuText = string.Format(menuText,
                blavedPayIDTransfer.Asset, assetUrl,
                blavedPayIDTransfer.ToUserId,
                blavedPayIDTransfer.Amount.AmountCut(), blavedPayIDTransfer.Asset, blavedPayIDTransfer.Amount.AmountToUSD(assetPrice).AmountCutUSD());


            InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(text: buttonBackText, callbackData: CallbackRequestRoute.Main),
                },
            });

            await SendMessageAsync(user, menuText, inlineKeyboard, isEdit, cancellationToken);
        }

        public async Task<Message?> BlavedPayIDTransferAlert(UserModel user, BlavedPayIDTransferModel blavedPayIDTransfer, CancellationToken cancellationToken, bool isEdit = true)
        {
            var menuText = _interfaceTranslatorService.GetTranslation("M.BlavedPayIDTransferAlert", user.Language);
            var disableNotificationsBlavedPayText = _interfaceTranslatorService.GetTranslation("B.BlavedPayNotificationsDisable", user.Language);
            var buttonBackText = _interfaceTranslatorService.GetTranslation("B.BackMainMenu", user.Language);

            decimal coinPrice = await _infoService.GetCoinPriceUSDT(blavedPayIDTransfer.Asset);

            var assetUrl = _appConfig.AssetConfiguration.CoinUrl[blavedPayIDTransfer.Asset];

            menuText = string.Format(menuText,
               blavedPayIDTransfer.Asset, assetUrl,
               blavedPayIDTransfer.UserId,
               blavedPayIDTransfer.Amount.AmountCut(), blavedPayIDTransfer.Asset, blavedPayIDTransfer.Amount.AmountToUSD(coinPrice).AmountCutUSD());

            InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                   InlineKeyboardButton.WithCallbackData(text: disableNotificationsBlavedPayText, callbackData: CallbackRequestRoute.BlavedPayNotificationsDisableBlavedPay),
                },
                new[]
                {
                   InlineKeyboardButton.WithCallbackData(text: buttonBackText, callbackData: CallbackRequestRoute.Main),
                }
            });

            return await SendMessageAsync(user, menuText, inlineKeyboard, isEdit, cancellationToken);

        }


        public async Task BlavedPayNotificationDisableAnswer(UserModel user, string callbackQueryId, CancellationToken cancellationToken)
        {
            var menuText = _interfaceTranslatorService.GetTranslation("M.BlavedPayNotificationDisableAnswer", user.Language);

            await SendMessageAnswerAsync(menuText, callbackQueryId, cancellationToken);
        }

        #region TransferHistory

        public async Task<Message?> BlavedPayIDTransferHistoryList(UserModel user, List<BlavedPayIDTransferModel> history, CancellationToken cancellationToken, int currentPage = 1, bool isEdit = true)
        {
            var menuText = _interfaceTranslatorService.GetTranslation("M.BlavedPayIDTransferHistoryList", user.Language);
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
                    return new[] { InlineKeyboardButton.WithCallbackData($"{x.Amount.AmountCut()} {x.Asset} (${x.Amount.AmountToUSD(assetPrice).AmountCutUSD()})", $"{CallbackRequestRoute.BlavedPayIDTransferHistoryViewPutNum}:{x.Id}") };
                }))).ToList();

            if (currentPage > 1 || endIndex < history.Count)
            {
                var pageNavigationRow = new List<InlineKeyboardButton>();
                if (currentPage > 1)
                {
                    pageNavigationRow.Add(InlineKeyboardButton.WithCallbackData(buttonBackPageText, $"{CallbackRequestRoute.BlavedPayIDTransferHistoryListPutPage}:{currentPage - 1}"));
                }
                if (endIndex < history.Count)
                {
                    pageNavigationRow.Add(InlineKeyboardButton.WithCallbackData(buttonNextPageText, $"{CallbackRequestRoute.BlavedPayIDTransferHistoryListPutPage}:{currentPage + 1}"));
                }
                buttons.Add(pageNavigationRow.ToArray());
            }
            
            buttons.Add(new[] { InlineKeyboardButton.WithCallbackData(buttonBackText, CallbackRequestRoute.BlavedPayID) });
            

            InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(buttons);

            return await SendMessageAsync(user, menuText, inlineKeyboard, isEdit, cancellationToken);
        }
        public async Task<Message?> BlavedPayIDTransferHistoryView(UserModel user, BlavedPayIDTransferModel blavedPayIDTransfer, CancellationToken cancellationToken, bool isEdit = true)
        {
            var menuText = _interfaceTranslatorService.GetTranslation("M.BlavedPayIDTransferHistoryView", user.Language);
            var buttonBackText = _interfaceTranslatorService.GetTranslation("B.Back", user.Language);

            var asset = blavedPayIDTransfer.Asset;
            decimal coinPrice = await _infoService.GetCoinPriceUSDT(asset);

            var assetUrl = _appConfig.AssetConfiguration.CoinUrl[asset];

            menuText = string.Format(menuText,
                asset, assetUrl,
                blavedPayIDTransfer.ToUserId,
                blavedPayIDTransfer.Amount.AmountCut(), asset, blavedPayIDTransfer.Amount.AmountToUSD(coinPrice).AmountCutUSD(),
                blavedPayIDTransfer.CreatedAt.ToShortDateString(),
                blavedPayIDTransfer.CreatedAt.ToShortTimeString());

            InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(text: buttonBackText, callbackData:$"{CallbackRequestRoute.BlavedPayIDTransferHistoryListPutPage}:1"),

                },
            });

            return await SendMessageAsync(user, menuText, inlineKeyboard, isEdit, cancellationToken);
        }
        public async Task BlavedPayIDTransferHistoryDontHaveAnswer(UserModel user, string callbackQueryId, CancellationToken cancellationToken)
        {
            var menuText = _interfaceTranslatorService.GetTranslation("M.BlavedPayIDTransferHistoryDontHaveAnswer", user.Language);

            await SendMessageAnswerAsync(menuText, callbackQueryId, cancellationToken);
        }

        #endregion
    }
}
