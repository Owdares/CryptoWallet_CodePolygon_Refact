using Blaved.Core.Objects.Models;
using Blaved.Core.Objects.Models.Cryptocurrency;

namespace Blaved.Core.Interfaces.Services
{
    public interface IWithdrawService
    {
        Task BinanceWithdraw(UserModel user, CryptoAssetModel assetModel);
        Task<List<WithdrawModel>> BinanceWithdrawValidateOrder();
    }
}
