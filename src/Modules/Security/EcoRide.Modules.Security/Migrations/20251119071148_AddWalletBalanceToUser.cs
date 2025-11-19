using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcoRide.Modules.Security.Migrations
{
    /// <inheritdoc />
    public partial class AddWalletBalanceToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "WalletBalance",
                schema: "security",
                table: "users",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WalletBalance",
                schema: "security",
                table: "users");
        }
    }
}
