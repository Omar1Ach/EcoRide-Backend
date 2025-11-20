using EcoRide.BuildingBlocks.Application.Messaging;
using EcoRide.BuildingBlocks.Domain;
using EcoRide.Modules.Security.Application.DTOs;
using EcoRide.Modules.Security.Domain.Repositories;

namespace EcoRide.Modules.Security.Application.Queries.GetWalletBalance;

public sealed class GetWalletBalanceQueryHandler : IQueryHandler<GetWalletBalanceQuery, WalletBalanceDto>
{
    private readonly IUserRepository _userRepository;

    public GetWalletBalanceQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<WalletBalanceDto>> Handle(
        GetWalletBalanceQuery request,
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);

        if (user is null)
        {
            return Result.Failure<WalletBalanceDto>(
                new Error("Wallet.UserNotFound", "User not found"));
        }

        return Result.Success(new WalletBalanceDto(user.WalletBalance));
    }
}
