using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

using Newtonsoft.Json;

namespace Blaved.Core.Objects.Models
{
    public class ExchangeModel
    {
        [Key]
        public long Id { get; set; }

        public long UserId { get; set; }

        [JsonIgnore]
        public UserModel User { get; set; }

        public string ExchangeMethod { get; set; }
        public string ExchangeId { get; set; }
        public string ToAsset { get; set; }
        public string FromAsset { get; set; }

        [Column(TypeName = "decimal(36, 8)")]
        public decimal ToAmount { get; set; }

        [Column(TypeName = "decimal(36, 8)")]
        public decimal FromAmount { get; set; }

        [Column(TypeName = "decimal(36, 8)")]
        public decimal Fee { get; set; }

        [Column(TypeName = "decimal(36, 8)")]
        public decimal HiddenFee { get; set; }

        [Column(TypeName = "decimal(36, 8)")]
        public decimal ChargeToReferral { get; set; }

        [Column(TypeName = "decimal(36, 8)")]
        public decimal ChargeToCapital { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
