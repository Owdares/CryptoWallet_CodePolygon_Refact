using Blaved.Core.Interfaces;
using Blaved.Core.Interfaces.Services;
using Blaved.Core.Objects.Models;
using Microsoft.Extensions.Logging;

namespace Blaved.Core.Services
{
    public class BlavedPayService : IBlavedPayService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<BlavedPayService> _logger;

        public BlavedPayService(IUnitOfWork unitOfWork, ILogger<BlavedPayService> logger)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }
        public async Task<BlavedPayIDTransferModel> BlavedPayIDTransferConfirm(UserModel user)
        {
            _logger.LogInformation("Transfer started Blaved Pay ID");

            var amount = user.MessagesBlavedPayIDModel.Amount;
            var asset = user.MessagesBlavedPayIDModel.Asset;
            var toUserId = user.MessagesBlavedPayIDModel.ToUserId;

            var blavedPayIDTransferModel = new BlavedPayIDTransferModel()
            {
                Amount = amount,
                Asset = asset,
                UserId = user.UserId,
                ToUserId = toUserId,
            };

            await _unitOfWork.BlavedPayIDRepository.AddBlavedPayIDTransfer(blavedPayIDTransferModel);
            await _unitOfWork.BalanceRepository.SubtractFromBalance(user.UserId, amount, asset);
            await _unitOfWork.BalanceRepository.AddToBalance(toUserId, amount, asset);

            await _unitOfWork.SaveChanges();

            _logger.LogInformation("Transfer completed Blaved Pay ID: {0}", new { blavedPayIDTransferModel.ToUserId, blavedPayIDTransferModel.Amount, blavedPayIDTransferModel.Asset });

            return blavedPayIDTransferModel;
        }
    }
}
