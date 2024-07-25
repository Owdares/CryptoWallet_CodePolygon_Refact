using Blaved.Core.Objects.Models;

namespace Blaved.Core.Interfaces.Repository
{
    public interface ICheckActivatedRepository
    {
        Task AddCheckActivated(CheckActivatedModel checkActivated);
        Task<CheckActivatedModel?> GetCheckActivated(long userId, string url);
    }
}
