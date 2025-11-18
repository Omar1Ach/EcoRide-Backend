using EcoRide.Modules.Fleet.Domain.Aggregates;
using EcoRide.Modules.Fleet.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetTopologySuite.Geometries;
using ValueObjects = EcoRide.Modules.Fleet.Domain.ValueObjects;

namespace EcoRide.Modules.Fleet.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for Vehicle entity with PostGIS support
/// </summary>
public sealed class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> builder)
    {
        builder.ToTable("vehicles", "fleet");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(v => v.Code)
            .HasColumnName("code")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(v => v.Type)
            .HasColumnName("type")
            .IsRequired()
            .HasConversion(
                type => type.ToString(),
                value => Enum.Parse<VehicleType>(value));

        builder.Property(v => v.Status)
            .HasColumnName("status")
            .IsRequired()
            .HasConversion(
                status => status.ToString(),
                value => Enum.Parse<VehicleStatus>(value));

        builder.Property(v => v.BatteryLevel)
            .HasColumnName("battery_level")
            .IsRequired()
            .HasConversion(
                battery => battery.Value,
                value => ValueObjects.BatteryLevel.Create(value).Value);

        // PostGIS Point mapping for location
        builder.Property(v => v.Location)
            .HasColumnName("location")
            .HasColumnType("geography(Point, 4326)")
            .IsRequired()
            .HasConversion(
                location => new Point(location.Longitude, location.Latitude) { SRID = 4326 },
                point => ValueObjects.Location.Create(point.Y, point.X).Value);

        builder.Property(v => v.LastLocationUpdate)
            .HasColumnName("last_location_update")
            .IsRequired();

        builder.Property(v => v.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(v => v.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        // Indexes
        builder.HasIndex(v => v.Code)
            .HasDatabaseName("idx_vehicles_code")
            .IsUnique();

        builder.HasIndex(v => v.Status)
            .HasDatabaseName("idx_vehicles_status");

        // Spatial index for location (PostGIS GIST index)
        builder.HasIndex(v => v.Location)
            .HasDatabaseName("idx_vehicles_location")
            .HasMethod("gist");

        // Ignore domain events
        builder.Ignore(v => v.DomainEvents);
    }
}
