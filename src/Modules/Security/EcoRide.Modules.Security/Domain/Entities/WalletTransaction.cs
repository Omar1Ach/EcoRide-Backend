using EcoRide.BuildingBlocks.Domain;

namespace EcoRide.Modules.Security.Domain.Entities;

/// <summary>
/// Wallet transaction entity for tracking wallet top-ups
/// US-008: Wallet Management - Transaction history
/// </summary>
public sealed class WalletTransaction : Entity<Guid>
{
    public Guid UserId { get; private set; }
    public decimal Amount { get; private set; }
    public string TransactionType { get; private set; } // "TopUp", "Deduction"
    public string PaymentMethod { get; private set; } // "CreditCard", "Wallet"
    public string? PaymentDetails { get; private set; }
    public decimal BalanceBefore { get; private set; }
    public decimal BalanceAfter { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // EF Core constructor
    private WalletTransaction()
    {
        TransactionType = string.Empty;
        PaymentMethod = string.Empty;
    }

    private WalletTransaction(
        Guid id,
        Guid userId,
        decimal amount,
        string transactionType,
        string paymentMethod,
        string? paymentDetails,
        decimal balanceBefore,
        decimal balanceAfter)
    {
        Id = id;
        UserId = userId;
        Amount = amount;
        TransactionType = transactionType;
        PaymentMethod = paymentMethod;
        PaymentDetails = paymentDetails;
        BalanceBefore = balanceBefore;
        BalanceAfter = balanceAfter;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Create a wallet top-up transaction
    /// </summary>
    public static Result<WalletTransaction> CreateTopUp(
        Guid userId,
        decimal amount,
        string paymentMethod,
        string? paymentDetails,
        decimal balanceBefore,
        decimal balanceAfter)
    {
        if (userId == Guid.Empty)
        {
            return Result.Failure<WalletTransaction>(
                new Error("WalletTransaction.InvalidUserId", "User ID is required"));
        }

        if (amount <= 0)
        {
            return Result.Failure<WalletTransaction>(
                new Error("WalletTransaction.InvalidAmount", "Amount must be greater than 0"));
        }

        if (string.IsNullOrWhiteSpace(paymentMethod))
        {
            return Result.Failure<WalletTransaction>(
                new Error("WalletTransaction.InvalidPaymentMethod", "Payment method is required"));
        }

        var transaction = new WalletTransaction(
            Guid.NewGuid(),
            userId,
            amount,
            "TopUp",
            paymentMethod,
            paymentDetails,
            balanceBefore,
            balanceAfter);

        return Result.Success(transaction);
    }

    /// <summary>
    /// Create a wallet deduction transaction (for trip payments)
    /// </summary>
    public static Result<WalletTransaction> CreateDeduction(
        Guid userId,
        decimal amount,
        decimal balanceBefore,
        decimal balanceAfter)
    {
        if (userId == Guid.Empty)
        {
            return Result.Failure<WalletTransaction>(
                new Error("WalletTransaction.InvalidUserId", "User ID is required"));
        }

        if (amount <= 0)
        {
            return Result.Failure<WalletTransaction>(
                new Error("WalletTransaction.InvalidAmount", "Amount must be greater than 0"));
        }

        var transaction = new WalletTransaction(
            Guid.NewGuid(),
            userId,
            amount,
            "Deduction",
            "Wallet",
            "Trip payment",
            balanceBefore,
            balanceAfter);

        return Result.Success(transaction);
    }
}
