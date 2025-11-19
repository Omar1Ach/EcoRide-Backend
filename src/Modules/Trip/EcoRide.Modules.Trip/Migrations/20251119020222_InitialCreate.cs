using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcoRide.Modules.Trip.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "trip");

            migrationBuilder.CreateTable(
                name: "reservations",
                schema: "trip",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    vehicle_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    cancelled_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    converted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reservations", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "idx_reservations_expires_at",
                schema: "trip",
                table: "reservations",
                column: "expires_at");

            migrationBuilder.CreateIndex(
                name: "idx_reservations_status",
                schema: "trip",
                table: "reservations",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "idx_reservations_user_id",
                schema: "trip",
                table: "reservations",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "idx_reservations_user_status",
                schema: "trip",
                table: "reservations",
                columns: new[] { "user_id", "status" });

            migrationBuilder.CreateIndex(
                name: "idx_reservations_vehicle_id",
                schema: "trip",
                table: "reservations",
                column: "vehicle_id");

            migrationBuilder.CreateIndex(
                name: "idx_reservations_vehicle_status",
                schema: "trip",
                table: "reservations",
                columns: new[] { "vehicle_id", "status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "reservations",
                schema: "trip");
        }
    }
}
