using EcoRide.BuildingBlocks.Application.Messaging;
using EcoRide.Modules.Security.Application.DTOs;

namespace EcoRide.Modules.Security.Application.Queries.GetWalletTransactionHistory;

/// <summary>
/// Query to get wallet transaction history
/// US-008: Wallet Management (TC-074)
/// </summary>
public sealed record GetWalletTransactionHistoryQuery(
    Guid UserId,
    int PageNumber = 1,
    int PageSize = 20) : IQuery<WalletTransactionHistoryDto>;
