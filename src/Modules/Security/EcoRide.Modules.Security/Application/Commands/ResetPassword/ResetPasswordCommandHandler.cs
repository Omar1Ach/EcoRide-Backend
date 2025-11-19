using EcoRide.BuildingBlocks.Application.Messaging;
using EcoRide.BuildingBlocks.Domain;
using EcoRide.Modules.Security.Application.Data;
using EcoRide.Modules.Security.Application.Services;
using EcoRide.Modules.Security.Domain.Repositories;
using EcoRide.Modules.Security.Domain.ValueObjects;

namespace EcoRide.Modules.Security.Application.Commands.ResetPassword;

/// <summary>
/// Handler for ResetPasswordCommand
/// Validates reset code and updates password
/// </summary>
public sealed class ResetPasswordCommandHandler : ICommandHandler<ResetPasswordCommand, string>
{
    private readonly IUserRepository _userRepository;
    private readonly ISecurityUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;

    public ResetPasswordCommandHandler(
        IUserRepository userRepository,
        ISecurityUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<string>> Handle(
        ResetPasswordCommand request,
        CancellationToken cancellationToken)
    {
        // Validate email
        var emailResult = Email.Create(request.Email);
        if (emailResult.IsFailure)
        {
            return Result.Failure<string>(emailResult.Error);
        }

        // Validate new password
        if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 8)
        {
            return Result.Failure<string>(
                new Error("Password.TooShort", "Password must be at least 8 characters long"));
        }

        // Get user by email
        var user = await _userRepository.GetByEmailAsync(emailResult.Value, cancellationToken);
        if (user is null)
        {
            return Result.Failure<string>(
                new Error("ResetPassword.InvalidCode", "Invalid or expired reset code"));
        }

        // Validate reset token (using first 6 characters as code)
        if (string.IsNullOrWhiteSpace(user.PasswordResetToken))
        {
            return Result.Failure<string>(
                new Error("ResetPassword.InvalidCode", "Invalid or expired reset code"));
        }

        var storedCode = user.PasswordResetToken.Substring(0, 6).ToUpper();
        if (storedCode != request.ResetCode.ToUpper())
        {
            return Result.Failure<string>(
                new Error("ResetPassword.InvalidCode", "Invalid or expired reset code"));
        }

        // Validate token expiry
        var validationResult = user.ValidatePasswordResetToken(user.PasswordResetToken);
        if (validationResult.IsFailure)
        {
            return Result.Failure<string>(
                new Error("ResetPassword.InvalidCode", "Invalid or expired reset code"));
        }

        // Hash new password
        var passwordHash = _passwordHasher.Hash(request.NewPassword);

        // Reset password
        var resetResult = user.ResetPassword(passwordHash);
        if (resetResult.IsFailure)
        {
            return Result.Failure<string>(resetResult.Error);
        }

        // Save changes
        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success("Password has been reset successfully. You can now login with your new password.");
    }
}
