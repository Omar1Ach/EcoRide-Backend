using EcoRide.BuildingBlocks.Domain;
using MediatR;

namespace EcoRide.BuildingBlocks.Application.Messaging;

/// <summary>
/// Handler for commands that don't return a value
/// </summary>
public interface ICommandHandler<in TCommand> : IRequestHandler<TCommand, Result>
    where TCommand : ICommand
{
}

/// <summary>
/// Handler for commands that return a value
/// </summary>
public interface ICommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, Result<TResponse>>
    where TCommand : ICommand<TResponse>
{
}
