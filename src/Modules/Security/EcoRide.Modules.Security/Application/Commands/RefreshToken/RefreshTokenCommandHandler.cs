using EcoRide.BuildingBlocks.Application.Messaging;
using EcoRide.BuildingBlocks.Domain;
using EcoRide.Modules.Security.Application.Data;
using EcoRide.Modules.Security.Application.DTOs;
using EcoRide.Modules.Security.Application.Services;
using EcoRide.Modules.Security.Domain.Repositories;

namespace EcoRide.Modules.Security.Application.Commands.RefreshToken;

/// <summary>
/// Handler for RefreshTokenCommand
/// Validates refresh token and generates new access token
/// Implements refresh token rotation for security
/// </summary>
public sealed class RefreshTokenCommandHandler : ICommandHandler<RefreshTokenCommand, RefreshTokenResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly ISecurityUnitOfWork _unitOfWork;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public RefreshTokenCommandHandler(
        IUserRepository userRepository,
        ISecurityUnitOfWork unitOfWork,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<Result<RefreshTokenResponse>> Handle(
        RefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        // Get user by ID
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            return Result.Failure<RefreshTokenResponse>(
                new Error("RefreshToken.UserNotFound", "User not found"));
        }

        // Check if account is active
        if (!user.IsActive)
        {
            return Result.Failure<RefreshTokenResponse>(
                new Error("RefreshToken.AccountInactive", "Account is inactive"));
        }

        // Validate refresh token
        var validationResult = user.ValidateRefreshToken(request.RefreshToken);
        if (validationResult.IsFailure)
        {
            return Result.Failure<RefreshTokenResponse>(validationResult.Error);
        }

        // Generate new access token
        var accessToken = _jwtTokenGenerator.GenerateAccessToken(
            user.Id,
            user.Email,
            user.Role.ToString());

        // Implement refresh token rotation: Generate new refresh token
        var newRefreshToken = _jwtTokenGenerator.GenerateRefreshToken();
        var refreshTokenExpiry = DateTime.UtcNow.AddDays(7); // 7 days validity

        user.SetRefreshToken(newRefreshToken, refreshTokenExpiry);

        // Save changes
        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new RefreshTokenResponse(
            accessToken,
            newRefreshToken,
            DateTime.UtcNow.AddMinutes(15))); // Access token expires in 15 minutes
    }
}
