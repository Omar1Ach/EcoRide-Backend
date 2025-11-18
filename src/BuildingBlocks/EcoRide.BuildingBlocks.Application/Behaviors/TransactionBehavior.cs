using EcoRide.BuildingBlocks.Application.Data;
using EcoRide.BuildingBlocks.Application.Messaging;
using MediatR;

namespace EcoRide.BuildingBlocks.Application.Behaviors;

/// <summary>
/// Pipeline behavior that wraps command execution in a database transaction
/// Only applies to ICommand requests (write operations)
/// </summary>
public class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public TransactionBehavior(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Only apply transaction to commands, not queries
        if (!IsCommand(request))
        {
            return await next();
        }

        var response = await next();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return response;
    }

    private static bool IsCommand(TRequest request)
    {
        return request is ICommand ||
               request.GetType().GetInterfaces().Any(i =>
                   i.IsGenericType &&
                   i.GetGenericTypeDefinition() == typeof(ICommand<>));
    }
}
