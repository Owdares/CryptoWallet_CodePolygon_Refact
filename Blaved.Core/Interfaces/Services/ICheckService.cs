using Blaved.Core.Objects.Models;

namespace Blaved.Core.Interfaces.Services
{
    public interface ICheckService
    {
        Task CheckDelete(string url);
        Task<CheckModel> CheckCreate(long userId, string Asset, decimal amount, int count);
        Task CheckActivated(UserModel user, CheckModel checkModel);
    }
}
