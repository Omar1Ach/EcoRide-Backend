using EcoRide.BuildingBlocks.Application.Messaging;
using EcoRide.BuildingBlocks.Domain;
using EcoRide.Modules.Security.Application.DTOs;
using EcoRide.Modules.Security.Domain.Repositories;

namespace EcoRide.Modules.Security.Application.Queries.GetUserProfile;

public sealed class GetUserProfileQueryHandler : IQueryHandler<GetUserProfileQuery, UserProfileDto>
{
    private readonly IUserRepository _userRepository;

    public GetUserProfileQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<UserProfileDto>> Handle(
        GetUserProfileQuery request,
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);

        if (user is null)
        {
            return Result.Failure<UserProfileDto>(
                new Error("User.NotFound", "User not found"));
        }

        var profileDto = new UserProfileDto(
            user.Id,
            user.FullName.Value,
            user.Email.Value,
            user.PhoneNumber.Value,
            user.EmailVerified,
            user.PhoneVerified,
            user.KycStatus.ToString(),
            user.CreatedAt,
            user.UpdatedAt);

        return Result.Success(profileDto);
    }
}
