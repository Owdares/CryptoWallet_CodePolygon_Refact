using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Blaved.Core.Objects.Models
{
    public class CheckModel
    {
        [Key]
        public long Id { get; set; }

        public long UserId { get; set; }

        [JsonIgnore]
        public UserModel User { get; set; }

        public string Url { get; set; }
        public string Asset { get; set; }

        [Column(TypeName = "decimal(36, 8)")]
        public decimal Amount { get; set; }
        public int Count { get; set; }
        public string? Password { get; set; }
        public bool IsDeleted { get; set; }

        [JsonIgnore]
        public List<CheckActivatedModel> CheckActivatedModels { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Timestamp]
        public byte[]? Timestamp { get; set; }
    }
}
