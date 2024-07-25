using Blaved.Core.Objects.Models;

using Blaved.Core.Objects.Models.Cryptocurrency;

namespace Blaved.Core.Interfaces.Services
{
    public interface IWalletService
    {
        Task<bool> BonusBalanceToBalance(UserModel user);
        Task WithdrawConfirm(UserModel user, CryptoAssetModel assetModel);
        Task WithdrawValidate();
        Task DepositScanTransaction(string network);
    }
}
