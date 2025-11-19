using EcoRide.BuildingBlocks.Application.Messaging;
using EcoRide.BuildingBlocks.Domain;
using EcoRide.Modules.Security.Application.Data;
using EcoRide.Modules.Security.Application.Services;
using EcoRide.Modules.Security.Domain.Repositories;
using EcoRide.Modules.Security.Domain.ValueObjects;

namespace EcoRide.Modules.Security.Application.Commands.ForgotPassword;

/// <summary>
/// Handler for ForgotPasswordCommand
/// Generates password reset token and sends it via SMS
/// </summary>
public sealed class ForgotPasswordCommandHandler : ICommandHandler<ForgotPasswordCommand, string>
{
    private readonly IUserRepository _userRepository;
    private readonly ISecurityUnitOfWork _unitOfWork;
    private readonly ISmsService _smsService;

    public ForgotPasswordCommandHandler(
        IUserRepository userRepository,
        ISecurityUnitOfWork unitOfWork,
        ISmsService smsService)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _smsService = smsService;
    }

    public async Task<Result<string>> Handle(
        ForgotPasswordCommand request,
        CancellationToken cancellationToken)
    {
        // Validate email
        var emailResult = Email.Create(request.Email);
        if (emailResult.IsFailure)
        {
            return Result.Failure<string>(emailResult.Error);
        }

        // Get user by email
        var user = await _userRepository.GetByEmailAsync(emailResult.Value, cancellationToken);

        // Don't reveal if user exists or not for security
        if (user is null)
        {
            return Result.Success("If an account with that email exists, a password reset code has been sent to the registered phone number.");
        }

        // Check if account is active
        if (!user.IsActive)
        {
            return Result.Success("If an account with that email exists, a password reset code has been sent to the registered phone number.");
        }

        // Generate password reset token
        var resetToken = user.GeneratePasswordResetToken();

        // Send reset token via SMS (using first 6 characters as code for user convenience)
        var resetCode = resetToken.Substring(0, 6).ToUpper();
        var smsResult = await _smsService.SendOtpAsync(
            user.PhoneNumber,
            resetCode,
            cancellationToken);

        if (smsResult.IsFailure)
        {
            return Result.Failure<string>(smsResult.Error);
        }

        // Save changes
        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success("If an account with that email exists, a password reset code has been sent to the registered phone number.");
    }
}
