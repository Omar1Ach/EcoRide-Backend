using EcoRide.Modules.Trip.Domain.Aggregates;
using EcoRide.Modules.Trip.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcoRide.Modules.Trip.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for Reservation entity
/// </summary>
public sealed class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
{
    public void Configure(EntityTypeBuilder<Reservation> builder)
    {
        builder.ToTable("reservations", "trip");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(r => r.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(r => r.VehicleId)
            .HasColumnName("vehicle_id")
            .IsRequired();

        builder.Property(r => r.Status)
            .HasColumnName("status")
            .IsRequired()
            .HasConversion(
                status => status.ToString(),
                value => Enum.Parse<ReservationStatus>(value));

        builder.Property(r => r.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(r => r.ExpiresAt)
            .HasColumnName("expires_at")
            .IsRequired();

        builder.Property(r => r.CancelledAt)
            .HasColumnName("cancelled_at");

        builder.Property(r => r.ConvertedAt)
            .HasColumnName("converted_at");

        builder.Property(r => r.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        // Indexes for performance
        builder.HasIndex(r => r.UserId)
            .HasDatabaseName("idx_reservations_user_id");

        builder.HasIndex(r => r.VehicleId)
            .HasDatabaseName("idx_reservations_vehicle_id");

        builder.HasIndex(r => r.Status)
            .HasDatabaseName("idx_reservations_status");

        builder.HasIndex(r => r.ExpiresAt)
            .HasDatabaseName("idx_reservations_expires_at");

        // Composite index for finding active reservations
        builder.HasIndex(r => new { r.UserId, r.Status })
            .HasDatabaseName("idx_reservations_user_status");

        builder.HasIndex(r => new { r.VehicleId, r.Status })
            .HasDatabaseName("idx_reservations_vehicle_status");

        // Ignore domain events
        builder.Ignore(r => r.DomainEvents);
    }
}
