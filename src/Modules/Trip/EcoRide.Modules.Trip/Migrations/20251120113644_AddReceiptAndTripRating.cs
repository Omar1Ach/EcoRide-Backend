using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcoRide.Modules.Trip.Migrations
{
    /// <inheritdoc />
    public partial class AddReceiptAndTripRating : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "rated_at",
                schema: "trip",
                table: "trips",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "rating_comment",
                schema: "trip",
                table: "trips",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "rating_stars",
                schema: "trip",
                table: "trips",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "receipts",
                schema: "trip",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    trip_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    receipt_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    vehicle_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    trip_start_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    trip_end_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    duration_minutes = table.Column<int>(type: "integer", nullable: false),
                    distance_meters = table.Column<int>(type: "integer", nullable: false),
                    start_latitude = table.Column<double>(type: "double precision", nullable: false),
                    start_longitude = table.Column<double>(type: "double precision", nullable: false),
                    end_latitude = table.Column<double>(type: "double precision", nullable: false),
                    end_longitude = table.Column<double>(type: "double precision", nullable: false),
                    base_cost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    time_cost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    total_cost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    payment_method = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    payment_details = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    wallet_balance_before = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    wallet_balance_after = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_receipts", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "idx_receipts_receipt_number",
                schema: "trip",
                table: "receipts",
                column: "receipt_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_receipts_trip_id",
                schema: "trip",
                table: "receipts",
                column: "trip_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_receipts_user_id",
                schema: "trip",
                table: "receipts",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "receipts",
                schema: "trip");

            migrationBuilder.DropColumn(
                name: "rated_at",
                schema: "trip",
                table: "trips");

            migrationBuilder.DropColumn(
                name: "rating_comment",
                schema: "trip",
                table: "trips");

            migrationBuilder.DropColumn(
                name: "rating_stars",
                schema: "trip",
                table: "trips");
        }
    }
}
