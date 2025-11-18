using EcoRide.BuildingBlocks.Domain;
using MediatR;

namespace EcoRide.BuildingBlocks.Application.Messaging;

/// <summary>
/// Handler for queries (read operations)
/// </summary>
public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>
{
}
