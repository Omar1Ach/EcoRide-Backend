using EcoRide.BuildingBlocks.Application.Messaging;
using EcoRide.Modules.Security.Application.DTOs;

namespace EcoRide.Modules.Security.Application.Queries.GetWalletBalance;

/// <summary>
/// Query to get user's wallet balance
/// US-008: Wallet Management
/// </summary>
public sealed record GetWalletBalanceQuery(Guid UserId) : IQuery<WalletBalanceDto>;
