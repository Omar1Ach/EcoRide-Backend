using EcoRide.BuildingBlocks.Application.Messaging;
using EcoRide.BuildingBlocks.Domain;
using EcoRide.Modules.Security.Application.DTOs;
using EcoRide.Modules.Security.Application.Services;
using EcoRide.Modules.Security.Domain.Aggregates;
using EcoRide.Modules.Security.Domain.Repositories;
using EcoRide.Modules.Security.Domain.ValueObjects;
using EcoRide.Modules.Security.Infrastructure.Persistence;

namespace EcoRide.Modules.Security.Application.Commands.RegisterUser;

/// <summary>
/// Handler for RegisterUserCommand
/// </summary>
public sealed class RegisterUserCommandHandler : ICommandHandler<RegisterUserCommand, RegisterUserResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IOtpRepository _otpRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ISmsService _smsService;
    private readonly SecurityDbContext _dbContext;

    public RegisterUserCommandHandler(
        IUserRepository userRepository,
        IOtpRepository otpRepository,
        IPasswordHasher passwordHasher,
        ISmsService smsService,
        SecurityDbContext dbContext)
    {
        _userRepository = userRepository;
        _otpRepository = otpRepository;
        _passwordHasher = passwordHasher;
        _smsService = smsService;
        _dbContext = dbContext;
    }

    public async Task<Result<RegisterUserResponse>> Handle(
        RegisterUserCommand request,
        CancellationToken cancellationToken)
    {
        // Create value objects
        var emailResult = Email.Create(request.Email);
        if (emailResult.IsFailure)
        {
            return Result.Failure<RegisterUserResponse>(emailResult.Error);
        }

        var phoneResult = PhoneNumber.Create(request.PhoneNumber);
        if (phoneResult.IsFailure)
        {
            return Result.Failure<RegisterUserResponse>(phoneResult.Error);
        }

        var fullNameResult = FullName.Create(request.FullName);
        if (fullNameResult.IsFailure)
        {
            return Result.Failure<RegisterUserResponse>(fullNameResult.Error);
        }

        // Check if user already exists
        var exists = await _userRepository.ExistsAsync(
            emailResult.Value,
            phoneResult.Value,
            cancellationToken);

        if (exists)
        {
            return Result.Failure<RegisterUserResponse>(
                new Error("User.AlreadyExists", "User with this email or phone already exists"));
        }

        // Hash password
        var passwordHash = _passwordHasher.Hash(request.Password);

        // Create user aggregate
        var userResult = User.CreatePendingRegistration(
            emailResult.Value,
            phoneResult.Value,
            passwordHash,
            fullNameResult.Value);

        if (userResult.IsFailure)
        {
            return Result.Failure<RegisterUserResponse>(userResult.Error);
        }

        var user = userResult.Value;

        // Generate OTP
        var otpCode = OtpCode.Generate(phoneResult.Value);

        // Send SMS
        var smsResult = await _smsService.SendOtpAsync(
            phoneResult.Value,
            otpCode.Code,
            cancellationToken);

        if (smsResult.IsFailure)
        {
            return Result.Failure<RegisterUserResponse>(smsResult.Error);
        }

        // Save to repository
        await _userRepository.AddAsync(user, cancellationToken);
        await _otpRepository.AddAsync(otpCode, cancellationToken);

        // Commit transaction
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(new RegisterUserResponse(
            user.Id,
            $"OTP sent to {phoneResult.Value}. Please verify within 5 minutes."));
    }
}
