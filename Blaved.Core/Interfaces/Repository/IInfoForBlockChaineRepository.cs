using Blaved.Core.Objects.Models.Info;

namespace Blaved.Core.Interfaces.Repository
{
    public interface IInfoForBlockChainRepository
    {
        Task<InfoForBlockChainModel?> GetInfoForBlockChaine(string Asset, string network);
        Task UpdateLastScanBlock(string Asset, string network, long lastScanBlock);
    }
}
