using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace Blaved.Core.Objects.Models
{
    public class UserModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long UserId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string Language { get; set; }
        public bool IsBanned { get; set; }
        public string WhereMenu { get; set; }
        public int MessageId { get; set; }
        public long? WhoseReferral { get; set; }
        public int RateReferralExchange { get; set; }
        public string Status { get; set; }
        public bool AcceptedTermsOfUse { get; set; }
        public bool EnabledNotificationsBlavedPay {  get; set; }

        [JsonIgnore]
        public BlockChainWalletModel BlockChainWalletModel { get; set; }

        [JsonIgnore]
        public BalanceModel BalanceModel { get; set; }

        [JsonIgnore]
        public BonusBalanceModel BonusBalanceModel { get; set; }

        [JsonIgnore]
        public MessagesBlavedPayIDModel MessagesBlavedPayIDModel { get;set; }

        [JsonIgnore]
        public MessagesCheckModel MessagesCheckModel { get; set; }

        [JsonIgnore]
        public MessagesWithdrawModel MessagesWithdrawModel { get; set; }

        [JsonIgnore]
        public MessagesExchangeModel MessagesExchangeModel { get; set; }

        [JsonIgnore]
        public List<WithdrawModel> WithdrawModels { get; set; }

        [JsonIgnore]
        public List<DepositModel> DepositModels { get; set; }

        [JsonIgnore]
        public List<CheckModel> CheckModels { get; set; }

        [JsonIgnore]
        public List<ExchangeModel> ExchangeModels { get; set; }

        [JsonIgnore]
        public List<BlavedPayIDTransferModel> BlavedPayIDTransferModels { get; set; }

        [JsonIgnore]
        public List<CheckActivatedModel> CheckActivatedModels { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Timestamp]
        public byte[]? Timestamp { get; set; }
    }

    public class BlockChainWalletModel
    {
        [Key]
        public long Id { get; set; }
        public string AddressBSC { get; set; }
        public string PrivatKeyBSC { get; set; }

        public string AddressETH { get; set; }
        public string PrivatKeyETH { get; set; }

        public string AddressMATIC { get; set; }
        public string PrivatKeyMATIC { get; set; }

        public long UserId { get; set; }

        [JsonIgnore]
        public UserModel User { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Timestamp]
        public byte[]? Timestamp { get; set; }
        public static string GetAddressPropertyName(string network)
        {
            switch (network)
            {
                case "ETH": return nameof(AddressETH);
                case "BSC": return nameof(AddressBSC);
                case "MATIC": return nameof(AddressMATIC);

                default: throw new Exception("Unknown Property Name");
            }
        }

        public string GetAddress(string network)
        {
            switch (network)
            {
                case "ETH": return AddressETH;
                case "BSC": return AddressBSC;
                case "MATIC": return AddressMATIC;

                default: throw new Exception("Unknown Property Name");
            }
        }
        public string GetPrivateKey(string network)
        {
            switch (network)
            {
                case "ETH": return PrivatKeyETH;
                case "BSC": return PrivatKeyBSC;
                case "MATIC": return PrivatKeyMATIC;

                default: throw new Exception("Unknown Property Name");
            }
        }
    }

    public class BalanceModel
    {
        [Key]
        public long Id { get; set; }

        [Column(TypeName = "decimal(24, 8)")]
        public decimal BalanceUSDT { get; set; }

        [Column(TypeName = "decimal(36, 8)")]
        public decimal BalanceBNB { get; set; }

        [Column(TypeName = "decimal(36, 8)")]
        public decimal BalanceBUSD { get; set; }

        [Column(TypeName = "decimal(36, 8)")]
        public decimal BalanceADA { get; set; }

        [Column(TypeName = "decimal(36, 8)")]
        public decimal BalanceDOGE { get; set; }

        [Column(TypeName = "decimal(36, 8)")]
        public decimal BalanceETH { get; set; }

        [Column(TypeName = "decimal(24, 8)")]
        public decimal BalanceUSDC { get; set; }

        [Column(TypeName = "decimal(36, 8)")]
        public decimal BalanceAPE { get; set; }

        [Column(TypeName = "decimal(36, 8)")]
        public decimal BalanceSHIB { get; set; }

        [Column(TypeName = "decimal(36, 8)")]
        public decimal BalanceLINK { get; set; }

        [Column(TypeName = "decimal(36, 8)")]
        public decimal BalanceMATIC { get; set; }

        [Column(TypeName = "decimal(36, 8)")]
        public decimal BalanceSAND { get; set; }

        public long UserId { get; set; }

        [JsonIgnore]
        public UserModel User { get; set; }

       
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Timestamp]
        public byte[]? Timestamp { get; set; }
        public static string GetBalancePropertyName(string Asset)
        {
            switch (Asset)
            {
                case "USDT": return nameof(BalanceUSDT);
                case "BNB": return nameof(BalanceBNB);
                case "BUSD": return nameof(BalanceBUSD);
                case "DOGE": return nameof(BalanceDOGE);
                case "ETH": return nameof(BalanceETH);
                case "LINK": return nameof(BalanceLINK);
                case "MATIC": return nameof(BalanceMATIC);
                case "USDC": return nameof(BalanceUSDC);
                case "SAND": return nameof(BalanceSAND);
                case "APE": return nameof(BalanceAPE);
                case "ADA": return nameof(BalanceADA);
                case "SHIB": return nameof(BalanceSHIB);
                default: throw new Exception();
            }
        }
        public virtual decimal GetBalance(string Asset)
        {
            switch (Asset)
            {
                case "USDT": return BalanceUSDT;
                case "BNB": return BalanceBNB;
                case "BUSD": return BalanceBUSD;
                case "DOGE": return BalanceDOGE;
                case "ETH": return BalanceETH;
                case "LINK": return BalanceLINK;
                case "MATIC": return BalanceMATIC;
                case "USDC": return BalanceUSDC;
                case "SAND": return BalanceSAND;
                case "APE": return BalanceAPE;
                case "ADA": return BalanceADA;
                case "SHIB": return BalanceSHIB;
                default: return 0m;
            }
        }
    }

    public class BonusBalanceModel
    {
        [Key]
        public long Id { get; set; }

        [Column(TypeName = "decimal(24, 8)")]
        public decimal BonusBalanceUSDT { get; set; }

        [Column(TypeName = "decimal(36, 8)")]
        public decimal BonusBalanceBNB { get; set; }

        [Column(TypeName = "decimal(36, 8)")]
        public decimal BonusBalanceBUSD { get; set; }

        [Column(TypeName = "decimal(36, 8)")]
        public decimal BonusBalanceADA { get; set; }

        [Column(TypeName = "decimal(36, 8)")]
        public decimal BonusBalanceDOGE { get; set; }

        [Column(TypeName = "decimal(36, 8)")]
        public decimal BonusBalanceETH { get; set; }

        [Column(TypeName = "decimal(24, 8)")]
        public decimal BonusBalanceUSDC { get; set; }

        [Column(TypeName = "decimal(36, 8)")]
        public decimal BonusBalanceAPE { get; set; }

        [Column(TypeName = "decimal(36, 8)")]
        public decimal BonusBalanceSHIB { get; set; }

        [Column(TypeName = "decimal(36, 8)")]
        public decimal BonusBalanceLINK { get; set; }

        [Column(TypeName = "decimal(36, 8)")]
        public decimal BonusBalanceMATIC { get; set; }

        [Column(TypeName = "decimal(36, 8)")]
        public decimal BonusBalanceSAND { get; set; }

        public long UserId { get; set; }

        [JsonIgnore]
        public UserModel User { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Timestamp]
        public byte[]? Timestamp { get; set; }
        public static string GetBalancePropertyName(string Asset)
        {
            switch (Asset)
            {
                case "USDT": return nameof(BonusBalanceUSDT);
                case "BNB": return nameof(BonusBalanceBNB);
                case "BUSD": return nameof(BonusBalanceBUSD);
                case "DOGE": return nameof(BonusBalanceDOGE);
                case "ETH": return nameof(BonusBalanceETH);
                case "LINK": return nameof(BonusBalanceLINK);
                case "MATIC": return nameof(BonusBalanceMATIC);
                case "USDC": return nameof(BonusBalanceUSDC);
                case "SAND": return nameof(BonusBalanceSAND);
                case "APE": return nameof(BonusBalanceAPE);
                case "ADA": return nameof(BonusBalanceADA);
                case "SHIB": return nameof(BonusBalanceSHIB);
                default: throw new Exception();
            }
        }
        public virtual decimal GetBalance(string Asset)
        {
            switch (Asset)
            {
                case "USDT": return BonusBalanceUSDT;
                case "BNB": return BonusBalanceBNB;
                case "BUSD": return BonusBalanceBUSD;
                case "DOGE": return BonusBalanceDOGE;
                case "ETH": return BonusBalanceETH;
                case "LINK": return BonusBalanceLINK;
                case "MATIC": return BonusBalanceMATIC;
                case "USDC": return BonusBalanceUSDC;
                case "SAND": return BonusBalanceSAND;
                case "APE": return BonusBalanceAPE;
                case "ADA": return BonusBalanceADA;
                case "SHIB": return BonusBalanceSHIB;
                default: return 0m;
            }
        }
    }

    public class MessagesWithdrawModel
    {
        [Key]
        public long Id { get; set; }
        public string Address { get; set; }

        [Column(TypeName = "decimal(36, 8)")]
        public decimal Amount { get; set; }
        public string Network { get; set; }
        public string Asset { get; set; }

        public long UserId { get; set; }

        [JsonIgnore]
        public UserModel User { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    }

    public class MessagesExchangeModel
    {
        [Key]
        public long Id { get; set; }
        public string FromAsset { get; set; }
        public string ToAsset { get; set; }

        [Column(TypeName = "decimal(36, 8)")]
        public decimal Amount { get; set; }

        public long UserId { get; set; }

        [JsonIgnore]
        public UserModel User { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class MessagesCheckModel
    {
        [Key]
        public long Id { get; set; }
        public string Asset { get; set; }

        [Column(TypeName = "decimal(36, 8)")]
        public decimal Amount { get; set; }
        public int Count { get; set; }

        public long UserId { get; set; }

        [JsonIgnore]
        public UserModel User { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class MessagesBlavedPayIDModel
    {
        [Key]
        public long Id { get; set; }
        public long ToUserId { get; set; }
        public string Asset { get; set; }

        [Column(TypeName = "decimal(36, 8)")]
        public decimal Amount { get; set; }

        public long UserId { get; set; }

        [JsonIgnore]
        public UserModel User { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
