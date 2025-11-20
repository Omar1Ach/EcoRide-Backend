using EcoRide.BuildingBlocks.Application.Messaging;
using EcoRide.Modules.Security.Application.DTOs;

namespace EcoRide.Modules.Security.Application.Commands.AddFundsToWallet;

/// <summary>
/// Command to add funds to user's wallet
/// US-008: Wallet Management (TC-070 to TC-074)
/// </summary>
public sealed record AddFundsToWalletCommand(
    Guid UserId,
    decimal Amount,
    string PaymentMethodId) : ICommand<WalletBalanceDto>;
