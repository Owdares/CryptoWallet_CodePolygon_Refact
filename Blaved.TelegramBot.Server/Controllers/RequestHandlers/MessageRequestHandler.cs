using Telegram.Bot.Types;
using System.Reflection;
using Blaved.Core.Utility;
using Microsoft.Extensions.Options;
using Serilog.Context;
using Bleved.TelegramBot.Server.Controllers.Routes;
using Bleved.TelegramBot.Server.Controllers.Attributes;
using Blaved.Core.Objects.Models;
using Blaved.Core.Interfaces.Services;
using Blaved.Core.Objects.Models.Configurations;
using Blaved.Core.Interfaces;
using Blaved.Core.Objects.Models.Cryptocurrency;

namespace Bleved.TelegramBot.Server.Controllers.RequestHandlers
{
    public class MessageRequestHandler
    {
        private readonly IWalletService _walletService;
        private readonly ICheckService _checkService;
        private readonly IExchangeService _exchangeService;
        private readonly IBlavedPayService _blavedPayService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBotMenu _botMenu;
        private readonly IInfoService _infoService;
        private readonly IUserService _userService;
        private readonly AppConfig _appConfig;
        private readonly ILogger<MessageRequestHandler> _logger;
        public MessageRequestHandler(IUnitOfWork unitOfWork, IBotMenu botMenu, IWalletService walletService,
            ICheckService checkService, IExchangeService exchangeService, IBlavedPayService blavedPayService, IInfoService infoService,
            IUserService userService, IOptions<AppConfig> appConfig, ILogger<MessageRequestHandler> logger)
        {
            _appConfig = appConfig.Value;
            _walletService = walletService;
            _unitOfWork = unitOfWork;
            _botMenu = botMenu;
            _checkService = checkService;
            _exchangeService = exchangeService;
            _blavedPayService = blavedPayService;
            _infoService = infoService;
            _userService = userService;
            _logger = logger;
        }

