namespace EcoRide.Modules.Security.Application.DTOs;

/// <summary>
/// DTO for wallet transaction
/// US-008: Wallet Management - Transaction history
/// </summary>
public sealed record WalletTransactionDto(
    Guid TransactionId,
    decimal Amount,
    string TransactionType,
    string PaymentMethod,
    string? PaymentDetails,
    decimal BalanceBefore,
    decimal BalanceAfter,
    DateTime CreatedAt);
