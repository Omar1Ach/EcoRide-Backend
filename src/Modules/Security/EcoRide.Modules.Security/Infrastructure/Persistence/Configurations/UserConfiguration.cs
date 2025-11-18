using EcoRide.Modules.Security.Domain.Aggregates;
using EcoRide.Modules.Security.Domain.Enums;
using EcoRide.Modules.Security.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcoRide.Modules.Security.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for User entity
/// </summary>
public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users", "security");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(u => u.Email)
            .HasColumnName("email")
            .HasMaxLength(255)
            .IsRequired()
            .HasConversion(
                email => email.Value,
                value => Email.Create(value).Value);

        builder.Property(u => u.PhoneNumber)
            .HasColumnName("phone")
            .HasMaxLength(20)
            .IsRequired()
            .HasConversion(
                phone => phone.Value,
                value => PhoneNumber.Create(value).Value);

        builder.Property(u => u.PasswordHash)
            .HasColumnName("password_hash")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(u => u.FullName)
            .HasColumnName("full_name")
            .HasMaxLength(100)
            .IsRequired()
            .HasConversion(
                name => name.Value,
                value => FullName.Create(value).Value);

        builder.Property(u => u.Role)
            .HasColumnName("role")
            .IsRequired()
            .HasConversion(
                role => role.ToString(),
                value => Enum.Parse<UserRole>(value));

        builder.Property(u => u.KycStatus)
            .HasColumnName("kyc_status")
            .IsRequired()
            .HasConversion(
                status => status.ToString(),
                value => Enum.Parse<KycStatus>(value));

        builder.Property(u => u.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(u => u.PhoneVerified)
            .HasColumnName("phone_verified")
            .IsRequired();

        builder.Property(u => u.EmailVerified)
            .HasColumnName("email_verified")
            .IsRequired();

        builder.Property(u => u.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(u => u.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.Property(u => u.DeletedAt)
            .HasColumnName("deleted_at");

        // Indexes
        builder.HasIndex(u => u.Email)
            .HasDatabaseName("idx_users_email")
            .IsUnique()
            .HasFilter("deleted_at IS NULL");

        builder.HasIndex(u => u.PhoneNumber)
            .HasDatabaseName("idx_users_phone")
            .IsUnique()
            .HasFilter("deleted_at IS NULL");

        // Ignore domain events (not persisted)
        builder.Ignore(u => u.DomainEvents);
    }
}
