using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blaved.Core.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateExchange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ChargeToRefferal",
                table: "Exchanges",
                type: "decimal(36,8)",
                nullable: false,
                defaultValue: 0m);

           
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChargeToRefferal",
                table: "Exchanges");

           
        }
    }
}
