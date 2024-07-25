using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Blaved.Core.Data.Migrations
{
    /// <inheritdoc />
    public partial class Initially : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HotTransfers",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Network = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Asset = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Hash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FromAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ToAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(36,8)", nullable: false),
                    Status = table.Column<long>(type: "bigint", nullable: false),
                    Fee = table.Column<decimal>(type: "decimal(36,8)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HotTransfers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InfoForBlockChaines",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Asset = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Network = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastScanBlock = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Timestamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InfoForBlockChaines", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Language = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsBanned = table.Column<bool>(type: "bit", nullable: false),
                    WhereMenu = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MessageId = table.Column<int>(type: "int", nullable: false),
                    WhoseReferral = table.Column<long>(type: "bigint", nullable: true),
                    RateReferralExchange = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AcceptedTermsOfUse = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Timestamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "Balances",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BalanceUSDT = table.Column<decimal>(type: "decimal(24,8)", nullable: false),
                    BalanceBNB = table.Column<decimal>(type: "decimal(36,8)", nullable: false),
                    BalanceBUSD = table.Column<decimal>(type: "decimal(36,8)", nullable: false),
                    BalanceADA = table.Column<decimal>(type: "decimal(36,8)", nullable: false),
                    BalanceDOGE = table.Column<decimal>(type: "decimal(36,8)", nullable: false),
                    BalanceETH = table.Column<decimal>(type: "decimal(36,8)", nullable: false),
                    BalanceUSDC = table.Column<decimal>(type: "decimal(24,8)", nullable: false),
                    BalanceAPE = table.Column<decimal>(type: "decimal(36,8)", nullable: false),
                    BalanceSHIB = table.Column<decimal>(type: "decimal(36,8)", nullable: false),
                    BalanceLINK = table.Column<decimal>(type: "decimal(36,8)", nullable: false),
                    BalanceMATIC = table.Column<decimal>(type: "decimal(36,8)", nullable: false),
                    BalanceSAND = table.Column<decimal>(type: "decimal(36,8)", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Timestamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Balances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Balances_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BlavedPayIDTransfers",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    ToUserId = table.Column<long>(type: "bigint", nullable: false),
                    Asset = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(36,8)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlavedPayIDTransfers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BlavedPayIDTransfers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BlockChainWallets",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AddressBSC = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PrivatKeyBSC = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AddressETH = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PrivatKeyETH = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AddressMATIC = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PrivatKeyMATIC = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Timestamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlockChainWallets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BlockChainWallets_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BonusBalances",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BonusBalanceUSDT = table.Column<decimal>(type: "decimal(24,8)", nullable: false),
                    BonusBalanceBNB = table.Column<decimal>(type: "decimal(36,8)", nullable: false),
                    BonusBalanceBUSD = table.Column<decimal>(type: "decimal(36,8)", nullable: false),
                    BonusBalanceADA = table.Column<decimal>(type: "decimal(36,8)", nullable: false),
                    BonusBalanceDOGE = table.Column<decimal>(type: "decimal(36,8)", nullable: false),
                    BonusBalanceETH = table.Column<decimal>(type: "decimal(36,8)", nullable: false),
                    BonusBalanceUSDC = table.Column<decimal>(type: "decimal(24,8)", nullable: false),
                    BonusBalanceAPE = table.Column<decimal>(type: "decimal(36,8)", nullable: false),
                    BonusBalanceSHIB = table.Column<decimal>(type: "decimal(36,8)", nullable: false),
                    BonusBalanceLINK = table.Column<decimal>(type: "decimal(36,8)", nullable: false),
                    BonusBalanceMATIC = table.Column<decimal>(type: "decimal(36,8)", nullable: false),
                    BonusBalanceSAND = table.Column<decimal>(type: "decimal(36,8)", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Timestamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BonusBalances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BonusBalances_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Checks",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Asset = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(36,8)", nullable: false),
                    Count = table.Column<int>(type: "int", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Timestamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Checks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Checks_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Deposits",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    Network = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Asset = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Hash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AddressFrom = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AddressTo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(36,8)", nullable: false),
                    Fee = table.Column<decimal>(type: "decimal(36,8)", nullable: false),
                    IsInside = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Deposits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Deposits_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Exchanges",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    ExchangeMethod = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExchangeId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ToAsset = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FromAsset = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ToAmount = table.Column<decimal>(type: "decimal(36,8)", nullable: false),
                    FromAmount = table.Column<decimal>(type: "decimal(36,8)", nullable: false),
                    Fee = table.Column<decimal>(type: "decimal(36,8)", nullable: false),
                    HiddenFee = table.Column<decimal>(type: "decimal(36,8)", nullable: false),
                    ChargeToCapital = table.Column<decimal>(type: "decimal(36,8)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Exchanges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Exchanges_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MessagesBlavedPayIDs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ToUserId = table.Column<long>(type: "bigint", nullable: false),
                    Asset = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(36,8)", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessagesBlavedPayIDs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MessagesBlavedPayIDs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MessagesChecks",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Asset = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(36,8)", nullable: false),
                    Count = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessagesChecks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MessagesChecks_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MessagesExchanges",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FromAsset = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ToAsset = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(36,8)", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessagesExchanges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MessagesExchanges_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MessagesWithdraws",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(36,8)", nullable: false),
                    Network = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Asset = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessagesWithdraws", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MessagesWithdraws_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WithdrawOrders",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    IdOrder = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Network = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Asset = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AddressTo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(36,8)", nullable: false),
                    Fee = table.Column<decimal>(type: "decimal(36,8)", nullable: false),
                    ChargeToCapital = table.Column<decimal>(type: "decimal(36,8)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WithdrawOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WithdrawOrders_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Withdraws",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    Network = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Asset = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Hash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OrderId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AddressFrom = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AddressTo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(36,8)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Fee = table.Column<decimal>(type: "decimal(36,8)", nullable: false),
                    ChargeToCapital = table.Column<decimal>(type: "decimal(36,8)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Withdraws", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Withdraws_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CheckActivateds",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    CheckId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CheckActivateds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CheckActivateds_Checks_CheckId",
                        column: x => x.CheckId,
                        principalTable: "Checks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CheckActivateds_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "InfoForBlockChaines",
                columns: new[] { "Id", "Asset", "CreatedAt", "LastScanBlock", "Network" },
                values: new object[,]
                {
                    { 1L, "USDT", new DateTime(2024, 2, 12, 22, 11, 49, 326, DateTimeKind.Utc).AddTicks(1972), 0L, "BSC" },
                    { 2L, "USDT", new DateTime(2024, 2, 12, 22, 11, 49, 326, DateTimeKind.Utc).AddTicks(1975), 0L, "ETH" },
                    { 3L, "USDT", new DateTime(2024, 2, 12, 22, 11, 49, 326, DateTimeKind.Utc).AddTicks(1976), 0L, "MATIC" },
                    { 4L, "USDC", new DateTime(2024, 2, 12, 22, 11, 49, 326, DateTimeKind.Utc).AddTicks(1977), 0L, "BSC" },
                    { 5L, "USDC", new DateTime(2024, 2, 12, 22, 11, 49, 326, DateTimeKind.Utc).AddTicks(1978), 0L, "ETH" },
                    { 6L, "USDC", new DateTime(2024, 2, 12, 22, 11, 49, 326, DateTimeKind.Utc).AddTicks(1979), 0L, "MATIC" },
                    { 7L, "ETH", new DateTime(2024, 2, 12, 22, 11, 49, 326, DateTimeKind.Utc).AddTicks(1979), 0L, "BSC" },
                    { 8L, "ETH", new DateTime(2024, 2, 12, 22, 11, 49, 326, DateTimeKind.Utc).AddTicks(1980), 0L, "ETH" },
                    { 9L, "BNB", new DateTime(2024, 2, 12, 22, 11, 49, 326, DateTimeKind.Utc).AddTicks(1981), 0L, "BSC" },
                    { 10L, "BNB", new DateTime(2024, 2, 12, 22, 11, 49, 326, DateTimeKind.Utc).AddTicks(1982), 0L, "ETH" },
                    { 11L, "MATIC", new DateTime(2024, 2, 12, 22, 11, 49, 326, DateTimeKind.Utc).AddTicks(1983), 0L, "BSC" },
                    { 12L, "MATIC", new DateTime(2024, 2, 12, 22, 11, 49, 326, DateTimeKind.Utc).AddTicks(1983), 0L, "MATIC" },
                    { 13L, "SAND", new DateTime(2024, 2, 12, 22, 11, 49, 326, DateTimeKind.Utc).AddTicks(1984), 0L, "ETH" },
                    { 14L, "SAND", new DateTime(2024, 2, 12, 22, 11, 49, 326, DateTimeKind.Utc).AddTicks(1985), 0L, "MATIC" },
                    { 15L, "DOGE", new DateTime(2024, 2, 12, 22, 11, 49, 326, DateTimeKind.Utc).AddTicks(1986), 0L, "BSC" },
                    { 16L, "ADA", new DateTime(2024, 2, 12, 22, 11, 49, 326, DateTimeKind.Utc).AddTicks(1987), 0L, "BSC" },
                    { 17L, "LINK", new DateTime(2024, 2, 12, 22, 11, 49, 326, DateTimeKind.Utc).AddTicks(1987), 0L, "BSC" },
                    { 18L, "LINK", new DateTime(2024, 2, 12, 22, 11, 49, 326, DateTimeKind.Utc).AddTicks(1988), 0L, "ETH" },
                    { 19L, "FTM", new DateTime(2024, 2, 12, 22, 11, 49, 326, DateTimeKind.Utc).AddTicks(1989), 0L, "BSC" },
                    { 20L, "FTM", new DateTime(2024, 2, 12, 22, 11, 49, 326, DateTimeKind.Utc).AddTicks(1990), 0L, "ETH" },
                    { 21L, "SHIB", new DateTime(2024, 2, 12, 22, 11, 49, 326, DateTimeKind.Utc).AddTicks(1991), 0L, "BSC" },
                    { 22L, "SHIB", new DateTime(2024, 2, 12, 22, 11, 49, 326, DateTimeKind.Utc).AddTicks(1992), 0L, "ETH" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Balances_UserId",
                table: "Balances",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BlavedPayIDTransfers_UserId",
                table: "BlavedPayIDTransfers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_BlockChainWallets_UserId",
                table: "BlockChainWallets",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BonusBalances_UserId",
                table: "BonusBalances",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CheckActivateds_CheckId",
                table: "CheckActivateds",
                column: "CheckId");

            migrationBuilder.CreateIndex(
                name: "IX_CheckActivateds_UserId",
                table: "CheckActivateds",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Checks_UserId",
                table: "Checks",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Deposits_UserId",
                table: "Deposits",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Exchanges_UserId",
                table: "Exchanges",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_MessagesBlavedPayIDs_UserId",
                table: "MessagesBlavedPayIDs",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MessagesChecks_UserId",
                table: "MessagesChecks",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MessagesExchanges_UserId",
                table: "MessagesExchanges",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MessagesWithdraws_UserId",
                table: "MessagesWithdraws",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WithdrawOrders_UserId",
                table: "WithdrawOrders",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Withdraws_UserId",
                table: "Withdraws",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Balances");

            migrationBuilder.DropTable(
                name: "BlavedPayIDTransfers");

            migrationBuilder.DropTable(
                name: "BlockChainWallets");

            migrationBuilder.DropTable(
                name: "BonusBalances");

            migrationBuilder.DropTable(
                name: "CheckActivateds");

            migrationBuilder.DropTable(
                name: "Deposits");

            migrationBuilder.DropTable(
                name: "Exchanges");

            migrationBuilder.DropTable(
                name: "HotTransfers");

            migrationBuilder.DropTable(
                name: "InfoForBlockChaines");

            migrationBuilder.DropTable(
                name: "MessagesBlavedPayIDs");

            migrationBuilder.DropTable(
                name: "MessagesChecks");

            migrationBuilder.DropTable(
                name: "MessagesExchanges");

            migrationBuilder.DropTable(
                name: "MessagesWithdraws");

            migrationBuilder.DropTable(
                name: "WithdrawOrders");

            migrationBuilder.DropTable(
                name: "Withdraws");

            migrationBuilder.DropTable(
                name: "Checks");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
