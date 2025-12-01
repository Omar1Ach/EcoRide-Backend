using EcoRide.BuildingBlocks.Application.Messaging;
using EcoRide.BuildingBlocks.Domain;
using EcoRide.Modules.Security.Application.DTOs;
using EcoRide.Modules.Security.Domain.Repositories;
using EcoRide.Modules.Security.Domain.ValueObjects;
using EcoRide.Modules.Security.Infrastructure.Persistence;

namespace EcoRide.Modules.Security.Application.Commands.UpdateUserProfile;

/// <summary>
/// Handler for UpdateUserProfileCommand
/// </summary>
public sealed class UpdateUserProfileCommandHandler : ICommandHandler<UpdateUserProfileCommand, UserProfileDto>
{
    private readonly IUserRepository _userRepository;
    private readonly SecurityDbContext _dbContext;

    public UpdateUserProfileCommandHandler(
        IUserRepository userRepository,
        SecurityDbContext dbContext)
    {
        _userRepository = userRepository;
        _dbContext = dbContext;
    }

    public async Task<Result<UserProfileDto>> Handle(
        UpdateUserProfileCommand request,
        CancellationToken cancellationToken)
    {
        // Get user
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);

        if (user is null)
        {
            return Result.Failure<UserProfileDto>(
                new Error("User.NotFound", "User not found"));
        }

        // Create value objects
        var fullNameResult = FullName.Create(request.FullName);
        if (fullNameResult.IsFailure)
        {
            return Result.Failure<UserProfileDto>(fullNameResult.Error);
        }

        var emailResult = Email.Create(request.Email);
        if (emailResult.IsFailure)
        {
            return Result.Failure<UserProfileDto>(emailResult.Error);
        }

        var phoneResult = PhoneNumber.Create(request.PhoneNumber);
        if (phoneResult.IsFailure)
        {
            return Result.Failure<UserProfileDto>(phoneResult.Error);
        }

        // Check if new email or phone is already taken by another user
        if (user.Email.Value != emailResult.Value.Value ||
            user.PhoneNumber.Value != phoneResult.Value.Value)
        {
            var existingUser = await _userRepository.GetByEmailAsync(emailResult.Value, cancellationToken);
            if (existingUser != null && existingUser.Id != user.Id)
            {
                return Result.Failure<UserProfileDto>(
                    new Error("User.EmailAlreadyExists", "Email is already in use"));
            }

            existingUser = await _userRepository.GetByPhoneAsync(phoneResult.Value, cancellationToken);
            if (existingUser != null && existingUser.Id != user.Id)
            {
                return Result.Failure<UserProfileDto>(
                    new Error("User.PhoneAlreadyExists", "Phone number is already in use"));
            }
        }

        // Update profile
        var updateResult = user.UpdateProfile(
            fullNameResult.Value,
            emailResult.Value,
            phoneResult.Value);

        if (updateResult.IsFailure)
        {
            return Result.Failure<UserProfileDto>(updateResult.Error);
        }

        // Update repository
        _userRepository.Update(user);

        // Commit transaction
        await _dbContext.SaveChangesAsync(cancellationToken);

        // Return updated profile
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
