using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcoRide.Modules.Security.Migrations
{
    /// <inheritdoc />
    public partial class AddSecurityFieldsToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FailedLoginAttempts",
                schema: "security",
                table: "users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastLoginAt",
                schema: "security",
                table: "users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LockoutEnd",
                schema: "security",
                table: "users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PasswordResetToken",
                schema: "security",
                table: "users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PasswordResetTokenExpiry",
                schema: "security",
                table: "users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RefreshToken",
                schema: "security",
                table: "users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RefreshTokenExpiryTime",
                schema: "security",
                table: "users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "TwoFactorEnabled",
                schema: "security",
                table: "users",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FailedLoginAttempts",
                schema: "security",
                table: "users");

            migrationBuilder.DropColumn(
                name: "LastLoginAt",
                schema: "security",
                table: "users");

            migrationBuilder.DropColumn(
                name: "LockoutEnd",
                schema: "security",
                table: "users");

            migrationBuilder.DropColumn(
                name: "PasswordResetToken",
                schema: "security",
                table: "users");

            migrationBuilder.DropColumn(
                name: "PasswordResetTokenExpiry",
                schema: "security",
                table: "users");

            migrationBuilder.DropColumn(
                name: "RefreshToken",
                schema: "security",
                table: "users");

            migrationBuilder.DropColumn(
                name: "RefreshTokenExpiryTime",
                schema: "security",
                table: "users");

            migrationBuilder.DropColumn(
                name: "TwoFactorEnabled",
                schema: "security",
                table: "users");
        }
    }
}
