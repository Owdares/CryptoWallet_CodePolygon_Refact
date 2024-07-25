using Blaved.Core.Objects.Models;

namespace Blaved.Core.Interfaces.Services
{
    public interface IBlavedPayService
    {
        Task<BlavedPayIDTransferModel> BlavedPayIDTransferConfirm(UserModel user);
    }
}
