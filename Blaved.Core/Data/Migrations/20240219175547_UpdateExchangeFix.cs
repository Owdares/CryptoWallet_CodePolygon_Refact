using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blaved.Core.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateExchangeFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ChargeToRefferal",
                table: "Exchanges",
                newName: "ChargeToReferral");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ChargeToReferral",
                table: "Exchanges",
                newName: "ChargeToRefferal");
        }
    }
}
