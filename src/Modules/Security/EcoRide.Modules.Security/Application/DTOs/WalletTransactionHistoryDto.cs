namespace EcoRide.Modules.Security.Application.DTOs;

/// <summary>
/// DTO for wallet transaction history response
/// US-008: Wallet Management
/// </summary>
public sealed record WalletTransactionHistoryDto(
    List<WalletTransactionDto> Transactions,
    int TotalCount,
    int PageNumber,
    int PageSize,
    int TotalPages);
