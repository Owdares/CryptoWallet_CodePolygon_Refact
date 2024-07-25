using Blaved.Core.Interfaces;
using Blaved.Core.Interfaces.Services;
using Blaved.Core.Objects.Models;
using Blaved.Core.Objects.Models.Configurations;
using Blaved.Core.Objects.Models.Cryptocurrency;
using Blaved.Core.Utility;
using Bleved.TelegramBot.Server.Controllers.Attributes;
using Bleved.TelegramBot.Server.Controllers.Routes;
using Microsoft.Extensions.Options;
using NBitcoin;
using Serilog.Context;
using System.Reflection;
using System.Text.RegularExpressions;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;

namespace Bleved.TelegramBot.Server.Controllers.RequestHandlers
{
    public class CallbackRequestHandler
    {
        private readonly IWalletService _walletService;
        private readonly ICheckService _checkService;
        private readonly IExchangeService _exchangeService;
        private readonly IBlavedPayService _blavedPayService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBotMenu _botMenu;
        private readonly IInfoService _infoService;
        private readonly AppConfig _appConfig;
        private readonly ILogger<CallbackRequestHandler> _logger;
        public CallbackRequestHandler(IUnitOfWork unitOfWork, IBotMenu botMenu, IWalletService walletService, 
            ICheckService checkService, IExchangeService exchangeService, IBlavedPayService blavedPayService, 
            IInfoService infoService, IOptionsSnapshot<AppConfig> appConfig, ILogger<CallbackRequestHandler> logger)
        {
            _appConfig = appConfig.Value;
            _walletService = walletService;
            _unitOfWork = unitOfWork;
            _botMenu = botMenu;
            _checkService = checkService;
            _exchangeService = exchangeService;
            _blavedPayService = blavedPayService;
            _infoService = infoService;
            _logger = logger;
        }

