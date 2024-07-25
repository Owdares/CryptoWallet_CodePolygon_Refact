using Blaved.Core.Interfaces;
using Blaved.Core.Interfaces.Services;
using Blaved.Core.Interfaces.Services.BlockChain;
using Blaved.Core.Objects.Models;
using Blaved.Core.Objects.Models.Configurations;
using Blaved.Core.Services;
using Microsoft.Extensions.Options;
using Blaved.Core.Objects.Models.Cryptocurrency;

namespace Blaved.TelegramBot.Server.Services
{
    public class AlertsService : IAlertsService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBotMenu _botMenu;
        private readonly IInfoService _infoService;

        public AlertsService(IUnitOfWork unitOfWork, IBotMenu botMenu, IInfoService infoService)
        {
            _infoService = infoService;
            _unitOfWork = unitOfWork;
            _botMenu = botMenu;
        }
        public async Task WithdrawCompletedAlert(WithdrawModel transaction)
        {
            var user = await _unitOfWork.UserRepository.GetUser(transaction.UserId);

            var message = await _botMenu.Wallet.WithdrawCompletedAlert(user!, transaction, false);
            await _unitOfWork.UserRepository.UpdateUserMessageId(user!.UserId, message!.MessageId);
            await _unitOfWork.SaveChanges();
        }
        public async Task DepositCompletedAlert(DepositModel depositModel)
        {
            var user = await _unitOfWork.UserRepository.GetUser(depositModel.UserId);
            var assetInfo = await _infoService.GetInfoForDepositAsset(depositModel.Asset, depositModel.Network);

            var message = await _botMenu.Wallet.DepositAlert(user!, assetInfo, depositModel, false);
            await _unitOfWork.UserRepository.UpdateUserMessageId(user!.UserId, message!.MessageId);
            await _unitOfWork.SaveChanges();
        }
    }
}
