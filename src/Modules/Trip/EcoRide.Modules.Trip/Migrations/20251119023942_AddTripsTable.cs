using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcoRide.Modules.Trip.Migrations
{
    /// <inheritdoc />
    public partial class AddTripsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "trips",
                schema: "trip",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    vehicle_id = table.Column<Guid>(type: "uuid", nullable: false),
                    reservation_id = table.Column<Guid>(type: "uuid", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false),
                    start_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    start_latitude = table.Column<double>(type: "double precision", nullable: false),
                    start_longitude = table.Column<double>(type: "double precision", nullable: false),
                    end_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    end_latitude = table.Column<double>(type: "double precision", nullable: true),
                    end_longitude = table.Column<double>(type: "double precision", nullable: true),
                    total_cost = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    duration_minutes = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trips", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "idx_trips_status",
                schema: "trip",
                table: "trips",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "idx_trips_user_id",
                schema: "trip",
                table: "trips",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "idx_trips_user_status",
                schema: "trip",
                table: "trips",
                columns: new[] { "user_id", "status" });

            migrationBuilder.CreateIndex(
                name: "idx_trips_vehicle_id",
                schema: "trip",
                table: "trips",
                column: "vehicle_id");

            migrationBuilder.CreateIndex(
                name: "idx_trips_vehicle_status",
                schema: "trip",
                table: "trips",
                columns: new[] { "vehicle_id", "status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "trips",
                schema: "trip");
        }
    }
}
