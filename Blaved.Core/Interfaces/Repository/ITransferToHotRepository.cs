using Blaved.Core.Objects.Models;

namespace Blaved.Core.Interfaces.Repository
{
    public interface ITransferToHotRepository
    {
        Task AddTransferToHot(HotTransferModel hotTransfer);
    }
}
