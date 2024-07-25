using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Blaved.Core.Objects.Models
{
    public class HotTransferModel
    {
        [Key]
        public long Id { get; set; }
        public string Network { get; set; }
        public string Asset { get; set; }
        public string Hash { get; set; }
        public string FromAddress { get; set; }
        public string ToAddress { get; set; }

        [Column(TypeName = "decimal(36, 8)")]
        public decimal Amount { get; set; }
        public long Status { get; set; }

        [Column(TypeName = "decimal(36, 8)")]
        public decimal Fee { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
