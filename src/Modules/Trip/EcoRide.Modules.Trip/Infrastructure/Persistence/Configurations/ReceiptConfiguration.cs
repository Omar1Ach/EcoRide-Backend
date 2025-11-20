using EcoRide.Modules.Trip.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcoRide.Modules.Trip.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for Receipt entity
/// US-006: End Trip & Payment - Receipt storage
/// </summary>
public sealed class ReceiptConfiguration : IEntityTypeConfiguration<Receipt>
{
    public void Configure(EntityTypeBuilder<Receipt> builder)
    {
        builder.ToTable("receipts", "trip");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(r => r.TripId)
            .HasColumnName("trip_id")
            .IsRequired();

        builder.Property(r => r.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(r => r.ReceiptNumber)
            .HasColumnName("receipt_number")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(r => r.VehicleCode)
            .HasColumnName("vehicle_code")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(r => r.TripStartTime)
            .HasColumnName("trip_start_time")
            .IsRequired();

        builder.Property(r => r.TripEndTime)
            .HasColumnName("trip_end_time")
            .IsRequired();

        builder.Property(r => r.DurationMinutes)
            .HasColumnName("duration_minutes")
            .IsRequired();

        builder.Property(r => r.DistanceMeters)
            .HasColumnName("distance_meters")
            .IsRequired();

        builder.Property(r => r.StartLatitude)
            .HasColumnName("start_latitude")
            .IsRequired();

        builder.Property(r => r.StartLongitude)
            .HasColumnName("start_longitude")
            .IsRequired();

        builder.Property(r => r.EndLatitude)
            .HasColumnName("end_latitude")
            .IsRequired();

        builder.Property(r => r.EndLongitude)
            .HasColumnName("end_longitude")
            .IsRequired();

        builder.Property(r => r.BaseCost)
            .HasColumnName("base_cost")
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(r => r.TimeCost)
            .HasColumnName("time_cost")
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(r => r.TotalCost)
            .HasColumnName("total_cost")
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(r => r.PaymentMethod)
            .HasColumnName("payment_method")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(r => r.PaymentDetails)
            .HasColumnName("payment_details")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(r => r.WalletBalanceBefore)
            .HasColumnName("wallet_balance_before")
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(r => r.WalletBalanceAfter)
            .HasColumnName("wallet_balance_after")
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(r => r.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        // Indexes
        builder.HasIndex(r => r.TripId)
            .IsUnique()
            .HasDatabaseName("idx_receipts_trip_id");

        builder.HasIndex(r => r.UserId)
            .HasDatabaseName("idx_receipts_user_id");

        builder.HasIndex(r => r.ReceiptNumber)
            .IsUnique()
            .HasDatabaseName("idx_receipts_receipt_number");
    }
}
