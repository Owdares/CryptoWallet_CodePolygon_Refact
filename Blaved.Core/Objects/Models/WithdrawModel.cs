using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Binance.Net.Enums;
using Newtonsoft.Json;

namespace Blaved.Core.Objects.Models
{
    public class WithdrawModel
    {
        [Key]
        public long Id { get; set; }

        public long UserId { get; set; }

        [JsonIgnore]
        public UserModel User { get; set; }

        public string Network { get; set; }
        public string Asset { get; set; }
        public string Hash { get; set; }
        public string OrderId { get; set; }
        public string AddressFrom { get; set; }
        public string AddressTo { get; set; }

        [Column(TypeName = "decimal(36, 8)")]
        public decimal Amount { get; set; }
        public WithdrawalStatus Status { get; set; }

        [Column(TypeName = "decimal(36, 8)")]
        public decimal Fee { get; set; }

        [Column(TypeName = "decimal(36, 8)")]
        public decimal ChargeToCapital { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
