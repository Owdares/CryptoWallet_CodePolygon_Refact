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
    public class ExchangeMenu : BotMenuBase, IExchangeMenu
    {
        private readonly ITelegramBotClient _botClient;
        private readonly IInfoService _infoService;
        private readonly InterfaceTranslatorService _interfaceTranslatorService;
        private readonly AppConfig _appConfig;
        public ExchangeMenu(ITelegramBotClient botClient, IInfoService infoService, InterfaceTranslatorService interfaceTranslatorService,
            IOptions<AppConfig> appConfig, ILogger<ExchangeMenu> logger) : base(botClient, logger)
        {
            _appConfig = appConfig.Value;
            _botClient = botClient;
            _infoService = infoService;
            _interfaceTranslatorService = interfaceTranslatorService;
        }

        public async Task<Message?> ExchangeCreateWaitFromAsset(UserModel user, CancellationToken cancellationToken, int currentPage = 1, bool isEdit = true)
        {
            var menuText = _interfaceTranslatorService.GetTranslation("M.ExchangeCreateWaitFromAsset", user.Language);
            var buttonNextPageText = _interfaceTranslatorService.GetTranslation("B.NextPage", user.Language);
            var buttonBackPageText = _interfaceTranslatorService.GetTranslation("B.BackPage", user.Language);
            var buttonBackText = _interfaceTranslatorService.GetTranslation("B.Back", user.Language);

            var AssetList = _appConfig.AssetConfiguration.CoinList;

            var buttons = AssetList.Select(name => InlineKeyboardButton.WithCallbackData(text: name, callbackData: $"{CallbackRequestRoute.ExchangeCreateWaitToAssetPutFromAssetPage}:{name}_{currentPage}")).ToArray();

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
                    pageNavigationRow.Add(InlineKeyboardButton.WithCallbackData(buttonBackPageText, $"{CallbackRequestRoute.ExchangeCreateWaitFromAssetPutPage}:{currentPage - 1}"));
                }
                if (endIndex < AssetList.Count)
                {
                    pageNavigationRow.Add(InlineKeyboardButton.WithCallbackData(buttonNextPageText, $"{CallbackRequestRoute.ExchangeCreateWaitFromAssetPutPage}:{currentPage + 1}"));
                }
                buttonRows.Add(pageNavigationRow.ToArray());
            }

            buttonRows.Add(new[] { InlineKeyboardButton.WithCallbackData(text: buttonBackText, callbackData: CallbackRequestRoute.Exchange) });

            var inlineKeyboard = new InlineKeyboardMarkup(buttonRows);

            return await SendMessageAsync(user, menuText, inlineKeyboard, isEdit, cancellationToken);
        }
        public async Task<Message?> ExchangeCreateWaitToAsset(UserModel user, string fromAsset, List<InfoForExchangeModel> infoForExchangeUnionList, CancellationToken cancellationToken, int currentPage = 1, bool isEdit = true)
        {
            var menuText = _interfaceTranslatorService.GetTranslation("M.ExchangeCreateWaitToAsset", user.Language);
            var buttonNextPageText = _interfaceTranslatorService.GetTranslation("B.NextPage", user.Language);
            var buttonBackPageText = _interfaceTranslatorService.GetTranslation("B.BackPage", user.Language);
            var buttonBackText = _interfaceTranslatorService.GetTranslation("B.BackUpdateAsset", user.Language);

            var assetUrl = _appConfig.AssetConfiguration.CoinUrl[fromAsset];

            menuText = string.Format(menuText,
                fromAsset, assetUrl);

            var buttons = infoForExchangeUnionList.Select(x => InlineKeyboardButton.WithCallbackData(text: x.ToAsset, callbackData: $"{CallbackRequestRoute.ExchangeCreateWaitAmountPutFromAssetToAsset}:{fromAsset}_{x.ToAsset}_{currentPage}")).ToArray();

            int buttonsPerRow = 3;
            int buttonsPerColum = 4;

            int buttonsPerPage = buttonsPerRow * buttonsPerColum;

            int startIndex = (currentPage - 1) * buttonsPerPage;
            int endIndex = Math.Min(startIndex + buttonsPerPage, infoForExchangeUnionList.Count);

            var buttonRows = buttons
                .Skip(startIndex)
                .Take(buttonsPerPage)
                .Select((button, index) => new { Button = button, Index = index, RowIndex = index / buttonsPerRow, ColumnIndex = index % buttonsPerRow })
                .GroupBy(x => x.RowIndex)
                .Take(buttonsPerColum)
                .Select(group => group.OrderBy(x => x.ColumnIndex).Select(x => x.Button).ToArray())
                .ToList();

            if (currentPage > 1 || endIndex < infoForExchangeUnionList.Count)
            {
                var pageNavigationRow = new List<InlineKeyboardButton>();
                if (currentPage > 1)
                {
                    pageNavigationRow.Add(InlineKeyboardButton.WithCallbackData(buttonBackPageText, $"{CallbackRequestRoute.ExchangeCreateWaitToAssetPutFromAssetPage}:{fromAsset}_{currentPage - 1}"));
                }
                if (endIndex < infoForExchangeUnionList.Count)
                {
                    pageNavigationRow.Add(InlineKeyboardButton.WithCallbackData(buttonNextPageText, $"{CallbackRequestRoute.ExchangeCreateWaitToAssetPutFromAssetPage}:{fromAsset}_{currentPage + 1}"));
                }
                buttonRows.Add(pageNavigationRow.ToArray());
            }

            buttonRows.Add(new[] { InlineKeyboardButton.WithCallbackData(buttonBackText, $"{CallbackRequestRoute.ExchangeCreateWaitFromAssetPutPage}:1") });

            var inlineKeyboard = new InlineKeyboardMarkup(buttonRows);

            return await SendMessageAsync(user, menuText, inlineKeyboard, isEdit, cancellationToken);
        }
        public async Task<Message?> ExchangeCreateWaitAmount(UserModel user, InfoForExchangeModel exchangeCoinInfoModel, CancellationToken cancellationToken, bool isEdit = true)
        {
            var menuText = _interfaceTranslatorService.GetTranslation("M.ExchangeCreateWaitAmount", user.Language);
            var buttonBackText = _interfaceTranslatorService.GetTranslation("B.BackUpdateAsset", user.Language);
            var buttonMinAmountText = _interfaceTranslatorService.GetTranslation("B.MinAmount", user.Language);
            var buttonMaxAmountText = _interfaceTranslatorService.GetTranslation("B.MaxAmount", user.Language);

            var fromAsset = user.MessagesExchangeModel.FromAsset;
            var toAsset = user.MessagesExchangeModel.ToAsset;

            var userBalance = user.BalanceModel.GetBalance(fromAsset);
            decimal coinPrice = await _infoService.GetCoinPriceUSDT(fromAsset);

            var convertFee = exchangeCoinInfoModel.ExchangeInternalFee;
            var maxSumConvert = userBalance - convertFee;
            var minSumConvert = exchangeCoinInfoModel.MinAmount;

            var fromAssetUrl = _appConfig.AssetConfiguration.CoinUrl[fromAsset];
            var toAssetUrl = _appConfig.AssetConfiguration.CoinUrl[toAsset];

            menuText = string.Format(menuText,
                fromAsset, fromAssetUrl, toAsset, toAssetUrl,
                minSumConvert.AmountCut(), fromAsset, minSumConvert.AmountToUSD(coinPrice).AmountCutUSD(),
                maxSumConvert.AmountCut(), fromAsset, maxSumConvert.AmountToUSD(coinPrice).AmountCutUSD(),
                convertFee.AmountCut(), fromAsset, convertFee.AmountToUSD(coinPrice).AmountCutUSD(),
                userBalance.AmountCut(), fromAsset, userBalance.AmountToUSD(coinPrice).AmountCutUSD());

            buttonMinAmountText = string.Format(buttonMinAmountText, minSumConvert.AmountCut(), minSumConvert.AmountToUSD(coinPrice).AmountCutUSD());
            buttonMaxAmountText = string.Format(buttonMaxAmountText, maxSumConvert.AmountCut(), maxSumConvert.AmountToUSD(coinPrice).AmountCutUSD());

            InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(text: buttonMinAmountText, callbackData: $"{CallbackRequestRoute.ExchangeCreatePutAmount}:{minSumConvert}"),
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(text: buttonMaxAmountText, callbackData: $"{CallbackRequestRoute.ExchangeCreatePutAmount}:{maxSumConvert}"),
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(text: buttonBackText,callbackData:$"{CallbackRequestRoute.ExchangeCreateWaitToAssetPutFromAssetPage}:{exchangeCoinInfoModel.FromAsset}_1")
                },

            });

            return await SendMessageAsync(user, menuText, inlineKeyboard, isEdit, cancellationToken);
        }
        public async Task<Message?> Exchange(UserModel user, CancellationToken cancellationToken, bool isEdit = true)
        {
            var menuText = _interfaceTranslatorService.GetTranslation("M.Exchange", user.Language);
            var buttonConvertText = _interfaceTranslatorService.GetTranslation("B.ExchangeCreate", user.Language);
            var historyMenuText = _interfaceTranslatorService.GetTranslation("B.ExchangeHistoryList", user.Language);
            var buttonBackText = _interfaceTranslatorService.GetTranslation("B.Back", user.Language);


            InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(text:buttonConvertText, callbackData: $"{CallbackRequestRoute.ExchangeCreateWaitFromAssetPutPage}:1")
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(text:historyMenuText, callbackData: $"{CallbackRequestRoute.ExchangeHistoryListPutPage}:1")
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(text:buttonBackText,callbackData: CallbackRequestRoute.Main)
                },

            });

            return await SendMessageAsync(user, menuText, inlineKeyboard, isEdit, cancellationToken);
        }
        public async Task<Message?> ExchangeCreateNotCorrectAmount(UserModel user, InfoForExchangeModel exchangeCoinInfoModel, CancellationToken cancellationToken, bool isEdit = true)
        {
            var menuText = _interfaceTranslatorService.GetTranslation("M.ExchangeCreateNotCorrectAmount", user.Language);
            var buttonBackText = _interfaceTranslatorService.GetTranslation("B.BackUpdateAsset", user.Language);

            var buttonMinAmountText = _interfaceTranslatorService.GetTranslation("B.MinAmount", user.Language);
            var buttonMaxAmountText = _interfaceTranslatorService.GetTranslation("B.MaxAmount", user.Language);

            var fromAsset = user.MessagesExchangeModel.FromAsset;
            var toAsset = user.MessagesExchangeModel.ToAsset;

            var userBalance = user.BalanceModel.GetBalance(fromAsset);
            decimal coinPrice = await _infoService.GetCoinPriceUSDT(fromAsset);

            var convertFee = exchangeCoinInfoModel.ExchangeInternalFee;
            var maxSumConvert = userBalance - convertFee;
            var minSumConvert = exchangeCoinInfoModel.MinAmount;

            var fromAssetUrl = _appConfig.AssetConfiguration.CoinUrl[fromAsset];
            var toAssetUrl = _appConfig.AssetConfiguration.CoinUrl[toAsset];

            menuText = string.Format(menuText,
                fromAsset, fromAssetUrl, toAsset, toAssetUrl,
                minSumConvert.AmountCut(), fromAsset, minSumConvert.AmountToUSD(coinPrice).AmountCutUSD(),
                maxSumConvert.AmountCut(), fromAsset, maxSumConvert.AmountToUSD(coinPrice).AmountCutUSD(),
                convertFee.AmountCut(), fromAsset, convertFee.AmountToUSD(coinPrice).AmountCutUSD(),
                userBalance.AmountCut(), fromAsset, userBalance.AmountToUSD(coinPrice).AmountCutUSD());

            buttonMinAmountText = string.Format(buttonMinAmountText, minSumConvert.AmountCut(), minSumConvert.AmountToUSD(coinPrice).AmountCutUSD());
            buttonMaxAmountText = string.Format(buttonMaxAmountText, maxSumConvert.AmountCut(), maxSumConvert.AmountToUSD(coinPrice).AmountCutUSD());

            InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(text: buttonMinAmountText, callbackData: $"{CallbackRequestRoute.ExchangeCreatePutAmount}:{minSumConvert}"),
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(text: buttonMaxAmountText, callbackData: $"{CallbackRequestRoute.ExchangeCreatePutAmount}:{maxSumConvert}"),
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(text: buttonBackText,callbackData:$"{CallbackRequestRoute.ExchangeCreateWaitToAssetPutFromAssetPage}:{exchangeCoinInfoModel.FromAsset}_1")
                },

            });

            return await SendMessageAsync(user, menuText, inlineKeyboard, isEdit, cancellationToken);
        }

        public async Task<Message?> ExchangeConfirm(UserModel user, ExchangeQuoteModel exchangeQuoteModel, CancellationToken cancellationToken, bool isEdit = true)
        {
            var menuText = _interfaceTranslatorService.GetTranslation("M.ExchangeConfirm", user.Language);
            var buttonCompleteText = _interfaceTranslatorService.GetTranslation("B.Confirm", user.Language);
            var buttonCancelText = _interfaceTranslatorService.GetTranslation("B.Cancel", user.Language);
            var buttonChangeSumText = _interfaceTranslatorService.GetTranslation("B.BackUpdateAmount", user.Language);

            string fromAsset = exchangeQuoteModel.FromAsset;
            string toAsset = exchangeQuoteModel.ToAsset;
            decimal fromAmount = exchangeQuoteModel.FromAmount;
            decimal toAmount = exchangeQuoteModel.ToAmount;
            decimal converFee = exchangeQuoteModel.Fee;

            decimal fromCoinPrice = await _infoService.GetCoinPriceUSDT(fromAsset);
            decimal toCoinPrice = await _infoService.GetCoinPriceUSDT(toAsset);

            var fromAssetUrl = _appConfig.AssetConfiguration.CoinUrl[fromAsset];
            var toAssetUrl = _appConfig.AssetConfiguration.CoinUrl[toAsset];

            var exchangeMoreInfoUrl = _appConfig.UrlConfiguration.MediaUrl.ExchangeMoreInfoByLanguage[user.Language];

            var fromAmountWithFee = (fromAmount + converFee).AmountRound();

            menuText = string.Format(menuText,
                 fromAsset, fromAssetUrl, toAsset, toAssetUrl,
                fromAmountWithFee.AmountCut(), fromAsset, fromAmountWithFee.AmountToUSD(fromCoinPrice).AmountCutUSD(),
                converFee.AmountCut(), fromAsset, converFee.AmountToUSD(fromCoinPrice).AmountCutUSD(),
                toAmount.AmountCut(), toAsset, toAmount.AmountToUSD(toCoinPrice).AmountCutUSD(),
                exchangeMoreInfoUrl);

            InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                 new[]
                 {
                     InlineKeyboardButton.WithCallbackData(text: buttonCompleteText, callbackData:$"{CallbackRequestRoute.ExchangeConvertConfirmPutQuoteId}:{exchangeQuoteModel.QuoteId}"),
                     InlineKeyboardButton.WithCallbackData(text: buttonCancelText, callbackData: CallbackRequestRoute.Exchange),
                 },
                 new[]
                 {
                     InlineKeyboardButton.WithCallbackData(text: buttonChangeSumText, callbackData:$"{CallbackRequestRoute.ExchangeCreateWaitAmountPutFromAssetToAsset}:{fromAsset}_{toAsset}"),
                 },
            });

            return await SendMessageAsync(user, menuText, inlineKeyboard, isEdit, cancellationToken);
        }
        public async Task<Message?> ExchangeCompleted(UserModel user, ExchangeModel exchangeModel, CancellationToken cancellationToken, bool isEdit = true)
        {
            var menuText = _interfaceTranslatorService.GetTranslation("M.ExchangeCompleted", user.Language);
            var buttonBackText = _interfaceTranslatorService.GetTranslation("B.BackMainMenu", user.Language);

            decimal fromCoinPrice = await _infoService.GetCoinPriceUSDT(exchangeModel.FromAsset);
            decimal toCoinPrice = await _infoService.GetCoinPriceUSDT(exchangeModel.ToAsset);

            var fromAssetUrl = _appConfig.AssetConfiguration.CoinUrl[exchangeModel.FromAsset];
            var toAssetUrl = _appConfig.AssetConfiguration.CoinUrl[exchangeModel.ToAsset];

            var amountWithFee = (exchangeModel.FromAmount + exchangeModel.Fee).AmountRound();

            menuText = string.Format(menuText,
                exchangeModel.FromAsset, fromAssetUrl,  exchangeModel.ToAsset, toAssetUrl,
                amountWithFee.AmountCut(), exchangeModel.FromAsset, amountWithFee.AmountToUSD(fromCoinPrice).AmountCutUSD(),
                exchangeModel.Fee.AmountCut(), exchangeModel.FromAsset, exchangeModel.Fee.AmountToUSD(fromCoinPrice).AmountCutUSD(),
                exchangeModel.ToAmount.AmountCut(), exchangeModel.ToAsset, exchangeModel.ToAmount.AmountToUSD(toCoinPrice).AmountCutUSD());


            InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                     InlineKeyboardButton.WithCallbackData(text: buttonBackText, callbackData:$"{CallbackRequestRoute.Main}"),
                },
            });

            return await SendMessageAsync(user, menuText, inlineKeyboard, isEdit, cancellationToken);
        }

        public async Task ExchangeCreateErrorAnswer(UserModel user, string callbackQueryId, CancellationToken cancellationToken)
        {
            var menuText = _interfaceTranslatorService.GetTranslation("M.ExchangeError", user.Language);

            await SendMessageAnswerAsync(menuText, callbackQueryId, cancellationToken);
        }
        public async Task ExchangeCreateErrorOverdueConfirmAnswer(UserModel user, string callbackQueryId, CancellationToken cancellationToken)
        {
            var menuText = _interfaceTranslatorService.GetTranslation("M.ExchangeCreateErrorOverdueConfirmAnswer", user.Language);

            await SendMessageAnswerAsync(menuText, callbackQueryId, cancellationToken);
        }
        public async Task<Message?> ExchangeCreateErrorQuote(UserModel user, CancellationToken cancellationToken, bool isEdit = true)
        {
            var menuText = _interfaceTranslatorService.GetTranslation("M.ExchangeCreateErrorQuote", user.Language);
            var buttonBackText = _interfaceTranslatorService.GetTranslation("B.Back", user.Language);

            InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(text:buttonBackText,callbackData: CallbackRequestRoute.Exchange)
                }
            });

            return await SendMessageAsync(user, menuText, inlineKeyboard, isEdit, cancellationToken);
        }
        public async Task ExchangeCreateCoinDontHavePairAnswer(UserModel user, string callbackQueryId, CancellationToken cancellationToken)
        {
            var menuText = _interfaceTranslatorService.GetTranslation("M.ExchangeCreateCoinDontHavePairAnswer", user.Language);

            await SendMessageAnswerAsync(menuText, callbackQueryId, cancellationToken);
        }
        public async Task ExchangeCreateInsufficientBalanceAnswer(UserModel user, InfoForExchangeModel infoForExchange, string callbackQueryId, CancellationToken cancellationToken)
        {
            var menuText = _interfaceTranslatorService.GetTranslation("M.ExchangeCreateInsufficientBalanceAnswer", user.Language);

            string fromAsset = infoForExchange.FromAsset;

            var userBalance = user.BalanceModel.GetBalance(fromAsset);
            decimal coinPrice = await _infoService.GetCoinPriceUSDT(fromAsset);

            var convertFee = infoForExchange.ExchangeInternalFee;
            var minSumConvert = infoForExchange.MinAmount;

            menuText = string.Format(menuText,
                minSumConvert.AmountCut(), fromAsset, minSumConvert.AmountToUSD(coinPrice).AmountCutUSD(),
                convertFee.AmountCut(), fromAsset, convertFee.AmountToUSD(coinPrice).AmountCutUSD(),
                userBalance.AmountCut(), fromAsset, userBalance.AmountToUSD(coinPrice).AmountCutUSD());

            await SendMessageAnswerAsync(menuText, callbackQueryId, cancellationToken);
        }


        #region ExchangeHistory

        public async Task<Message?> ExchangeHistoryList(UserModel user, List<ExchangeModel> history, CancellationToken cancellationToken, int currentPage = 1, bool isEdit = true)
        {
            var menuText = _interfaceTranslatorService.GetTranslation("M.ExchangeHistoryList", user.Language);
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
                    var assetPrice = await _infoService.GetCoinPriceUSDT(x.FromAsset);
                    return new[] { InlineKeyboardButton.WithCallbackData($"{(x.FromAmount + x.Fee).AmountCut()} {x.FromAsset}/{x.ToAsset} (${(x.FromAmount + x.Fee).AmountToUSD(assetPrice).AmountCutUSD()})", $"{CallbackRequestRoute.ExchangeHistoryViewPutNum}:{x.Id}") };
                }))).ToList();

            if (currentPage > 1 || endIndex < history.Count)
            {
                var pageNavigationRow = new List<InlineKeyboardButton>();
                if (currentPage > 1)
                {
                    pageNavigationRow.Add(InlineKeyboardButton.WithCallbackData(buttonBackPageText, $"{CallbackRequestRoute.ExchangeHistoryListPutPage}:{currentPage - 1}"));
                }
                if (endIndex < history.Count)
                {
                    pageNavigationRow.Add(InlineKeyboardButton.WithCallbackData(buttonNextPageText, $"{CallbackRequestRoute.ExchangeHistoryListPutPage}:{currentPage + 1}"));
                }
                buttons.Add(pageNavigationRow.ToArray());
            }
            
            buttons.Add(new[] { InlineKeyboardButton.WithCallbackData(buttonBackText, CallbackRequestRoute.Exchange) });
            

            InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(buttons);

            return await SendMessageAsync(user, menuText, inlineKeyboard, isEdit, cancellationToken);
        }
        public async Task<Message?> ExchangeHistoryView(UserModel user, ExchangeModel exchangeModel, CancellationToken cancellationToken, bool isEdit = true)
        {
            var menuText = _interfaceTranslatorService.GetTranslation("M.ExchangeHistoryView", user.Language);
            var buttonBackText = _interfaceTranslatorService.GetTranslation("B.Back", user.Language);

            decimal fromCoinPrice = await _infoService.GetCoinPriceUSDT(exchangeModel.FromAsset);
            decimal toCoinPrice = await _infoService.GetCoinPriceUSDT(exchangeModel.ToAsset);

            var fromAssetUrl = _appConfig.AssetConfiguration.CoinUrl[exchangeModel.FromAsset];
            var toAssetUrl = _appConfig.AssetConfiguration.CoinUrl[exchangeModel.ToAsset];

            var amountWithFee = (exchangeModel.FromAmount + exchangeModel.Fee).AmountRound();

            menuText = string.Format(menuText,
                exchangeModel.FromAsset, fromAssetUrl, exchangeModel.ToAsset, toAssetUrl,
                amountWithFee.AmountCut(), exchangeModel.FromAsset, amountWithFee.AmountToUSD(fromCoinPrice).AmountCutUSD(),
                exchangeModel.Fee.AmountCut(), exchangeModel.FromAsset, exchangeModel.Fee.AmountToUSD(fromCoinPrice).AmountCutUSD(),
                exchangeModel.ToAmount.AmountCut(), exchangeModel.ToAsset, exchangeModel.ToAmount.AmountToUSD(toCoinPrice).AmountCutUSD(),
                exchangeModel.CreatedAt.ToShortDateString(),
                exchangeModel.CreatedAt.ToShortTimeString());

            InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(text: buttonBackText, callbackData:$"{CallbackRequestRoute.ExchangeHistoryListPutPage}:1"),

                },
            });

            return await SendMessageAsync(user, menuText, inlineKeyboard, isEdit, cancellationToken);
        }
        public async Task ExchangeHistoryDontHaveAnswer(UserModel user, string callbackQueryId, CancellationToken cancellationToken)
        {
            var menuText = _interfaceTranslatorService.GetTranslation("M.ExchangeHistoryDontHaveAnswer", user.Language);

            await SendMessageAnswerAsync(menuText, callbackQueryId, cancellationToken);
        }

        #endregion
    }
}
