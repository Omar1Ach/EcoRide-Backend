using EcoRide.Modules.Security.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcoRide.Modules.Security.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for WalletTransaction entity
/// US-008: Wallet Management
/// </summary>
internal sealed class WalletTransactionConfiguration : IEntityTypeConfiguration<WalletTransaction>
{
    public void Configure(EntityTypeBuilder<WalletTransaction> builder)
    {
        builder.ToTable("wallet_transactions", "security");

        builder.HasKey(wt => wt.Id);

        builder.Property(wt => wt.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(wt => wt.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(wt => wt.Amount)
            .HasColumnName("amount")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(wt => wt.TransactionType)
            .HasColumnName("transaction_type")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(wt => wt.PaymentMethod)
            .HasColumnName("payment_method")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(wt => wt.PaymentDetails)
            .HasColumnName("payment_details")
            .HasMaxLength(200);

        builder.Property(wt => wt.BalanceBefore)
            .HasColumnName("balance_before")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(wt => wt.BalanceAfter)
            .HasColumnName("balance_after")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(wt => wt.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        // Indexes
        builder.HasIndex(wt => wt.UserId)
            .HasDatabaseName("idx_wallet_transactions_user_id");

        builder.HasIndex(wt => wt.CreatedAt)
            .HasDatabaseName("idx_wallet_transactions_created_at");

        builder.HasIndex(wt => new { wt.UserId, wt.CreatedAt })
            .HasDatabaseName("idx_wallet_transactions_user_created");
    }
}
