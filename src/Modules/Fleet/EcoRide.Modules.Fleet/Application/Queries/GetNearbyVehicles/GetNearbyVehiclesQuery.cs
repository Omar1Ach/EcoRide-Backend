using EcoRide.BuildingBlocks.Application.Messaging;
using EcoRide.Modules.Fleet.Application.DTOs;

namespace EcoRide.Modules.Fleet.Application.Queries.GetNearbyVehicles;

/// <summary>
/// Query to get nearby available vehicles
/// </summary>
public sealed record GetNearbyVehiclesQuery(
    double Latitude,
    double Longitude,
    int RadiusMeters = 500
) : IQuery<List<VehicleDto>>;
