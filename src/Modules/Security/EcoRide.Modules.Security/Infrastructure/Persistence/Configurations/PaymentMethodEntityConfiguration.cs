using EcoRide.Modules.Security.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcoRide.Modules.Security.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for PaymentMethodEntity
/// US-006: Credit card payment fallback
/// </summary>
public sealed class PaymentMethodEntityConfiguration : IEntityTypeConfiguration<PaymentMethodEntity>
{
    public void Configure(EntityTypeBuilder<PaymentMethodEntity> builder)
    {
        builder.ToTable("payment_methods", "security");

        builder.HasKey(pm => pm.Id);

        builder.Property(pm => pm.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(pm => pm.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(pm => pm.CardLast4)
            .HasColumnName("card_last4")
            .HasMaxLength(4)
            .IsRequired();

        builder.Property(pm => pm.CardType)
            .HasColumnName("card_type")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(pm => pm.ExpiryMonth)
            .HasColumnName("expiry_month")
            .IsRequired();

        builder.Property(pm => pm.ExpiryYear)
            .HasColumnName("expiry_year")
            .IsRequired();

        builder.Property(pm => pm.IsDefault)
            .HasColumnName("is_default")
            .IsRequired();

        builder.Property(pm => pm.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(pm => pm.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        // Indexes
        builder.HasIndex(pm => pm.UserId)
            .HasDatabaseName("idx_payment_methods_user_id");

        builder.HasIndex(pm => new { pm.UserId, pm.IsDefault })
            .HasDatabaseName("idx_payment_methods_user_default");
    }
}