        public async Task CallbackHandlerAsync(CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            if (callbackQuery.Message is not { } callbackQueryMessage)
                return;

            LogContext.PushProperty("CallbackQueryId", callbackQuery.Id);
            LogContext.PushProperty("UserId", callbackQuery.Message.Chat.Id);

            var user = await _unitOfWork.UserRepository.GetUser(callbackQueryMessage.Chat.Id);

            if (user is not null)
            {
                try
                {
                    if (user.MessageId == callbackQueryMessage.MessageId)
                    {
                        if (!user.IsBanned)
                        {
                            await CallbackControllerAsync(user, callbackQuery, cancellationToken);
                        }
                        else
                        {
                            await CallbackUserIsBanedAsync(user, callbackQuery, cancellationToken);
                        }
                    }
                    else
                    {
                        await CallbackMessageIdOldAsync(user, callbackQuery, cancellationToken);
                    }
                }
                catch (ApiRequestException ex)
                {
                    _logger.LogError(ex, "Error processing request");

                    await _botMenu.Main.TelegramAPIError(callbackQuery.Id, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing request");

                    await _botMenu.Main.ServerError(user, cancellationToken);

                }
            }
        }
        private async Task CallbackControllerAsync(UserModel user, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            if (callbackQuery.Data is not { } callbackData)
                return;

            LogContext.PushProperty("Data", callbackData);

            var methods = GetType().GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(method => method.GetCustomAttributes(typeof(BotRequestRouteAttribute), false).Length > 0)
                .Select(method => new
                {
                    Key = ((BotRequestRouteAttribute)method.GetCustomAttributes(typeof(BotRequestRouteAttribute), false)[0]).Key,
                    Method = (Func<UserModel, CallbackQuery, CancellationToken, Task>)Delegate.CreateDelegate(typeof(Func<UserModel, CallbackQuery, CancellationToken, Task>), this, method)
                })
                .ToDictionary(entry => entry.Key, entry => entry.Method);

            var match = Regex.Split(callbackData, @":");
            string callbackDataPrefix = match.Length > 1 ? match[0] : callbackData;

            var handler = methods.SingleOrDefault(x => callbackDataPrefix == x.Key);

            if (handler.Value != null)
            {
                LogContext.PushProperty("MethodProcessor", handler.Value.Method.Name);
                await handler.Value(user, callbackQuery, cancellationToken);
            }
        }

        private async Task CallbackUserIsBanedAsync(UserModel user, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            LogContext.PushProperty("MethodProcessor", "CallbackUserIsBanedAsync");

            await _botMenu.Main.UserHasBanned(user, cancellationToken);
        }
        private async Task CallbackMessageIdOldAsync(UserModel user, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            LogContext.PushProperty("MethodProcessor", "CallbackMessageIdOldAsync");

            var message = await _botMenu.Main.Main(user, cancellationToken, false);
            await _unitOfWork.UserRepository.UpdateUserMessageId(user.UserId, message!.MessageId);
            await _unitOfWork.UserRepository.UpdateUserWhereMenu(user.UserId, MessageRequestRoute.Default);
            await _unitOfWork.SaveChanges();
        }

        private async Task<bool> WithdrawEnabledStatus(UserModel user, CallbackQuery callbackQuery, CancellationToken cancellationToken, 
            string? asset = null, string? network = null, AssetModel? assetModel = null)
        {
            if (asset != null && !_appConfig.FunctionConfiguration.AssetEnable.Withdraw[asset]
                || assetModel != null && !assetModel.WithdrawAllEnable)
            {
                await _botMenu.Main.AssetDisabledAnswer(user, callbackQuery.Id, cancellationToken);
                return false;
            }
            if (network != null && !_appConfig.FunctionConfiguration.NetworkEnable.Withdraw[network]
                || assetModel != null && assetModel.SelectNetwork != null && !assetModel.SelectNetwork.WithdrawEnabled)
            {
                await _botMenu.Main.NetworkDisabledAnswer(user, callbackQuery.Id, cancellationToken);
                return false;
            }
            if (!_appConfig.FunctionConfiguration.FunctionEnable.Withdraw)
            {
                await _botMenu.Main.FunctionDisabledAnswer(user, callbackQuery.Id, cancellationToken);
                return false;
            }
            return true;
        }
        private async Task<bool> DepositEnabledStatus(UserModel user, CallbackQuery callbackQuery, CancellationToken cancellationToken,
            string? asset = null, string? network = null, AssetModel? assetModel = null)
        {
            if (asset != null && !_appConfig.FunctionConfiguration.AssetEnable.Deposit[asset] 
                || assetModel != null && !assetModel.DepositAllEnable)
            {
                await _botMenu.Main.AssetDisabledAnswer(user, callbackQuery.Id, cancellationToken);
                return false;
            }
            if (network != null && !_appConfig.FunctionConfiguration.NetworkEnable.Deposit[network]
                || assetModel != null && assetModel.SelectNetwork != null && !assetModel.SelectNetwork.DepositEnabled)
            {
                await _botMenu.Main.NetworkDisabledAnswer(user, callbackQuery.Id, cancellationToken);
                return false;
            }
            if (!_appConfig.FunctionConfiguration.FunctionEnable.Deposit)
            {
                await _botMenu.Main.FunctionDisabledAnswer(user, callbackQuery.Id, cancellationToken);
                return false;
            }
            return true;
        }
        [BotRequestRoute(CallbackRequestRoute.WalletPutPage)]
        private async Task WalletPutPage_ProcessorAsync(UserModel user, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            string[] parts = callbackQuery.Data!.Split(':');
            int pageNum = int.Parse(parts[1]);

            if(!user.AcceptedTermsOfUse) 
            {
                await _unitOfWork.UserRepository.UpdateUserTermsOfUse(user.UserId, true);
                await _unitOfWork.SaveChanges();

                await _botMenu.Main.AcceptTermsOfUseAnswer(user, callbackQuery.Id, cancellationToken);
            }
            var blavedAssets = await _infoService.GetInfoForBlavedAsset();
            await _botMenu.Wallet.Wallet(user, blavedAssets, cancellationToken, pageNum);
        }

        [BotRequestRoute(CallbackRequestRoute.BonusBalancePutPage)]
        private async Task BonusBalancePutPage_ProcessorAsync(UserModel user, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            string[] parts = callbackQuery.Data!.Split(':');
            int pageNum = int.Parse(parts[1]);

            var blavedAssets = await _infoService.GetInfoForBlavedAsset();
            await _botMenu.Wallet.BonusBalance(user, blavedAssets, cancellationToken, pageNum);
        }

        [BotRequestRoute(CallbackRequestRoute.CommingSoon)]
        private async Task CommingSoon_ProcessorAsync(UserModel user, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            await _botMenu.Main.SectionIsTemporarilyUnavailableAnswer(user, callbackQuery.Id, cancellationToken);
        }

        [BotRequestRoute(CallbackRequestRoute.LanguageUpdatePutLanguage)]
        private async Task LanguageUpdatePutLanguage_ProcessorAsync(UserModel user, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            string[] parts = callbackQuery.Data!.Split(':');
            string language = parts[1];

            await _unitOfWork.UserRepository.UpdateUserLanguage(user.UserId, language);
            await _unitOfWork.SaveChanges();
            await _botMenu.Settings.Language(user, cancellationToken);
        }

        [BotRequestRoute(CallbackRequestRoute.Main)]
        private async Task Main_ProcessorAsync(UserModel user, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            await _unitOfWork.UserRepository.UpdateUserWhereMenu(user.UserId, MessageRequestRoute.Default);
            await _unitOfWork.SaveChanges();

            await _botMenu.Main.Main(user, cancellationToken);
        }

        [BotRequestRoute(CallbackRequestRoute.Settings)]
        private async Task Settings_ProcessorAsync(UserModel user, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            await _botMenu.Settings.Settings(user, cancellationToken);
        }

        [BotRequestRoute(CallbackRequestRoute.Help)]
        private async Task Help_ProcessorAsync(UserModel user, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            await _botMenu.Help.HelpPage(user, cancellationToken);
        }

        [BotRequestRoute(CallbackRequestRoute.Referrals)]
        private async Task Referrals_ProcessorAsync(UserModel user, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            var referralsCount = await _unitOfWork.UserRepository.GetUserReferralsCount(user.UserId);

            decimal fullAmountChargeToReferralInUSD = 0;

            foreach (var item in _appConfig.AssetConfiguration.CoinList)
            {
                var amountInAsset = await _unitOfWork.UserRepository.GetUserChargeAmountToReferral(user.UserId, item);
                var coinPrice = await _infoService.GetCoinPriceUSDT(item);
                fullAmountChargeToReferralInUSD += amountInAsset.AmountToUSD(coinPrice);
            }

            await _botMenu.Settings.Referrals(user, referralsCount, fullAmountChargeToReferralInUSD, cancellationToken);
        }

        [BotRequestRoute(CallbackRequestRoute.BonusBalanceToBalance)]
        private async Task BonusBalanceToBalance_ProcessorAsync(UserModel user, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            var result = await _walletService.BonusBalanceToBalance(user);

            if(result)
            {
                await _botMenu.Wallet.BonusBalanceToMainBalanceCompleteAnswer(user, callbackQuery.Id, cancellationToken);

                var blavedAssets = await _infoService.GetInfoForBlavedAsset();
                await _botMenu.Wallet.BonusBalance(user, blavedAssets, cancellationToken);
            }
            else
            {
                await _botMenu.Wallet.BonusBalanceToMainBalanceErrorAnswer(user, callbackQuery.Id, cancellationToken);
            }
        }

        [BotRequestRoute(CallbackRequestRoute.Language)]
        private async Task Language_ProcessorAsync(UserModel user, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            await _botMenu.Settings.Language(user, cancellationToken);
        }

        [BotRequestRoute(CallbackRequestRoute.TermsOfUsePutPage)]
        private async Task TermsOfUsePutPage_ProcessorAsync(UserModel user, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            string[] parts = callbackQuery.Data!.Split(':');
            int pageNum = int.Parse(parts[1]);

            await _botMenu.Settings.TermsOfUse(user, cancellationToken, pageNum);
        }

        [BotRequestRoute(CallbackRequestRoute.SettingsNotificationsEnableBlavedPay)]
        private async Task SettingsNotificationsEnableBlavedPay_ProcessorAsync(UserModel user, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            await _unitOfWork.UserRepository.UpdateEnabledNotificationsBlavedPay(user.UserId, true);
            await _unitOfWork.SaveChanges();

            await _botMenu.Settings.NotificationsBlavedPay(user, cancellationToken);
        }
        [BotRequestRoute(CallbackRequestRoute.SettingsNotificationsDisableBlavedPay)]
        private async Task SettingsNotificationsDisableBlavedPay_ProcessorAsync(UserModel user, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            await _unitOfWork.UserRepository.UpdateEnabledNotificationsBlavedPay(user.UserId, false);
            await _unitOfWork.SaveChanges();

            await _botMenu.Settings.NotificationsBlavedPay(user, cancellationToken);

        }
        [BotRequestRoute(CallbackRequestRoute.Notifications)]
        private async Task Notifications_ProcessorAsync(UserModel user, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            await _botMenu.Settings.Notifications(user, cancellationToken);

        }
        [BotRequestRoute(CallbackRequestRoute.NotificationsBlavedPay)]
        private async Task NotificationsBlavedPay_ProcessorAsync(UserModel user, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            await _botMenu.Settings.NotificationsBlavedPay(user, cancellationToken);

        }

        #region TransactionHistory

        [BotRequestRoute(CallbackRequestRoute.WalletTransactionHistory)]
        private async Task WalletTransactionHistory_ProcessorAsync(UserModel user, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            await _botMenu.Wallet.WalletTransactionHistory(user, cancellationToken);
        }

        [BotRequestRoute(CallbackRequestRoute.WithdrawHistoryPutNum)]
        private async Task WithdrawHistoryPutNum_ProcessorAsync(UserModel user, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            string[] parts = callbackQuery.Data!.Split(':');
            int num = int.Parse(parts[1]);

            if (!_appConfig.FunctionConfiguration.FunctionEnable.Withdraw)
            {
                await _botMenu.Main.FunctionDisabledAnswer(user, callbackQuery.Id, cancellationToken);
                return;
            }

            var withdraw = await _unitOfWork.WithdrawRepository.GetWithdraw(num);

            await _botMenu.Wallet.WithdrawHistoryView(user, withdraw!, cancellationToken);
        }

        [BotRequestRoute(CallbackRequestRoute.WithdrawHistoryWaitNumPutNetworkPage)]
        private async Task WithdrawHistoryWaitNumPutNetworkPage_ProcessorAsync(UserModel user, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            string[] parts = callbackQuery.Data!.Split(':');
            string[] partsPut = parts[1].Split('_');

            string network = partsPut[0];
            int page = int.Parse(partsPut[1]);

            if (!_appConfig.FunctionConfiguration.NetworkEnable.Withdraw[network])
            {
                await _botMenu.Main.AssetDisabledAnswer(user, callbackQuery.Id, cancellationToken);
                return;
            }
            if (!_appConfig.FunctionConfiguration.FunctionEnable.Withdraw)
            {
                await _botMenu.Main.FunctionDisabledAnswer(user, callbackQuery.Id, cancellationToken);
                return;
            }

            var withdrawList = await _unitOfWork.WithdrawRepository.GetWithdrawList(user.UserId, network);
            if (withdrawList.Count > 0) 
            {
                await _botMenu.Wallet.WithdrawHistoryList(user, network, withdrawList, cancellationToken, page);
            }
            else
            {
                await _botMenu.Wallet.WithdrawHistoryNoneAnswer(user, callbackQuery.Id, cancellationToken);
            }
            
        }

        [BotRequestRoute(CallbackRequestRoute.WithdrawHistoryWaitNetwork)]
        private async Task WithdrawHistoryWaitNetwork_ProcessorAsync(UserModel user, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            if (!_appConfig.FunctionConfiguration.FunctionEnable.Withdraw)
            {
                await _botMenu.Main.FunctionDisabledAnswer(user, callbackQuery.Id, cancellationToken);
                return;
            }

            await _botMenu.Wallet.WithdrawHistoryWaitNetwork(user, cancellationToken);
        }

        [BotRequestRoute(CallbackRequestRoute.DepositHistoryPutNum)]
        private async Task DepositHistoryPutNum_ProcessorAsync(UserModel user, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            string[] parts = callbackQuery.Data!.Split(':');
            int num = int.Parse(parts[1]);

            if (!_appConfig.FunctionConfiguration.FunctionEnable.Deposit)
            {
                await _botMenu.Main.FunctionDisabledAnswer(user, callbackQuery.Id, cancellationToken);
                return;
            }

            var withdraw = await _unitOfWork.DepositRepository.GetDeposit(num);

            await _botMenu.Wallet.DepositHistoryView(user, withdraw!, cancellationToken);
        }

        [BotRequestRoute(CallbackRequestRoute.DepositHistoryWaitNumPutNetworkPage)]
        private async Task DepositeHistoryWaitNumPutNetworkPage_ProcessorAsync(UserModel user, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            string[] parts = callbackQuery.Data!.Split(':');
            string[] partsPut = parts[1].Split('_');

            string network = partsPut[0];
            int pageNum = int.Parse(partsPut[1]);

            if (!_appConfig.FunctionConfiguration.NetworkEnable.Deposit[network])
            {
                await _botMenu.Main.AssetDisabledAnswer(user, callbackQuery.Id, cancellationToken);
                return;
            }
            if (!_appConfig.FunctionConfiguration.FunctionEnable.Deposit)
            {
                await _botMenu.Main.FunctionDisabledAnswer(user, callbackQuery.Id, cancellationToken);
                return;
            }

            var depositList = await _unitOfWork.DepositRepository.GetDepositList(user.UserId, network);
            if (depositList.Count > 0)
            {
                await _botMenu.Wallet.DepositHistoryList(user, network, depositList, cancellationToken, pageNum);
            }
            else
            {
                await _botMenu.Wallet.DepositHistoryNoneAnswer(user, callbackQuery.Id, cancellationToken);
            }
        }

        [BotRequestRoute(CallbackRequestRoute.DepositHistoryWaitNetwork)]
        private async Task DepositHistoryWaitNetwork_ProcessorAsync(UserModel user, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            if (!_appConfig.FunctionConfiguration.FunctionEnable.Deposit)
            {
                await _botMenu.Main.FunctionDisabledAnswer(user, callbackQuery.Id, cancellationToken);
                return;
            }

            await _botMenu.Wallet.DepositHistoryWaitNetwork(user, cancellationToken);
        }

        #endregion

        #region Check

        [BotRequestRoute(CallbackRequestRoute.Check)]
        private async Task Check_ProcessorAsync(UserModel user, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            if (!_appConfig.FunctionConfiguration.FunctionEnable.Check)
            {
                await _botMenu.Main.FunctionDisabledAnswer(user, callbackQuery.Id, cancellationToken);
                return;
            }

            await _botMenu.Check.Check(user, cancellationToken);
        }

        [BotRequestRoute(CallbackRequestRoute.CheckViewPutUrl)]
        private async Task CheckViewPutUrl_ProcessorAsync(UserModel user, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            string[] parts = callbackQuery.Data!.Split(':');
            string url = parts[1];

            if (!_appConfig.FunctionConfiguration.FunctionEnable.Check)
            {
                await _botMenu.Main.FunctionDisabledAnswer(user, callbackQuery.Id, cancellationToken);
                return;
            }

            await _unitOfWork.UserRepository.UpdateUserWhereMenu(user.UserId, MessageRequestRoute.Default);
            await _unitOfWork.SaveChanges();

            var result = await _unitOfWork.CheckRepository.GetCheck(url);
            if (result != null && result.Count > 0)
            {
                await _botMenu.Check.CheckView(user, result, cancellationToken);
            }
            else { throw new Exception(); }
        }

        [BotRequestRoute(CallbackRequestRoute.CheckListPutPage)]
        private async Task CheckListPutPage_ProcessorAsync(UserModel user, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            string[] parts = callbackQuery.Data!.Split(':');
            int page = int.Parse(parts[1]);

            if (!_appConfig.FunctionConfiguration.FunctionEnable.Check)
            {
                await _botMenu.Main.FunctionDisabledAnswer(user, callbackQuery.Id, cancellationToken);
                return;
            }

            var result = await _unitOfWork.CheckRepository.GetCheckList(user.UserId);
            result = result.Where(x => x.Count > 0).ToList();
            if (result.Count > 0)
            {
                await _botMenu.Check.CheckList(user, result, cancellationToken, page);
            }
            else
            {
                await _botMenu.Check.CheckDontHaveAnswer(user, callbackQuery.Id, cancellationToken);
            }
        }

        [BotRequestRoute(CallbackRequestRoute.CheckUpdateWaitPasswordPutUrl)]
        private async Task CheckUpdateWaitPasswordPutUrl_ProcessorAsync(UserModel user, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            string[] parts = callbackQuery.Data!.Split(':');
            string url = parts[1];

            if (!_appConfig.FunctionConfiguration.FunctionEnable.Check)
            {
                await _botMenu.Main.FunctionDisabledAnswer(user, callbackQuery.Id, cancellationToken);
                return;
            }

            var result = await _unitOfWork.CheckRepository.GetCheck(url);
            if (result != null)
            {
                await _unitOfWork.UserRepository.UpdateUserWhereMenu(user.UserId, $"{MessageRequestRoute.CheckUpdateMessagePasswordPutUrl}:{url}");
                await _unitOfWork.SaveChanges();
                await _botMenu.Check.CheckUpdateWaitPassword(user, url, cancellationToken);
            }
            else { throw new Exception(); }
        }

        [BotRequestRoute(CallbackRequestRoute.CheckUpdatePasswordConfirmPutUrlPassword)]
        private async Task CheckUpdatePasswordConfirmPutUrlPassword_ProcessorAsync(UserModel user, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            string[] parts = callbackQuery.Data!.Split(':');
            string[] partsPut = parts[1].Split('_');

            string url = partsPut[0];
            string password = partsPut[1];

            if (!_appConfig.FunctionConfiguration.FunctionEnable.Check)
            {
                await _botMenu.Main.FunctionDisabledAnswer(user, callbackQuery.Id, cancellationToken);
                return;
            }

            var result = await _unitOfWork.CheckRepository.GetCheck(url);
            if (result != null)
            {
                var check = await _unitOfWork.CheckRepository.UpdateCheckPassword(url, password);
                await _unitOfWork.SaveChanges();
                await _botMenu.Check.CheckView(user, check, cancellationToken);
            }
            else { throw new Exception(); }
        }

        [BotRequestRoute(CallbackRequestRoute.CheckDeletePutUrl)]
        private async Task CheckDeletePutUrl_ProcessorAsync(UserModel user, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            string[] parts = callbackQuery.Data!.Split(':');
            string url = parts[1];

            if (!_appConfig.FunctionConfiguration.FunctionEnable.Check)
            {
                await _botMenu.Main.FunctionDisabledAnswer(user, callbackQuery.Id, cancellationToken);
                return;
            }

            var result = await _unitOfWork.CheckRepository.GetCheck(url);
            if (result != null)
            {
                await _checkService.CheckDelete(url);

                await _botMenu.Check.Check(user, cancellationToken);
            }
            else { throw new Exception(); }
        }

        [BotRequestRoute(CallbackRequestRoute.CheckPasswordDeletePutUrl)]
        private async Task CheckPasswordDeletePutUrl_ProcessorAsync(UserModel user, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            string[] parts = callbackQuery.Data!.Split(':');
            string url = parts[1];

            if (!_appConfig.FunctionConfiguration.FunctionEnable.Check)
            {
                await _botMenu.Main.FunctionDisabledAnswer(user, callbackQuery.Id, cancellationToken);
                return;
            }

            var result = await _unitOfWork.CheckRepository.GetCheck(url);
            if (result != null)
            {
                var check = await _unitOfWork.CheckRepository.DeleteCheckPassword(url);
                await _unitOfWork.SaveChanges();
                await _botMenu.Check.CheckView(user, check!, cancellationToken);
            }
            else { throw new Exception(); }
        }



        [BotRequestRoute(CallbackRequestRoute.CheckCreateWaitAssetPutPage)]
        private async Task CheckCreateWaitAssetPutPage_ProcessorAsync(UserModel user, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            string[] parts = callbackQuery.Data!.Split(':');
            int num = int.Parse(parts[1]);

            if (!_appConfig.FunctionConfiguration.FunctionEnable.Check)
            {
                await _botMenu.Main.FunctionDisabledAnswer(user, callbackQuery.Id, cancellationToken);
                return;
            }

            await _unitOfWork.UserRepository.UpdateUserWhereMenu(user.UserId, MessageRequestRoute.Default);
            await _unitOfWork.SaveChanges();
            await _botMenu.Check.CheckCreateWaitAsset(user, cancellationToken, num);
        }

        [BotRequestRoute(CallbackRequestRoute.CheckCreateWaitAmountPutAsset)]
        private async Task CheckCreateWaitAmountPutAsset_ProcessorAsync(UserModel user, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            string[] parts = callbackQuery.Data!.Split(':');
            string Asset = parts[1];

            if (!_appConfig.FunctionConfiguration.AssetEnable.Check[Asset])
            {
                await _botMenu.Main.AssetDisabledAnswer(user, callbackQuery.Id, cancellationToken);
                return;
            }
            if (!_appConfig.FunctionConfiguration.FunctionEnable.Check)
            {
                await _botMenu.Main.FunctionDisabledAnswer(user, callbackQuery.Id, cancellationToken);
                return;
            }

            var infoForCheckModel = await _infoService.GetInfoForCheck(Asset);
            if (ValidateData.ValidateCheckCreate_Balance(user, infoForCheckModel))
            {
                await _unitOfWork.MessagesCheckRepository.UpdateMessagesCheckAsset(user.UserId, Asset);
                await _unitOfWork.UserRepository.UpdateUserWhereMenu(user.UserId, MessageRequestRoute.CheckCreateMessageAmount);
                await _unitOfWork.SaveChanges();
                await _botMenu.Check.CheckCreateWaitAmount(user, cancellationToken);
            }
            else
            {
                await _botMenu.Check.CheckCreateInsufficientBalanceAnswer(user, infoForCheckModel, callbackQuery.Id, cancellationToken);
            }
        }

        [BotRequestRoute(CallbackRequestRoute.CheckCreatePutAmount)]
        private async Task CheckCreatePutAmount_ProcessorAsync(UserModel user, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            string[] parts = callbackQuery.Data!.Split(':');
            decimal amount = decimal.Parse(parts[1]);

            if (!_appConfig.FunctionConfiguration.AssetEnable.Check[user.MessagesCheckModel.Asset])
            {
                await _botMenu.Main.AssetDisabledAnswer(user, callbackQuery.Id, cancellationToken);
                return;
            }
            if (!_appConfig.FunctionConfiguration.FunctionEnable.Check)
            {
                await _botMenu.Main.FunctionDisabledAnswer(user, callbackQuery.Id, cancellationToken);
                return;
            }

            var infoForCheckModel = await _infoService.GetInfoForCheck(user.MessagesCheckModel.Asset);

            if (ValidateData.ValidateCheckCreate_Amount(user, amount, infoForCheckModel))
            {
                await _unitOfWork.UserRepository.UpdateUserWhereMenu(user.UserId, MessageRequestRoute.CheckCreateMessageCount);
                await _unitOfWork.MessagesCheckRepository.UpdateMessagesCheckAmount(user.UserId, amount);
                await _unitOfWork.SaveChanges();
                await _botMenu.Check.CheckCreateWaitCount(user, cancellationToken);
            }
            else
            {
                await _botMenu.Check.CheckCreateAmountNotCorrect(user, cancellationToken);
            }
        }


        [BotRequestRoute(CallbackRequestRoute.CheckCreateWaitCount)]
        private async Task CheckCreateWaitCount_ProcessorAsync(UserModel user, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            if (!_appConfig.FunctionConfiguration.AssetEnable.Check[user.MessagesCheckModel.Asset])
            {
                await _botMenu.Main.AssetDisabledAnswer(user, callbackQuery.Id, cancellationToken);
                return;
            }
            if (!_appConfig.FunctionConfiguration.FunctionEnable.Check)
            {
                await _botMenu.Main.FunctionDisabledAnswer(user, callbackQuery.Id, cancellationToken);
                return;
            }

            await _unitOfWork.UserRepository.UpdateUserWhereMenu(user.UserId, MessageRequestRoute.CheckCreateMessageCount);
            await _unitOfWork.SaveChanges();
            await _botMenu.Check.CheckCreateWaitCount(user, cancellationToken);
        }

        [BotRequestRoute(CallbackRequestRoute.CheckCreatePutCount)]
        private async Task CheckCreatePutCount_ProcessorAsync(UserModel user, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            string[] parts = callbackQuery.Data!.Split(':');
            int count = int.Parse(parts[1]);

            if (!_appConfig.FunctionConfiguration.AssetEnable.Check[user.MessagesCheckModel.Asset])
            {
                await _botMenu.Main.AssetDisabledAnswer(user, callbackQuery.Id, cancellationToken);
                return;
            }
            if (!_appConfig.FunctionConfiguration.FunctionEnable.Check)
            {
                await _botMenu.Main.FunctionDisabledAnswer(user, callbackQuery.Id, cancellationToken);
                return;
            }

            var infoForCheckModel = await _infoService.GetInfoForCheck(user.MessagesCheckModel.Asset);

            if (ValidateData.ValidateCheckCreate_Count(user, count, infoForCheckModel))
            {
                await _unitOfWork.UserRepository.UpdateUserWhereMenu(user.UserId, MessageRequestRoute.Default);
                await _unitOfWork.MessagesCheckRepository.UpdateMessagesCheckCount(user.UserId, count);
                await _unitOfWork.SaveChanges();

                await _botMenu.Check.CheckConfirm(user, cancellationToken);
            }
            else
            {
                await _botMenu.Check.CheckCreateCountNotCorrect(user, cancellationToken);
            }
        }

        [BotRequestRoute(CallbackRequestRoute.CheckConfirm)]
        private async Task CheckConfirm_ProcessorAsync(UserModel user, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            if (!_appConfig.FunctionConfiguration.AssetEnable.Check[user.MessagesCheckModel.Asset])
            {
                await _botMenu.Main.AssetDisabledAnswer(user, callbackQuery.Id, cancellationToken);
                return;
            }
            if (!_appConfig.FunctionConfiguration.FunctionEnable.Check)
            {
                await _botMenu.Main.FunctionDisabledAnswer(user, callbackQuery.Id, cancellationToken);
                return;
            }

            var infoForCheckModel = await _infoService.GetInfoForCheck(user.MessagesCheckModel.Asset);

            if (ValidateData.ValidateCheckCreate_Count(user, user.MessagesCheckModel.Count, infoForCheckModel))
            {
                var result = await _checkService.CheckCreate(user.UserId, user.MessagesCheckModel.Asset, user.MessagesCheckModel.Amount, user.MessagesCheckModel.Count);
                await _botMenu.Check.CheckView(user, result, cancellationToken);
            }
            else
            {
                throw new Exception();
            }
        }

        #endregion

        #region Deposite

        [BotRequestRoute(CallbackRequestRoute.DepositWaitNetworkPutAsset)]
        private async Task DepositWaitNetwork_ProcessorAsync(UserModel user, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            string[] parts = callbackQuery.Data!.Split(':');
            string asset = parts[1];

            var depositInfo = await _infoService.GetInfoForDepositAsset(asset, null);

            var depositEnabledStatus = await DepositEnabledStatus(user, callbackQuery, cancellationToken, asset);
            if (!depositEnabledStatus)
                return;

            await _botMenu.Wallet.DepositWaitNetwork(user, depositInfo, cancellationToken);
        }

        [BotRequestRoute(CallbackRequestRoute.DepositViewAddressPutAssetNetwork)]
        private async Task DepositeViewAddressPutAssetNetwork_ProcessorAsync(UserModel user, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            string[] parts = callbackQuery.Data!.Split(':');
            string[] partsPut = parts[1].Split('_');

            string asset = partsPut[0];
            string network = partsPut[1];

            var depositInfo = await _infoService.GetInfoForDepositAsset(asset, network);

            var depositEnabledStatus = await DepositEnabledStatus(user, callbackQuery, cancellationToken, asset, network, depositInfo);
            if (!depositEnabledStatus)
                return;

            await _botMenu.Wallet.DepositViewAddress(user, depositInfo, cancellationToken);
        }

        [BotRequestRoute(CallbackRequestRoute.DepositWaitAssetPutPage)]
        private async Task DepositWaitAssetPutPage_ProcessorAsync(UserModel user, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            string[] parts = callbackQuery.Data!.Split(':');
            int num = int.Parse(parts[1]);

            var depositEnabledStatus = await DepositEnabledStatus(user, callbackQuery, cancellationToken);
            if (!depositEnabledStatus)
                return;

            var depositInfo = await _infoService.GetInfoForDepositAsset();

            await _botMenu.Wallet.DepositWaitAsset(user, depositInfo, cancellationToken, num);
        }

        #endregion

        #region Withdraw

        [BotRequestRoute(CallbackRequestRoute.WithdrawCreateWaitNetworkPutAsset)]
        private async Task WithdrawCreateWaitNetworkPutAsset_ProcessorAsync(UserModel user, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            string[] parts = callbackQuery.Data!.Split(':');
            string asset = parts[1];

            var withdrawEnabledStatus = await WithdrawEnabledStatus(user, callbackQuery, cancellationToken, asset);
            if (!withdrawEnabledStatus)
                return;

            await _unitOfWork.UserRepository.UpdateUserWhereMenu(user.UserId, MessageRequestRoute.Default);
            await _unitOfWork.SaveChanges();

            var infoForWithdrawAsset = await _infoService.GetInfoForWithdrawAsset(asset, null);

            await _botMenu.Wallet.WithdrawCreateWaitNetwork(user, infoForWithdrawAsset, cancellationToken);
        }

        [BotRequestRoute(CallbackRequestRoute.WithdrawCreateWaitAddressPutAssetNetwork)]
        private async Task WithdrawCreateWaitAddressPutAssetNetwork_ProcessorAsync(UserModel user, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            string[] parts = callbackQuery.Data!.Split(':');
            string[] partsPut = parts[1].Split('_');

            string asset = partsPut[0];
            string network = partsPut[1];

            var infoForWithdrawAsset = await _infoService.GetInfoForWithdrawAsset(asset, network);

            var withdrawEnabledStatus = await WithdrawEnabledStatus(user, callbackQuery, cancellationToken, asset, network, infoForWithdrawAsset);
            if (!withdrawEnabledStatus)
                return;

            if (ValidateData.ValidateWithdrawCreate_Balance(user, infoForWithdrawAsset))
            {
                await _unitOfWork.UserRepository.UpdateUserWhereMenu(user.UserId, MessageRequestRoute.WithdrawCreateMessageAddress);
                await _unitOfWork.MessagesWithdrawRepository.UpdateMessagesWithdrawAsset(user.UserId, asset);
                await _unitOfWork.MessagesWithdrawRepository.UpdateMessagesWithdrawNetwork(user.UserId, network);
                await _unitOfWork.SaveChanges();

                await _botMenu.Wallet.WithdrawCreateWaitAddress(user, infoForWithdrawAsset, cancellationToken);
            }
            else
            {
                await _botMenu.Wallet.WithdrawCreateInsufficientBalanceAnswer(user, infoForWithdrawAsset, callbackQuery.Id, cancellationToken);
            }

        }

        [BotRequestRoute(CallbackRequestRoute.WithdrawCreateWaitAmount)]
        private async Task WithdrawCreateWaitAmount_ProcessorAsync(UserModel user, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            var asset = user.MessagesWithdrawModel.Asset;
            var network = user.MessagesWithdrawModel.Network;
            var infoForWithdraw = await _infoService.GetInfoForWithdrawAsset(asset, network);

            var withdrawEnabledStatus = await WithdrawEnabledStatus(user, callbackQuery, cancellationToken, asset, network, infoForWithdraw);
            if (!withdrawEnabledStatus)
                return;

            await _unitOfWork.UserRepository.UpdateUserWhereMenu(user.UserId, MessageRequestRoute.WithdrawCreateMessageAmount);
            await _unitOfWork.SaveChanges();

            await _botMenu.Wallet.WithdrawCreateWaitAmount(user, infoForWithdraw, cancellationToken);
        }

        [BotRequestRoute(CallbackRequestRoute.WithdrawCreatePutAmount)]
        private async Task WithdrawCreatePutAmount_ProcessorAsync(UserModel user, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            string[] parts = callbackQuery.Data!.Split(':');
            decimal amount = decimal.Parse(parts[1]);

            var asset = user.MessagesWithdrawModel.Asset;
            var network = user.MessagesWithdrawModel.Network;

            var infoForWithdraw = await _infoService.GetInfoForWithdrawAsset(asset, network);

            var withdrawEnabledStatus = await WithdrawEnabledStatus(user, callbackQuery, cancellationToken, asset, network, infoForWithdraw);
            if (!withdrawEnabledStatus)
                return;

            if (ValidateData.ValidateWithdrawCreate_Amount(user, amount, infoForWithdraw))
            {
                await _unitOfWork.MessagesWithdrawRepository.UpdateMessagesWithdrawAmount(user.UserId, amount);
                await _unitOfWork.UserRepository.UpdateUserWhereMenu(user.UserId, MessageRequestRoute.Default);
                await _unitOfWork.SaveChanges();

                await _botMenu.Wallet.WithdrawConfirm(user, infoForWithdraw, cancellationToken);
            }
            else
            {
                await _botMenu.Wallet.WithdrawCreateNotCorrectAmount(user, cancellationToken);
            }
        }

        [BotRequestRoute(CallbackRequestRoute.WithdrawCreateWaitAssetPutPage)]
        private async Task WithdrawCreateWaitAssetPutPage_ProcessorAsync(UserModel user, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            string[] parts = callbackQuery.Data!.Split(':');
            int num = int.Parse(parts[1]);

            var withdrawEnabledStatus = await WithdrawEnabledStatus(user, callbackQuery, cancellationToken);
            if (!withdrawEnabledStatus)
                return;

            await _botMenu.Wallet.WithdrawCreateWaitAsset(user, cancellationToken, num);
        }

        [BotRequestRoute(CallbackRequestRoute.WithdrawConfirm)]
        private async Task WithdrawConfirm_ProcessorAsync(UserModel user, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            var asset = user.MessagesWithdrawModel.Asset;
            var network = user.MessagesWithdrawModel.Network;

            var infoForWithdraw = await _infoService.GetInfoForWithdrawAsset(asset, network);

            var withdrawEnabledStatus = await WithdrawEnabledStatus(user, callbackQuery, cancellationToken, asset, network, infoForWithdraw);
            if (!withdrawEnabledStatus)
                return;

            if (ValidateData.ValidateWithdrawCreate_Amount(user, user.MessagesWithdrawModel.Amount, infoForWithdraw))
            {
                await _walletService.WithdrawConfirm(user, infoForWithdraw);

                await _botMenu.Wallet.WithdrawSentAnswer(user, callbackQuery.Id, cancellationToken);

                var blavedAssets = await _infoService.GetInfoForBlavedAsset();
                await _botMenu.Wallet.Wallet(user, blavedAssets, cancellationToken);
            }
            else
            {
                throw new Exception("Ошибка при проверке валидности вывода");
            }
            

        }

        #endregion

        #region Exchange

        [BotRequestRoute(CallbackRequestRoute.ExchangeCreateWaitFromAssetPutPage)]
        private async Task ExchangeCreateWaitFromAssetPutPage_ProcessorAsync(UserModel user, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            string[] parts = callbackQuery.Data!.Split(':');
            int num = int.Parse(parts[1]);

            if (!_appConfig.FunctionConfiguration.FunctionEnable.Exchange)
            {
                await _botMenu.Main.FunctionDisabledAnswer(user, callbackQuery.Id, cancellationToken);
                return;
            }

            await _botMenu.Exchange.ExchangeCreateWaitFromAsset(user, cancellationToken, num);
        }

        [BotRequestRoute(CallbackRequestRoute.ExchangeCreateWaitToAssetPutFromAssetPage)]
        private async Task ExchangeCreateWaitToAssetPutFromAssetPage_ProcessorAsync(UserModel user, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            string[] parts = callbackQuery.Data!.Split(':');
            string[] partsPut = parts[1].Split('_');

            string fromAsset = partsPut[0];
            int num = int.Parse(partsPut[1]);

            if (!_appConfig.FunctionConfiguration.AssetEnable.Exchange[fromAsset])
            {
                await _botMenu.Main.AssetDisabledAnswer(user, callbackQuery.Id, cancellationToken);
                return;
            }
            if (!_appConfig.FunctionConfiguration.FunctionEnable.Exchange)
            {
                await _botMenu.Main.FunctionDisabledAnswer(user, callbackQuery.Id, cancellationToken);
                return;
            }

            var infoForExchangeUnionList = await _infoService.GetInfoForExchange(fromAsset);

            if(infoForExchangeUnionList.Count > 0)
            {
                await _unitOfWork.UserRepository.UpdateUserWhereMenu(user.UserId, MessageRequestRoute.Default);
                await _unitOfWork.SaveChanges();

                await _botMenu.Exchange.ExchangeCreateWaitToAsset(user, fromAsset, infoForExchangeUnionList, cancellationToken, num);
            }
            else
            {
                await _botMenu.Exchange.ExchangeCreateCoinDontHavePairAnswer(user, callbackQuery.Id, cancellationToken);
            }
        }

        [BotRequestRoute(CallbackRequestRoute.ExchangeCreateWaitAmountPutFromAssetToAsset)]
        private async Task ExchangeCreateWaitAmountPutFromAssetToAsset_ProcessorAsync(UserModel user, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            string[] parts = callbackQuery.Data!.Split(':');
            string[] partsPut = parts[1].Split('_');

            string fromAsset = partsPut[0];
            string toAsset = partsPut[1];

            if (!_appConfig.FunctionConfiguration.AssetEnable.Exchange[fromAsset])
            {
                await _botMenu.Main.AssetDisabledAnswer(user, callbackQuery.Id, cancellationToken);
                return;
            }
            if (!_appConfig.FunctionConfiguration.FunctionEnable.Exchange)
            {
                await _botMenu.Main.FunctionDisabledAnswer(user, callbackQuery.Id, cancellationToken);
                return;
            }

            var infoForExchange = await _infoService.GetInfoForExchange(fromAsset, toAsset);

            if(ValidateData.ValidateExchangeCreate_Balance(user, infoForExchange))
            {
                await _unitOfWork.MessagesExchangeRepository.UpdateMessagesExchangeFromAsset(user.UserId, fromAsset);
                await _unitOfWork.MessagesExchangeRepository.UpdateMessagesExchangeToAsset(user.UserId, toAsset);
                await _unitOfWork.UserRepository.UpdateUserWhereMenu(user.UserId, MessageRequestRoute.ExchangeCreateMessageAmount);
                await _unitOfWork.SaveChanges();
                await _botMenu.Exchange.ExchangeCreateWaitAmount(user, infoForExchange, cancellationToken);
            }
            else
            {
                await _botMenu.Exchange.ExchangeCreateInsufficientBalanceAnswer(user, infoForExchange, callbackQuery.Id, cancellationToken);
            }
        }

        [BotRequestRoute(CallbackRequestRoute.ExchangeCreatePutAmount)]
        private async Task ExchangeCreatePutAmount_ProcessorAsync(UserModel user, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            string[] parts = callbackQuery.Data!.Split(':');
            decimal amount = decimal.Parse(parts[1]);

            if (!_appConfig.FunctionConfiguration.AssetEnable.Exchange[user.MessagesExchangeModel.FromAsset])
            {
                await _botMenu.Main.AssetDisabledAnswer(user, callbackQuery.Id, cancellationToken);
                return;
            }
            if (!_appConfig.FunctionConfiguration.FunctionEnable.Exchange)
            {
                await _botMenu.Main.FunctionDisabledAnswer(user, callbackQuery.Id, cancellationToken);
                return;
            }

            var infoForExchange = await _infoService.GetInfoForExchange(user.MessagesExchangeModel.FromAsset, user.MessagesExchangeModel.ToAsset);

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
                        await _botMenu.Exchange.ExchangeConfirm(user, quote.Data, cancellationToken);
                    }
                    else
                    {
                        await _botMenu.Exchange.ExchangeCreateErrorQuote(user, cancellationToken);
                    }
                }
            }
            else
            {
                await _botMenu.Exchange.ExchangeCreateNotCorrectAmount(user, infoForExchange, cancellationToken);
            }
        }


        [BotRequestRoute(CallbackRequestRoute.ExchangeConvertConfirmPutQuoteId)]
        private async Task ExchangeConvertConfirm_ProcessorAsync(UserModel user, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            string[] parts = callbackQuery.Data!.Split(':');
            string quote = parts[1];

            if (!_appConfig.FunctionConfiguration.AssetEnable.Exchange[user.MessagesExchangeModel.FromAsset])
            {
                await _botMenu.Main.AssetDisabledAnswer(user, callbackQuery.Id, cancellationToken);
                return;
            }
            if (!_appConfig.FunctionConfiguration.FunctionEnable.Exchange)
            {
                await _botMenu.Main.FunctionDisabledAnswer(user, callbackQuery.Id, cancellationToken);
                return;
            }

            var infoForExchange = await _infoService.GetInfoForExchange(user.MessagesExchangeModel.FromAsset, user.MessagesExchangeModel.ToAsset);

            if (ValidateData.ValidateExchangeCreate_Amount(user, user.MessagesExchangeModel.Amount, infoForExchange))
            {
                var result = await _exchangeService.ConvertQuoteAccept(user, quote, infoForExchange);
                if (result.Status && result.Data != null)
                {
                    await _botMenu.Exchange.ExchangeCompleted(user, result.Data, cancellationToken);
                }
                else 
                { 
                    await _botMenu.Exchange.ExchangeCreateErrorOverdueConfirmAnswer(user, callbackQuery.Id, cancellationToken);

                    var quoteNew = await _exchangeService.ConvertQuoteRequest(user, infoForExchange);
                    if (quoteNew.Status && quoteNew.Data != null)
                    {
                        await _botMenu.Exchange.ExchangeConfirm(user, quoteNew.Data, cancellationToken);
                    }
                    else
                    {
                        await _botMenu.Exchange.ExchangeCreateErrorQuote(user, cancellationToken);
                    }
                }
            }
            else { throw new Exception(); }
        }

        [BotRequestRoute(CallbackRequestRoute.Exchange)]
        private async Task Exchange_ProcessorAsync(UserModel user, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            if (!_appConfig.FunctionConfiguration.FunctionEnable.Exchange)
            {
                await _botMenu.Main.FunctionDisabledAnswer(user, callbackQuery.Id, cancellationToken);
                return;
            }

            await _botMenu.Exchange.Exchange(user, cancellationToken);
        }


        [BotRequestRoute(CallbackRequestRoute.ExchangeHistoryViewPutNum)]
        private async Task ExchangeHistoryViewPutNum_ProcessorAsync(UserModel user, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            string[] parts = callbackQuery.Data!.Split(':');
            int num = int.Parse(parts[1]);

            if (!_appConfig.FunctionConfiguration.FunctionEnable.Exchange)
            {
                await _botMenu.Main.FunctionDisabledAnswer(user, callbackQuery.Id, cancellationToken);
                return;
            }

            var result = await _unitOfWork.ExchangeRepository.GetExchange(num);
            if (result != null)
            {
                await _botMenu.Exchange.ExchangeHistoryView(user, result, cancellationToken);
            }
            else { throw new Exception(); }
        }

        [BotRequestRoute(CallbackRequestRoute.ExchangeHistoryListPutPage)]
        private async Task ExchangeHistoryListPutPage_ProcessorAsync(UserModel user, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            string[] parts = callbackQuery.Data!.Split(':');
            int page = int.Parse(parts[1]);

            if (!_appConfig.FunctionConfiguration.FunctionEnable.Exchange)
            {
                await _botMenu.Main.FunctionDisabledAnswer(user, callbackQuery.Id, cancellationToken);
                return;
            }

            var result = await _unitOfWork.ExchangeRepository.GetExchangeList(user.UserId);
            if (result.Count > 0)
            {
                await _botMenu.Exchange.ExchangeHistoryList(user, result, cancellationToken, page);
            }
            else
            {
                await _botMenu.Exchange.ExchangeHistoryDontHaveAnswer(user, callbackQuery.Id, cancellationToken);
            }
        }

        #endregion

        #region BlavedPay

        [BotRequestRoute(CallbackRequestRoute.BlavedPay)]
        private async Task BlavedPay_ProcessorAsync(UserModel user, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            if (!_appConfig.FunctionConfiguration.FunctionEnable.BlavedPay)
            {
                await _botMenu.Main.FunctionDisabledAnswer(user, callbackQuery.Id, cancellationToken);
                return;
            }

            await _botMenu.BlavedPay.BlavedPay(user, cancellationToken);

        }

        [BotRequestRoute(CallbackRequestRoute.BlavedPayID)]
        private async Task BlavedPayID_ProcessorAsync(UserModel user, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            if (!_appConfig.FunctionConfiguration.FunctionEnable.BlavedPay)
            {
                await _botMenu.Main.FunctionDisabledAnswer(user, callbackQuery.Id, cancellationToken);
                return;
            }

            await _botMenu.BlavedPay.BlavedPayID(user, cancellationToken);
        }

        [BotRequestRoute(CallbackRequestRoute.BlavedPayIDTransferCreateWaitAssetPutPage)]
        private async Task BlavedPayIDTransferCreateWaitAssetPutPage_ProcessorAsync(UserModel user, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            string[] parts = callbackQuery.Data!.Split(':');
            int num = int.Parse(parts[1]);

            if (!_appConfig.FunctionConfiguration.FunctionEnable.BlavedPay)
            {
                await _botMenu.Main.FunctionDisabledAnswer(user, callbackQuery.Id, cancellationToken);
                return;
            }

            await _unitOfWork.UserRepository.UpdateUserWhereMenu(user.UserId, MessageRequestRoute.Default);
            await _unitOfWork.SaveChanges();

            await _botMenu.BlavedPay.BlavedPayIDTransferCreateWaitAsset(user, cancellationToken, num);

        }

        [BotRequestRoute(CallbackRequestRoute.BlavedPayIDTransferCreateWaitIdPutAsset)]
        private async Task BlavedPayIDTransferCreateWaitIdPutAsset_ProcessorAsync(UserModel user, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            string[] parts = callbackQuery.Data!.Split(':');
            string Asset = parts[1];

            if (!_appConfig.FunctionConfiguration.AssetEnable.BlavedPay[Asset])
            {
                await _botMenu.Main.AssetDisabledAnswer(user, callbackQuery.Id, cancellationToken);
                return;
            }
            if (!_appConfig.FunctionConfiguration.FunctionEnable.BlavedPay)
            {
                await _botMenu.Main.FunctionDisabledAnswer(user, callbackQuery.Id, cancellationToken);
                return;
            }

            var infoForBlavedPay = await _infoService.GetInfoForBlavedPayID(Asset);

            if (ValidateData.ValidateBlavedPayIDTransferCreate_Balance(user, infoForBlavedPay)) 
            {
                await _unitOfWork.MessagesBlavedPayIDRepository.UpdateMessagesBlavedPayIDAsset(user.UserId, Asset);
                await _unitOfWork.UserRepository.UpdateUserWhereMenu(user.UserId, MessageRequestRoute.BlavedPayIDTransferCreateMessageId);
                await _unitOfWork.SaveChanges();
                await _botMenu.BlavedPay.BlavedPayIDTransferCreateWaitID(user, Asset, cancellationToken);
            }
            else
            {
                await _botMenu.BlavedPay.BlavedPayIDTransferCreateInsufficientBalanceAnswer(user, infoForBlavedPay, callbackQuery.Id, cancellationToken);
            }
        }

        [BotRequestRoute(CallbackRequestRoute.BlavedPayIDTransferCreateWaitAmount)]
        private async Task BlavedPayIDTransferCreateWaitAmount_ProcessorAsync(UserModel user, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            if (!_appConfig.FunctionConfiguration.AssetEnable.BlavedPay[user.MessagesBlavedPayIDModel.Asset])
            {
                await _botMenu.Main.AssetDisabledAnswer(user, callbackQuery.Id, cancellationToken);
                return;
            }
            if (!_appConfig.FunctionConfiguration.FunctionEnable.BlavedPay)
            {
                await _botMenu.Main.FunctionDisabledAnswer(user, callbackQuery.Id, cancellationToken);
                return;
            }

            await _unitOfWork.UserRepository.UpdateUserWhereMenu(user.UserId, MessageRequestRoute.BlavedPayIDTransferCreateMessageAmount);
            await _unitOfWork.SaveChanges();

            await _botMenu.BlavedPay.BlavedPayIDTransferCreateWaitAmount(user, cancellationToken);
        }

        [BotRequestRoute(CallbackRequestRoute.BlavedPayIDTransferCreatePutAmount)]
        private async Task BlavedPayIDTransferCreatePutAmount_ProcessorAsync(UserModel user, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            string[] parts = callbackQuery.Data!.Split(':');
            decimal amount = decimal.Parse(parts[1]);

            if (!_appConfig.FunctionConfiguration.AssetEnable.BlavedPay[user.MessagesBlavedPayIDModel.Asset])
            {
                await _botMenu.Main.AssetDisabledAnswer(user, callbackQuery.Id, cancellationToken);
                return;
            }
            if (!_appConfig.FunctionConfiguration.FunctionEnable.BlavedPay)
            {
                await _botMenu.Main.FunctionDisabledAnswer(user, callbackQuery.Id, cancellationToken);
                return;
            }

            var infoForBlavedPay = await _infoService.GetInfoForBlavedPayID(user.MessagesBlavedPayIDModel.Asset);

            if (ValidateData.ValidateBlavedPayIDTransferCreate_Amount(user, amount, infoForBlavedPay))
            {
                await _unitOfWork.MessagesBlavedPayIDRepository.UpdateMessagesBlavedPayIDAmount(user.UserId, amount);
                await _unitOfWork.UserRepository.UpdateUserWhereMenu(user.UserId, MessageRequestRoute.Default);
                await _unitOfWork.SaveChanges();

                await _botMenu.BlavedPay.BlavedPayIDTransferConfirm(user, cancellationToken);
            }
            else
            {
                await _botMenu.BlavedPay.BlavedPayIDTransferCreateNotCorrectAmount(user, cancellationToken);
            }
        }

        [BotRequestRoute(CallbackRequestRoute.BlavedPayIDTransferConfirm)]
        private async Task BlavedPayIDTransferConfirm_ProcessorAsync(UserModel user, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            if (!_appConfig.FunctionConfiguration.AssetEnable.BlavedPay[user.MessagesBlavedPayIDModel.Asset])
            {
                await _botMenu.Main.AssetDisabledAnswer(user, callbackQuery.Id, cancellationToken);
                return;
            }
            if (!_appConfig.FunctionConfiguration.FunctionEnable.BlavedPay)
            {
                await _botMenu.Main.FunctionDisabledAnswer(user, callbackQuery.Id, cancellationToken);
                return;
            }

            var infoForBlavedPay = await _infoService.GetInfoForBlavedPayID(user.MessagesBlavedPayIDModel.Asset);

            if (ValidateData.ValidateBlavedPayIDTransferCreate_Amount(user, user.MessagesBlavedPayIDModel.Amount, infoForBlavedPay))
            {
                var result = await _blavedPayService.BlavedPayIDTransferConfirm(user);

                await _botMenu.BlavedPay.BlavedPayIDTransferCompleted(user, result, cancellationToken);

                var toUser = await _unitOfWork.UserRepository.GetUser(result.ToUserId);
                if (toUser != null && toUser.EnabledNotificationsBlavedPay)
                {
                    var messageAlert = await _botMenu.BlavedPay.BlavedPayIDTransferAlert(toUser!, result, cancellationToken, false);
                    await _unitOfWork.UserRepository.UpdateUserMessageId(toUser!.UserId, messageAlert!.MessageId);
                    await _unitOfWork.SaveChanges();
                }
            }
            else
            {
                throw new Exception();
            }
        }

       
        [BotRequestRoute(CallbackRequestRoute.BlavedPayIDTransferHistoryViewPutNum)]
        private async Task BlavedPayIDTransferHistoryViewPutNum_ProcessorAsync(UserModel user, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            string[] parts = callbackQuery.Data!.Split(':');
            int num = int.Parse(parts[1]);

            if (!_appConfig.FunctionConfiguration.FunctionEnable.BlavedPay)
            {
                await _botMenu.Main.FunctionDisabledAnswer(user, callbackQuery.Id, cancellationToken);
                return;
            }

            var result = await _unitOfWork.BlavedPayIDRepository.GetBlavedPayIDTransfer(num);
            if (result != null)
            {
                await _botMenu.BlavedPay.BlavedPayIDTransferHistoryView(user, result, cancellationToken);
            }
            else { throw new Exception(); }
        }

        [BotRequestRoute(CallbackRequestRoute.BlavedPayIDTransferHistoryListPutPage)]
        private async Task BlavedPayIDTransferHistoryListPutPage_ProcessorAsync(UserModel user, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            string[] parts = callbackQuery.Data!.Split(':');
            int page = int.Parse(parts[1]);

            if (!_appConfig.FunctionConfiguration.FunctionEnable.BlavedPay)
            {
                await _botMenu.Main.FunctionDisabledAnswer(user, callbackQuery.Id, cancellationToken);
                return;
            }

            var result = await _unitOfWork.BlavedPayIDRepository.GetBlavedPayIDTransferList(user.UserId);
            if (result.Count > 0)
            {
                await _botMenu.BlavedPay.BlavedPayIDTransferHistoryList(user, result, cancellationToken, page);
            }
            else
            {
                await _botMenu.BlavedPay.BlavedPayIDTransferHistoryDontHaveAnswer(user, callbackQuery.Id, cancellationToken);
            }
        }

        [BotRequestRoute(CallbackRequestRoute.BlavedPayNotificationsDisableBlavedPay)]
        private async Task BlavedPayNotificationsDisableBlavedPay_ProcessorAsync(UserModel user, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            await _unitOfWork.UserRepository.UpdateEnabledNotificationsBlavedPay(user.UserId, false);
            await _unitOfWork.SaveChanges();

            await _botMenu.BlavedPay.BlavedPayNotificationDisableAnswer(user, callbackQuery.Id, cancellationToken);
            await _botMenu.BlavedPay.BlavedPay(user, cancellationToken); 
        }

        #endregion
    }
}
