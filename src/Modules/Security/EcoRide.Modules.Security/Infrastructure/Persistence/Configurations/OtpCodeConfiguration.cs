using EcoRide.Modules.Security.Domain.Aggregates;
using EcoRide.Modules.Security.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcoRide.Modules.Security.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for OtpCode entity
/// </summary>
public sealed class OtpCodeConfiguration : IEntityTypeConfiguration<OtpCode>
{
    public void Configure(EntityTypeBuilder<OtpCode> builder)
    {
        builder.ToTable("otp_codes", "security");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(o => o.PhoneNumber)
            .HasColumnName("phone")
            .HasMaxLength(20)
            .IsRequired()
            .HasConversion(
                phone => phone.Value,
                value => PhoneNumber.Create(value).Value);

        builder.Property(o => o.Code)
            .HasColumnName("code")
            .HasMaxLength(6)
            .IsRequired();

        builder.Property(o => o.ExpiresAt)
            .HasColumnName("expires_at")
            .IsRequired();

        builder.Property(o => o.Attempts)
            .HasColumnName("attempts")
            .IsRequired();

        builder.Property(o => o.Verified)
            .HasColumnName("verified")
            .IsRequired();

        builder.Property(o => o.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        // Indexes
        builder.HasIndex(o => o.PhoneNumber)
            .HasDatabaseName("idx_otp_codes_phone");

        builder.HasIndex(o => o.ExpiresAt)
            .HasDatabaseName("idx_otp_codes_expires_at");

        // Ignore domain events (not persisted)
        builder.Ignore(o => o.DomainEvents);
    }
}
