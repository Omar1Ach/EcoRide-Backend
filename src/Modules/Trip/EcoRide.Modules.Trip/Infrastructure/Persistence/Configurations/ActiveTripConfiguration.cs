using EcoRide.Modules.Trip.Domain.Aggregates;
using EcoRide.Modules.Trip.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcoRide.Modules.Trip.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for ActiveTrip entity
/// </summary>
public sealed class ActiveTripConfiguration : IEntityTypeConfiguration<ActiveTrip>
{
    public void Configure(EntityTypeBuilder<ActiveTrip> builder)
    {
        builder.ToTable("trips", "trip");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(t => t.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(t => t.VehicleId)
            .HasColumnName("vehicle_id")
            .IsRequired();

        builder.Property(t => t.ReservationId)
            .HasColumnName("reservation_id");

        builder.Property(t => t.Status)
            .HasColumnName("status")
            .IsRequired()
            .HasConversion(
                status => status.ToString(),
                value => Enum.Parse<TripStatus>(value));

        builder.Property(t => t.StartTime)
            .HasColumnName("start_time")
            .IsRequired();

        builder.Property(t => t.StartLatitude)
            .HasColumnName("start_latitude")
            .IsRequired();

        builder.Property(t => t.StartLongitude)
            .HasColumnName("start_longitude")
            .IsRequired();

        builder.Property(t => t.EndTime)
            .HasColumnName("end_time");

        builder.Property(t => t.EndLatitude)
            .HasColumnName("end_latitude");

        builder.Property(t => t.EndLongitude)
            .HasColumnName("end_longitude");

        builder.Property(t => t.TotalCost)
            .HasColumnName("total_cost")
            .HasPrecision(10, 2)
            .IsRequired();

        builder.Property(t => t.DurationMinutes)
            .HasColumnName("duration_minutes")
            .IsRequired();

        // Rating fields (US-006: Trip rating feature)
        builder.Property(t => t.RatingStars)
            .HasColumnName("rating_stars");

        builder.Property(t => t.RatingComment)
            .HasColumnName("rating_comment")
            .HasMaxLength(500);

        builder.Property(t => t.RatedAt)
            .HasColumnName("rated_at");

        builder.Property(t => t.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(t => t.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        // Indexes for performance
        builder.HasIndex(t => t.UserId)
            .HasDatabaseName("idx_trips_user_id");

        builder.HasIndex(t => t.VehicleId)
            .HasDatabaseName("idx_trips_vehicle_id");

        builder.HasIndex(t => t.Status)
            .HasDatabaseName("idx_trips_status");

        builder.HasIndex(t => new { t.UserId, t.Status })
            .HasDatabaseName("idx_trips_user_status");

        builder.HasIndex(t => new { t.VehicleId, t.Status })
            .HasDatabaseName("idx_trips_vehicle_status");

        // Ignore domain events
        builder.Ignore(t => t.DomainEvents);
    }
}
