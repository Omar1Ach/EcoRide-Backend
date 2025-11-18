using EcoRide.BuildingBlocks.Application.Messaging;
using EcoRide.Modules.Security.Application.DTOs;

namespace EcoRide.Modules.Security.Application.Commands.RegisterUser;

/// <summary>
/// Command to register a new user
/// </summary>
public sealed record RegisterUserCommand(
    string Email,
    string PhoneNumber,
    string Password,
    string FullName
) : ICommand<RegisterUserResponse>;
