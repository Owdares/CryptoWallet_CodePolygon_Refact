using Blaved.Core.Objects.Models;

namespace Blaved.Core.Interfaces.Repository
{
    public interface ICheckRepository
    {
        Task<CheckModel?> GetCheck(int num);
        Task<CheckModel?> GetCheck(string url);
        Task<List<CheckModel>> GetCheckList(long userId);
        Task<List<CheckModel>> GetCheckList();
        Task AddCheck(CheckModel check);
        Task SubtractFromCheckCount(string url, int useCount);
        Task<CheckModel> UpdateCheckPassword(string url, string password);
        Task<CheckModel> DeleteCheckPassword(string url);
        Task DeleteCheck(string url);
    }
}
