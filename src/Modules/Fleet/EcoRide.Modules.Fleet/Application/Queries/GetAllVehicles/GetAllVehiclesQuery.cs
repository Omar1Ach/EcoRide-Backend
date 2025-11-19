using EcoRide.BuildingBlocks.Application.Messaging;
using EcoRide.Modules.Fleet.Application.DTOs;

namespace EcoRide.Modules.Fleet.Application.Queries.GetAllVehicles;

/// <summary>
/// Query to get all vehicles with optional filtering
/// </summary>
public sealed record GetAllVehiclesQuery(
    string? Status = null,  // Filter by status: Available, Reserved, InUse, etc.
    string? Type = null,    // Filter by type: Scooter, Bike
    int? MinBatteryLevel = null,  // Filter by minimum battery level
    int PageNumber = 1,
    int PageSize = 50
) : IQuery<GetAllVehiclesResponse>;

/// <summary>
/// Response containing list of vehicles with pagination
/// </summary>
public sealed record GetAllVehiclesResponse(
    List<VehicleDto> Vehicles,
    int TotalCount,
    int PageNumber,
    int PageSize,
    int TotalPages
);
