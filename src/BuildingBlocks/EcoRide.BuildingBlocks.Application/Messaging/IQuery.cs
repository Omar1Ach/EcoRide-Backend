using EcoRide.BuildingBlocks.Domain;
using MediatR;

namespace EcoRide.BuildingBlocks.Application.Messaging;

/// <summary>
/// Marker interface for queries (read operations)
/// </summary>
public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{
}
