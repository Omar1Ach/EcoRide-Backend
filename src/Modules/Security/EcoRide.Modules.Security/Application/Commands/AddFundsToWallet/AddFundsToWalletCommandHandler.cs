using EcoRide.BuildingBlocks.Application.Messaging;
using EcoRide.BuildingBlocks.Domain;
using EcoRide.Modules.Security.Application.Data;
using EcoRide.Modules.Security.Application.DTOs;
using EcoRide.Modules.Security.Domain.Entities;
using EcoRide.Modules.Security.Domain.Repositories;

namespace EcoRide.Modules.Security.Application.Commands.AddFundsToWallet;

/// <summary>
/// Handler for adding funds to wallet
/// US-008: Wallet Management
/// </summary>
public sealed class AddFundsToWalletCommandHandler : ICommandHandler<AddFundsToWalletCommand, WalletBalanceDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IWalletTransactionRepository _transactionRepository;
    private readonly ISecurityUnitOfWork _unitOfWork;

    public AddFundsToWalletCommandHandler(
        IUserRepository userRepository,
        IWalletTransactionRepository transactionRepository,
        ISecurityUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _transactionRepository = transactionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<WalletBalanceDto>> Handle(
        AddFundsToWalletCommand request,
        CancellationToken cancellationToken)
    {
        // Validate amount (TC-071, TC-072)
        if (request.Amount < 10)
        {
            return Result.Failure<WalletBalanceDto>(
                new Error("Wallet.MinAmount", "Minimum amount is 10 MAD"));
        }

        if (request.Amount > 1000)
        {
            return Result.Failure<WalletBalanceDto>(
                new Error("Wallet.MaxAmount", "Maximum amount is 1000 MAD"));
        }

        // Get user
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            return Result.Failure<WalletBalanceDto>(
                new Error("Wallet.UserNotFound", "User not found"));
        }

        // Process payment (simulate Stripe payment - TC-073)
        // In production, integrate with actual Stripe API
        var balanceBefore = user.WalletBalance;

        // Add funds to wallet (TC-070)
        user.AddToWallet(request.Amount);

        // Create transaction record (TC-074)
        var transactionResult = WalletTransaction.CreateTopUp(
            request.UserId,
            request.Amount,
            "CreditCard",
            $"Card ending in {request.PaymentMethodId.Substring(Math.Max(0, request.PaymentMethodId.Length - 4))}",
            balanceBefore,
            user.WalletBalance);

        if (transactionResult.IsFailure)
        {
            return Result.Failure<WalletBalanceDto>(transactionResult.Error);
        }

        _transactionRepository.Add(transactionResult.Value);

        // Save changes
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new WalletBalanceDto(user.WalletBalance));
    }
}
