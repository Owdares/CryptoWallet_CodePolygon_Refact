using Blaved.Core.Interfaces;
using Blaved.Core.Interfaces.Services.BlockChain;
using Blaved.Core.Interfaces.Services;
using Blaved.Core.Objects.Models;
using Blaved.Core.Objects.Models.Configurations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog.Context;

namespace Blaved.Core.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBlockChainAccountService _accountService;
        private readonly AppConfig _appConfig;
        private readonly ILogger<UserService> _logger;
        public UserService(IUnitOfWork unitOfWork, IBlockChainAccountService accountService,
            IOptions<AppConfig> appConfig, ILogger<UserService> logger)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _accountService = accountService;
            _appConfig = appConfig.Value;
        }
        public async Task UserRegistration(long userId, string? firstName, string? lastName, long? whoseReferral, string? language, bool acceptedTermsOfUse = false, int RateReferralExchange = 30)
        {
            LogContext.PushProperty("FirstName", firstName);
            LogContext.PushProperty("LastName", lastName);
            LogContext.PushProperty("Language", language);
            LogContext.PushProperty("WhoseReferral", whoseReferral);

            _logger.LogInformation("Start user registration");

            var userCount = await _unitOfWork.UserRepository.GetUsersCount();
            var accountBEP20 = _accountService.GetNewChainAccount(userCount, "BSC");
            var accountERC20 = _accountService.GetNewChainAccount(userCount, "ETH");
            var accountPolygon = _accountService.GetNewChainAccount(userCount, "MATIC");

            language = language != null && _appConfig.AssetConfiguration.LanguageAbbreviations.TryGetValue(language, out var abbreviation) && _appConfig.AssetConfiguration.LanguageList.Contains(abbreviation)
                ? abbreviation
                : "EN";

            var userModel = new UserModel()
            {
                UserId = userId,
                FirstName = firstName,
                Language = language,
                IsBanned = false,
                LastName = lastName,
                RateReferralExchange = RateReferralExchange,
                WhereMenu = "Default",
                WhoseReferral = whoseReferral,
                Status = "User",
                AcceptedTermsOfUse = acceptedTermsOfUse,
                EnabledNotificationsBlavedPay = true,
                BalanceModel = new BalanceModel()
                {
                    //значения по умолчанию decimal
                },
                BlockChainWalletModel = new BlockChainWalletModel()
                {
                    AddressBSC = accountBEP20.Address,
                    AddressETH = accountERC20.Address,
                    AddressMATIC = accountPolygon.Address,
                    PrivatKeyBSC = accountBEP20.PrivateKey,
                    PrivatKeyETH = accountERC20.PrivateKey,
                    PrivatKeyMATIC = accountPolygon.PrivateKey

                },
                BonusBalanceModel = new BonusBalanceModel()
                {
                    //значения по умолчанию decimal
                },

                MessagesBlavedPayIDModel = new MessagesBlavedPayIDModel()
                {
                    //значения по умолчанию decimal
                    Asset = "",
                },
                MessagesCheckModel = new MessagesCheckModel()
                {
                    //значения по умолчанию decimal
                    Asset = "",
                },
                MessagesExchangeModel = new MessagesExchangeModel()
                {
                    //значения по умолчанию decimal
                    FromAsset = "",
                    ToAsset = "",
                },
                MessagesWithdrawModel = new MessagesWithdrawModel()
                {
                    //значения по умолчанию decimal
                    Asset = "",
                    Network = "",
                    Address = ""
                },
            };

            await _unitOfWork.UserRepository.AddUser(userModel);
            await _unitOfWork.SaveChanges();

            _logger.LogInformation("User registration completed");
        }
    }
}
