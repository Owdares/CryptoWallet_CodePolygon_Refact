using Blaved.Core.Interfaces;
using Blaved.Core.Interfaces.Services;
using Blaved.Core.Objects.Models;
using Blaved.Core.Utility;
using Microsoft.Extensions.Logging;

namespace Blaved.Core.Services
{
    public class CheckService : ICheckService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CheckService> _logger;
        public CheckService(IUnitOfWork unitOfWork, ILogger<CheckService> logger)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }
        public async Task CheckDelete(string url)
        {
            _logger.LogInformation("Check deletion has started");

            var checkModel = await _unitOfWork.CheckRepository.GetCheck(url);
            if (checkModel == null)
            {
                throw new Exception($"Failed to receive receipt for deletion: {url}");
            }
            if (checkModel.Count > 0)
            {
                decimal amountForAdd = (checkModel.Count * checkModel.Amount).AmountRound();

                await _unitOfWork.BalanceRepository.AddToBalance(checkModel.UserId, amountForAdd, checkModel.Asset);

                _logger.LogInformation("Refund from check completed: {0}", new { checkModel.Url, checkModel.Amount, checkModel.Asset, checkModel.Count });

            }
            await _unitOfWork.CheckRepository.DeleteCheck(url);
            await _unitOfWork.SaveChanges();

            _logger.LogInformation("Check deletion completed: {0}", new { checkModel.Url, checkModel.Amount, checkModel.Asset, checkModel.Count });
        }
        public async Task<CheckModel> CheckCreate(long userId, string Asset, decimal amount, int count)
        {
            _logger.LogInformation("Check create has started");

            string url = GenerateNewUniqueUrl();

            var checkModel = new CheckModel()
            {
                UserId = userId,
                Asset = Asset,
                Count = count,
                Amount = amount,
                Url = url,
            };

            decimal amountForSubstract = (count * amount).AmountRound();

            await _unitOfWork.BalanceRepository.SubtractFromBalance(userId, amountForSubstract, Asset);
            await _unitOfWork.CheckRepository.AddCheck(checkModel);
            await _unitOfWork.SaveChanges();

            _logger.LogInformation("Check create completed: {0}", new { checkModel.Url, checkModel.Amount, checkModel.Asset, checkModel.Count });

            return checkModel;
        }
        public async Task CheckActivated(UserModel user, CheckModel checkModel)
        {
            _logger.LogInformation("Check activation has started");
            var checkActivatedModel = new CheckActivatedModel()
            {
                CheckId = checkModel.Id,
                UserId = user.UserId
            };
            await _unitOfWork.CheckRepository.SubtractFromCheckCount(checkModel.Url, 1);
            await _unitOfWork.CheckActivatedRepository.AddCheckActivated(checkActivatedModel);
            await _unitOfWork.BalanceRepository.AddToBalance(user.UserId, checkModel.Amount, checkModel.Asset);
            await _unitOfWork.SaveChanges();

            _logger.LogInformation("Check activation completed: {0}", new { checkModel.Url, checkModel.Amount, checkModel.Asset });
        }

        private string GenerateNewUniqueUrl()
        {
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            Random random = new Random();

            var randomBytes = new byte[8];
            random.NextBytes(randomBytes);

            return new string(randomBytes.Select(b => chars[b % chars.Length]).ToArray());
        }
    }
}
