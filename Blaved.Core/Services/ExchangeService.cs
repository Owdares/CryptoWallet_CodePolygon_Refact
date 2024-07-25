using Binance.Net.Objects.Models.Spot.Convert;
using Blaved.Core.Interfaces;
using Blaved.Core.Interfaces.Services;
using Blaved.Core.Interfaces.Services.Binance;
using Blaved.Core.Objects.Models;
using Blaved.Core.Objects.Models.Configurations;
using Blaved.Core.Objects.Models.Info;
using Blaved.Core.Utility;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Blaved.Core.Services
{
    public class ExchangeService : IExchangeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBinanceService _binanceService;
        private readonly AppConfig _appConfig;
        private readonly ILogger<ExchangeService> _logger;

        public ExchangeService(IUnitOfWork unitOfWork, IBinanceService binanceService, IOptions<AppConfig> appConfig, ILogger<ExchangeService> logger)
        {
            _logger = logger;
            _appConfig = appConfig.Value;
            _unitOfWork = unitOfWork;
            _binanceService = binanceService;
        }

        public async Task<Result<ExchangeQuoteModel>> ConvertQuoteRequest(UserModel user, InfoForExchangeModel infoForExchangeModel)
        {
            decimal internalFee = infoForExchangeModel.ExchangeInternalFee;

            _logger.LogInformation("Convert quote request started: {@Request}", new { user.MessagesExchangeModel.FromAsset, user.MessagesExchangeModel.ToAsset, user.MessagesExchangeModel.Amount });

            var hiddenFee = CalculationTool.CalculationHiddenExchangeFee(user.MessagesExchangeModel.Amount, _appConfig.FunctionConfiguration.ExchangeHiddenFeeInPercent).AmountRound();
            var amountForQuote = (user.MessagesExchangeModel.Amount - hiddenFee).AmountRound();

            var quote = await _binanceService.ConvertQuote(user.MessagesExchangeModel.FromAsset, user.MessagesExchangeModel.ToAsset, amountForQuote);
            if (!quote.Status || quote.Data == null)
            {
                return new Result<ExchangeQuoteModel>(false, null);
            }

            var exchangeQuoteModel = new ExchangeQuoteModel()
            {
                Fee = internalFee,
                FromAmount = user.MessagesExchangeModel.Amount,
                QuoteId = quote.Data.QuoteId,
                ToAmount = quote.Data.BaseQuantity,
                FromAsset = user.MessagesExchangeModel.FromAsset,
                ToAsset = user.MessagesExchangeModel.ToAsset
            };

            _logger.LogInformation("Convert quote request completed: {@Response}",
               new
               {
                   exchangeQuoteModel.FromAmount,
                   exchangeQuoteModel.FromAsset,
                   exchangeQuoteModel.ToAmount,
                   exchangeQuoteModel.ToAsset,
                   exchangeQuoteModel.Fee,
                   hiddenFee
               });

            return new Result<ExchangeQuoteModel>(true, exchangeQuoteModel);
        }
        public async Task<Result<ExchangeModel>> ConvertQuoteAccept(UserModel user, string quoteId, InfoForExchangeModel infoForExchangeModel)
        {
            _logger.LogInformation("Convert started: {@Request}", new { quoteId });

            var quoteAccept = await _binanceService.ConvertQuoteAccept(quoteId);
            if (!quoteAccept.Status || quoteAccept.Data == null)
            {
                return new Result<ExchangeModel>(false, null);
            }

            const int maxRetries = 5;
            const int delayMilliseconds = 1000;

            BinanceConvertOrderStatus? binanceConvertOrderStatus = null;
            for (int attempt = 0; attempt < maxRetries; attempt++)
            {
                var convertModel = await _binanceService.GetConvertOrderStatus(quoteAccept.Data.OrderId);

                if (convertModel.Status && convertModel.Data != null)
                {
                    if (convertModel.Data.Status == Binance.Net.Enums.ConvertOrderStatus.Success ||
                        convertModel.Data.Status == Binance.Net.Enums.ConvertOrderStatus.AcceptSuccess)
                    {
                        binanceConvertOrderStatus = convertModel.Data;
                        break;
                    }
                    else if (convertModel.Data.Status == Binance.Net.Enums.ConvertOrderStatus.Fail)
                    {
                        _logger.LogError($"Getting convert status failed - Data Status = Fail");
                        return new Result<ExchangeModel>(false, null);
                    }
                }
                else
                {
                    return new Result<ExchangeModel>(false, null);
                }

                await Task.Delay(delayMilliseconds);
            }
            if (binanceConvertOrderStatus == null)
            {
                _logger.LogError($"Receiving convert status failed - timed out");
                return new Result<ExchangeModel>(false, null);
            }

            var hiddenFee = CalculationTool.CalculationHiddenExchangeFee(user.MessagesExchangeModel.Amount, _appConfig.FunctionConfiguration.ExchangeHiddenFeeInPercent).AmountRound();
            decimal internalFee = infoForExchangeModel.ExchangeInternalFee;
            decimal chargeToCapital = (internalFee + hiddenFee).AmountRound();

            var exchangeModel = new ExchangeModel()
            {
                ExchangeMethod = infoForExchangeModel.Method,
                ExchangeId = binanceConvertOrderStatus.OrderId,
                ToAsset = binanceConvertOrderStatus.BaseAsset,
                FromAsset = binanceConvertOrderStatus.QuoteAsset,
                UserId = user.UserId,
                FromAmount = user.MessagesExchangeModel.Amount,
                ToAmount = binanceConvertOrderStatus.BaseQuantity,
                Fee = internalFee,
                HiddenFee = hiddenFee,
                ChargeToCapital = chargeToCapital
            };

            if (user.WhoseReferral is { } whoseReferral)
            {
                var referralUser = await _unitOfWork.UserRepository.GetUser(whoseReferral);
                if (referralUser != null)
                {
                    var amountForReferal = CalculationTool.CalculationReferalRate(exchangeModel.Fee, referralUser.RateReferralExchange).AmountRound();
                    exchangeModel.ChargeToCapital = (exchangeModel.ChargeToCapital - amountForReferal).AmountRound();
                    exchangeModel.ChargeToReferral = amountForReferal;
                    await _unitOfWork.BonusBalanceRepository.AddToBonusBalance(referralUser.UserId, amountForReferal, exchangeModel.FromAsset);

                    _logger.LogInformation("Referral convert commission has been deducted: {@Referral}", new { referralUser.UserId, amountForReferal, exchangeModel.FromAsset });
                }
            }

            decimal amountForSubstact = (exchangeModel.FromAmount + internalFee).AmountRound();

            await _unitOfWork.BalanceRepository.SubtractFromBalance(exchangeModel.UserId, amountForSubstact, exchangeModel.FromAsset);
            await _unitOfWork.BalanceRepository.AddToBalance(exchangeModel.UserId, exchangeModel.ToAmount, exchangeModel.ToAsset);
            await _unitOfWork.ExchangeRepository.AddExchange(exchangeModel);
            await _unitOfWork.SaveChanges();

            _logger.LogInformation("Convert completed: {@Response}",
                new
                {
                    exchangeModel.FromAmount,
                    exchangeModel.FromAsset,
                    exchangeModel.ToAmount,
                    exchangeModel.ToAsset,
                    exchangeModel.Fee,
                    exchangeModel.HiddenFee
                });

            return new Result<ExchangeModel>(true, exchangeModel);

        }
    }
}