        public async Task MessageHandlerAsync(Message message, CancellationToken cancellationToken)
        {
            LogContext.PushProperty("MessageId", message.MessageId);
            LogContext.PushProperty("UserId", message.Chat.Id);

            var user = await _unitOfWork.UserRepository.GetUser(message.Chat.Id);
            if (user is not null)
            {
                try
                {
                    if (!user.IsBanned)
                    {
                        await MessageControllerAsync(user, message, cancellationToken);
                    }
                    else
                    {
                        await MessageUserIsBanedAsync(user, message, cancellationToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing request");

                    var messageError = await _botMenu.Main.ServerError(user, cancellationToken, false);
                    await _unitOfWork.UserRepository.UpdateUserMessageId(user.UserId, messageError!.MessageId);
                    await _unitOfWork.SaveChanges();
                }
            }
            else
            {
                await MessageUserNotFoundAsync(message, cancellationToken);
            }
        }
        private async Task MessageControllerAsync(UserModel user, Message message, CancellationToken cancellationToken)
        {
            if (message.Text is not { } messageText)
                return;

            LogContext.PushProperty("Data", messageText);

            var methods = GetType().GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(method => method.GetCustomAttributes(typeof(BotRequestRouteAttribute), false).Length > 0)
                .Select(method => new
                {
                    Key = ((BotRequestRouteAttribute)method.GetCustomAttributes(typeof(BotRequestRouteAttribute), false)[0]).Key,
                    Method = (Func<UserModel, Message, CancellationToken, Task>)Delegate.CreateDelegate(typeof(Func<UserModel, Message, CancellationToken, Task>), this, method)
                })
                .ToDictionary(entry => entry.Key, entry => entry.Method);

            var handler = methods.SingleOrDefault(x => user.WhereMenu.StartsWith(x.Key));

            if (handler.Value != null)
            {
                LogContext.PushProperty("MethodProcessor", handler.Value.Method.Name);
                await handler.Value(user, message, cancellationToken);
            }

        }

        private async Task MessageUserIsBanedAsync(UserModel user, Message message, CancellationToken cancellationToken)
        {
            var messageAnswer = await _botMenu.Main.UserHasBanned(user, cancellationToken, false);
            await _unitOfWork.UserRepository.UpdateUserMessageId(user.UserId, messageAnswer!.MessageId);
            await _unitOfWork.SaveChanges();
        }
        private async Task MessageUserNotFoundAsync(Message message, CancellationToken cancellationToken)
        {
            if (message.Text is not { } messageText)
                return;

            if (messageText.StartsWith($"/start {_appConfig.UrlConfiguration.ReferralUrlClip}"))
            {
                string[] parts = messageText.Split(_appConfig.UrlConfiguration.ReferralUrlClip);

                if (!string.IsNullOrEmpty(parts[1]) && long.TryParse(parts[1], out long whoseReferalIdParse))
                {
                    var whoseReferalUser = await _unitOfWork.UserRepository.GetUser(whoseReferalIdParse);

                    await _userService.UserRegistration(message.Chat.Id, message.Chat.FirstName, message.Chat.Username, whoseReferalUser?.UserId, message.From?.LanguageCode);
                    await _unitOfWork.SaveChanges();

                    var user = await _unitOfWork.UserRepository.GetUser(message.Chat.Id);

                    var messageAnswer = await _botMenu.Main.Main(user!, cancellationToken, false);
                    await _unitOfWork.UserRepository.UpdateUserMessageId(user!.UserId, messageAnswer!.MessageId);
                    await _unitOfWork.SaveChanges();
                }
            }

            else if (messageText.StartsWith($"/start {_appConfig.UrlConfiguration.CheckUrlClip}"))
            {
                string[] parts = messageText.Split(_appConfig.UrlConfiguration.CheckUrlClip);

                if (!string.IsNullOrEmpty(parts[1]))
                {
                    var check = await _unitOfWork.CheckRepository.GetCheck(parts[1]);

                    if (check != null)
                    {
                        if (check.Count > 0)
                        {
                            await _userService.UserRegistration(message.Chat.Id, message.Chat.FirstName, message.Chat.Username, check.UserId, message.From?.LanguageCode);
                        }
                        else
                        {
                            await _userService.UserRegistration(message.Chat.Id, message.Chat.FirstName, message.Chat.Username, null, message.From?.LanguageCode);
                        }
                        await _unitOfWork.SaveChanges();

                        var user = await _unitOfWork.UserRepository.GetUser(message.Chat.Id);

                        if (!_appConfig.FunctionConfiguration.FunctionEnable.Check)
                        {
                            await FunctionDisabled(user, cancellationToken);
                            return;
                        }

                        await CheckActivatedProcessor(user!, check, cancellationToken);
                    }
                    else
                    {
                        await _userService.UserRegistration(message.Chat.Id, message.Chat.FirstName, message.Chat.Username, null, message.From?.LanguageCode);
                        await _unitOfWork.SaveChanges();

                        var userd = await _unitOfWork.UserRepository.GetUser(message.Chat.Id);

                        var message1 = await _botMenu.Check.CheckActivationNotFound(userd!, cancellationToken, false);
                        await _unitOfWork.UserRepository.UpdateUserMessageId(userd!.UserId, message1!.MessageId);
                        await _unitOfWork.SaveChanges();
                    }
                }
            }

            else if (messageText.ToLower() == "/start")
            {
                await _userService.UserRegistration(message.Chat.Id, message.Chat.FirstName, message.Chat.Username, null, message.From?.LanguageCode);
                await _unitOfWork.SaveChanges();

                var user = await _unitOfWork.UserRepository.GetUser(message.Chat.Id);
                var messageAnswer = await _botMenu.Main.Main(user!, cancellationToken, false);
                await _unitOfWork.UserRepository.UpdateUserMessageId(user!.UserId, messageAnswer!.MessageId);
                await _unitOfWork.SaveChanges();
            }
        }

        private async Task CheckActivatedProcessor(UserModel user, CheckModel check, CancellationToken cancellationToken)
        {

            var checkActivated = await _unitOfWork.CheckActivatedRepository.GetCheckActivated(user.UserId, check.Url);

            if (checkActivated is not null)
            {
                var message = await _botMenu.Check.CheckActivationAforeUsed(user, cancellationToken, false);
                await _unitOfWork.UserRepository.UpdateUserMessageId(user.UserId, message!.MessageId);
            }
            else
            {
                if (check.Count < 1)
                {
                    var message = await _botMenu.Check.CheckActivationCountNull(user, cancellationToken, false);
                    await _unitOfWork.UserRepository.UpdateUserMessageId(user.UserId, message!.MessageId);
                    await _unitOfWork.SaveChanges();
                    return;
                }
                if (check.Password != null)
                {
                    await _unitOfWork.UserRepository.UpdateUserWhereMenu(user.UserId, $"{MessageRequestRoute.CheckActivatedMessagePasswordPutUrl}:{check.Url}");

                    var message = await _botMenu.Check.CheckActivationWaitPassword(user, check, cancellationToken, false);
                    await _unitOfWork.UserRepository.UpdateUserMessageId(user.UserId, message!.MessageId);
                    await _unitOfWork.SaveChanges();
                    return;
                }

                await _checkService.CheckActivated(user, check);

                var messageActivation = await _botMenu.Check.CheckActivation(user, check, cancellationToken, false);
                await _unitOfWork.UserRepository.UpdateUserMessageId(user.UserId, messageActivation!.MessageId);
            }
            await _unitOfWork.SaveChanges();

        }

        private async Task<bool> WithdrawEnabledStatus(UserModel user, Message message, CancellationToken cancellationToken,
            string? asset = null, string? network = null, AssetModel? assetModel = null)
        {
            if (!_appConfig.FunctionConfiguration.FunctionEnable.Withdraw
                || asset != null && !_appConfig.FunctionConfiguration.AssetEnable.Withdraw[asset]
                || network != null && !_appConfig.FunctionConfiguration.NetworkEnable.Withdraw[network])
            {
                await FunctionDisabled(user, cancellationToken);
                return false;
            }
            return true;
        }

        [BotRequestRoute(MessageRequestRoute.Default)]
        private async Task Message_ProcessorAsync(UserModel user, Message message, CancellationToken cancellationToken)
        {
            if (message.Text is not { } messageText)
                return;

            if (messageText.ToLower() == "/start")
            {
                await _unitOfWork.UserRepository.UpdateUserWhereMenu(user.UserId, MessageRequestRoute.Default);
                await _unitOfWork.SaveChanges();

                var messageAnswer = await _botMenu.Main.Main(user, cancellationToken, false);
                await _unitOfWork.UserRepository.UpdateUserMessageId(user.UserId, messageAnswer!.MessageId);
                await _unitOfWork.SaveChanges();
            }

            if (messageText.StartsWith($"/start {_appConfig.UrlConfiguration.CheckUrlClip}"))
            {
                string[] parts = messageText.Split(_appConfig.UrlConfiguration.CheckUrlClip);

                if (!string.IsNullOrEmpty(parts[1]))
                {
                    if (!_appConfig.FunctionConfiguration.FunctionEnable.Check)
                    {
                        await FunctionDisabled(user, cancellationToken);
                        return;
                    }

                    var check = await _unitOfWork.CheckRepository.GetCheck(parts[1]);
                    if (check != null)
                    {
                        await CheckActivatedProcessor(user, check, cancellationToken);
                    }
                }
            }
        }

        #region Check

        [BotRequestRoute(MessageRequestRoute.CheckUpdateMessagePasswordPutUrl)]
        private async Task CheckUpdateMessagePasswordPutUrl_ProcessorAsync(UserModel user, Message message, CancellationToken cancellationToken)
        {
            string[] parts = user.WhereMenu.Split(':');

            string url = parts[1];
            string password = message.Text ?? "";

            if (!_appConfig.FunctionConfiguration.FunctionEnable.Check)
            {
                await FunctionDisabled(user, cancellationToken);
                return;
            }

            if (ValidateData.ValidateCheckUpdate_Password(password))
            {
                await _unitOfWork.UserRepository.UpdateUserWhereMenu(user.UserId, "Default");
                await _unitOfWork.CheckRepository.UpdateCheckPassword(url, password);
                await _unitOfWork.SaveChanges();
                var messageAnswer = await _botMenu.Check.CheckUpdatePasswordConfirm(user, password, url, cancellationToken, false);
                await _unitOfWork.UserRepository.UpdateUserMessageId(user.UserId, messageAnswer!.MessageId);
            }
            else
            {
                var messageAnswer = await _botMenu.Check.CheckUpdatePasswordNotCorrect(user, url, cancellationToken, false);
                await _unitOfWork.UserRepository.UpdateUserMessageId(user.UserId, messageAnswer!.MessageId);
            }
            await _unitOfWork.SaveChanges();
        }

        [BotRequestRoute(MessageRequestRoute.CheckActivatedMessagePasswordPutUrl)]
        private async Task CheckActivatedMessagePasswordPutUrl_ProcessorAsync(UserModel user, Message message, CancellationToken cancellationToken)
        {
            string[] parts = user.WhereMenu.Split(':');

            string url = parts[1];
            string password = message.Text ?? "";

            if (!_appConfig.FunctionConfiguration.FunctionEnable.Check)
            {
                await FunctionDisabled(user, cancellationToken);
                return;
            }

            var checkModel = await _unitOfWork.CheckRepository.GetCheck(url);

            if (checkModel != null && ValidateData.ValidateCheckActivated_Password(checkModel, password))
            {
                await _checkService.CheckActivated(user, checkModel);

                var messageAnswer = await _botMenu.Check.CheckActivation(user, checkModel, cancellationToken, false);
                await _unitOfWork.UserRepository.UpdateUserMessageId(user.UserId, messageAnswer!.MessageId);
            }
            else
            {
                var messageAnswer = await _botMenu.Check.CheckActivationPasswordNotCorrect(user, cancellationToken, false);
                await _unitOfWork.UserRepository.UpdateUserMessageId(user.UserId, messageAnswer!.MessageId);
            }
            await _unitOfWork.SaveChanges();
        }

        [BotRequestRoute(MessageRequestRoute.CheckCreateMessageAmount)]
        private async Task CheckCreateMessageAmount_ProcessorAsync(UserModel user, Message message, CancellationToken cancellationToken)
        {
            if (!decimal.TryParse(message.Text, out decimal amount))
            {
                var messageAnswer = await _botMenu.Check.CheckCreateAmountNotCorrect(user, cancellationToken, false);
                await _unitOfWork.UserRepository.UpdateUserMessageId(user.UserId, messageAnswer!.MessageId);
            }
            if (!_appConfig.FunctionConfiguration.FunctionEnable.Check || !_appConfig.FunctionConfiguration.AssetEnable.Check[user.MessagesCheckModel.Asset])
            {
                await FunctionDisabled(user, cancellationToken);
                return;
            }

            var infoForCheckModel = await _infoService.GetInfoForCheck(user.MessagesCheckModel.Asset);

            if (ValidateData.ValidateCheckCreate_Amount(user, amount, infoForCheckModel))
            {
                await _unitOfWork.UserRepository.UpdateUserWhereMenu(user.UserId, MessageRequestRoute.CheckCreateMessageCount);
                await _unitOfWork.MessagesCheckRepository.UpdateMessagesCheckAmount(user.UserId, amount);
                await _unitOfWork.SaveChanges();
                var messageAnswer = await _botMenu.Check.CheckCreateWaitCount(user, cancellationToken, false);
                await _unitOfWork.UserRepository.UpdateUserMessageId(user.UserId, messageAnswer!.MessageId);
            }
            else
            {
                var messageAnswer = await _botMenu.Check.CheckCreateAmountNotCorrect(user, cancellationToken, false);
                await _unitOfWork.UserRepository.UpdateUserMessageId(user.UserId, messageAnswer!.MessageId);
            }
            await _unitOfWork.SaveChanges();
        }

        [BotRequestRoute(MessageRequestRoute.CheckCreateMessageCount)]
        private async Task CheckCreateMessageCount_ProcessorAsync(UserModel user, Message message, CancellationToken cancellationToken)
        {
            if (!int.TryParse(message.Text, out int count))
            {
                var messageAnswer = await _botMenu.Check.CheckCreateCountNotCorrect(user, cancellationToken, false);
                await _unitOfWork.UserRepository.UpdateUserMessageId(user.UserId, messageAnswer!.MessageId);
            }

            if (!_appConfig.FunctionConfiguration.FunctionEnable.Check || !_appConfig.FunctionConfiguration.AssetEnable.Check[user.MessagesCheckModel.Asset])
            {
                await FunctionDisabled(user, cancellationToken);
                return;
            }

            var infoForCheckModel = await _infoService.GetInfoForCheck(user.MessagesCheckModel.Asset);

            if (ValidateData.ValidateCheckCreate_Count(user, count, infoForCheckModel))
            {
                await _unitOfWork.UserRepository.UpdateUserWhereMenu(user.UserId, MessageRequestRoute.Default);
                await _unitOfWork.MessagesCheckRepository.UpdateMessagesCheckCount(user.UserId, count);
                await _unitOfWork.SaveChanges();
                var messageAnswer = await _botMenu.Check.CheckConfirm(user, cancellationToken, false);
                await _unitOfWork.UserRepository.UpdateUserMessageId(user.UserId, messageAnswer!.MessageId);
            }
            else
            {
                var messageAnswer = await _botMenu.Check.CheckCreateCountNotCorrect(user, cancellationToken, false);
                await _unitOfWork.UserRepository.UpdateUserMessageId(user.UserId, messageAnswer!.MessageId);
            }
            await _unitOfWork.SaveChanges();
        }

        #endregion

        #region Withdraw

        [BotRequestRoute(MessageRequestRoute.WithdrawCreateMessageAmount)]
        private async Task WithdrawCreateMessageAmount_ProcessorAsync(UserModel user, Message message, CancellationToken cancellationToken)
        {
            if (!decimal.TryParse(message.Text, out decimal amount))
            {
                var messageAnswer = await _botMenu.Wallet.WithdrawCreateNotCorrectAmount(user, cancellationToken, false);
                await _unitOfWork.UserRepository.UpdateUserMessageId(user.UserId, messageAnswer!.MessageId);
            }

            if (!_appConfig.FunctionConfiguration.FunctionEnable.Withdraw 
                || !_appConfig.FunctionConfiguration.AssetEnable.Withdraw[user.MessagesWithdrawModel.Asset]
                || !_appConfig.FunctionConfiguration.NetworkEnable.Withdraw[user.MessagesWithdrawModel.Network])
            {
                await FunctionDisabled(user, cancellationToken);
                return;
            }

            var infoFroWithdraw = await _infoService.GetInfoForWithdrawAsset(user.MessagesWithdrawModel.Asset, user.MessagesWithdrawModel.Network);

            if (ValidateData.ValidateWithdrawCreate_Amount(user, amount, infoFroWithdraw))
            {
                await _unitOfWork.MessagesWithdrawRepository.UpdateMessagesWithdrawAmount(user.UserId, amount);
                await _unitOfWork.UserRepository.UpdateUserWhereMenu(user.UserId, MessageRequestRoute.Default);
                await _unitOfWork.SaveChanges();
                var messageAnswer = await _botMenu.Wallet.WithdrawConfirm(user, infoFroWithdraw, cancellationToken, false);
                await _unitOfWork.UserRepository.UpdateUserMessageId(user.UserId, messageAnswer!.MessageId);
            }
            else
            {
                var messageAnswer = await _botMenu.Wallet.WithdrawCreateNotCorrectAmount(user, cancellationToken, false);
                await _unitOfWork.UserRepository.UpdateUserMessageId(user.UserId, messageAnswer!.MessageId);
            }
            await _unitOfWork.SaveChanges();
        }

        [BotRequestRoute(MessageRequestRoute.WithdrawCreateMessageAddress)]
        private async Task WithdrawCreateMessageAddress_ProcessorAsync(UserModel user, Message message, CancellationToken cancellationToken)
        {
            string address = message.Text ?? "";
            var asset = user.MessagesWithdrawModel.Asset;
            var network = user.MessagesWithdrawModel.Network;

            var infoForWithdrawAsset = await _infoService.GetInfoForWithdrawAsset(asset, network);

            var withdrawEnabledStatus = await WithdrawEnabledStatus(user, message, cancellationToken, asset, network, infoForWithdrawAsset);
            if (!withdrawEnabledStatus)
                return;

            if (ValidateData.ValidateWithdrawCreate_Address(address, network))
            {
                await _unitOfWork.MessagesWithdrawRepository.UpdateMessagesWithdrawAddress(user.UserId, address);
                await _unitOfWork.UserRepository.UpdateUserWhereMenu(user.UserId, MessageRequestRoute.WithdrawCreateMessageAmount);
                await _unitOfWork.SaveChanges();
                var messageAnswer = await _botMenu.Wallet.WithdrawCreateWaitAmount(user, infoForWithdrawAsset, cancellationToken, false);
                await _unitOfWork.UserRepository.UpdateUserMessageId(user.UserId, messageAnswer!.MessageId);
            }
            else
            {
                var messageAnswer = await _botMenu.Wallet.WithdrawCreateNotCorrectAddress(user, cancellationToken, false);
                await _unitOfWork.UserRepository.UpdateUserMessageId(user.UserId, messageAnswer!.MessageId);
            }
            await _unitOfWork.SaveChanges();
        }

        #endregion

        #region Exchange

        [BotRequestRoute(MessageRequestRoute.ExchangeCreateMessageAmount)]
        private async Task ExchangeCreateMessageAmount_ProcessorAsync(UserModel user, Message message, CancellationToken cancellationToken)
        {
            var infoForExchange = await _infoService.GetInfoForExchange(user.MessagesExchangeModel.FromAsset, user.MessagesExchangeModel.ToAsset);

            if (!decimal.TryParse(message.Text, out decimal amount))
            {
                var messageAnswer = await _botMenu.Exchange.ExchangeCreateNotCorrectAmount(user, infoForExchange, cancellationToken, false);
                await _unitOfWork.UserRepository.UpdateUserMessageId(user.UserId, messageAnswer!.MessageId);
            }

            if (!_appConfig.FunctionConfiguration.FunctionEnable.Exchange
                || !_appConfig.FunctionConfiguration.AssetEnable.Exchange[user.MessagesExchangeModel.FromAsset])
            {
                await FunctionDisabled(user, cancellationToken);
                return;
            }

            if (ValidateData.ValidateExchangeCreate_Amount(user, amount, infoForExchange))
            {
                await _unitOfWork.MessagesExchangeRepository.UpdateMessagesExchangeAmount(user.UserId, amount);
                await _unitOfWork.UserRepository.UpdateUserWhereMenu(user.UserId, MessageRequestRoute.Default);
                await _unitOfWork.SaveChanges();

                if (infoForExchange.Method == "Binance.Convert")
                {
                    var quote = await _exchangeService.ConvertQuoteRequest(user, infoForExchange);
                    if (quote.Status && quote.Data != null)
                    {
                        var messageAnswer = await _botMenu.Exchange.ExchangeConfirm(user, quote.Data, cancellationToken, false);
                        await _unitOfWork.UserRepository.UpdateUserMessageId(user.UserId, messageAnswer!.MessageId);
                    }
                    else
                    {
                        var messageError = await _botMenu.Exchange.ExchangeCreateErrorQuote(user, cancellationToken, false);
                        await _unitOfWork.UserRepository.UpdateUserMessageId(user.UserId, messageError!.MessageId);
                    }
                }
            }
            else
            {
                var messageAnswer = await _botMenu.Exchange.ExchangeCreateNotCorrectAmount(user, infoForExchange, cancellationToken, false);
                await _unitOfWork.UserRepository.UpdateUserMessageId(user.UserId, messageAnswer!.MessageId);
            }
            await _unitOfWork.SaveChanges();
        }

        #endregion

        #region BlavedPay

        [BotRequestRoute(MessageRequestRoute.BlavedPayIDTransferCreateMessageId)]
        private async Task BlavedPayIDTransferCreateMessageId_ProcessorAsync(UserModel user, Message message, CancellationToken cancellationToken)
        {
            if (!long.TryParse(message.Text, out long id))
            {
                var messageAnsfer = await _botMenu.BlavedPay.BlavedPayIDTransferCreateNotCorrectID(user, cancellationToken, false);
                await _unitOfWork.UserRepository.UpdateUserMessageId(user.UserId, messageAnsfer!.MessageId);
                await _unitOfWork.SaveChanges();
                return;
            }

            if (!_appConfig.FunctionConfiguration.FunctionEnable.BlavedPay
               || !_appConfig.FunctionConfiguration.AssetEnable.BlavedPay[user.MessagesBlavedPayIDModel.Asset])
            {
                await FunctionDisabled(user, cancellationToken);
                return;
            }

            var userForId = await _unitOfWork.UserRepository.GetUser(id);
            if (userForId != null && id != user.UserId)
            {
                await _unitOfWork.MessagesBlavedPayIDRepository.UpdateMessagesBlavedPayIDID(user.UserId, id);
                await _unitOfWork.UserRepository.UpdateUserWhereMenu(user.UserId, MessageRequestRoute.BlavedPayIDTransferCreateMessageAmount);
                await _unitOfWork.SaveChanges();
                var messageAnsfer = await _botMenu.BlavedPay.BlavedPayIDTransferCreateWaitAmount(user, cancellationToken, false);
                await _unitOfWork.UserRepository.UpdateUserMessageId(user.UserId, messageAnsfer!.MessageId);
            }
            else
            {
                var messageAnsfer = await _botMenu.BlavedPay.BlavedPayIDTransferCreateUserNotFound(user, cancellationToken, false);
                await _unitOfWork.UserRepository.UpdateUserMessageId(user.UserId, messageAnsfer!.MessageId);
            }
            await _unitOfWork.SaveChanges();
        }

        [BotRequestRoute(MessageRequestRoute.BlavedPayIDTransferCreateMessageAmount)]
        private async Task BlavedPayTransferCreateMessageAmount_ProcessorAsync(UserModel user, Message message, CancellationToken cancellationToken)
        {
            if (!decimal.TryParse(message.Text, out decimal amount))
            {
                var messageAnsfer = await _botMenu.BlavedPay.BlavedPayIDTransferCreateNotCorrectAmount(user, cancellationToken, false);
                await _unitOfWork.UserRepository.UpdateUserMessageId(user.UserId, messageAnsfer!.MessageId);
            }

            if (!_appConfig.FunctionConfiguration.FunctionEnable.BlavedPay
               || !_appConfig.FunctionConfiguration.AssetEnable.BlavedPay[user.MessagesBlavedPayIDModel.Asset])
            {
                await FunctionDisabled(user, cancellationToken);
                return;
            }

            var infoForBlavedPay = await _infoService.GetInfoForBlavedPayID(user.MessagesBlavedPayIDModel.Asset);

            if (ValidateData.ValidateBlavedPayIDTransferCreate_Amount(user, amount, infoForBlavedPay))
            {
                await _unitOfWork.MessagesBlavedPayIDRepository.UpdateMessagesBlavedPayIDAmount(user.UserId, amount);
                await _unitOfWork.UserRepository.UpdateUserWhereMenu(user.UserId, MessageRequestRoute.Default);
                await _unitOfWork.SaveChanges();

                var messageAnsfer = await _botMenu.BlavedPay.BlavedPayIDTransferConfirm(user, cancellationToken, false);
                await _unitOfWork.UserRepository.UpdateUserMessageId(user.UserId, messageAnsfer!.MessageId);
            }
            else
            {
                var messageAnsfer = await _botMenu.BlavedPay.BlavedPayIDTransferCreateNotCorrectAmount(user, cancellationToken, false);
                await _unitOfWork.UserRepository.UpdateUserMessageId(user.UserId, messageAnsfer!.MessageId);
            }
            await _unitOfWork.SaveChanges();
        }
        #endregion

        private async Task FunctionDisabled(UserModel user, CancellationToken cancellationToken)
        {
            await _unitOfWork.UserRepository.UpdateUserWhereMenu(user.UserId, "Default");
            await _unitOfWork.SaveChanges();

            var messageAnswer = await _botMenu.Main.Main(user, cancellationToken, false);
            await _unitOfWork.UserRepository.UpdateUserMessageId(user.UserId, messageAnswer!.MessageId);
            await _unitOfWork.SaveChanges();
        }
    }
}
