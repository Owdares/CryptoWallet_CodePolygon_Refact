using System.ComponentModel.DataAnnotations;

namespace Blaved.Core.Objects.Models.Info
{
    public class InfoForBlockChainModel
    {
        [Key]
        public long Id { get; set; }
        public string Asset { get; set; }
        public string Network { get; set; }
        public long LastScanBlock { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Timestamp]
        public byte[]? Timestamp { get; set; }
    }
}
