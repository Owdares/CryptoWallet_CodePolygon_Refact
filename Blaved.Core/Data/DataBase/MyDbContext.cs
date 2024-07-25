using Blaved.Core.Objects.Models;
using Blaved.Core.Objects.Models.Configurations;
using Blaved.Core.Objects.Models.Info;
using Blaved.Core.Utility;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Options;

namespace Blaved.Core.Data.DataBase
{
    public class MyDbContext : DbContext
    {
        public DbSet<UserModel> Users { get; set; }
        public DbSet<BalanceModel> Balances { get; set; }
        public DbSet<BonusBalanceModel> BonusBalances { get; set; }
        public DbSet<BlockChainWalletModel> BlockChainWallets { get; set; }
        public DbSet<MessagesBlavedPayIDModel> MessagesBlavedPayIDs { get; set; }
        public DbSet<MessagesCheckModel> MessagesChecks { get; set; }
        public DbSet<MessagesWithdrawModel> MessagesWithdraws { get; set; }
        public DbSet<MessagesExchangeModel> MessagesExchanges { get; set; }
        public DbSet<CheckModel> Checks { get; set; }
        public DbSet<CheckActivatedModel> CheckActivateds { get; set; }
        public DbSet<WithdrawModel> Withdraws { get; set; }
        public DbSet<DepositModel> Deposits { get; set; }
        public DbSet<ExchangeModel> Exchanges { get; set; }
        public DbSet<HotTransferModel> HotTransfers { get; set; }
        public DbSet<BlavedPayIDTransferModel> BlavedPayIDTransfers { get; set; }
        public DbSet<WithdrawOrderModel> WithdrawOrders { get; set; }
        public DbSet<InfoForBlockChainModel> InfoForBlockChaines { get; set; }

        private readonly AppConfig _appConfig;

        public MyDbContext(DbContextOptions<MyDbContext> options, IOptions<AppConfig> appConfig) : base(options)
        {
            _appConfig = appConfig.Value;
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<UserModel>()
                .HasOne(u => u.BalanceModel)
                .WithOne(p => p.User)
                .HasForeignKey<BalanceModel>(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder
                .Entity<UserModel>()
                .HasOne(u => u.BonusBalanceModel)
                .WithOne(p => p.User)
                .HasForeignKey<BonusBalanceModel>(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder
                .Entity<UserModel>()
                .HasOne(u => u.BlockChainWalletModel)
                .WithOne(p => p.User)
                .HasForeignKey<BlockChainWalletModel>(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder
                .Entity<UserModel>()
                .HasOne(u => u.MessagesBlavedPayIDModel)
                .WithOne(p => p.User)
                .HasForeignKey<MessagesBlavedPayIDModel>(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder
                .Entity<UserModel>()
                .HasOne(u => u.MessagesCheckModel)
                .WithOne(p => p.User)
                .HasForeignKey<MessagesCheckModel>(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder
                .Entity<UserModel>()
                .HasOne(u => u.MessagesExchangeModel)
                .WithOne(p => p.User)
                .HasForeignKey<MessagesExchangeModel>(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder
                .Entity<UserModel>()
                .HasOne(u => u.MessagesWithdrawModel)
                .WithOne(p => p.User)
                .HasForeignKey<MessagesWithdrawModel>(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder
                .Entity<UserModel>()
                .HasMany(u => u.WithdrawModels)
                .WithOne(p => p.User)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder
                .Entity<UserModel>()
                .HasMany(u => u.DepositModels)
                .WithOne(p => p.User)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder
                .Entity<UserModel>()
                .HasMany(u => u.ExchangeModels)
                .WithOne(p => p.User)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder
                .Entity<UserModel>()
                .HasMany(u => u.BlavedPayIDTransferModels)
                .WithOne(p => p.User)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserModel>()
                .HasMany(user => user.CheckModels)
                .WithOne(check => check.User)
                .HasForeignKey(check => check.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserModel>()
                .HasMany(user => user.CheckActivatedModels)
                .WithOne(checkActivated => checkActivated.User)
                .HasForeignKey(checkActivated => checkActivated.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CheckModel>()
                .HasMany(check => check.CheckActivatedModels)
                .WithOne(checkActivated => checkActivated.Check)
                .HasForeignKey(checkActivated => checkActivated.CheckId)
                .OnDelete(DeleteBehavior.Restrict);


            var converter = new ValueConverter<string, string>(
                v => Cryptography.Encrypt(v, _appConfig.CryptographyConfiguration.Base64Key, _appConfig.CryptographyConfiguration.Base64IV),
                v => Cryptography.Decrypt(v, _appConfig.CryptographyConfiguration.Base64Key, _appConfig.CryptographyConfiguration.Base64IV)); 

            modelBuilder.Entity<BlockChainWalletModel>()
            .Property(e => e.PrivatKeyBSC)
            .HasConversion(converter);

            modelBuilder.Entity<BlockChainWalletModel>()
            .Property(e => e.PrivatKeyMATIC)
            .HasConversion(converter); 

            modelBuilder.Entity<BlockChainWalletModel>()
            .Property(e => e.PrivatKeyETH)
            .HasConversion(converter);
            
        }
    }
}
