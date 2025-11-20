using EcoRide.BuildingBlocks.Application.Messaging;
using EcoRide.Modules.Trip.Application.DTOs;

namespace EcoRide.Modules.Trip.Application.Queries.GetTripReceipt;

/// <summary>
/// Query to get trip receipt
/// US-007: Trip History - View receipt
/// </summary>
public sealed record GetTripReceiptQuery(Guid TripId, Guid UserId) : IQuery<ReceiptDto>;
