using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcoRide.Modules.Security.Migrations
{
    /// <inheritdoc />
    public partial class AddUserSettingsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "user_settings",
                schema: "security",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    push_notifications_enabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    dark_mode_enabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    haptic_feedback_enabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    language_code = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false, defaultValue: "en"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_settings", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "idx_user_settings_user_id",
                schema: "security",
                table: "user_settings",
                column: "user_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_settings",
                schema: "security");
        }
    }
}
