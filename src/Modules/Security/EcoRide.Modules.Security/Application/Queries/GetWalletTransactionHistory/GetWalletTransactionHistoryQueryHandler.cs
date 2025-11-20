using EcoRide.BuildingBlocks.Application.Messaging;
using EcoRide.BuildingBlocks.Domain;
using EcoRide.Modules.Security.Application.DTOs;
using EcoRide.Modules.Security.Domain.Repositories;

namespace EcoRide.Modules.Security.Application.Queries.GetWalletTransactionHistory;

public sealed class GetWalletTransactionHistoryQueryHandler
    : IQueryHandler<GetWalletTransactionHistoryQuery, WalletTransactionHistoryDto>
{
    private readonly IWalletTransactionRepository _transactionRepository;

    public GetWalletTransactionHistoryQueryHandler(IWalletTransactionRepository transactionRepository)
    {
        _transactionRepository = transactionRepository;
    }

    public async Task<Result<WalletTransactionHistoryDto>> Handle(
        GetWalletTransactionHistoryQuery request,
        CancellationToken cancellationToken)
    {
        if (request.PageNumber < 1)
        {
            return Result.Failure<WalletTransactionHistoryDto>(
                new Error("Pagination.InvalidPageNumber", "Page number must be greater than 0"));
        }

        if (request.PageSize < 1 || request.PageSize > 100)
        {
            return Result.Failure<WalletTransactionHistoryDto>(
                new Error("Pagination.InvalidPageSize", "Page size must be between 1 and 100"));
        }

        var (transactions, totalCount) = await _transactionRepository.GetByUserIdAsync(
            request.UserId,
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        var transactionDtos = transactions.Select(t => new WalletTransactionDto(
            t.Id,
            t.Amount,
            t.TransactionType,
            t.PaymentMethod,
            t.PaymentDetails,
            t.BalanceBefore,
            t.BalanceAfter,
            t.CreatedAt)).ToList();

        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

        return Result.Success(new WalletTransactionHistoryDto(
            transactionDtos,
            totalCount,
            request.PageNumber,
            request.PageSize,
            totalPages));
    }
}
